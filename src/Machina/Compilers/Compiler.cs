using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Machina.Types.Data;
using Machina.Descriptors.Cursors;

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
        internal bool addActionID = false;

        /// <summary>
        /// Add a trailing human representation of the action after the code line
        /// </summary>
        internal bool addActionString = false;

        /// <summary>
        /// Comment character (CC) used for comments by the compiler
        /// </summary>
        internal abstract char CC { get; }

        /// <summary>
        /// Encoding for text files produced by the compiler.
        /// </summary>
        internal abstract Encoding Encoding { get; }

        /// <summary>
        /// An empty constructor really...
        /// </summary>
        internal Compiler() { }


        ///// <summary>
        ///// Creates a textual program representation of a set of Actions using a brand-specific RobotCursor.
        ///// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        ///// robot configuration and assumes the robot controller will figure out the correct one.
        ///// </summary>
        ///// <param name="programName"></param>
        ///// <param name="writePointer"></param>
        ///// <returns></returns>
        //public abstract List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets, bool humanComments);

        /// <summary>
        /// Creates a RobotProgram as a textual representation of a set of Actions using a brand-specific RobotCursor.
        /// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        /// robot configuration and assumes the robot controller will figure out the correct one.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="writePointer"></param>
        /// <returns></returns>
        public abstract RobotProgram UNSAFEFullProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets, bool humanComments);

        public List<String> GenerateDisclaimerHeader(string programName)
        {
            // @TODO: convert this to a StringBuilder
            var header = new List<String>();
            // UTF chars don't convert well to ASCII... :(
            header.Add($@"{CC}{CC} ###\   ###\ #####\  ######\##\  ##\##\###\   ##\ #####\ ");
            header.Add($@"{CC}{CC} ####\ ####\##\\\##\##\\\\\\##\  ##\##\####\  ##\##\\\##\");
            header.Add($@"{CC}{CC} ##\####\##\#######\##\     #######\##\##\##\ ##\#######\");
            header.Add($@"{CC}{CC} ##\\##\\##\##\\\##\##\     ##\\\##\##\##\\##\##\##\\\##\");
            header.Add($@"{CC}{CC} ##\ \\\ ##\##\  ##\\######\##\  ##\##\##\ \####\##\  ##\");
            header.Add($@"{CC}{CC} \\\     \\\\\\  \\\ \\\\\\\\\\  \\\\\\\\\  \\\\\\\\  \\\");
            header.Add($"{CC}{CC} ");
            header.Add($"{CC}{CC} Program name: {programName}");
            header.Add($"{CC}{CC} Created: {DateTime.Now.ToString()}");
            header.Add($"{CC}{CC} ");
            header.Add($"{CC}{CC} DISCLAIMER: WORKING WITH ROBOTS CAN BE DANGEROUS!");
            header.Add($"{CC}{CC} When using robots in a real-time interactive environment, please make sure:");
            header.Add($"{CC}{CC}     - You have been adequately trained to use that particular machine,");
            header.Add($"{CC}{CC}     - you are in good physical and mental condition,");
            header.Add($"{CC}{CC}     - you are operating the robot under the utmost security measures,");
            header.Add($"{CC}{CC}     - you are following the facility's and facility staff's security protocols,");
            header.Add($"{CC}{CC}     - and the robot has the appropriate guarding in place, including, but not reduced to:");
            header.Add($"{CC}{CC}         e -stops, physical barriers, light curtains, etc.");
            header.Add($"{CC}{CC} The Machina software framework and its generated code is provided as is;");
            header.Add($"{CC}{CC} use at your own risk. This product is not intended for any use that may");
            header.Add($"{CC}{CC} involve potential risks of death (including lifesaving equipment),");
            header.Add($"{CC}{CC} personal injury, or severe property or environmental damage.");
            header.Add($"{CC}{CC} Machina is in a very early stage of development. You are using this software");
            header.Add($"{CC}{CC} at your own risk, no warranties are provided herewith, and unexpected");
            header.Add($"{CC}{CC} results / bugs may arise during its use. Always test and simulate your");
            header.Add($"{CC}{CC} applications thoroughly before running them on a real device.");
            header.Add($"{CC}{CC} The author/s shall not be liable for any injuries, damages or losses");
            header.Add($"{CC}{CC} consequence of using this software in any way whatsoever.");
            header.Add($"{CC}{CC} ");
            header.Add($"{CC}{CC} ");
            header.Add($"{CC}{CC} Copyright(c) {DateTime.Now.Year} Jose Luis Garcia del Castillo y Lopez");
            header.Add($"{CC}{CC} https://github.com/RobotExMachina");
            header.Add($"{CC}{CC} ");
            header.Add($"{CC}{CC} MIT License");
            header.Add($"{CC}{CC} ");
            header.Add($"{CC}{CC} Permission is hereby granted, free of charge, to any person obtaining a copy");
            header.Add($"{CC}{CC} of this software and associated documentation files(the \"Software\"), to deal");
            header.Add($"{CC}{CC} in the Software without restriction, including without limitation the rights");
            header.Add($"{CC}{CC} to use, copy, modify, merge, publish, distribute, sublicense, and / or sell");
            header.Add($"{CC}{CC} copies of the Software, and to permit persons to whom the Software is");
            header.Add($"{CC}{CC} furnished to do so, subject to the following conditions:");
            header.Add($"{CC}{CC} ");
            header.Add($"{CC}{CC} The above copyright notice and this permission notice shall be included in all");
            header.Add($"{CC}{CC} copies or substantial portions of the Software.");
            header.Add($"{CC}{CC} ");
            header.Add($"{CC}{CC} THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR");
            header.Add($"{CC}{CC} IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,");
            header.Add($"{CC}{CC} FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE");
            header.Add($"{CC}{CC} AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER");
            header.Add($"{CC}{CC} LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,");
            header.Add($"{CC}{CC} OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE");
            header.Add($"{CC}{CC} SOFTWARE.");
            header.Add("");

            return header;
        }
    }

}
