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
    //   █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗   ██████╗ ███████╗██╗     ███████╗ █████╗ ███████╗███████╗██████╗     █████╗ ██████╗  ██████╗ ███████╗
    //  ██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║   ██╔══██╗██╔════╝██║     ██╔════╝██╔══██╗██╔════╝██╔════╝██╔══██╗   ██╔══██╗██╔══██╗██╔════╝ ██╔════╝
    //  ███████║██║        ██║   ██║██║   ██║██╔██╗ ██║   ██████╔╝█████╗  ██║     █████╗  ███████║███████╗█████╗  ██║  ██║   ███████║██████╔╝██║  ███╗███████╗
    //  ██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║   ██╔══██╗██╔══╝  ██║     ██╔══╝  ██╔══██║╚════██║██╔══╝  ██║  ██║   ██╔══██║██╔══██╗██║   ██║╚════██║
    //  ██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║██╗██║  ██║███████╗███████╗███████╗██║  ██║███████║███████╗██████╔╝██╗██║  ██║██║  ██║╚██████╔╝███████║
    //  ╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚═╝╚═╝  ╚═╝╚══════╝╚══════╝╚══════╝╚═╝  ╚═╝╚══════╝╚══════╝╚═════╝ ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝
    //                                                                                                                                                        
    /// <summary>
    /// Arguments for ActionReleased events.
    /// </summary>
    public class ActionReleasedArgs : MachinaEventArgs
    {
        /// <summary>
        /// The last Action that was released to the device. 
        /// </summary>
        public Action LastAction { get; }

        /// <summary>
        /// How many actions are pending release to device?
        /// </summary>
        public int PendingReleaseToDevice { get; }

        /// <summary>
        /// Position of the TCP after last released Action.
        /// </summary>
        public Vector Position { get; }

        /// <summary>
        /// Orientation of the TCP after last released  Action.
        /// </summary>
        public Rotation Rotation { get; }

        /// <summary>
        /// Robot axes after last released Action.
        /// </summary>
        public Axes Axes { get; }

        /// <summary>
        /// Robot external axes after last released Action.
        /// </summary>
        public ExternalAxes ExternalAxes { get; }

        public ActionReleasedArgs(Action last, int pendingReleaseToDevice, Vector pos, Rotation ori, Axes axes, ExternalAxes extax)
        {
            LastAction = last;
            PendingReleaseToDevice = pendingReleaseToDevice;
            Position = pos;
            Rotation = ori;
            Axes = axes;
            ExternalAxes = ExternalAxes;
        }

        public override string ToString() => ToJSONString();

        public override string ToJSONString()
        {
            return string.Format("{{\"event\":\"action-released\",\"last\":\"{0}\",\"id\":{8},\"pend\":{1},\"pos\":{2},\"ori\":{3},\"quat\":{4},\"axes\":{5},\"extax\":{6},\"conf\":{7}}}",
                Utilities.Strings.EscapeDoubleQuotes(LastAction.ToInstruction()),
                PendingReleaseToDevice,
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

