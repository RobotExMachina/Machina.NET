using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Drivers.Communication.Protocols
{
    internal static class Factory
    {
        internal static Base GetTranslator(Driver driver)
        {
            switch (driver.parentControl.parentRobot.Brand)
            {
                case RobotType.ABB:
                    return new ABBCommunicationProtocol();

                case RobotType.UR:
                    return new URCommunicationProtocol();

                case RobotType.KUKA:
                    return new KUKACommunicationProtocol();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
