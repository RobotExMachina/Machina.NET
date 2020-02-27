using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machina.Types.Geometry;

namespace Machina
{
    //  ████████╗███████╗███╗   ███╗██████╗ ███████╗██████╗  █████╗ ████████╗██╗   ██╗██████╗ ███████╗
    //  ╚══██╔══╝██╔════╝████╗ ████║██╔══██╗██╔════╝██╔══██╗██╔══██╗╚══██╔══╝██║   ██║██╔══██╗██╔════╝
    //     ██║   █████╗  ██╔████╔██║██████╔╝█████╗  ██████╔╝███████║   ██║   ██║   ██║██████╔╝█████╗  
    //     ██║   ██╔══╝  ██║╚██╔╝██║██╔═══╝ ██╔══╝  ██╔══██╗██╔══██║   ██║   ██║   ██║██╔══██╗██╔══╝  
    //     ██║   ███████╗██║ ╚═╝ ██║██║     ███████╗██║  ██║██║  ██║   ██║   ╚██████╔╝██║  ██║███████╗
    //     ╚═╝   ╚══════╝╚═╝     ╚═╝╚═╝     ╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝╚══════╝
    //                                                                                                
    /// <summary>
    /// Sets the temperature of the 3D printer part, and optionally waits for the part to reach the temp to resume eexecution.
    /// </summary>
    public class ActionTemperature : Action
    {
        public double temperature;
        public RobotPartType robotPart;
        public bool wait;
        public bool relative;

        public override ActionType Type => ActionType.Temperature;

        public ActionTemperature(double temperature, RobotPartType robotPart, bool wait, bool relative) : base()
        {
            this.temperature = temperature;
            this.robotPart = robotPart;
            this.wait = wait;
            this.relative = relative;
        }

        public override string ToString()
        {
            if (relative)
            {
                return string.Format("{0} {1} temperature by {2} C{3}",
                    this.temperature < 0 ? "Decrease" : "Increase",
                    Enum.GetName(typeof(RobotPartType), this.robotPart),
                    Math.Round(this.temperature, Geometry.STRING_ROUND_DECIMALS_TEMPERATURE),
                    this.wait ? " and wait" : "");
            }
            else
            {
                return string.Format("Set {0} temperature to {1} C{2}",
                    Enum.GetName(typeof(RobotPartType), this.robotPart),
                    Math.Round(this.temperature, Geometry.STRING_ROUND_DECIMALS_TEMPERATURE),
                    this.wait ? " and wait" : "");
            }
        }

        public override string ToInstruction() => null;
    }
}
