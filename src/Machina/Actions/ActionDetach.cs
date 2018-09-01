using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //  ██████╗ ███████╗████████╗ █████╗  ██████╗██╗  ██╗
    //  ██╔══██╗██╔════╝╚══██╔══╝██╔══██╗██╔════╝██║  ██║
    //  ██║  ██║█████╗     ██║   ███████║██║     ███████║
    //  ██║  ██║██╔══╝     ██║   ██╔══██║██║     ██╔══██║
    //  ██████╔╝███████╗   ██║   ██║  ██║╚██████╗██║  ██║
    //  ╚═════╝ ╚══════╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝
    //                                                   
    /// <summary>
    /// Detaches any tool currently attached to the robot.
    /// </summary>
    public class ActionDetach : Action
    {
        public override ActionType Type => ActionType.Detach;

        public ActionDetach() : base() { }

        public override string ToString()
        {
            return "Detach all tools";
        }

        public override string ToInstruction()
        {
            return $"Detach();";
        }
    }
}
