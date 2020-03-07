using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machina.Types.Geometry;

namespace Machina.Drivers
{
    /// <summary>
    /// A dummy Driver object to use for offline control
    /// </summary>
    class DriverOffline : Driver
    {
        private RobotLogger logger;

        private Dictionary<ConnectionType, bool> _availableConnectionTypes = new Dictionary<ConnectionType, bool>()
        {
            { ConnectionType.User, true },
            { ConnectionType.Machina, true }
        };
        public override Dictionary<ConnectionType, bool> AvailableConnectionTypes { get { return _availableConnectionTypes; } }


        public DriverOffline(Control ctrl) : base(ctrl) {
            logger = ctrl.logger;
        }

        public override bool ConnectToDevice(int deviceId)
        {
            logger.Info("Cannot connect to a device in Offline mode");
            return false;
        }

        public override bool ConnectToDevice(string ip, int port)
        {
            logger.Info("Cannot connect to a device in Offline mode");
            return false;
        }

        public override void DebugDump()
        {
            logger.Debug("Nothing to debug for a Driver in Offline mode");
        }

        public override bool DisconnectFromDevice()
        {
            logger.Info("Cannot disconnect to a device in Offline mode");
            return false;
        }

        public override Axes GetCurrentJoints()
        {
            return null;
        }

        public override Rotation GetCurrentOrientation()
        {
            return null;
        }

        public override Vector GetCurrentPosition()
        {
            return null;
        }

        public override ExternalAxes GetCurrentExternalAxes()
        {
            return null;
        }

        public override void Reset()
        {
            logger.Info("Cannot reset driver in offline mode");
        }

        public override bool Dispose()
        {
            return true;
        }

        public override bool SetRunMode(CycleType mode)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GetDeviceDriverModules(Dictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }
    }

}
