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
        private TCPCommunicationManagerABB _tcpManager;
        private RobotStudioManager _rsBridge;

        private Dictionary<ConnectionType, bool> _availableConnectionTypes = new Dictionary<ConnectionType, bool>()
        {
            { ConnectionType.User, true },
            { ConnectionType.Machina, true }
        };
        public override Dictionary<ConnectionType, bool> AvailableConnectionTypes { get { return _availableConnectionTypes; } }

        private RobotLogger logger;


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

            logger = this.parentControl.logger;
        }

        /// <summary>
        /// Start a TCP connection to device via its address on the network.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public override bool ConnectToDevice(string ip, int port)
        {
            _tcpManager = new TCPCommunicationManagerABB(this, this.WriteCursor, this.parentControl.motionCursor, ip, port);  // @TODO: the motionCursor should be part of the driver props?

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
            this.Port = _rsBridge.Port;

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
                logger.Debug($"CurrentJoints: {jnt}");
                return jnt;
            }

            return this._tcpManager.initAx;
        }

        public override Rotation GetCurrentOrientation()
        {
            if (_rsBridge != null && _rsBridge.Connected)
            {
                var ori = _rsBridge.GetCurrentOrientation();
                logger.Debug($"GetCurrentOrientation: {ori}");
                return ori;
            }
            

            return this._tcpManager.initRot;
        }

        public override Vector GetCurrentPosition()
        {
            if (_rsBridge != null && _rsBridge.Connected)
            {
                var pos = _rsBridge.GetCurrentPosition();
                logger.Debug($"GetCurrentPosition: {pos}");
                return pos;
            }

            return this._tcpManager.initPos;  // will be null if not initialized...
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
            throw new NotImplementedException();
        }

        public override bool SetRunMode(CycleType mode)
        {
            throw new NotImplementedException();
        }

        internal override bool ConfigureBuffer(int minActions, int maxActions)
        {
            return this._tcpManager.ConfigureBuffer(minActions, maxActions);
        }

    }
}
