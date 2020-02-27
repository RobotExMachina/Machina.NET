using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machina.Types.Geometry;

namespace Machina
{
    //  ████████╗██████╗  █████╗ ███╗   ██╗███████╗██╗      █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ╚══██╔══╝██╔══██╗██╔══██╗████╗  ██║██╔════╝██║     ██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //     ██║   ██████╔╝███████║██╔██╗ ██║███████╗██║     ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //     ██║   ██╔══██╗██╔══██║██║╚██╗██║╚════██║██║     ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //     ██║   ██║  ██║██║  ██║██║ ╚████║███████║███████╗██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //     ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚══════╝╚══════╝╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                                            
    /// <summary>
    /// An action representing a Translation transform in along a guiding vector.
    /// </summary>
    public class ActionTranslation : Action
    {
        public Vector translation;
        public bool relative;

        public override ActionType Type => ActionType.Translation;

        public ActionTranslation(double x, double y, double z, bool relTrans) : base()
        {
            this.translation = new Vector(x, y, z);
            this.relative = relTrans;
        }

        public ActionTranslation(Vector trans, bool relTrans) : base()
        {
            this.translation = new Vector(trans);  // shallow copy
            this.relative = relTrans;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("Move {0} mm", translation) :
                string.Format("Move to {0} mm", translation);
        }

        public override string ToInstruction()
        {
            return string.Format("{0}({1},{2},{3});",
                (this.relative ? "Move" : "MoveTo"),
                Math.Round(this.translation.X, Geometry.STRING_ROUND_DECIMALS_MM),
                Math.Round(this.translation.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                Math.Round(this.translation.Z, Geometry.STRING_ROUND_DECIMALS_MM)
            );
        }
    }
}
