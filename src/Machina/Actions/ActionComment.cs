using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //   ██████╗ ██████╗ ███╗   ███╗███╗   ███╗███████╗███╗   ██╗████████╗
    //  ██╔════╝██╔═══██╗████╗ ████║████╗ ████║██╔════╝████╗  ██║╚══██╔══╝
    //  ██║     ██║   ██║██╔████╔██║██╔████╔██║█████╗  ██╔██╗ ██║   ██║   
    //  ██║     ██║   ██║██║╚██╔╝██║██║╚██╔╝██║██╔══╝  ██║╚██╗██║   ██║   
    //  ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║ ╚═╝ ██║███████╗██║ ╚████║   ██║   
    //   ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝     ╚═╝╚══════╝╚═╝  ╚═══╝   ╚═╝   
    //
    /// <summary>
    /// Adds a line comment to the compiled code
    /// </summary>
    public class ActionComment : Action
    {
        public string comment;

        public override ActionType Type => ActionType.Comment;

        public ActionComment(string comment) : base()
        {
            this.comment = comment;
        }

        public override string ToString()
        {
            return string.Format("Comment: \"{0}\"", comment);
        }

        public override string ToInstruction()
        {
            return null;
        }
    }
}
