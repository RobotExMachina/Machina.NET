using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Controllers
{
    /// <summary>
    /// A static factory class that creates ControlManagers based on ControlType.
    /// </summary>
    internal static class ControlFactory
    {
        internal static ControlManager GetControlManager(Control control)
        {
            switch (control.ControlMode)
            {
                case ControlType.Stream:
                case ControlType.Online:
                    return new StreamControlManager(control);

                case ControlType.Offline:
                    return new OfflineControlManager(control);

                default:
                    return null;
            }
        }
    }
}
