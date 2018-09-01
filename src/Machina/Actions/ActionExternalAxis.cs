using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //  ███████╗██╗  ██╗████████╗███████╗██████╗ ███╗   ██╗ █████╗ ██╗      █████╗ ██╗  ██╗██╗███████╗
    //  ██╔════╝╚██╗██╔╝╚══██╔══╝██╔════╝██╔══██╗████╗  ██║██╔══██╗██║     ██╔══██╗╚██╗██╔╝██║██╔════╝
    //  █████╗   ╚███╔╝    ██║   █████╗  ██████╔╝██╔██╗ ██║███████║██║     ███████║ ╚███╔╝ ██║███████╗
    //  ██╔══╝   ██╔██╗    ██║   ██╔══╝  ██╔══██╗██║╚██╗██║██╔══██║██║     ██╔══██║ ██╔██╗ ██║╚════██║
    //  ███████╗██╔╝ ██╗   ██║   ███████╗██║  ██║██║ ╚████║██║  ██║███████╗██║  ██║██╔╝ ██╗██║███████║
    //  ╚══════╝╚═╝  ╚═╝   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝╚══════╝
    //                                                                                                
    /// <summary>
    /// Set the vlaue of an external axis to the device.
    /// </summary>
    public class ActionExternalAxis : Action
    {
        public int axisNumber;
        public double value;
        public bool relative;

        public ActionExternalAxis(int axisNumber, double value, bool relative)
        {
            this.type = ActionType.ExternalAxis;
            this.axisNumber = axisNumber;
            this.value = value;
            this.relative = relative;
        }

        public override string ToString()
        {
            return this.relative ?
                $"Increase external axis #{this.axisNumber} by {this.value}" :
                $"Set external axis #{this.axisNumber} to {this.value}";
        }

        public override string ToInstruction()
        {
            return relative ?
                $"ExternalAxis({this.axisNumber},{Math.Round(this.value, Geometry.STRING_ROUND_DECIMALS_MM)});" :
                $"ExternalAxisTo({this.axisNumber},{Math.Round(this.value, Geometry.STRING_ROUND_DECIMALS_MM)});";
        }
    }
}
