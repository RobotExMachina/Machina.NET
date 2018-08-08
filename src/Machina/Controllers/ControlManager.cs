using Machina.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Controllers
{
    
    /// <summary>
    /// A an abstract class that manages setup and initialization of Control objects based on ControlType.
    /// </summary>
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

        /// <summary>
        /// Initialize the managed Control object by setting Comm, WriteCursor, StateCursor, etc.
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            if (_control.Driver != null && !_control.Driver.Dispose())
            {
                throw new Exception("Couldn't dispose current Driver...");
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

    

   
}
