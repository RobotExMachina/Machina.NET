using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Machina.Drivers.Communication;
using Machina.Drivers.Communication.Protocols;

namespace Machina.Drivers.Communication
{
    /// <summary>
    /// A class that manages TCP communication with UR devices, including sending/receiving messages, 
    /// queuing them, releasing them to the TCP server when appropriate, and raising events on 
    /// buffer empty.
    /// </summary>
    internal class TCPCommunicationManagerUR
    {
        /**
         * Machina communication with UR robots:
         *  - Machina will connect via TCP/IP socket client to the robot's real time client at port 30003.
         *  - From this port, it will receive a buffer with information about the robot state at 125Hz.
         *  - To this port, Machina will send string buffers with compiled programs containing a number of actions
         *      determined by _maxStreamCount. 
         *  - Machina will also establish a TCP server to receive acknowledgement messages from the robot. Every uplaoded
         *      program contains instructions to connect to this server, and send an acknowledgement. This will help 
         *      Machina track program execution at runtime.
         *  - Machina will listens to these acknowledgements, and when, upon remining _sendNewBatchOn, compile and upload
         *      a new program with the next batch of actions. If _sendNewBatchOn is less than 2, Machina will
         *      duplicate the remaining actions.
         *  
         *  References:
         *      https://www.universal-robots.com/how-tos-and-faqs/how-to/ur-how-tos/remote-control-via-tcpip-16496/
         */

        internal TCPConnectionStatus ClientSocketStatus { get; private set; }

        private RobotCursor _writeCursor;
        private RobotCursor _motionCursor;
        private Driver _parentDriver;
        internal RobotLogger logger;

        /// <summary>
        ///  The client socket that connects to the robot's secondary client.
        /// </summary>
        private TcpClient _clientSocket;
        private NetworkStream _clientNetworkStream;
        private Thread _clientReceivingThread;
        private Thread _clientSendingThread;
        private string _robotIP;
        public string IP => _robotIP;
        private int _robotPort = 30003;
        public int Port => _robotPort;
        private bool _isDeviceBufferFull = false;

        private TcpListener _serverSocket;
        private Thread _serverListeningThread;
        private byte[] _serverListeningBytes = new byte[2048];
        private string _serverListeningMsg;
        private int[] _serverListeningInts;
        private bool _isServerListeningRunning = false;
        private Thread _serverSendingThread;
        private byte[] _ServerSendingBytes;
        public string ServerIP => _serverIP;
        private string _serverIP;
        public int ServerPort => _serverPort;
        private int _serverPort = 7003;
        private TcpClient _robotSocket;
        private NetworkStream _robotNetworkStream;

        private string _driverScript;

        private Protocols.Base _translator;
        //private StringBuilder _sb = new StringBuilder();
        //private List<string> _msgs;
        //private List<List<string>> _messageBuffer = new List<List<string>>();
        private List<int> _sentIDs = new List<int>();
        private List<int> _receivedIDs = new List<int>();
        private byte[] _sendMsgBytes;
        private byte[] _receiveMsgBytes = new byte[2048];
        private int _receiveByteCount;
        private string _response;
        private string[] _responseChunks;

        private int _sentMessages = 0;
        private int _receivedMessages = 0;
        private int _maxStreamCount = 10;
        private int _sendNewBatchOn = 2;

        private bool _bufferEmptyEventIsRaiseable = true;


        internal TCPCommunicationManagerUR(Driver driver, RobotCursor writeCursor, RobotCursor motionCursor, string robotIP, int robotPort)
        {
            this._parentDriver = driver;
            this.logger = driver.parentControl.Logger;
            this._writeCursor = writeCursor;
            this._motionCursor = motionCursor;
            this._robotIP = robotIP;
            //this._robotPort = robotPort;  // It will always be 30003, user need not care about this

            this._translator = Protocols.Factory.GetTranslator(this._parentDriver);

        }

        internal bool Disconnect()
        {
            if (_clientSocket != null)
            {
                // Upload an empty script to stop the running program
                LoadEmptyScript();
                UploadScriptToDevice(_driverScript, false);

                ClientSocketStatus = TCPConnectionStatus.Disconnected;
                _clientSocket.Client.Disconnect(false);
                _clientSocket.Close();
                if (_clientNetworkStream != null) _clientNetworkStream.Dispose();

                _isServerListeningRunning = false;

                return true;
            }

            return false;
        }

