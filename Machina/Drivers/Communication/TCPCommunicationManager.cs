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
        private Driver _driver;

        private TcpClient clientSocket = new TcpClient();
        private NetworkStream clientNetworkStream;
        private Thread receivingThread;
        private Thread sendingThread;
        private string _ip = "";
        private int _port = 0;
        private bool _isDeviceBufferFull = false;

        private Protocols.ProtocolBase _translator;
        private List<string> _messageBuffer = new List<string>();
        private byte[] _msgBytes;

        private int _sentMessages = 0;
        private int _receivedMessages = 0;
        private int _maxStreamCount = 10;
        private int _sendNewBatchOn = 2;


        internal TCPCommunicationManager(Driver driver, RobotCursor writeCursor, string ip, int port)
        {
            this._driver = driver;
            this._writeCursor = writeCursor;
            this._ip = ip;
            this._port = port;

            this._translator = ProtocolFactory.GetTranslator(this._driver);
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
                            _msgBytes = Encoding.ASCII.GetBytes(msg);
                            clientNetworkStream.Write(_msgBytes, 0, _msgBytes.Length);
                            _sentMessages++;
                            Console.WriteLine("Sending mgs: " + msg);
                        }
                    }
                }

                Thread.Sleep(30);
            }
        }

        //private boolpublic SendActionAsMessage(bool hasPriority)
        //{
        //    if (!WriteCursor.ApplyNextAction())
        //    {
        //        Console.WriteLine("Could not apply next action");
        //        return false;
        //    }

        //    Action a = WriteCursor.GetLastAction();

        //    if (a == null)
        //        throw new Exception("Last action wasn't correctly stored...?");

        //    string msg = GetActionMessage(a, WriteCursor);
        //    if (msg == "")
        //    {
        //        Console.WriteLine("Applied action to cursor, but not necessary to stream to robot");
        //        return false;
        //    }

        //    byte[] msgBytes = Encoding.ASCII.GetBytes(msg);

        //    clientNetworkStream.Write(msgBytes, 0, msgBytes.Length);

        //    Console.WriteLine("Sending mgs: " + msg);

        //    return true;
        //}


        private void ReceivingMethod(object obj)
        {
            // Expire the thread on disconnection
            while (Status != TCPConnectionStatus.Disconnected)
            {
                if (clientSocket.Available > 0)
                {
                    var buffer = new byte[1024];
                    var byteCount = clientSocket.GetStream().Read(buffer, 0, buffer.Length);
                    var response = Encoding.UTF8.GetString(buffer, 0, byteCount);

                    var msgs = response.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var res in msgs)
                    {
                        Console.WriteLine($"  RES: Server response was {res};");
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


    }
}
