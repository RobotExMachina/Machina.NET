using Machina.Drivers.Protocols;
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
    /// A class that manages TCP communication with devices, including sending/receiving messages, 
    /// queuing them, releasing them to the TCP server when appropriate, and raining events on 
    /// buffer empty.
    /// </summary>
    internal class TCPCommunicationManager
    {
        internal TCPConnectionStatus Status { get; private set; }

        private RobotCursor _writeCursor;
        private RobotCursor _motionCursor;
        private Driver _parentDriver;

        private TcpClient clientSocket = new TcpClient();
        private NetworkStream clientNetworkStream;
        private Thread receivingThread;
        private Thread sendingThread;
        private string _ip = "";
        private int _port = 0;
        private bool _isDeviceBufferFull = false;

        private Protocols.ProtocolBase _translator;
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


        internal TCPCommunicationManager(Driver driver, RobotCursor writeCursor, RobotCursor motionCursor, string ip, int port)
        {
            this._parentDriver = driver;
            this._writeCursor = writeCursor;
            this._motionCursor = motionCursor;
            this._ip = ip;
            this._port = port;

            this._translator = ProtocolFactory.GetTranslator(this._parentDriver);
        }

        internal bool Disconnect()
        {

            if (clientSocket != null)
            {
                Status = TCPConnectionStatus.Disconnected;
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
                clientSocket.Connect(this._ip, this._port);
                Status = TCPConnectionStatus.Connected;
                clientNetworkStream = clientSocket.GetStream();
                clientSocket.ReceiveBufferSize = 1024;
                clientSocket.SendBufferSize = 1024;

                sendingThread = new Thread(SendingMethod);
                sendingThread.IsBackground = true;
                sendingThread.Start();

                receivingThread = new Thread(ReceivingMethod);
                receivingThread.IsBackground = true;
                receivingThread.Start();

                return clientSocket.Connected;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new Exception("ERROR: could not establish TCP connection");
            }

            return false;
        }
        

        private void SendingMethod(object obj)
        {
            // Expire the thread on disconnection
            while (Status != TCPConnectionStatus.Disconnected)
            {
                //while (this.WriteCursor.actionBuffer.AreActionsPending())
                while (this.ShouldSend() && this._writeCursor.AreActionsPending())
                {
                    //if (SendActionAsMessage(true)) _sentMessages++;
                    var msgs = this._translator.GetMessagesForNextAction(this._writeCursor);
                    if (msgs != null)
                    {
                        foreach (var msg in msgs)
                        {
                            _sendMsgBytes = Encoding.ASCII.GetBytes(msg);
                            clientNetworkStream.Write(_sendMsgBytes, 0, _sendMsgBytes.Length);
                            _sentMessages++;
                            Console.WriteLine("Sending mgs: " + msg);
                        }
                    }
                }

                Thread.Sleep(30);
            }
        }

        private void ReceivingMethod(object obj)
        {
            // Expire the thread on disconnection
            while (Status != TCPConnectionStatus.Disconnected)
            {
                if (clientSocket.Available > 0)
                {
                    _receiveByteCount = clientSocket.GetStream().Read(_receiveMsgBytes, 0, _receiveMsgBytes.Length);
                    _response = Encoding.UTF8.GetString(_receiveMsgBytes, 0, _receiveByteCount);

                    var msgs = _response.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var msg in msgs)
                    {
                        Console.WriteLine($"  RES: Server response was {msg};");
                        ParseResponse(msg);
                        _receivedMessages++;
                    }
                }

                Thread.Sleep(30);
            }
        }
       
        private bool ShouldSend()
        {
            if (_isDeviceBufferFull)
            {
                if (_sentMessages - _receivedMessages < _sendNewBatchOn)
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

        /// <summary>
        /// Parse the response and decide what to do with it.
        /// </summary>
        /// <param name="res"></param>
        private void ParseResponse(string res)
        {
            // If first char is an id marker (otherwise, we can't know which action it is)
            // @TODO: this is hardcoded for ABB, do this programmatically...
            if (res[0] == '@')
            {
                // @TODO: dd some sanity here for incorrectly formatted messages
                _responseChunks = res.Split(' ');
                string idStr = _responseChunks[0].Substring(1);
                int id = Convert.ToInt32(idStr);
                this._motionCursor.ApplyActionsUntilId(id);
                //Console.WriteLine(_motionCursor);
            }
        }

    }
}