        internal bool Connect()
        {
            try
            {
                _clientSocket = new TcpClient();
                _clientSocket.Connect(this._robotIP, this._robotPort);
                ClientSocketStatus = TCPConnectionStatus.Connected;
                _clientNetworkStream = _clientSocket.GetStream();
                _clientSocket.ReceiveBufferSize = 2048;
                _clientSocket.SendBufferSize = 1024;

                //// We don't need a sending thread to the client anymore, since the driver script will only be uplaoded once.
                //_clientSendingThread = new Thread(ClientSendingMethod);
                //_clientSendingThread.IsBackground = true;
                //_clientSendingThread.Start();

                _clientReceivingThread = new Thread(ClientReceivingMethod);
                _clientReceivingThread.IsBackground = true;
                _clientReceivingThread.Start();

                if (!Machina.Net.GetLocalIPAddressInNetwork(_robotIP, "255.255.255.0", out _serverIP))
                {
                    throw new Exception("ERROR: Could not figure out local IP");
                }
                logger.Debug("Machina local IP: " + _serverIP);

                _serverSocket = new TcpListener(IPAddress.Parse(_serverIP), _serverPort);
                _serverSocket.Start();

                _isServerListeningRunning = true;
                _serverListeningThread = new Thread(ServerReceivingMethod);
                _serverListeningThread.IsBackground = true;
                _serverListeningThread.Start();

                //LoadStreamProgramParts();
                LoadDriverScript();
                UploadScriptToDevice(_driverScript, false);

                return _clientSocket.Connected;
            }
            catch (Exception ex)
            {
                logger.Error("Something went wrong trying to connect to robot...");
                logger.Debug(ex);
                throw new Exception();
            }

            //return false;
        }


        internal bool ConfigureBuffer(int minActions, int maxActions)
        {
            this._maxStreamCount = maxActions;
            this._sendNewBatchOn = minActions;
            return true;
        }


        //private void ClientSendingMethod(object obj)
        //{
        //    while (ClientSocketStatus != TCPConnectionStatus.Disconnected)
        //    {

        //        Thread.Sleep(30);

        //    }
        //}

        /// <summary>
        /// This method reads the buffer coming from the robot socket server and parses it into state info.
        /// </summary>
        /// <param name="obj"></param>
        private void ClientReceivingMethod(object obj)
        {
            //// @TODO: Parse the 30002 buffer to get information about the robot state

            // Expire the thread on disconnection
            while (ClientSocketStatus != TCPConnectionStatus.Disconnected)
            {
                //if (_clientSocket.Available > 0)
                //{
                //    _receiveByteCount = _clientSocket.GetStream().Read(_receiveMsgBytes, 0, _receiveMsgBytes.Length);
                //    _response = Encoding.UTF8.GetString(_receiveMsgBytes, 0, _receiveByteCount);

                //    var msgs = _response.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                //    foreach (var msg in msgs)
                //    {
                //        Console.WriteLine($"  RES: Server response was {msg};");
                //        if (ParseResponse(msg))
                //            _receivedMessages++;
                //    }
                //}

                Thread.Sleep(30);
            }
        }

        /// <summary>
        /// This method sends buffered instructions to the client socket on the robot whenever necessary
        /// </summary>
        /// <param name="obj"></param>
        private void ServerSendingMethod(object obj)
        {
            // Expire thread if no socket
            while (_robotSocket != null)
            {
                while (this.ShouldSend() && this._writeCursor.AreActionsPending())
                {
                    _sendMsgBytes = this._translator.GetBytesForNextAction(this._writeCursor);

                    // If the action had instruction represenation
                    if (_sendMsgBytes != null)
                    {
                        _robotNetworkStream.Write(_sendMsgBytes, 0, _sendMsgBytes.Length);
                        _sentIDs.Add(this._writeCursor.GetLastAction().id);
                        _sentMessages++;

                        logger.Debug("Sending:");
                        logger.Debug("  " + this._writeCursor.GetLastAction());
                        logger.Debug("  [" + string.Join(",", (Util.ByteArrayToInt32Array(_sendMsgBytes))) + "]");
                    }
                }

                RaiseBufferEmptyEventCheck();

                Thread.Sleep(30);
            }
        }

