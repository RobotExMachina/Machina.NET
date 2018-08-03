using System;
using System.Collections.Generic;

namespace Machina
{
    //   ██████╗ ██████╗ ███╗   ███╗██████╗ ██╗██╗     ███████╗██████╗ 
    //  ██╔════╝██╔═══██╗████╗ ████║██╔══██╗██║██║     ██╔════╝██╔══██╗
    //  ██║     ██║   ██║██╔████╔██║██████╔╝██║██║     █████╗  ██████╔╝
    //  ██║     ██║   ██║██║╚██╔╝██║██╔═══╝ ██║██║     ██╔══╝  ██╔══██╗
    //  ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║     ██║███████╗███████╗██║  ██║
    //   ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝     ╚═╝╚══════╝╚══════╝╚═╝  ╚═╝

    //  ██╗  ██╗██╗   ██╗███╗   ███╗ █████╗ ███╗   ██╗
    //  ██║  ██║██║   ██║████╗ ████║██╔══██╗████╗  ██║
    //  ███████║██║   ██║██╔████╔██║███████║██╔██╗ ██║
    //  ██╔══██║██║   ██║██║╚██╔╝██║██╔══██║██║╚██╗██║
    //  ██║  ██║╚██████╔╝██║ ╚═╝ ██║██║  ██║██║ ╚████║
    //  ╚═╝  ╚═╝ ╚═════╝ ╚═╝     ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝
    /// <summary>
    /// A quick compiler for human-readable instructions.
    /// </summary>
    internal class CompilerHuman : Compiler
    {
        public static readonly char COMMENT_CHAR = '/';

        internal CompilerHuman() : base(COMMENT_CHAR) { }

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

                line = string.Format("[{0}] {1}", it, a.ToString());
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
