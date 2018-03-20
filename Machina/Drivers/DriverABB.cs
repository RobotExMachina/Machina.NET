using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Net.Sockets;

using System.Threading;

using Machina.Drivers.Communication;

namespace Machina.Drivers
{

    //   █████╗ ██████╗ ██████╗ 
    //  ██╔══██╗██╔══██╗██╔══██╗
    //  ███████║██████╔╝██████╔╝
    //  ██╔══██║██╔══██╗██╔══██╗
    //  ██║  ██║██████╔╝██████╔╝
    //  ╚═╝  ╚═╝╚═════╝ ╚═════╝ 
    //                          
    class DriverABB : Driver
    {
        //private RobotStudioManager _rsBridge;

        private TCPCommunicationManager _tcpManager;



        //  ██████╗ ██╗   ██╗██████╗ ██╗     ██╗ ██████╗
        //  ██╔══██╗██║   ██║██╔══██╗██║     ██║██╔════╝
        //  ██████╔╝██║   ██║██████╔╝██║     ██║██║     
        //  ██╔═══╝ ██║   ██║██╔══██╗██║     ██║██║     
        //  ██║     ╚██████╔╝██████╔╝███████╗██║╚██████╗
        //  ╚═╝      ╚═════╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝
        //                                                                             
        /// <summary>
        /// Main constructor
        /// </summary>
        public DriverABB(Control ctrl) : base(ctrl) { }


        public override bool ConnectToDevice(string ip, int port)
        {
            _tcpManager = new TCPCommunicationManager(this, this.WriteCursor, ip, port);
            if (_tcpManager.Connect())
            {
                this.IP = ip;
                this.Port = port;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Performs all necessary actions to establish a connection to a real/virtual device, 
        /// including connecting to the controller, loggin in, checking required states, etc.
        /// </summary>
        /// <param name="deviceId"></param>
        public override bool ConnectToDevice(int deviceId)
        {
            //_isConnected = false;

            //if (this.masterControl.connectionMode == ConnectionManagerType.Machina)
            //{

            //    _rsBridge = new RobotStudioManager(this);
            //    _rsBridge.Connect(deviceId);


            //    //// If here, everything went well and successfully connected 
            //    //isConnected = true;

            //    // If on 'stream' mode, set up stream connection flow
            //    if (masterControl.GetControlMode() == ControlType.Stream)
            //    {
            //        if (!SetupStreamingMode())
            //        {
            //            Console.WriteLine("Could not initialize 'stream' mode in controller");
            //            Reset();
            //            return false;
            //        }
            //    }
            //    else
            //    {
            //        // if on Execute mode on _rsBridge, do nothing (programs will be uploaded in batch)
            //    }


            //}
            //else
            //{
            //    if (!SetupStreamingMode())
            //    {
            //        Console.WriteLine("Could not initialize 'stream' mode in controller");
            //        Reset();
            //        return false;
            //    }
            //}


            //DebugDump();

            //return _isConnected;

            throw new NotImplementedException();
        }



        /// <summary>
        /// Forces disconnection from current controller and manages associated logoffs, disposals, etc.
        /// </summary>
        /// <returns></returns>
        public override bool DisconnectFromDevice()
        {
            //Reset();
            //_rsBridge.Disconnect();

            bool success = _tcpManager.Disconnect();

            return success;
        }

        public override bool Dispose()
        {
            throw new NotImplementedException();
        }

        public override Joints GetCurrentJoints()
        {
            // @TODO: make a sync request to the server and get position from it!
            return null;
        }

        public override Rotation GetCurrentOrientation()
        {
            // @TODO: make a sync request to the server and get position from it!
            return null;
        }

        public override Vector GetCurrentPosition()
        {
            // @TODO: make a sync request to the server and get position from it!
            return null;
        }

        public override void DebugDump()
        {
            throw new NotImplementedException();
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }





        //███████╗████████╗██████╗ ███████╗ █████╗ ███╗   ███╗██╗███╗   ██╗ ██████╗ 
        //██╔════╝╚══██╔══╝██╔══██╗██╔════╝██╔══██╗████╗ ████║██║████╗  ██║██╔════╝ 
        //███████╗   ██║   ██████╔╝█████╗  ███████║██╔████╔██║██║██╔██╗ ██║██║  ███╗
        //╚════██║   ██║   ██╔══██╗██╔══╝  ██╔══██║██║╚██╔╝██║██║██║╚██╗██║██║   ██║
        //███████║   ██║   ██║  ██║███████╗██║  ██║██║ ╚═╝ ██║██║██║ ╚████║╚██████╔╝
        //╚══════╝   ╚═╝   ╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝╚═╝╚═╝  ╚═══╝ ╚═════╝ 

        /// <summary>
        /// Performs necessary operations to set up 'stream' control mode in the controller
        /// </summary>
        /// <returns></returns>
        private bool SetupStreamingMode()
        {
            //// If manager is Machina, try to rely on the RobotStudio bridge to set up the streaming module
            //if (this.masterControl.connectionMode == ConnectionManagerType.Machina)
            //{
            //    if (!_rsBridge.LoadStreamingModule())
            //    {
            //        Console.WriteLine("Could not load streaming module");
            //        return false;
            //    }

            //    if (!_rsBridge.ResetProgramPointer())
            //    {
            //        Console.WriteLine("Could not reset the program pointer");
            //        return false;
            //    }

            //    if (!_rsBridge.StartProgramExecution())
            //    {
            //        Console.WriteLine("Could not load start the streaming module");
            //        return false;
            //    }

            //}
            

            // Hurray!
            return true;
        }











        ///// <summary>
        ///// This function will look at the state of the program pointer, the streamQueue, 
        ///// and if necessary will add a new target to the stream. This is meant to be called
        ///// to initiate the stream update chain, like when adding a new target, or pnum event handling.
        ///// </summary>
        //public override void TickStreamQueue(bool hasPriority)
        //{
        //    //Console.WriteLine($"TICKING StreamQueue: {WriteCursor.ActionsPending()} actions pending");
        //    //if (WriteCursor.AreActionsPending())
        //    //{
        //    //    Console.WriteLine("About to set targets");
        //    //    //SetNextVirtualTarget(hasPriority);
        //    //    //SendActionAsMessage(hasPriority);  // this is now watched by the thread
        //    //    //TickStreamQueue(hasPriority);  // call this in case there are more in the queue...
        //    //}
        //    //else
        //    //{
        //    //    Console.WriteLine($"Not setting targets, actions pending {WriteCursor.ActionsPending()}");
        //    //}
        //}




        

    }
}
