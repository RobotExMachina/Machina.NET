using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machina.Types.Geometry;

namespace Machina
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
        public Joints Axes { get; }

        /// <summary>
        /// Robot external axes after last issued Action.
        /// </summary>
        public ExternalAxes ExternalAxes { get; }

        public ActionIssuedArgs(Action last, Vector pos, Rotation ori, Joints axes, ExternalAxes extax)
        {
            this.LastAction = last;
            this.Position = pos;
            this.Rotation = ori;
            this.Axes = axes;
            this.ExternalAxes = ExternalAxes;
        }

        public override string ToString() => ToJSONString();

        public override string ToJSONString()
        {
            return string.Format("{{\"event\":\"action-issued\",\"last\":\"{0}\",\"id\":{7},\"pos\":{1},\"ori\":{2},\"quat\":{3},\"axes\":{4},\"extax\":{5},\"conf\":{6}}}",
                Utilities.Strings.EscapeDoubleQuotes(this.LastAction.ToInstruction()),
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
