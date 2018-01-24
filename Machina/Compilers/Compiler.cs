using System;
using System.Collections.Generic;
using System.IO;

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
        /// Add a trailing human representation of the action after the code line
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

        public List<String> GenerateDisclaimerHeader(string programName)
        {
            var header = new List<String>();
            header.Add($"{commentCharacter}{commentCharacter} ███╗   ███╗ █████╗  ██████╗██╗  ██╗██╗███╗   ██╗ █████╗ ");
            header.Add($"{commentCharacter}{commentCharacter} ████╗ ████║██╔══██╗██╔════╝██║  ██║██║████╗  ██║██╔══██╗");
            header.Add($"{commentCharacter}{commentCharacter} ██╔████╔██║███████║██║     ███████║██║██╔██╗ ██║███████║");
            header.Add($"{commentCharacter}{commentCharacter} ██║╚██╔╝██║██╔══██║██║     ██╔══██║██║██║╚██╗██║██╔══██║");
            header.Add($"{commentCharacter}{commentCharacter} ██║ ╚═╝ ██║██║  ██║╚██████╗██║  ██║██║██║ ╚████║██║  ██║");
            header.Add($"{commentCharacter}{commentCharacter} ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝");
            header.Add($"{commentCharacter}{commentCharacter} ");
            header.Add($"{commentCharacter}{commentCharacter} Program name: {programName}");
            header.Add($"{commentCharacter}{commentCharacter} Created: {DateTime.Now.ToString()}");
            header.Add($"{commentCharacter}{commentCharacter} ");
            header.Add($"{commentCharacter}{commentCharacter} DISCLAIMER"); 
            header.Add($"{commentCharacter}{commentCharacter} WORKING WITH ROBOTS CAN BE DANGEROUS!");
            header.Add($"{commentCharacter}{commentCharacter} When using robots in a real-time interactive environment, please make sure:");
            header.Add($"{commentCharacter}{commentCharacter}     - You have been adequately trained to use that particular machine,");
            header.Add($"{commentCharacter}{commentCharacter}     - you are in good physical and mental condition,");
            header.Add($"{commentCharacter}{commentCharacter}     - you are operating the robot under the utmost security measures,");
            header.Add($"{commentCharacter}{commentCharacter}     - you are following the facility's and facility staff's security protocols,");
            header.Add($"{commentCharacter}{commentCharacter}     - and the robot has the appropriate guarding in place, including, but not reduced to:");
            header.Add($"{commentCharacter}{commentCharacter}         e -stops, physical barriers, light curtains, etc.");
            header.Add($"{commentCharacter}{commentCharacter} The Machina software framework and its generated code is provided as is;");
            header.Add($"{commentCharacter}{commentCharacter} use at your own risk. This product is not intended for any use that may");
            header.Add($"{commentCharacter}{commentCharacter} involve potential risks of death (including lifesaving equipment),");
            header.Add($"{commentCharacter}{commentCharacter} personal injury, or severe property or environmental damage.");
            header.Add($"{commentCharacter}{commentCharacter} Machina is in a very early stage of development. You are using this software");
            header.Add($"{commentCharacter}{commentCharacter} at your own risk, no warranties are provided herewith, and unexpected");
            header.Add($"{commentCharacter}{commentCharacter} results / bugs may arise during its use. Always test and simulate your");
            header.Add($"{commentCharacter}{commentCharacter} applications thoroughly before running them on a real device.");
            header.Add($"{commentCharacter}{commentCharacter} The author/s shall not be liable for any injuries, damages or losses");
            header.Add($"{commentCharacter}{commentCharacter} consequence of using this software in any way whatsoever.");
            header.Add($"{commentCharacter}{commentCharacter} ");
            header.Add($"{commentCharacter}{commentCharacter} (c) Jose Luis Garcia del Castillo y Lopez, {DateTime.Now.Year}");
            header.Add($"{commentCharacter}{commentCharacter} https://github.com/garciadelcastillo/Machina");
            header.Add($"{commentCharacter}{commentCharacter} MIT License");

            return header;
        }
    }

}
