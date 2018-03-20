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
        private TCPCommunicationManager _tcpManager;
        private RobotStudioManager _rsBridge;

        private Dictionary<ConnectionType, bool> _availableConnectionTypes = new Dictionary<ConnectionType, bool>()
        {
            { ConnectionType.User, true },
            { ConnectionType.Machina, true }
        };
        public override Dictionary<ConnectionType, bool> AvailableConnectionTypes { get { return _availableConnectionTypes; } }


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
        public DriverABB(Control ctrl) : base(ctrl)
        {
            if (this.parentControl.connectionMode == ConnectionType.Machina)
            {
                _rsBridge = new RobotStudioManager(this);
            }
        }

        /// <summary>
        /// Start a TCP connection to device via its address on the network.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public override bool ConnectToDevice(string ip, int port)
        {
            _tcpManager = new TCPCommunicationManager(this, this.WriteCursor, this.parentControl.motionCursor, ip, port);  // @TODO: the motionCursor should be part of the driver props?

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
            if (this.parentControl.connectionMode != ConnectionType.Machina)
            {
                throw new Exception("Can only connect to ConnectToDevice(int deviceId) in ConnectionType.Machina mode");
            }

            // 1. use RSmanager to connecto to devide
            //    load the streaming module
            // 2. if successful, use the fetched address to initialize the TCP server
            // 3. release mastership right away to make the program more solid? 

            if (!_rsBridge.Connect(deviceId))
            {
                throw new Exception("Could not connect automatically to device");
            }

            

            // If on 'stream' mode, set up stream connection flow
            if (this.parentControl.ControlMode == ControlType.Stream)
            {
                if (!_rsBridge.SetupStreamingMode())
                {
                     throw new Exception("Could not initialize Streaming Mode in the controller");
                }
            }
            else
            {
                // if on Execute mode on _rsBridge, do nothing (programs will be uploaded in batch)
                throw new NotImplementedException();
            }

            this.IP = _rsBridge.IP;
            this.Port = _rsBridge.WritePort;

            if (!this.ConnectToDevice(this.IP, this.Port))
            {
                throw new Exception("Could not establish TCP connection to the controller");
            }

            DebugDump();

            return true;
        }



        /// <summary>
        /// Forces disconnection from current controller and manages associated logoffs, disposals, etc.
        /// </summary>
        /// <returns></returns>
        public override bool DisconnectFromDevice()
        {
            //Reset();
            //_rsBridge.Disconnect();

            bool success = true;

            if (_tcpManager != null)
            {
                success &= _tcpManager.Disconnect();
            }

            if (_rsBridge != null)
            {
                success &= _rsBridge.Disconnect();
            }

            return success;
        }

        public override bool Dispose()
        {
            return DisconnectFromDevice();
        }

        public override Joints GetCurrentJoints()
        {
            if (_rsBridge != null && _rsBridge.Connected)
            {
                var jnt = _rsBridge.GetCurrentJoints();
                Console.WriteLine($"CurrentJoints: {jnt}");
                return jnt;
            }

            // @TODO: if on TCP without bridge, make a sync request to the server and fetch state from it!
            return null;
        }

        public override Rotation GetCurrentOrientation()
        {
            if (_rsBridge != null && _rsBridge.Connected)
            {
                var ori = _rsBridge.GetCurrentOrientation();
                Console.WriteLine($"GetCurrentOrientation: {ori}");
                return ori;
            }

            // @TODO: if on TCP without bridge, make a sync request to the server and fetch state from it!
            return null;
        }

        public override Vector GetCurrentPosition()
        {
            if (_rsBridge != null && _rsBridge.Connected)
            {
                var pos = _rsBridge.GetCurrentPosition();
                Console.WriteLine($"GetCurrentPosition: {pos}");
                return pos;
            }

            // @TODO: if on TCP without bridge, make a sync request to the server and fetch state from it!
            return null;
        }

        public override void DebugDump()
        {
            
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

        public override bool SetRunMode(CycleType mode)
        {
            throw new NotImplementedException();
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
