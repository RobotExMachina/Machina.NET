using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machina.Types.Geometry;

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
        public ExternalAxesTarget target;
        public bool relative;

        public override ActionType Type => ActionType.ExternalAxis;

        internal static readonly Dictionary<ExternalAxesTarget, string> TARGET_DOMAINS = new Dictionary<ExternalAxesTarget, string>()
        {
            {ExternalAxesTarget.All, "" },
            {ExternalAxesTarget.Cartesian, "for cartesian targets." },
            {ExternalAxesTarget.Joint, "for joint targets." }
        };

        public ActionExternalAxis(int axisNumber, double value, ExternalAxesTarget target, bool relative) : base()
        {
            this.axisNumber = axisNumber;
            this.value = value;
            this.target = target;
            this.relative = relative;
        }

        public override string ToString()
        {
            string str = this.relative ?
                $"Increase external axis #{this.axisNumber} by {this.value}" :
                $"Set external axis #{this.axisNumber} to {this.value}";

            str += " " + TARGET_DOMAINS[this.target];

            return str;
        }

        public override string ToInstruction()
        {
            string inst = relative ?
                $"ExternalAxis({this.axisNumber},{Math.Round(this.value, MMath.STRING_ROUND_DECIMALS_MM)}" :
                $"ExternalAxisTo({this.axisNumber},{Math.Round(this.value, MMath.STRING_ROUND_DECIMALS_MM)}";

            if (this.target != ExternalAxesTarget.All)
            {
                inst += ",\"" + this.target.ToString() + "\"";
            }

            inst += ");";

            return inst;
        }
    }
}
