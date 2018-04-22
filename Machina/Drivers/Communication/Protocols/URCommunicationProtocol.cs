using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Drivers.Communication.Protocols
{
    class URCommunicationProtocol : Base
    {
        // This protocol basically sends the literal URScript line/s based on the action, 
        // and attaches an immediate socket response with the id to it, to keep track of runtime.
        // Not ideal, but SG is around the freaking corner! 

        // From the Machina_Server.mod file, must be consistent!
        internal static readonly char STR_MESSAGE_END_CHAR = ';';
        internal static readonly char STR_MESSAGE_ID_CHAR = '@';
        internal static readonly char STR_MESSAGE_RESPONSE_CHAR = '>';


        /// <summary>
        /// Given an Action and a RobotCursor representing the state of the robot after application, 
        /// return a List of strings with the messages necessary to perform this Action adhering to 
        /// the Machina-ABB-Server protocol.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cursor"></param>
        /// <returns></returns>
        internal override List<string> GetActionMessages(Action action, RobotCursor cursor)
        {
            List<string> msgs = new List<string>();
            string dec;
            if (!CompilerUR.GenerateInstructionDeclaration(action, cursor, false, out dec))
                return null;

            // The message type in the response is currently ignored by Machina, but let's send a zero anyway to not break string splitting and other checks... 
            string res = $"  socket_send_string(\"{STR_MESSAGE_ID_CHAR}{action.id} 0{STR_MESSAGE_END_CHAR}\")";

            msgs.Add(dec);
            msgs.Add(res);
            return msgs;
        }

    }
}
