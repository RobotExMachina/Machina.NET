using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //  ██████╗ ██████╗ ███████╗ ██████╗██╗███████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔══██╗██╔══██╗██╔════╝██╔════╝██║██╔════╝██║██╔═══██╗████╗  ██║
    //  ██████╔╝██████╔╝█████╗  ██║     ██║███████╗██║██║   ██║██╔██╗ ██║
    //  ██╔═══╝ ██╔══██╗██╔══╝  ██║     ██║╚════██║██║██║   ██║██║╚██╗██║
    //  ██║     ██║  ██║███████╗╚██████╗██║███████║██║╚██████╔╝██║ ╚████║
    //  ╚═╝     ╚═╝  ╚═╝╚══════╝ ╚═════╝╚═╝╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                   
    /// <summary>
    /// An Action to change current precision settings.
    /// </summary>
    public class ActionPrecision : Action
    {
        public double precision;
        public bool relative;

        public override ActionType Type => ActionType.Precision;

        public ActionPrecision(double value, bool relative) : base()
        {
            this.precision = value;
            this.relative = relative;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} precision radius by {1} mm", this.precision < 0 ? "Decrease" : "Increase", this.precision) :
                string.Format("Set precision radius to {0} mm", this.precision);
        }

        public override string ToInstruction()
        {
            return relative ?
                $"Precision({this.precision});" :
                $"PrecisionTo({this.precision});";
        }
    }
}
