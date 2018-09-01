using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //   █████╗ ████████╗████████╗ █████╗  ██████╗██╗  ██╗
    //  ██╔══██╗╚══██╔══╝╚══██╔══╝██╔══██╗██╔════╝██║  ██║
    //  ███████║   ██║      ██║   ███████║██║     ███████║
    //  ██╔══██║   ██║      ██║   ██╔══██║██║     ██╔══██║
    //  ██║  ██║   ██║      ██║   ██║  ██║╚██████╗██║  ██║
    //  ╚═╝  ╚═╝   ╚═╝      ╚═╝   ╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝
    //                                                    
    /// <summary>
    /// Attaches a Tool to the robot flange. 
    /// If the robot already had a tool, this will be substituted.
    /// </summary>
    public class ActionAttach : Action
    {
        public Tool tool;
        internal bool translationFirst;

        public ActionAttach(Tool tool) : base()
        {
            this.type = ActionType.Attach;

            this.tool = tool;
            this.translationFirst = tool.translationFirst;
        }

        public override string ToString()
        {
            return string.Format("Attach tool \"{0}\"", this.tool.name);
        }

        public override string ToInstruction()
        {
            return $"Attach(\"{this.tool.name}\");";
        }
    }
}
