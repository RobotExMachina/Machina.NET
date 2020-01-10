using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machina.Drivers;

namespace Machina.Controllers
{
    /// <summary>
    /// A manager for Control objects running ControlType.Stream
    /// </summary>
    internal class StreamControlManager : ControlManager
    {
        public StreamControlManager(Control parent) : base(parent) { }

        public override bool Terminate()
        {
            throw new NotImplementedException();
        }

        internal override void SetCommunicationObject()
        {
            // @TODO: shim assignment of correct robot model/brand
            switch (_control.parentRobot.Brand)
            {
                case RobotType.ABB:
                    _control.Driver = new DriverABB(_control);
                    break;

                case RobotType.UR:
                    _control.Driver = new DriverUR(_control);
                    break;
                    
                default:
                    throw new NotImplementedException();
            }
        }

        internal override void LinkWriteCursor()
        {
            // Pass the streamQueue object as a shared reference
            _control.Driver.LinkWriteCursor(_control.ReleaseCursor);
        }

        internal override void SetStateCursor()
        {
            //_control.stateCursor = _control.ExecutionCursor;
            _control.SetStateCursor(_control.ExecutionCursor);
        }
    }
}
