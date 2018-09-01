using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    public class ActionJointAcceleration : Action
    {
        public double jointAcceleration;
        public bool relative;

        public ActionJointAcceleration(double jointAcceleration, bool rel) : base()
        {
            this.type = ActionType.JointAcceleration;

            this.jointAcceleration = jointAcceleration;
            this.relative = rel;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} joint acceleration by {1} deg/s^2", this.jointAcceleration < 0 ? "Decrease" : "Increase", this.jointAcceleration) :
                string.Format("Set joint acceleration to {0} deg/s^2", this.jointAcceleration);
        }

        public override string ToInstruction()
        {
            return relative ?
                $"JointAcceleration({this.jointAcceleration});" :
                $"JointAccelerationTo({this.jointAcceleration});";
        }
    }
}
