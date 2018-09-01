using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //  ███████╗██╗  ██╗████████╗██████╗ ██╗   ██╗███████╗██╗ ██████╗ ███╗   ██╗██████╗  █████╗ ████████╗███████╗
    //  ██╔════╝╚██╗██╔╝╚══██╔══╝██╔══██╗██║   ██║██╔════╝██║██╔═══██╗████╗  ██║██╔══██╗██╔══██╗╚══██╔══╝██╔════╝
    //  █████╗   ╚███╔╝    ██║   ██████╔╝██║   ██║███████╗██║██║   ██║██╔██╗ ██║██████╔╝███████║   ██║   █████╗  
    //  ██╔══╝   ██╔██╗    ██║   ██╔══██╗██║   ██║╚════██║██║██║   ██║██║╚██╗██║██╔══██╗██╔══██║   ██║   ██╔══╝  
    //  ███████╗██╔╝ ██╗   ██║   ██║  ██║╚██████╔╝███████║██║╚██████╔╝██║ ╚████║██║  ██║██║  ██║   ██║   ███████╗
    //  ╚══════╝╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝   ╚══════╝
    //                                                                                                           
    /// <summary>
    /// Sets the extrusion rate in 3D printers in mm of filament per mm of lineal travel.
    /// </summary>
    public class ActionExtrusionRate : Action
    {
        public double rate;
        public bool relative;

        public ActionExtrusionRate(double rate, bool relative) : base()
        {
            this.type = ActionType.ExtrusionRate;

            this.rate = rate;
            this.relative = relative;
        }

        public override string ToString()
        {
            return this.relative ?
                $"{(this.rate < 0 ? "Decrease" : "Increase")} feed rate by {this.rate} mm/s" :
                $"Set feed rate to {this.rate} mm/s";
        }

        public override string ToInstruction() => null;
    }
}
