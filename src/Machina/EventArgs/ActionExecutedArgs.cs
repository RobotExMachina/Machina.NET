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
    //   █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗   ███████╗██╗  ██╗███████╗ ██████╗██╗   ██╗████████╗███████╗██████╗     █████╗ ██████╗  ██████╗ ███████╗
    //  ██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║   ██╔════╝╚██╗██╔╝██╔════╝██╔════╝██║   ██║╚══██╔══╝██╔════╝██╔══██╗   ██╔══██╗██╔══██╗██╔════╝ ██╔════╝
    //  ███████║██║        ██║   ██║██║   ██║██╔██╗ ██║   █████╗   ╚███╔╝ █████╗  ██║     ██║   ██║   ██║   █████╗  ██║  ██║   ███████║██████╔╝██║  ███╗███████╗
    //  ██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║   ██╔══╝   ██╔██╗ ██╔══╝  ██║     ██║   ██║   ██║   ██╔══╝  ██║  ██║   ██╔══██║██╔══██╗██║   ██║╚════██║
    //  ██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║██╗███████╗██╔╝ ██╗███████╗╚██████╗╚██████╔╝   ██║   ███████╗██████╔╝██╗██║  ██║██║  ██║╚██████╔╝███████║
    //  ╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚═╝╚══════╝╚═╝  ╚═╝╚══════╝ ╚═════╝ ╚═════╝    ╚═╝   ╚══════╝╚═════╝ ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝
    //                                                                                                                                                          
    /// <summary>
    /// Arguments for ActionExecuted events.
    /// </summary>
    public class ActionExecutedArgs : MachinaEventArgs
    {
        /// <summary>
        /// The last Action that was executed by the device. 
        /// </summary>
        public Action LastAction { get; }

        /// <summary>
        /// How many actions are pending execution on the device? This includes only the ones that have already been released to the device, excluding the ones still queued Machina-side. 
        /// </summary>
        public int PendingExecutionOnDevice { get; }

        /// <summary>
        /// How many actions are pending to be executed? This includes the ones released to the device plus the ones pending on the queue in Machina.
        /// </summary>
        public int PendingExecutionTotal { get; }

        /// <summary>
        /// Position of the TCP after last Action.
        /// </summary>
        public Vector Position { get; }

        /// <summary>
        /// Orientation of the TCP after last Action.
        /// </summary>
        public Rotation Rotation { get; }

        /// <summary>
        /// Robot axes after last Action.
        /// </summary>
        public Axes Axes { get; }

        /// <summary>
        /// Robot external axes after last Action.
        /// </summary>
        public ExternalAxes ExternalAxes { get; }

        public ActionExecutedArgs(Action last, int pendingExecutionOnDevice, int pendingExecutionTotal, Vector pos, Rotation ori, Axes axes, ExternalAxes extax)
        {
            LastAction = last;
            PendingExecutionOnDevice = pendingExecutionOnDevice;
            PendingExecutionTotal = pendingExecutionTotal;
            Position = pos;
            Rotation = ori;
            Axes = axes;
            ExternalAxes = ExternalAxes;
        }

        public override string ToString() => ToJSONString();

        public override string ToJSONString()
        {
            return string.Format("{{\"event\":\"action-executed\",\"last\":\"{0}\",\"id\":{9},\"pendDev\":{1},\"pendTot\":{2},\"pos\":{3},\"ori\":{4},\"quat\":{5},\"axes\":{6},\"extax\":{7},\"conf\":{8}}}",
                Utilities.Strings.EscapeDoubleQuotes(LastAction?.ToInstruction()) ?? "null",
                PendingExecutionOnDevice,
                PendingExecutionTotal,
                Position?.ToArrayString(MMath.STRING_ROUND_DECIMALS_MM) ?? "null",
                Rotation?.ToOrientation()?.ToArrayString(MMath.STRING_ROUND_DECIMALS_MM) ?? "null",
                Rotation?.Q.ToArrayString(MMath.STRING_ROUND_DECIMALS_QUAT) ?? "null",
                Axes?.ToArrayString(MMath.STRING_ROUND_DECIMALS_DEGS) ?? "null",
                ExternalAxes?.ToArrayString() ?? "null",
                "null",  // placeholder for whenever IK solvers are introduced...
                LastAction == null ? "null" : LastAction.Id.ToString());
        }
    }
}
