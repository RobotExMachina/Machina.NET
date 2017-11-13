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
    /// <summary>
    /// An abstract class that features methods to translate high-level robot actions into
    /// platform-specific programs. 
    /// </summary>
    /// 

    internal abstract class Compiler
    {
        /// <summary>
        /// Add a trailing action id to each declaration?
        /// </summary>
        internal bool ADD_ACTION_ID = false;

        /// <summary>
        /// Add a trailing human representation of the action after
        /// </summary>
        internal bool ADD_ACTION_STRING = false;

        /// <summary>
        /// Character used for comments by the compiler
        /// </summary>
        internal string commentCharacter = "";

        /// <summary>
        /// A constructor that takes several parameters particular to each compiler type
        /// </summary>
        /// <param name="commentChar"></param>
        protected Compiler(string commentChar)
        {
            this.commentCharacter = commentChar;
        }

        /// <summary>
        /// Creates a textual program representation of a set of Actions using a brand-specific RobotCursor.
        /// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        /// robot configuration and assumes the robot controller will figure out the correct one.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="writePointer"></param>
        /// <returns></returns>
        public abstract List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets, bool humanComments);
        
    }

}
