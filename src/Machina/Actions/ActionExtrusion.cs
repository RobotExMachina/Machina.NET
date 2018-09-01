using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //  ███████╗██╗  ██╗████████╗██████╗ ██╗   ██╗███████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔════╝╚██╗██╔╝╚══██╔══╝██╔══██╗██║   ██║██╔════╝██║██╔═══██╗████╗  ██║
    //  █████╗   ╚███╔╝    ██║   ██████╔╝██║   ██║███████╗██║██║   ██║██╔██╗ ██║
    //  ██╔══╝   ██╔██╗    ██║   ██╔══██╗██║   ██║╚════██║██║██║   ██║██║╚██╗██║
    //  ███████╗██╔╝ ██╗   ██║   ██║  ██║╚██████╔╝███████║██║╚██████╔╝██║ ╚████║
    //  ╚══════╝╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //
    /// <summary>
    /// Turns extrusion on/off in 3D printers.
    /// </summary>
    public class ActionExtrusion : Action
    {
        public bool extrude;

        public override ActionType Type => ActionType.Extrusion;

        public ActionExtrusion(bool extrude) : base()
        {
            this.extrude = extrude;
        }

        public override string ToString()
        {
            return $"Turn extrusion {(this.extrude ? "on" : "off")}";
        }

        public override string ToInstruction() => null;
    }
}
