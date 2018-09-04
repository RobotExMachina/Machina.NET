using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Machina.Drivers.Communication.Protocols;

namespace Machina.Drivers.Communication
{
    /// <summary>
    /// A class that manages TCP communication with ABB devices, including sending/receiving messages, 
    /// queuing them, releasing them to the TCP server when appropriate, and raising events on 
    /// buffer empty.
    /// </summary>
    internal class TCPCommunicationManagerABB
    {
        

        private RobotCursor _releaseCursor;
        private RobotCursor _executionCursor;
        private RobotCursor _motionCursor;

        private Driver _parentDriver;
        internal RobotLogger logger;

        private const int INIT_TIMEOUT = 5000;  // in millis
        internal Vector initPos;
        internal Rotation initRot;
        internal Joints initAx;
        internal ExternalAxes initExtAx;

        // Properties for Driver module
        private TcpClient _clientSocket;
        private NetworkStream _clientNetworkStream;
        private TCPConnectionStatus _clientStatus;
        private Thread _receivingThread;
        private Thread _sendingThread;
        private string _ip = "";
        public string IP => _ip;
        private int _port = 0;
        public int Port => _port;
        private bool _isDeviceBufferFull = false;

        private Protocols.Base _translator;
        private List<string> _messageBuffer = new List<string>();
        private byte[] _sendMsgBytes;
        private byte[] _receiveMsgBytes = new byte[1024];
        private int _receiveByteCount;
        private string _response;
        private string[] _responseChunks;

        private int _sentMessages = 0;
        private int _receivedMessages = 0;
        private int _maxStreamCount = 10;
        private int _sendNewBatchOn = 2;

        private string _deviceDriverVersion = null;

        // Props for Monitor module (if available)
        private TcpClient _monitorClientSocket;
        private TCPConnectionStatus _monitorStatus;
        private Thread _monitoringThread;
        private string _monitorIP;
        private int _monitorPort;
        private int _monitorReceiveByteCount;
        private string _monitorMessage;
        private byte[] _monitorReceiveMsgBytes = new byte[1024];
        private int _monitorReceivedMessages = 0;

        private bool _isMonitored = false;
        public bool IsMonitored => _isMonitored;




        internal TCPCommunicationManagerABB(Driver driver, RobotCursor releaseCursor, RobotCursor executionCursor, string ip, int port)
        {
            this._parentDriver = driver;
            this.logger = driver.parentControl.logger;
            this._releaseCursor = releaseCursor;
            this._executionCursor = executionCursor;
            this._motionCursor = driver.parentControl.MotionCursor;   // @TODO: homogeinize how the driver picks cursors from parent control: as constructor arguments or directly from the object 
            this._ip = ip;
            this._port = port;
            this._monitorIP = ip;               
            this._monitorPort = port + 1;  // these should be configurable...   

            this._translator = Protocols.Factory.GetTranslator(this._parentDriver);
        }

        internal bool Disconnect()
        {
            DisconnectMonitor();

            if (_clientSocket != null)
            {
                _clientStatus = TCPConnectionStatus.Disconnected;
                _clientSocket.Client.Disconnect(false);
                _clientSocket.Close();
                if (_clientNetworkStream != null) _clientNetworkStream.Dispose();
                return true;
            }
            
            return false;
        }

        internal bool Connect()
        {
            try
            {
                _clientSocket = new TcpClient();
                _clientSocket.Connect(this._ip, this._port);
                _clientStatus = TCPConnectionStatus.Connected;
                _clientNetworkStream = _clientSocket.GetStream();
                _clientSocket.ReceiveBufferSize = 1024;
                _clientSocket.SendBufferSize = 1024;

                _sendingThread = new Thread(SendingMethod);
                _sendingThread.IsBackground = true;
                _sendingThread.Start();

                _receivingThread = new Thread(ReceivingMethod);
                _receivingThread.IsBackground = true;
                _receivingThread.Start();

                if (!WaitForInitialization())
                {
                    logger.Error("Timeout when waiting for initialization data from the controller");
                    return false;
                }

                if (TryConnectMonitor())
                {
                    // Establish a MotionCursor on `Control`
                    this._parentDriver.parentControl.InitializeMotionCursor();
                    this._motionCursor = this._parentDriver.parentControl.MotionCursor;
                }

                return _clientSocket.Connected;
            }
            catch (Exception ex)
            {
                logger.Debug(ex);
                throw new Exception("ERROR: could not establish TCP connection");
            }

            //return false;
        }

        private bool TryConnectMonitor()
        {
            try
            {
                _monitorClientSocket = new TcpClient();
                _monitorClientSocket.Connect(this._monitorIP, this._monitorPort);
                _monitorStatus = TCPConnectionStatus.Connected;
                _monitorClientSocket.ReceiveBufferSize = 1024;
                _monitorClientSocket.SendBufferSize = 1024;

                _monitoringThread = new Thread(MonitoringMethod);
                _monitoringThread.IsBackground = true;
                _monitoringThread.Start();

                _isMonitored = true;

                return _monitorClientSocket.Connected;
            }
            catch (Exception ex)
            {
                logger.Info("Real-time monitoring not available on this device");
                DisconnectMonitor();
            }

            return false;
        }


