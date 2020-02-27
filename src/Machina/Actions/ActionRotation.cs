using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types.Geometry;

namespace Machina
{
    //  ██████╗  ██████╗ ████████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔══██╗██╔═══██╗╚══██╔══╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //  ██████╔╝██║   ██║   ██║   ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //  ██╔══██╗██║   ██║   ██║   ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //  ██║  ██║╚██████╔╝   ██║   ██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //  ╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                   
    /// <summary>
    /// An Action representing a Rotation transformation in Quaternion represnetation.
    /// </summary>
    public class ActionRotation : Action
    {
        public Rotation rotation;
        public bool relative;

        public override ActionType Type => ActionType.Rotation;

        public ActionRotation(double vx, double vy, double vz, double angle, bool relRot) : base()
        {
            this.rotation = new Rotation(vx, vy, vz, angle);
            this.relative = relRot;
        }

        public ActionRotation(Rotation rot, bool relRot) : base()
        {
            this.rotation = new Rotation(rot);  // shallow copy
            this.relative = relRot;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("Rotate {0} deg around {1}", rotation.Angle, rotation.Axis) :
                string.Format("Rotate to {0}", new Orientation(rotation));
        }

        public override string ToInstruction()
        {
            if (this.relative)
            {
                return string.Format("Rotate({0},{1},{2},{3});",
                    Math.Round(this.rotation.AA.X, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                    Math.Round(this.rotation.AA.Y, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                    Math.Round(this.rotation.AA.Z, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                    Math.Round(this.rotation.AA.Angle, Geometry.STRING_ROUND_DECIMALS_DEGS)
                );
            }

            // @TODO: improve this! 
            Orientation ori = new Orientation(this.rotation);
            return string.Format("RotateTo({0},{1},{2},{3},{4},{5});",
                Math.Round(ori.XAxis.X, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                Math.Round(ori.XAxis.Y, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                Math.Round(ori.XAxis.Z, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                Math.Round(ori.YAxis.X, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                Math.Round(ori.YAxis.Y, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                Math.Round(ori.YAxis.Z, Geometry.STRING_ROUND_DECIMALS_VECTOR)
            );
        }
    }
}
