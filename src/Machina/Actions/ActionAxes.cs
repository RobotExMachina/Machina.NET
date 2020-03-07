using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types.Geometry;

namespace Machina
{
    //   █████╗ ██╗  ██╗███████╗███████╗
    //  ██╔══██╗╚██╗██╔╝██╔════╝██╔════╝
    //  ███████║ ╚███╔╝ █████╗  ███████╗
    //  ██╔══██║ ██╔██╗ ██╔══╝  ╚════██║
    //  ██║  ██║██╔╝ ██╗███████╗███████║
    //  ╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝╚══════╝
    //                                  
    /// <summary>
    /// An Action representing the raw angular values of the device's joint rotations.
    /// </summary>
    public class ActionAxes : Action
    {
        public Axes joints;
        public bool relative;

        public override ActionType Type => ActionType.Axes;

        public ActionAxes(Axes joints, bool relative) : base()
        {
            this.joints = new Axes(joints);  // shallow copy
            this.relative = relative;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("Increase joint rotations by {0} deg", joints) :
                string.Format("Set joint rotations to {0} deg", joints);
        }

        public override string ToInstruction()
        {
            return string.Format("{0}({1},{2},{3},{4},{5},{6});",
                (this.relative ? "Axes" : "AxesTo"),
                Math.Round(this.joints.A1, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(this.joints.A2, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(this.joints.A3, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(this.joints.A4, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(this.joints.A5, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(this.joints.A6, Geometry.STRING_ROUND_DECIMALS_DEGS)
            );
        }
    }
}
