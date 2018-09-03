using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{

    //  ███████╗██████╗ ███████╗███████╗██████╗ 
    //  ██╔════╝██╔══██╗██╔════╝██╔════╝██╔══██╗
    //  ███████╗██████╔╝█████╗  █████╗  ██║  ██║
    //  ╚════██║██╔═══╝ ██╔══╝  ██╔══╝  ██║  ██║
    //  ███████║██║     ███████╗███████╗██████╔╝
    //  ╚══════╝╚═╝     ╚══════╝╚══════╝╚═════╝ 
    //                                          
    /// <summary>
    /// An Action to change the current speed setting.
    /// </summary>
    public class ActionSpeedPlus : Action
    {
        public SpeedType speedType;
        public double value;
        public bool relative;

        public override ActionType Type => ActionType.SpeedPlus;

        public ActionSpeedPlus(double value, SpeedType speedType, bool relative) : base()
        {
            this.speedType = speedType;
            this.value = value;
            this.relative = relative;
        }

        public override string ToString()
        {
            //return relative ?
            //    string.Format("{0} TCP speed by {1} mm/s", this.speed < 0 ? "Decrease" : "Increase", speed) :
            //    string.Format("Set TCP speed to {0} mm/s", speed);
            return "SpeedActionPlus";
        }

        public override string ToInstruction()
        {
            //return relative ?
            //    $"Speed({this.speed});" :
            //    $"SpeedTo({this.speed});";
            return "foo";
        }
    }
}
