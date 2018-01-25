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
        internal string commChar = "";

        /// <summary>
        /// A constructor that takes several parameters particular to each compiler type
        /// </summary>
        /// <param name="commentCharacter"></param>
        protected Compiler(string commentCharacter)
        {
            this.commChar = commentCharacter;
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
            // UTF chars don't convert well to ASCII... :(
            //header.Add($"{commChar}{commChar} ███╗   ███╗ █████╗  ██████╗██╗  ██╗██╗███╗   ██╗ █████╗ ");
            //header.Add($"{commChar}{commChar} ████╗ ████║██╔══██╗██╔════╝██║  ██║██║████╗  ██║██╔══██╗");
            //header.Add($"{commChar}{commChar} ██╔████╔██║███████║██║     ███████║██║██╔██╗ ██║███████║");
            //header.Add($"{commChar}{commChar} ██║╚██╔╝██║██╔══██║██║     ██╔══██║██║██║╚██╗██║██╔══██║");
            //header.Add($"{commChar}{commChar} ██║ ╚═╝ ██║██║  ██║╚██████╗██║  ██║██║██║ ╚████║██║  ██║");
            //header.Add($"{commChar}{commChar} ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝");
            header.Add($@"{commChar}{commChar} ###\   ###\ #####\  ######\##\  ##\##\###\   ##\ #####\ ");
            header.Add($@"{commChar}{commChar} ####\ ####\##\\\##\##\\\\\\##\  ##\##\####\  ##\##\\\##\");
            header.Add($@"{commChar}{commChar} ##\####\##\#######\##\     #######\##\##\##\ ##\#######\");
            header.Add($@"{commChar}{commChar} ##\\##\\##\##\\\##\##\     ##\\\##\##\##\\##\##\##\\\##\");
            header.Add($@"{commChar}{commChar} ##\ \\\ ##\##\  ##\\######\##\  ##\##\##\ \####\##\  ##\");
            header.Add($@"{commChar}{commChar} \\\     \\\\\\  \\\ \\\\\\\\\\  \\\\\\\\\  \\\\\\\\  \\\");
            header.Add($"{commChar}{commChar} ");
            header.Add($"{commChar}{commChar} Program name: {programName}");
            header.Add($"{commChar}{commChar} Created: {DateTime.Now.ToString()}");
            header.Add($"{commChar}{commChar} ");
            header.Add($"{commChar}{commChar} DISCLAIMER"); 
            header.Add($"{commChar}{commChar} WORKING WITH ROBOTS CAN BE DANGEROUS!");
            header.Add($"{commChar}{commChar} When using robots in a real-time interactive environment, please make sure:");
            header.Add($"{commChar}{commChar}     - You have been adequately trained to use that particular machine,");
            header.Add($"{commChar}{commChar}     - you are in good physical and mental condition,");
            header.Add($"{commChar}{commChar}     - you are operating the robot under the utmost security measures,");
            header.Add($"{commChar}{commChar}     - you are following the facility's and facility staff's security protocols,");
            header.Add($"{commChar}{commChar}     - and the robot has the appropriate guarding in place, including, but not reduced to:");
            header.Add($"{commChar}{commChar}         e -stops, physical barriers, light curtains, etc.");
            header.Add($"{commChar}{commChar} The Machina software framework and its generated code is provided as is;");
            header.Add($"{commChar}{commChar} use at your own risk. This product is not intended for any use that may");
            header.Add($"{commChar}{commChar} involve potential risks of death (including lifesaving equipment),");
            header.Add($"{commChar}{commChar} personal injury, or severe property or environmental damage.");
            header.Add($"{commChar}{commChar} Machina is in a very early stage of development. You are using this software");
            header.Add($"{commChar}{commChar} at your own risk, no warranties are provided herewith, and unexpected");
            header.Add($"{commChar}{commChar} results / bugs may arise during its use. Always test and simulate your");
            header.Add($"{commChar}{commChar} applications thoroughly before running them on a real device.");
            header.Add($"{commChar}{commChar} The author/s shall not be liable for any injuries, damages or losses");
            header.Add($"{commChar}{commChar} consequence of using this software in any way whatsoever.");
            header.Add($"{commChar}{commChar} ");
            header.Add($"{commChar}{commChar} (c) Jose Luis Garcia del Castillo y Lopez, {DateTime.Now.Year}");
            header.Add($"{commChar}{commChar} https://github.com/garciadelcastillo/Machina");
            header.Add($"{commChar}{commChar} MIT License");

            return header;
        }
    }

}
