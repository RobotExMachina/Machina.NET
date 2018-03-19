using Machina.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Controllers
{

    internal class ControlFactory
    {
        internal static ControlManager GetControlManager(Control control)
        {
            switch (control.ControlMode)
            {
                case ControlType.Stream:
                    return new StreamControlManager(control);

                case ControlType.Offline:
                    return new OfflineControlManager(control);
            }
            return null;
        }
    }

    internal abstract class ControlManager
    {
        protected Control _control;
        protected ControlType _controlType;
        public ControlType Type => _controlType;

        public ControlManager(Control parent)
        {
            this._control = parent;
            this._controlType = this._control.ControlMode;
        }

        //public abstract bool Initialize();
        public bool Initialize()
        {
            if (_control.Comm != null)
            {
                Console.WriteLine("Communication protocol might be active. Please terminate it first.");
                return false;
            }

            // @TODO: shim assignment of correct robot model/brand
            SetCommunicationObject();

            // Pass the streamQueue object as a shared reference
            LinkWriteCursor();

            // Figure out which cursor to use for stateRepresentation
            SetStateCursor();

            return true;
        }

        public abstract bool Terminate();


        internal abstract void SetCommunicationObject();
        internal abstract void LinkWriteCursor();
        internal abstract void SetStateCursor();

    }

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

    internal class OfflineControlManager : ControlManager
    {
        public OfflineControlManager(Control parent) : base(parent) { }

        public override bool Terminate()
        {
            throw new NotImplementedException();
        }


        internal override void SetCommunicationObject()
        {
            _control.Comm = new DriverOffline(_control);
        }

        internal override void LinkWriteCursor()
        {
            // Pass the streamQueue object as a shared reference
            _control.Comm.LinkWriteCursor(ref _control.writeCursor);
        }

        internal override void SetStateCursor()
        {
            _control.stateCursor = _control.virtualCursor;
        }

    }
}
