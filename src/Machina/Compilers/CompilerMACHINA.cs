using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //   ██████╗ ██████╗ ███╗   ███╗██████╗ ██╗██╗     ███████╗██████╗ 
    //  ██╔════╝██╔═══██╗████╗ ████║██╔══██╗██║██║     ██╔════╝██╔══██╗
    //  ██║     ██║   ██║██╔████╔██║██████╔╝██║██║     █████╗  ██████╔╝
    //  ██║     ██║   ██║██║╚██╔╝██║██╔═══╝ ██║██║     ██╔══╝  ██╔══██╗
    //  ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║     ██║███████╗███████╗██║  ██║
    //   ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝     ╚═╝╚══════╝╚══════╝╚═╝  ╚═╝
    //                                                                 
    //  ███╗   ███╗ █████╗  ██████╗██╗  ██╗██╗███╗   ██╗ █████╗        
    //  ████╗ ████║██╔══██╗██╔════╝██║  ██║██║████╗  ██║██╔══██╗       
    //  ██╔████╔██║███████║██║     ███████║██║██╔██╗ ██║███████║       
    //  ██║╚██╔╝██║██╔══██║██║     ██╔══██║██║██║╚██╗██║██╔══██║       
    //  ██║ ╚═╝ ██║██║  ██║╚██████╗██║  ██║██║██║ ╚████║██║  ██║       
    //  ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝       
    //                                                                 
    /// <summary>
    /// A simple compiler that returns a program in native Machina language, 
    /// i.e. serializing each Action into its own Instruction form.
    /// </summary>
    class CompilerMACHINA : Compiler
    {
        internal override Encoding Encoding => Encoding.UTF8;

        internal override char CC => '/';

        internal CompilerMACHINA() : base() { }

        public override List<Types.MachinaFile> UNSAFEFullProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets, bool humanComments)
        {
            return null;
        }

        public override List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets, bool humanComments)
        {
            // Which pending Actions are used for this program?
            // Copy them without flushing the buffer.
            List<Action> actions = block ?
                writer.actionBuffer.GetBlockPending(false) :
                writer.actionBuffer.GetAllPending(false);


            // ACTION LINES GENERATION
            List<string> actionLines = new List<string>();

            // DATA GENERATION
            // Use the write RobotCursor to generate the data
            int it = 0;
            string line = null;
            foreach (Action a in actions)
            {
                // Move writerCursor to this action state
                writer.ApplyNextAction();  // for the buffer to correctly manage them 

                line = a.ToInstruction();
                actionLines.Add(line);

                // Move on
                it++;
            }
            
            // PROGRAM ASSEMBLY
            // Initialize a module list
            List<string> module = new List<string>();

            // Banner
            module.AddRange(GenerateDisclaimerHeader(programName));
            module.Add("");

            // Code lines
            module.AddRange(actionLines);

            return module;
        }

    }
}
