using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machina;
using Machina.EventArgs;
using Machina.Types.Geometry;

namespace Machina.EventArgs
{
    //   █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗   ██╗███████╗███████╗██╗   ██╗███████╗██████╗     █████╗ ██████╗  ██████╗ ███████╗
    //  ██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║   ██║██╔════╝██╔════╝██║   ██║██╔════╝██╔══██╗   ██╔══██╗██╔══██╗██╔════╝ ██╔════╝
    //  ███████║██║        ██║   ██║██║   ██║██╔██╗ ██║   ██║███████╗███████╗██║   ██║█████╗  ██║  ██║   ███████║██████╔╝██║  ███╗███████╗
    //  ██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║   ██║╚════██║╚════██║██║   ██║██╔══╝  ██║  ██║   ██╔══██║██╔══██╗██║   ██║╚════██║
    //  ██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║██╗██║███████║███████║╚██████╔╝███████╗██████╔╝██╗██║  ██║██║  ██║╚██████╔╝███████║
    //  ╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚═╝╚═╝╚══════╝╚══════╝ ╚═════╝ ╚══════╝╚═════╝ ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝
    //                                                                                                                                    
    /// <summary>
    /// Arguments for ActionIssued events.
    /// </summary>
    public class ActionIssuedArgs : MachinaEventArgs
    {
        /// <summary>
        /// The last Action that was issued to the device. 
        /// </summary>
        public Action LastAction { get; }

        /// <summary>
        /// Position of the TCP after last issued Action.
        /// </summary>
        public Vector Position { get; }

        /// <summary>
        /// Orientation of the TCP after last issued  Action.
        /// </summary>
        public Rotation Rotation { get; }

        /// <summary>
        /// Robot axes after last issued Action.
        /// </summary>
        public Axes Axes { get; }

        /// <summary>
        /// Robot external axes after last issued Action.
        /// </summary>
        public ExternalAxes ExternalAxes { get; }

        public ActionIssuedArgs(Action last, Vector pos, Rotation ori, Axes axes, ExternalAxes extax)
        {
            LastAction = last;
            Position = pos;
            Rotation = ori;
            Axes = axes;
            ExternalAxes = ExternalAxes;
        }

        public override string ToString() => ToJSONString();

        public override string ToJSONString()
        {
            return string.Format("{{\"event\":\"action-issued\",\"last\":\"{0}\",\"id\":{7},\"pos\":{1},\"ori\":{2},\"quat\":{3},\"axes\":{4},\"extax\":{5},\"conf\":{6}}}",
                Utilities.Strings.EscapeDoubleQuotes(LastAction.ToInstruction()),
                Position?.ToArrayString(MMath.STRING_ROUND_DECIMALS_MM) ?? "null",
                Rotation?.ToOrientation()?.ToArrayString(MMath.STRING_ROUND_DECIMALS_MM) ?? "null",
                Rotation?.Q.ToArrayString(MMath.STRING_ROUND_DECIMALS_QUAT) ?? "null",
                Axes?.ToArrayString(MMath.STRING_ROUND_DECIMALS_DEGS) ?? "null",
                ExternalAxes?.ToArrayString() ?? "null",
                "null",  // placeholder for whenever IK solvers are introduced...
                LastAction.Id);
        }
    }
}
