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
using Machina.Types.Geometry;

namespace Machina.Drivers
{

    // ██╗  ██╗██╗   ██╗██╗  ██╗ █████╗ 
    // ██║ ██╔╝██║   ██║██║ ██╔╝██╔══██╗
    // █████╔╝ ██║   ██║█████╔╝ ███████║
    // ██╔═██╗ ██║   ██║██╔═██╗ ██╔══██║
    // ██║  ██╗╚██████╔╝██║  ██╗██║  ██║
    // ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝
    //                                                   
    class DriverKUKA : Driver
    {
        private TCPCommunicationManagerKUKA _tcpManager;
        //private RobotStudioManager _rsBridge;

        private Dictionary<ConnectionType, bool> _availableConnectionTypes = new Dictionary<ConnectionType, bool>()
        {
            { ConnectionType.User, true },
            { ConnectionType.Machina, false }
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
        public DriverKUKA(Control ctrl) : base(ctrl)
        {
            logger = this.parentControl.logger;
        }

        /// <summary>
        /// Returns the driver modules necessary to run on this device for Machina to talk to it.
        /// Takes a dictionary of values to be replaced on the modules, such as {"IP","192.168.125.1"} or {"PORT","7000"}.
        /// Returns a dict with filename-file pairs. 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override Dictionary<string, string> GetDeviceDriverModules(Dictionary<string, string> parameters)
        {
            if (!parameters.ContainsKey("HOSTNAME"))
            {
                Logger.Error("Cannot retrieve KUKA driver modules, most provide a HOSTNAME value.");
                return null;
            }

            if (!parameters.ContainsKey("PORT"))
            {
                Logger.Error("Cannot retrieve KUKA driver modules, most provide a PORT value.");
                return null;
            }

            int port = 0;
            if (!Int32.TryParse(parameters["PORT"], out port))
            {
                Logger.Error("Invalid PORT value");
                return null;
            }

            // the ip address for kuka robots should be set in the xml server configuration file
            // developed by Arastoo Khajehee https://github.com/Arastookhajehee
            string serverMod = IO.ReadTextResource("Machina.Resources.DriverModules.KUKA.machina_kuka_server.xml");
            string ipAddressString = string.Format("<IP>{0}</IP>", parameters["HOSTNAME"]);
            string portAddressString = string.Format("<PORT>{0}</PORT>", parameters["PORT"]);
            serverMod = serverMod.Replace("<IP>10.10.100.20</IP>", ipAddressString);
            serverMod = serverMod.Replace("<PORT>54600</PORT>", portAddressString);

            string driverMod = IO.ReadTextResource("Machina.Resources.DriverModules.KUKA.machina_kuka_driver.src");

            string dataMod = IO.ReadTextResource("Machina.Resources.DriverModules.KUKA.machina_kuka_data.dat");


            var files = new Dictionary<string, string>()
            {
                {"machina_kuka_driver.src", driverMod},
                {"machina_kuka_data.dat", dataMod},
                {"machina_kuka_server.xml", serverMod},
            };

            return files;
        }

        /// <summary>
        /// Start a TCP connection to device via its address on the network.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public override bool ConnectToDevice(string ip, int port)
        {
            _tcpManager = new TCPCommunicationManagerKUKA(this, this.ReleaseCursor, this.parentControl.ExecutionCursor, ip, port);  // @TODO: the motionCursor should be part of the driver props?

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
            throw new Exception("Can only connect to ConnectToDevice(int deviceId) in ConnectionType.Machina mode");

            return false;
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

            return success;
        }

        public override bool Dispose()
        {
            return DisconnectFromDevice();
        }

        public override Joints GetCurrentJoints()
        {
            // temp using just the init values...
            return this._tcpManager.initAx;
        }

        public override Rotation GetCurrentOrientation()
        {
            // temp using just the init values...
            return this._tcpManager.initRot;
        }

        public override Vector GetCurrentPosition()
        {
            // temp using just the init values...
            return this._tcpManager.initPos;
        }

        public override ExternalAxes GetCurrentExternalAxes()
        {
            // Do URs have the capacity to do external axes...?
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
