using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Drivers;

namespace Machina.Controllers
{
    internal class OfflineControlManager : ControlManager
    {
        public OfflineControlManager(Control parent) : base(parent) { }

        public override bool Terminate()
        {
            throw new NotImplementedException();
        }


        internal override void SetCommunicationObject()
        {
            _control.Driver = new DriverOffline(_control);
        }

        internal override void LinkWriteCursor()
        {
            // Pass the streamQueue object as a shared reference
            _control.Driver.LinkWriteCursor(_control.ReleaseCursor);
        }

        internal override void SetStateCursor()
        {
            _control.SetStateCursor(_control.IssueCursor);
        }

    }
}
