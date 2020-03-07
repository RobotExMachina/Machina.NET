using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machina.Descriptors.Cursors;

namespace Machina.Drivers.Communication.Protocols
{
    /// <summary>
    /// A base class representing a translator from abstract Machina Actions + Cursors
    /// to messages in the communication protocol used by the device's server/firmata.
    /// </summary>
    abstract class Base
    {
        /// <summary>
        /// Given a (write) cursor, apply next Action in the buffer and return a List of messages
        /// for the device's driver/firmata.
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        public List<string> GetMessagesForNextAction(RobotCursor cursor)
        {
            Action action;

            if (!cursor.ApplyNextAction(out action)) return null;  // cursor buffer is empty

            return GetActionMessages(action, cursor);  // this might be null if the action doesn't have a message representation here (e.g. PushSettings, Comment...)
        }

        /// <summary>
        /// Given a (write cursor, apply next Action in the buffer and return a byte[] representation
        /// of the message for the device's driver/firmata.
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        public virtual byte[] GetBytesForNextAction(RobotCursor cursor) => null;

        /// <summary>
        /// Given an Action and a RobotCursor representing the state of the robot after application, 
        /// return a List of strings with the messages necessary to perform this Action adhering to 
        /// the device's communication protocol.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cursor"></param>
        /// <returns></returns>
        internal virtual List<string> GetActionMessages(Action action, RobotCursor cursor) => null;
    }
    
}

