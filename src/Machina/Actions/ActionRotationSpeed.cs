using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{

    //  ██████╗  ██████╗ ████████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗    ███████╗██████╗ ███████╗███████╗██████╗ 
    //  ██╔══██╗██╔═══██╗╚══██╔══╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║    ██╔════╝██╔══██╗██╔════╝██╔════╝██╔══██╗
    //  ██████╔╝██║   ██║   ██║   ███████║   ██║   ██║██║   ██║██╔██╗ ██║    ███████╗██████╔╝█████╗  █████╗  ██║  ██║
    //  ██╔══██╗██║   ██║   ██║   ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║    ╚════██║██╔═══╝ ██╔══╝  ██╔══╝  ██║  ██║
    //  ██║  ██║╚██████╔╝   ██║   ██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║    ███████║██║     ███████╗███████╗██████╔╝
    //  ╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝    ╚══════╝╚═╝     ╚══════╝╚══════╝╚═════╝ 
    //                                                                                                               
    public class ActionRotationSpeed : Action
    {
        public double rotationSpeed;
        public bool relative;

        public override ActionType Type => ActionType.RotationSpeed;

        public ActionRotationSpeed(double rotSpeed, bool rel) : base()
        {
            this.rotationSpeed = rotSpeed;
            this.relative = rel;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} TCP rotation speed by {1} mm/s", this.rotationSpeed < 0 ? "Decrease" : "Increase", this.rotationSpeed) :
                string.Format("Set TCP rotation speed to {0} mm/s", this.rotationSpeed);
        }

        public override string ToInstruction()
        {
            return relative ?
                $"RotationSpeed({this.rotationSpeed});" :
                $"RotationSpeedTo({this.rotationSpeed});";
        }
    }
}
