using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //       ██╗ ██████╗ ██╗███╗   ██╗████████╗    ███████╗██████╗   ██╗ █████╗  ██████╗ ██████╗
    //       ██║██╔═══██╗██║████╗  ██║╚══██╔══╝    ██╔════╝██╔══██╗ ██╔╝██╔══██╗██╔════╝██╔════╝
    //       ██║██║   ██║██║██╔██╗ ██║   ██║       ███████╗██████╔╝██╔╝ ███████║██║     ██║     
    //  ██   ██║██║   ██║██║██║╚██╗██║   ██║       ╚════██║██╔═══╝██╔╝  ██╔══██║██║     ██║     
    //  ╚█████╔╝╚██████╔╝██║██║ ╚████║   ██║       ███████║██║   ██╔╝   ██║  ██║╚██████╗╚██████╗
    //   ╚════╝  ╚═════╝ ╚═╝╚═╝  ╚═══╝   ╚═╝       ╚══════╝╚═╝   ╚═╝    ╚═╝  ╚═╝ ╚═════╝ ╚═════╝
    //                                                                                          
    public class ActionJointSpeed : Action
    {
        public double jointSpeed;
        public bool relative;

        public ActionJointSpeed(double jointSpeed, bool rel) : base()
        {
            this.type = ActionType.JointSpeed;

            this.jointSpeed = jointSpeed;
            this.relative = rel;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} joint speed by {1} deg/s", this.jointSpeed < 0 ? "Decrease" : "Increase", this.jointSpeed) :
                string.Format("Set joint speed to {0} deg/s", this.jointSpeed);
        }

        public override string ToInstruction()
        {
            return relative ?
                $"JointSpeed({this.jointSpeed});" :
                $"JointSpeedTo({this.jointSpeed});";
        }
    }
}
