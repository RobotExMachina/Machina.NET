using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //   █████╗  ██████╗ ██████╗███████╗██╗     ███████╗██████╗  █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔══██╗██╔════╝██╔════╝██╔════╝██║     ██╔════╝██╔══██╗██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //  ███████║██║     ██║     █████╗  ██║     █████╗  ██████╔╝███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //  ██╔══██║██║     ██║     ██╔══╝  ██║     ██╔══╝  ██╔══██╗██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //  ██║  ██║╚██████╗╚██████╗███████╗███████╗███████╗██║  ██║██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //  ╚═╝  ╚═╝ ╚═════╝ ╚═════╝╚══════╝╚══════╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                                                 
    public class ActionAcceleration : Action
    {
        public double acceleration;
        public bool relative;

        public override ActionType Type => ActionType.Acceleration;

        public ActionAcceleration(double acc, bool relative) : base()
        {
            this.acceleration = acc;
            this.relative = relative;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} TCP acceleration by {1} mm/s^2", this.acceleration < 0 ? "Decrease" : "Increase", this.acceleration) :
                string.Format("Set TCP acceleration to {0} mm/s^2", this.acceleration);
        }

        public override string ToInstruction()
        {
            return relative ?
                $"Acceleration({this.acceleration});" :
                $"AccelerationTo({this.acceleration});";
        }
    }
}
