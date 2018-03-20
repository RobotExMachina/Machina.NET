using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Drivers
{
    /// <summary>
    /// A dummy Driver object to use for offline control
    /// </summary>
    class DriverOffline : Driver
    {
        private Dictionary<ConnectionType, bool> _availableConnectionTypes = new Dictionary<ConnectionType, bool>()
        {
            { ConnectionType.User, true },
            { ConnectionType.Machina, true }
        };
        public override Dictionary<ConnectionType, bool> AvailableConnectionTypes { get { return _availableConnectionTypes; } }


        public DriverOffline(Control ctrl) : base(ctrl) { }

        public override bool ConnectToDevice(int deviceId)
        {
            Console.WriteLine("Cannot connect to a device in Offline mode");
            return false;
        }

        public override bool ConnectToDevice(string ip, int port)
        {
            Console.WriteLine("Cannot connect to a device in Offline mode");
            return false;
        }

        public override void DebugDump()
        {
            Console.WriteLine("Nothing to debug for a Driver in Offline mode");
        }

        public override bool DisconnectFromDevice()
        {
            Console.WriteLine("Cannot disconnect to a device in Offline mode");
            return false;
        }

        public override Joints GetCurrentJoints()
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

        public override void Reset()
        {
            Console.WriteLine("Cannot reset driver in offline mode");
        }

        public override bool Dispose()
        {
            return true;
        }

        public override bool SetRunMode(CycleType mode)
        {
            throw new NotImplementedException();
        }
    }

}
