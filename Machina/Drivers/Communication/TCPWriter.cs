using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Machina.Drivers.Communication
{
    /// <summary>
    /// This class represents a TCP 
    /// </summary>
    class TCPWriter : ThreadedTCPClient
    {
        ActionBuffer buffer;
        RobotCursor writeCursor;

        List<string> outgoingMessages = new List<string>();

        internal override void ThreadedLoop(object obj)
        {
            while (Status != TCPConnectionStatus.Disconnected)
            {
                while (buffer.AreActionsPending())
                {
                    //string m = MessageQueue[0];

                    //SendMessage(m);

                    //MessageQueue.RemoveAt(0);
                }

                Thread.Sleep(30);
            }
        }

        public void SendMessage(string msg)
        {

        }
    }
}