        private bool DisconnectMonitor()
        {
            if (_monitorClientSocket != null)
            {
                try
                {
                    if (_monitorClientSocket.Connected)
                    {
                        _monitorClientSocket.Client.Disconnect(false);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Something went wrong trying to disconnect from monitor");
                    logger.Error(ex.ToString());
                }

                _monitorClientSocket.Close();
                _monitorStatus = TCPConnectionStatus.Disconnected;
                _isMonitored = false;

                return true;
            }

            return false;
        }




        private bool WaitForInitialization()
        {
            int time = 0;
            logger.Debug("Waiting for intialization data from controller...");

            // @TODO: this is awful, come on...
            while ((_deviceDriverVersion == null || initAx == null || initPos == null || initRot == null || initExtAx == null) && time < INIT_TIMEOUT)
            {
                time += 33;
                Thread.Sleep(33);
            }

            return _deviceDriverVersion != null || initAx != null && initPos != null && initRot != null && initExtAx == null;
        }


        internal bool ConfigureBuffer(int minActions, int maxActions)
        {
            this._maxStreamCount = maxActions;
            this._sendNewBatchOn = minActions;
            return true;
        }


        private void SendingMethod(object obj)
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "MachinaTCPDriverSendingThread";
            }

            // Expire the thread on disconnection
            while (_clientStatus != TCPConnectionStatus.Disconnected)
            {
                while (this.ShouldSend() && this._releaseCursor.AreActionsPending())
                {
                    var msgs = this._translator.GetMessagesForNextAction(this._releaseCursor);
                    if (msgs != null)
                    {
                        foreach (var msg in msgs)
                        {
                            _sendMsgBytes = Encoding.ASCII.GetBytes(msg);
                            _clientNetworkStream.Write(_sendMsgBytes, 0, _sendMsgBytes.Length);
                            _sentMessages++;
                            //Console.WriteLine("Sending mgs: " + msg);
                            logger.Debug($"Sent: {msg}");
                        }
                    }

                    // Action was released to the device, raise event
                    this._parentDriver.parentControl.RaiseActionReleasedEvent();
                }

                //RaiseBufferEmptyEventCheck();

                Thread.Sleep(30);
            }
        }

