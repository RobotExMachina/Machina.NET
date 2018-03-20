using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Drivers.Protocols
{
    internal static class ProtocolFactory
    {
        internal static ProtocolBase GetTranslator(Driver driver)
        {
            switch (driver.masterControl.parentRobot.Brand)
            {
                case RobotType.ABB:
                    return new ABBServerProtocol();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
