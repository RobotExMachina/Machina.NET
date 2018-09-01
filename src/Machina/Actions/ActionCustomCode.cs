using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //   ██████╗██╗   ██╗███████╗████████╗ ██████╗ ███╗   ███╗ ██████╗ ██████╗ ██████╗ ███████╗
    //  ██╔════╝██║   ██║██╔════╝╚══██╔══╝██╔═══██╗████╗ ████║██╔════╝██╔═══██╗██╔══██╗██╔════╝
    //  ██║     ██║   ██║███████╗   ██║   ██║   ██║██╔████╔██║██║     ██║   ██║██║  ██║█████╗  
    //  ██║     ██║   ██║╚════██║   ██║   ██║   ██║██║╚██╔╝██║██║     ██║   ██║██║  ██║██╔══╝  
    //  ╚██████╗╚██████╔╝███████║   ██║   ╚██████╔╝██║ ╚═╝ ██║╚██████╗╚██████╔╝██████╔╝███████╗
    //   ╚═════╝ ╚═════╝ ╚══════╝   ╚═╝    ╚═════╝ ╚═╝     ╚═╝ ╚═════╝ ╚═════╝ ╚═════╝ ╚══════╝
    //                                     
    /// <summary>
    /// Insert a line of custom code in the program. 
    /// </summary>                                                    
    public class ActionCustomCode : Action
    {
        public string statement;
        public bool isDeclaration;

        public ActionCustomCode(string statement, bool isDeclaration)
        {
            this.type = ActionType.CustomCode;
            this.statement = statement;
            this.isDeclaration = isDeclaration;
        }

        public override string ToString()
        {
            return this.isDeclaration ?
                $"Add custom declaration: \"{this.statement}\"" :
                $"Add custom instruction: \"{this.statement}\"";
        }

        public override string ToInstruction() =>
                $"CustomCode(\"{this.statement}\",{this.isDeclaration});";
    }
}