        private void ReceivingMethod(object obj)
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "MachinaTCPDriverListeningThread";
            }

            // Scope leftover chunks from response messages on this thread
            string leftOverChunk = "";

            // Expire the thread on disconnection
            while (_clientStatus != TCPConnectionStatus.Disconnected)
            {
                if (_clientSocket.Available > 0)
                {
                    _receiveByteCount = _clientSocket.GetStream().Read(_receiveMsgBytes, 0, _receiveMsgBytes.Length);
                    _response = Encoding.UTF8.GetString(_receiveMsgBytes, 0, _receiveByteCount);

                    var msgs = SplitResponse(_response, ref leftOverChunk);
                    foreach (var msg in msgs)
                    {
                        //Console.WriteLine($"  RES: Server response was {msg};");
                        logger.Debug("Received message from driver: " + msg);
                        ParseResponse(msg);
                        _receivedMessages++;
                    }
                }

                Thread.Sleep(30);
            }
        }


        private void MonitoringMethod(object obj)
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "MachinaTCPMonitorListeningThread";
            }

            // Scope leftover chunks from response messages on this thread
            string leftOverChunk = "";

            while (_monitorStatus != TCPConnectionStatus.Disconnected)
            {
                if (_monitorClientSocket.Available > 0)
                {
                    _monitorReceiveByteCount = _monitorClientSocket.GetStream().Read(_monitorReceiveMsgBytes, 0, _monitorReceiveMsgBytes.Length);
                    _monitorMessage = Encoding.UTF8.GetString(_monitorReceiveMsgBytes, 0, _monitorReceiveByteCount);

                    var msgs = SplitResponse(_monitorMessage, ref leftOverChunk);
                    foreach(var msg in msgs)
                    {
                        //logger.Debug("Received message from monitor: " + msg);
                        ParseResponse(msg);
                        _monitorReceivedMessages++;
                        // @TODO: do something with the message: update MotionCursor --> Inserted in `DataReceived()`, not great place, make this more programmatic.
                    }
                }

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

        //private void RaiseBufferEmptyEventCheck()
        //{
        //    if (this._writeCursor.AreActionsPending())
        //    {
        //        _bufferEmptyEventIsRaiseable = true;
        //    }
        //    else if (_bufferEmptyEventIsRaiseable)
        //    {
        //        this._parentDriver.parentControl.RaiseBufferEmptyEvent();
        //        _bufferEmptyEventIsRaiseable = false;
        //    }
        //}


        /// <summary>
        /// Take the response buffer and split it into single messages.
        /// ABB cannot use strings longer that 80 chars, so some messages must be sent in chunks. 
        /// This function parses the response for message continuation and end chars, 
        /// and returns a list of joint messages if appropriate.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private string[] SplitResponse(string response, ref string unfinishedChunk)
        {
            // If there were leftovers from the previous message, attach them to the response
            if (unfinishedChunk.Length != 0)
            {
                response = unfinishedChunk + response;
                unfinishedChunk = "";
            }

            bool isThisResponseComplete = response[response.Length - 1] == ABBCommunicationProtocol.STR_MESSAGE_END_CHAR;

            string[] chunks = response.Split(new char[] { ABBCommunicationProtocol.STR_MESSAGE_END_CHAR }, StringSplitOptions.RemoveEmptyEntries);
            if (chunks.Length == 0)
                // Return empty array (and keep unfinished chunk for next response with body
                return chunks;

            // Store last chunk for next response and work with the rest. 
            if (!isThisResponseComplete)
            {
                unfinishedChunk = chunks[chunks.Length - 1];

                string[] copy = new string[chunks.Length - 1];
                Array.Copy(chunks, copy, chunks.Length - 1);

                chunks = copy;
            }
            
            // Join '>' chunks with whitespaces
            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i] = chunks[i].Replace(ABBCommunicationProtocol.STR_MESSAGE_CONTINUE_CHAR, ' ');
            }

            return chunks;
        }
        

        /// <summary>
        /// Parse the response and decide what to do with it.
        /// </summary>
        /// <param name="res"></param>
        private void ParseResponse(string res)
        {
            // If first char is an id marker (otherwise, we can't know which action it is)
            // @TODO: this is hardcoded for ABB, do this programmatically...
            if (res[0] == ABBCommunicationProtocol.STR_MESSAGE_ID_CHAR)
            {
                AcknowledgmentReceived(res);
            }
            else if (res[0] == ABBCommunicationProtocol.STR_MESSAGE_RESPONSE_CHAR)
            {
                //Console.WriteLine("RECEIVED: " + res);
                DataReceived(res);
            }
        }

        private void AcknowledgmentReceived(string res)
        {
            // @TODO: Add some sanity here for incorrectly formatted messages
            _responseChunks = res.Split(' ');
            string idStr = _responseChunks[0].Substring(1);
            int id = Convert.ToInt32(idStr);
            this._executionCursor.ApplyActionsUntilId(id);

            // Raise appropriate events
            this._parentDriver.parentControl.RaiseActionExecutedEvent();
            //this._parentDriver.parentControl.RaiseMotionCursorUpdatedEvent();
            //this._parentDriver.parentControl.RaiseActionCompletedEvent();
        }

        private void DataReceived(string res)
        {
            _responseChunks = res.Split(' ');
            int resType = Convert.ToInt32(_responseChunks[0].Substring(1));

            double[] data = new double[_responseChunks.Length - 1];
            for (int i = 0; i < data.Length; i++)
            {
                // @TODO: add sanity like Double.TryParse(...)
                data[i] = Double.Parse(_responseChunks[i + 1]);
            }

            switch (resType)
            {
                // ">20 1 2 1;" Sends version numbers
                case ABBCommunicationProtocol.RES_VERSION:
                    this._deviceDriverVersion = Convert.ToInt32(data[0]) + "." + Convert.ToInt32(data[1]) + "." + Convert.ToInt32(data[2]);
                    int comp = Util.CompareVersions(ABBCommunicationProtocol.MACHINA_SERVER_VERSION, _deviceDriverVersion);
                    if (comp > -1)
                    {
                        logger.Verbose($"Using ABB Driver version {ABBCommunicationProtocol.MACHINA_SERVER_VERSION}, found {_deviceDriverVersion}.");
                    }
                    else
                    {
                        logger.Warning($"Found driver version {_deviceDriverVersion}, expected at least {ABBCommunicationProtocol.MACHINA_SERVER_VERSION}. Please update driver module or unexpected behavior may arise.");
                    }
                    break;

                // ">21 400 300 500 0 0 1 0;"
                case ABBCommunicationProtocol.RES_POSE:
                    this.initPos = new Vector(data[0], data[1], data[2]);
                    this.initRot = new Rotation(new Quaternion(data[3], data[4], data[5], data[6]));
                    break;


                // ">22 0 0 0 0 90 0;"
                case ABBCommunicationProtocol.RES_JOINTS:
                    this.initAx = new Joints(data[0], data[1], data[2], data[3], data[4], data[5]);
                    break;

                case ABBCommunicationProtocol.RES_EXTAX:
                    this.initExtAx = new ExternalAxes(data[0], data[1], data[2], data[3], data[4], data[5]);
                    break;

                case ABBCommunicationProtocol.RES_FULL_POSE:
                    Vector pos = new Vector(data[0], data[1], data[2]);
                    Rotation rot = new Rotation(new Quaternion(data[3], data[4], data[5], data[6]));
                    Joints ax = new Joints(data[7], data[8], data[9], data[10], data[11], data[12]);
                    ExternalAxes extax = new ExternalAxes(data[13], data[14], data[15], data[16], data[17], data[18]);

                    this._motionCursor.UpdateFullPose(pos, rot, ax, extax);
                    this._parentDriver.parentControl.RaiseMotionUpdateEvent();

                    break;
            }

        }

    }
}
