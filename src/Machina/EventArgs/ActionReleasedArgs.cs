using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
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
        public Joints Axes { get; }

        /// <summary>
        /// Robot external axes after last released Action.
        /// </summary>
        public ExternalAxes ExternalAxes { get; }

        public ActionReleasedArgs(Action last, int pendingReleaseToDevice, Vector pos, Rotation ori, Joints axes, ExternalAxes extax)
        {
            this.LastAction = last;
            this.PendingReleaseToDevice = pendingReleaseToDevice;
            this.Position = pos;
            this.Rotation = ori;
            this.Axes = axes;
            this.ExternalAxes = ExternalAxes;
        }

        public override string ToString() => ToJSONString();

        public override string ToJSONString()
        {
            return string.Format("{{\"event\":\"action-released\",\"last\":\"{0}\",\"id\":\"{8}\",\"pend\":{1},\"pos\":{2},\"ori\":{3},\"quat\":{4},\"axes\":{5},\"extax\":{6},\"conf\":{7}}}",
                Util.EscapeDoubleQuotes(this.LastAction.ToInstruction()),
                this.PendingReleaseToDevice,
                this.Position?.ToArrayString() ?? "null",
                this.Rotation?.ToOrientation()?.ToArrayString() ?? "null",
                this.Rotation?.Q.ToArrayString() ?? "null",
                this.Axes?.ToArrayString() ?? "null",
                this.ExternalAxes?.ToArrayString() ?? "null",
                "null",  // placeholder for whenever IK solvers are introduced...
                this.LastAction.Id);
        }
    }

}

