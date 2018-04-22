using Machina.Drivers.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


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
        private TcpClient clientSocket = new TcpClient();
        private NetworkStream clientNetworkStream;
        private Thread clientReceivingThread;
        private Thread clientSendingThread;
        private string _robotIP = "";
        public string IP => _robotIP;
        private int _robotPort = 30003;
        public int Port => _robotPort;
        private bool _isDeviceBufferFull = false;

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
        private int _maxStreamCount = 10;
        private int _sendNewBatchOn = 1;

        private bool _bufferEmptyEventIsRaiseable = true;


        internal TCPCommunicationManagerUR(Driver driver, RobotCursor writeCursor, RobotCursor motionCursor, string ip, int port)
        {
            this._parentDriver = driver;
            this._writeCursor = writeCursor;
            this._motionCursor = motionCursor;
            this._robotIP = ip;
            //this._robotPort = port;  // It will always be 30002, user need not care about this

            this._translator = Protocols.Factory.GetTranslator(this._parentDriver);
        }

        internal bool Disconnect()
        {
            if (clientSocket != null)
            {
                ClientSocketStatus = TCPConnectionStatus.Disconnected;
                clientSocket.Client.Disconnect(false);
                clientSocket.Close();
                if (clientNetworkStream != null) clientNetworkStream.Dispose();
                return true;
            }

            return false;
        }

        internal bool Connect()
        {
            try
            {
                clientSocket = new TcpClient();
                clientSocket.Connect(this._robotIP, this._robotPort);
                ClientSocketStatus = TCPConnectionStatus.Connected;
                clientNetworkStream = clientSocket.GetStream();
                clientSocket.ReceiveBufferSize = 1024;
                clientSocket.SendBufferSize = 1024;

                clientSendingThread = new Thread(ClientSendingMethod);
                clientSendingThread.IsBackground = true;
                clientSendingThread.Start();

                clientReceivingThread = new Thread(ReceivingMethod);
                clientReceivingThread.IsBackground = true;
                clientReceivingThread.Start();

                return clientSocket.Connected;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new Exception("ERROR: could not establish TCP connection");
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
                    _sb.AppendLine("def machina_program():");
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
                    _sb.AppendLine("end");

                    Console.WriteLine("STREAMING PROGRAM: ");
                    Console.WriteLine(_sb.ToString());

                    _sendMsgBytes = Encoding.ASCII.GetBytes(_sb.ToString());
                    clientNetworkStream.Write(_sendMsgBytes, 0, _sendMsgBytes.Length);
                }


                //sb.AppendLine("  popup(\"PACKAGE\")");
                //sb.AppendLine("end");



                //var msgs = this._translator.GetMessagesForNextAction(this._writeCursor);
                //if (msgs != null)
                //{
                //    Console.WriteLine("Sending mgs: ");
                //    foreach (var msg in msgs)
                //    {
                //        _sendMsgBytes = Encoding.ASCII.GetBytes(msg + "\n");
                //        clientNetworkStream.Write(_sendMsgBytes, 0, _sendMsgBytes.Length);
                //        _sentMessages++;
                //        Console.WriteLine(msg);
                //    }
                //}



                //var msgs = this._translator.GetMessagesForNextAction(this._writeCursor);  // just to make the cursor move on...

                //StringBuilder sb = new StringBuilder();
                //sb.AppendLine("def machina_program():");
                //sb.AppendLine("  popup(\"PACKAGE\")");
                //sb.AppendLine("end");

                //Console.WriteLine("Uploading program: ");
                //Console.WriteLine(sb.ToString());

                //_sendMsgBytes = Encoding.ASCII.GetBytes(sb.ToString());
                //clientNetworkStream.Write(_sendMsgBytes, 0, _sendMsgBytes.Length);
                //_sentMessages++;


                RaiseBufferEmptyEventCheck();

                Thread.Sleep(30);
            }
        }

        private void ReceivingMethod(object obj)
        {
            // Expire the thread on disconnection
            while (ClientSocketStatus != TCPConnectionStatus.Disconnected)
            {
                if (clientSocket.Available > 0)
                {
                    _receiveByteCount = clientSocket.GetStream().Read(_receiveMsgBytes, 0, _receiveMsgBytes.Length);
                    _response = Encoding.UTF8.GetString(_receiveMsgBytes, 0, _receiveByteCount);

                    var msgs = _response.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var msg in msgs)
                    {
                        //Console.WriteLine($"  RES: Server response was {msg};");
                        //ParseResponse(msg);
                        //_receivedMessages++;
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
        /// Parse the response and decide what to do with it.
        /// </summary>
        /// <param name="res"></param>
        private void ParseResponse(string res)
        {
            // If first char is an id marker (otherwise, we can't know which action it is)
            // @TODO: this is hardcoded for ABB, do this programmatically...
            if (res[0] == CompilerUR.COMMENT_CHAR)
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
            }
        }

    }
}
