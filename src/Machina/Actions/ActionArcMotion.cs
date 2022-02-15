using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types.Geometry;

namespace Machina
{
    //  █████╗ ██████╗  ██████╗
    // ██╔══██╗██╔══██╗██╔════╝
    // ███████║██████╔╝██║     
    // ██╔══██║██╔══██╗██║     
    // ██║  ██║██║  ██║╚██████╗
    // ╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝
    //                         
    // ███╗   ███╗ ██████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    // ████╗ ████║██╔═══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    // ██╔████╔██║██║   ██║   ██║   ██║██║   ██║██╔██╗ ██║
    // ██║╚██╔╝██║██║   ██║   ██║   ██║██║   ██║██║╚██╗██║
    // ██║ ╚═╝ ██║╚██████╔╝   ██║   ██║╚██████╔╝██║ ╚████║
    // ╚═╝     ╚═╝ ╚═════╝    ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //      
    /// <summary>
    /// An Action representing an arc motion through two frames. 
    /// </summary>
    public class ActionArcMotion : Action
    {
        public Plane through;
        public Plane end;
        public bool relative;
        public bool positionOnly;  // use position only? 

        public override ActionType Type => ActionType.ArcMotion;

        public ActionArcMotion(Plane through, Plane end, bool relative, bool positionOnly) : base()
        {
            // no deep copies here, expecting a clean object 
            this.through = through;
            this.end = end;
            this.relative = relative;
            this.positionOnly = positionOnly;
        }

        public override string ToString()
        {
            string str;
            string throughStr = positionOnly ?
                this.through.Origin.ToString() :
                this.through.Origin + " " + this.through.Orientation;
            string endStr = positionOnly ?
                            this.end.Origin.ToString() :
                            this.end.Origin + " " + this.through.Orientation;

            return String.Format("{0} motion through {1} to {2}",
                relative ? "Relative arc" : "Arc",
                throughStr,
                endStr);
        }

        public override string ToInstruction()
        {
            string action = relative ? "ArcMotion" : "ArcMotionTo";

            string args;
            if (positionOnly)
            {
                args = string.Format("({0},{1},{2},{3},{4},{5})",
                    Math.Round(this.through.Origin.X, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.through.Origin.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.through.Origin.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.end.Origin.X, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.end.Origin.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.end.Origin.Z, Geometry.STRING_ROUND_DECIMALS_MM));
            }
            else
            {
                args = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}",
                    Math.Round(this.through.Origin.X, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.through.Origin.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.through.Origin.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.through.XAxis.X, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.through.XAxis.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.through.XAxis.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.through.YAxis.X, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.through.YAxis.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.through.YAxis.Z, Geometry.STRING_ROUND_DECIMALS_MM),

                    Math.Round(this.end.Origin.X, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.end.Origin.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.end.Origin.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.end.XAxis.X, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.end.XAxis.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.end.XAxis.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.end.YAxis.X, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.end.YAxis.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.end.YAxis.Z, Geometry.STRING_ROUND_DECIMALS_MM));
            }

            return $"{action}({args});";
        }

    }
}
