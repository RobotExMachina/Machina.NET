using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{

    //   █████╗ ██████╗ ███╗   ███╗ █████╗ ███╗   ██╗ ██████╗ ██╗     ███████╗
    //  ██╔══██╗██╔══██╗████╗ ████║██╔══██╗████╗  ██║██╔════╝ ██║     ██╔════╝
    //  ███████║██████╔╝██╔████╔██║███████║██╔██╗ ██║██║  ███╗██║     █████╗  
    //  ██╔══██║██╔══██╗██║╚██╔╝██║██╔══██║██║╚██╗██║██║   ██║██║     ██╔══╝  
    //  ██║  ██║██║  ██║██║ ╚═╝ ██║██║  ██║██║ ╚████║╚██████╔╝███████╗███████╗
    //  ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝╚══════╝
    //                                                                        
    /// <summary>
    /// An action to change the arm-angle value for 7-dof robotic arms. 
    /// </summary>
    public class ActionArmAngle : Action
    {
        public double angle;
        public bool relative;

        public override ActionType Type => ActionType.ArmAngle;

        public ActionArmAngle(double angle, bool relative) : base()
        {
            this.angle = angle;
            this.relative = relative;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} arm-angle by {1} deg", this.angle < 0 ? "Decrease" : "Increase", this.angle) :
                string.Format("Set arm-angle to {0} deg", this.angle);
        }

        public override string ToInstruction()
        {
            return relative ?
                $"ArmAngle({this.angle});" :
                $"ArmAngleTo({this.angle});";
        }
    }
}