        /// <summary>
        /// This method listens to int messages from the client socket on the robots, and parses them as ids
        /// of completed actions.
        /// </summary>
        /// <param name="obj"></param>
        private void ServerReceivingMethod(object obj)
        {
            // Do not kill threads by aborting them... https://stackoverflow.com/questions/1559255/whats-wrong-with-using-thread-abort/1560567#1560567
            while (_isServerListeningRunning)
            {
                logger.Verbose("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                _robotSocket = _serverSocket.AcceptTcpClient();
                logger.Verbose("Connected client: " + _robotIP);

                _serverSendingThread = new Thread(ServerSendingMethod);
                _serverSendingThread.IsBackground = true;
                _serverSendingThread.Start();

                _robotNetworkStream = _robotSocket.GetStream();

                // Loop to receive all the data sent by the client.
                int i;
                try
                {
                    while ((i = _robotNetworkStream.Read(_serverListeningBytes, 0, _serverListeningBytes.Length)) != 0)
                    {
                        _serverListeningInts = Util.ByteArrayToInt32Array(_serverListeningBytes, i, false);

                        logger.Debug("Received (id): [" + string.Join(",", _serverListeningInts) + "]");
                        foreach(var id in _serverListeningInts)
                        {
                            if (ProcessResponse(id))
                            {
                                _receivedMessages++;
                                logger.Debug("  REMAINING:" + CalculateRemaining());
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Something went wrong with the client... ");
                    logger.Error(e);
                }

                //Console.WriteLine("Closing client");
                _robotSocket.Close();
                _robotSocket = null;

                Thread.Sleep(30);
            }
        }

        private bool ShouldSend()
        {
            int remaining = CalculateRemaining();

            if (_isDeviceBufferFull)
            {
                if (remaining <= _sendNewBatchOn)
                {
                    _isDeviceBufferFull = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (remaining < _maxStreamCount)
                {
                    return true;
                }
                else
                {
                    _isDeviceBufferFull = true;
                    return false;
                }
            }
        }

        private void RaiseBufferEmptyEventCheck()
        {
            if (this._writeCursor.AreActionsPending())
            {
                _bufferEmptyEventIsRaiseable = true;
            }
            else if (_bufferEmptyEventIsRaiseable)
            {
                this._parentDriver.parentControl.RaiseBufferEmptyEvent();
                _bufferEmptyEventIsRaiseable = false;
            }
        }



        ///// <summary>
        ///// Parse the response and decide what to do with it. Returns true if the message was understood.
        ///// </summary>
        ///// <param name="res"></param>
        //private bool ProcessResponse(string res)
        //{
        //    // If first char is an id marker (otherwise, we can't know which action it is)
        //    // @TODO: this is hardcoded for ABB, do this programmatically...
        //    if (res[0] == URCommunicationProtocol.STR_MESSAGE_ID_CHAR)
        //    {
        //        // @TODO: dd some sanity here for incorrectly formatted messages
        //        _responseChunks = res.Split(' ');
        //        string idStr = _responseChunks[0].Substring(1);
        //        int id = Convert.ToInt32(idStr);
        //        _receivedIDs.Add(id);
        //        this._motionCursor.ApplyActionsUntilId(id);
        //        //Console.WriteLine(_motionCursor);
        //        this._parentDriver.parentControl.parentRobot.OnMotionCursorUpdated(EventArgs.Empty);

        //        Action lastAction = this._motionCursor.lastAction;
        //        int remaining = this._motionCursor.ActionsPending();
        //        ActionCompletedArgs e = new ActionCompletedArgs(lastAction, remaining);
        //        this._parentDriver.parentControl.parentRobot.OnActionCompleted(e);

        //        return true;
        //    }

        //    return false;
        //}

        private bool ProcessResponse(int id)
        {
            // Some messages actually contain several instructions (like a pop call may). 
            // In this case, ids are -1 except for the last instruction, that contains the right id.
            // If an id is below 1, just ignore it. 
            if (id < 1)
                return false;
            
            _receivedIDs.Add(id);
            this._motionCursor.ApplyActionsUntilId(id);

            // Raise appropriate events
            this._parentDriver.parentControl.RaiseMotionCursorUpdatedEvent();
            this._parentDriver.parentControl.RaiseActionCompletedEvent();

            return true;
        }

        private bool LoadEmptyScript()
        {
            _driverScript = Machina.IO.ReadTextResource("Machina.Resources.Modules.empty.script");
            return true;
        }

        private bool LoadDriverScript()
        {
            _driverScript = Machina.IO.ReadTextResource("Machina.Resources.Modules.machina_ur_driver.script");

            // @TODO: remove comments, trailing spaces and empty lines from script
            _driverScript = _driverScript.Replace("{{HOSTNAME}}", _serverIP);
            _driverScript = _driverScript.Replace("{{PORT}}", _serverPort.ToString());

            return true;
        }


        private bool UploadScriptToDevice(string script, bool consoleDump = false)
        {
            logger.Verbose("Uploading Machina UR Driver to device...");
            if (consoleDump) logger.Debug(script);

            _sendMsgBytes = Encoding.ASCII.GetBytes(script);
            _clientNetworkStream.Write(_sendMsgBytes, 0, _sendMsgBytes.Length);

            return true;
        }


        private int CalculateRemaining()
        {
            int slen = _sentIDs.Count;
            int rlen = _receivedIDs.Count;
            if (rlen == 0) return slen;

            int lastReceivedID = _receivedIDs.Last();
            int remaining = 0;
            for (int i = slen - 1; i >= 0; i--)
            {
                if (_sentIDs[i] == lastReceivedID)
                {
                    remaining = slen - 1 - i;
                    break;
                }
            }
            return remaining;
        }

        private void DebugLists()
        {
            logger.Debug("SENT IDS: ");
            string ids = "";
            foreach (var id in _sentIDs) ids += id + ", ";
            logger.Debug(ids);
            logger.Debug("");

            logger.Debug("RCVD IDS: ");
            ids = "";
            foreach (var id in _receivedIDs) ids += id + ", ";
            logger.Debug(ids);
            logger.Debug("");
        }

    }
}
