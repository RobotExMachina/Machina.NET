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
        private bool _isServerListeningRunning = false;
        public string ServerIP => _serverIP;
        private string _serverIP;
        public int ServerPort => _serverPort;
        private int _serverPort = 7003;

        private string _streamProgramHeader;
        private string _streamProgramFooter;

        private Protocols.Base _translator;
        private StringBuilder _sb = new StringBuilder();
        private List<string> _msgs;
        private List<string> _messageBuffer = new List<string>();
        private byte[] _sendMsgBytes;
        private byte[] _receiveMsgBytes = new byte[2048];
        private int _receiveByteCount;
        private string _response;
        private string[] _responseChunks;

        private int _sentMessages = 0;
        private int _receivedMessages = 0;
        private int _maxStreamCount = 5;
        private int _sendNewBatchOn = 3;

        private bool _bufferEmptyEventIsRaiseable = true;


        internal TCPCommunicationManagerUR(Driver driver, RobotCursor writeCursor, RobotCursor motionCursor, string robotIP, int robotPort)
        {
            this._parentDriver = driver;
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

                _clientSendingThread = new Thread(ClientSendingMethod);
                _clientSendingThread.IsBackground = true;
                _clientSendingThread.Start();

                _clientReceivingThread = new Thread(ClientReceivingMethod);
                _clientReceivingThread.IsBackground = true;
                _clientReceivingThread.Start();

                if (!Machina.Net.GetLocalIPAddressInNetwork(_robotIP, "255.255.255.0", out _serverIP))
                {
                    throw new Exception("ERROR: Could not figure out local IP");
                }
                Console.WriteLine("Machina local IP: " + _serverIP);
                _serverSocket = new TcpListener(IPAddress.Parse(_serverIP), _serverPort);
                _serverSocket.Start();

                _isServerListeningRunning = true;
                _serverListeningThread = new Thread(ServerReceivingMethod);
                _serverListeningThread.IsBackground = true;
                _serverListeningThread.Start();

                LoadStreamProgramParts();

                return _clientSocket.Connected;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: something went wrong trying to connect to robot...");
                Console.WriteLine(ex);
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


        private void ClientSendingMethod(object obj)
        {
            // Expire the thread on disconnection
            while (ClientSocketStatus != TCPConnectionStatus.Disconnected)
            {
                while (this.ShouldSend() && this._writeCursor.AreActionsPending())
                {
                    /** PSEUDO:
                     *      - Start a program
                     *      - Add header
                     *      - Add as many actions as bufferzise
                     *      - Add footer
                     *      - Send
                     *      
                     *      - Extend to add sending acknowledgements to a TCP server
                     */

                    _sb.Clear();
                    _sb.AppendLine(_streamProgramHeader);
                    int sentPrev = _sentMessages;
                    while (this.ShouldSend() && this._writeCursor.AreActionsPending())
                    {
                        _msgs = this._translator.GetMessagesForNextAction(this._writeCursor);
                        if (_msgs != null)
                        {
                            foreach (var msg in _msgs)
                            {
                                _sb.AppendLine(msg);
                            }
                            _sentMessages++;
                        }
                    }
                    _sb.AppendLine(_streamProgramFooter);

                    if (sentPrev < _sentMessages)
                    {
                        Console.WriteLine("STREAMING PROGRAM: ");
                        Console.WriteLine(_sb.ToString());

                        _sendMsgBytes = Encoding.ASCII.GetBytes(_sb.ToString());
                        _clientNetworkStream.Write(_sendMsgBytes, 0, _sendMsgBytes.Length);
                    }
                    else
                    {
                        Console.WriteLine("--> Pending action have no instruction representation");
                    }
                }
                                
                RaiseBufferEmptyEventCheck();

                Thread.Sleep(30);
            }
        }

        private void ClientReceivingMethod(object obj)
        {
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

        private void ServerReceivingMethod(object obj)
        {
            // Do not kill threads by aborting them... https://stackoverflow.com/questions/1559255/whats-wrong-with-using-thread-abort/1560567#1560567
            while (_isServerListeningRunning)
            {
                Console.Write("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                TcpClient client = _serverSocket.AcceptTcpClient();
                Console.WriteLine("Connected client: " + client);

                _serverListeningMsg = null;

                NetworkStream clientStream = client.GetStream();

                // Loop to receive all the data sent by the client.
                int i;
                try
                {
                    while ((i = clientStream.Read(_serverListeningBytes, 0, _serverListeningBytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        _serverListeningMsg = Encoding.ASCII.GetString(_serverListeningBytes, 0, i);
                        //Console.WriteLine("Received: {0}", _serverListeningMsg);

                        var msgs = _serverListeningMsg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var msg in msgs)
                        {
                            Console.WriteLine($"  Server received {msg};");
                            if (ProcessResponse(msg))
                            {
                                _receivedMessages++;
                                Console.WriteLine("  Sent:" + _sentMessages + " received:"+_receivedMessages);
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Something went wrong with the client... ");
                    Console.WriteLine(e);
                }
                
                Console.WriteLine("Closing client");
                client.Close();

                Thread.Sleep(30);
            }
        }

        private bool ShouldSend()
        {
            if (_isDeviceBufferFull)
            {
                if (_sentMessages - _receivedMessages <= _sendNewBatchOn)
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
                if (_sentMessages - _receivedMessages < _maxStreamCount)
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
                //Console.WriteLine("Raising OnBufferEmpty event");
                this._parentDriver.parentControl.parentRobot.OnBufferEmpty(EventArgs.Empty);
                _bufferEmptyEventIsRaiseable = false;
            }
        }



        /// <summary>
        /// Parse the response and decide what to do with it. Returns true if the message was understood.
        /// </summary>
        /// <param name="res"></param>
        private bool ProcessResponse(string res)
        {
            // If first char is an id marker (otherwise, we can't know which action it is)
            // @TODO: this is hardcoded for ABB, do this programmatically...
            if (res[0] == URCommunicationProtocol.STR_MESSAGE_ID_CHAR)
            {
                // @TODO: dd some sanity here for incorrectly formatted messages
                _responseChunks = res.Split(' ');
                string idStr = _responseChunks[0].Substring(1);
                int id = Convert.ToInt32(idStr);
                this._motionCursor.ApplyActionsUntilId(id);
                //Console.WriteLine(_motionCursor);
                this._parentDriver.parentControl.parentRobot.OnMotionCursorUpdated(EventArgs.Empty);

                Action lastAction = this._motionCursor.lastAction;
                int remaining = this._motionCursor.ActionsPending();
                ActionCompletedArgs e = new ActionCompletedArgs(lastAction, remaining);
                this._parentDriver.parentControl.parentRobot.OnActionCompleted(e);

                return true;
            }

            return false;
        }

        private bool LoadStreamProgramParts()
        {
            _streamProgramHeader = Machina.IO.ReadTextResource("Machina.Resources.Modules.Machina_UR_Stream_Program_Header.script");
            _streamProgramFooter = Machina.IO.ReadTextResource("Machina.Resources.Modules.Machina_UR_Stream_Program_Footer.script");

            _streamProgramHeader = _streamProgramHeader.Replace("{{HOST_IP}}", _serverIP);
            _streamProgramHeader = _streamProgramHeader.Replace("{{HOST_PORT}}", _serverPort.ToString());

            return true;
        }


        
    }
}
