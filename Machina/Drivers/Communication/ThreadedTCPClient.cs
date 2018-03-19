using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Machina.Drivers.Communication
{

    class ThreadedTCPClient
    {
        private TcpClient _socket;
        private string _address;
        private int _port;
        private TCPConnectionStatus _status;
        private Thread _thread;

        public Thread Thread
        {
            get { return _thread; }
            set { _thread = value; }
        }

        public TcpClient ClientSocket
        {
            get { return _socket; }
            set { _socket = value; }
        }

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public TCPConnectionStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }


        internal bool Connect(string address, int port) {
            _address = address;
            _port = port;
            _socket = new TcpClient();
            _socket.Connect(_address, _port);
            _status = TCPConnectionStatus.Connected;
            _socket.ReceiveBufferSize = 1024;
            _socket.SendBufferSize = 1024;
            
            _thread = new Thread(ThreadedLoop);
            _thread.IsBackground = true;
            _thread.Start();

            return _socket.Connected;
        }

        internal bool Disconnect()
        {
            _status = TCPConnectionStatus.Disconnected;
            _socket.Client.Disconnect(false);
            _socket.Close();
            Thread.Sleep(1000);

            return !_socket.Connected;
        }
        
        internal virtual void ThreadedLoop(object obj) { }
    }
}
