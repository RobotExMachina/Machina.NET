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
            _control.Comm = new DriverABB(_control);
        }

        internal override void LinkWriteCursor()
        {
            // Pass the streamQueue object as a shared reference
            _control.Comm.LinkWriteCursor(ref _control.writeCursor);
        }

        internal override void SetStateCursor()
        {
            _control.stateCursor = _control.motionCursor;
        }
    }
}
