using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Drivers.Communication.Protocols
{
    class ABBCommunicationProtocol : Base
    {
        internal static readonly string MACHINA_SERVER_VERSION = "1.3.0";

        // A RAPID-code oriented API:
        //                                                      // INSTRUCTION P1 P2 P3 P4...
        internal const int INST_MOVEL = 1;                      // MoveL X Y Z QW QX QY QZ
        internal const int INST_MOVEJ = 2;                      // MoveJ X Y Z QW QX QY QZ
        internal const int INST_MOVEABSJ = 3;                   // MoveAbsJ J1 J2 J3 J4 J5 J6
        internal const int INST_SPEED = 4;                      // (setspeed V_TCP[V_ORI V_LEAX V_REAX])
        internal const int INST_ZONE = 5;                       // (setzone FINE TCP[ORI EAX ORI LEAX REAX])
        internal const int INST_WAITTIME = 6;                   // WaitTime T
        internal const int INST_TPWRITE = 7;                    // TPWrite "MSG"
        internal const int INST_TOOL = 8;                       // (settool X Y Z QW QX QY QZ KG CX CY CZ)
        internal const int INST_NOTOOL = 9;                     // (settool tool0)
        internal const int INST_SETDO = 10;                     // SetDO "NAME" ON
        internal const int INST_SETAO = 11;                     // SetAO "NAME" V
        internal const int INST_EXT_JOINTS = 12;                // (setextjoints a1 a2 a3 a4 a5 a6) --> send non-string 9E9 for inactive axes
        internal const int INST_ACCELERATION = 13;              // (setacceleration values, TBD)
        internal const int INST_SING_AREA = 14;                 // SingArea bool (sets Wrist or Off)

        internal const int RES_VERSION = 20;                    // ">20 1 2 1;" Sends version numbers
        internal const int RES_POSE = 21;                       // ">21 400 300 500 0 0 1 0;"
        internal const int RES_JOINTS = 22;                     // ">22 0 0 0 0 90 0;"
        internal const int RES_EXTAX = 23;                      // ">23 1000 9E9 9E9 9E9 9E9 9E9;" Sends external axes values
        internal const int RES_FULL_POSE = 24;                  // ">24 X Y Z QW QX QY QZ J1 J2 J3 J4 J5 J6 A1 A2 A3 A4 A5 A6;" Sends all pose and joint info (probably on split messages)


        // Characters used for buffer parsing
        internal const char STR_MESSAGE_END_CHAR = ';';       // Marks the end of a message
        internal const char STR_MESSAGE_CONTINUE_CHAR = '>';  // Marks the end of an unfinished message, to be continued on next message. Useful when the message is too long and needs to be split in chunks
        internal const char STR_MESSAGE_ID_CHAR = '@';        // Flags a message as an acknowledgment message corresponding to a source id
        internal const char STR_MESSAGE_RESPONSE_CHAR = '$';  // Flags a message as a response to an information request(acknowledgments do not include it)


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

            switch (action.Type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    //// MoveL/J X Y Z QW QX QY QZ
                    msgs.Add(string.Format("{0}{1} {2} {3} {4} {5} {6} {7} {8} {9}{10}",
                        STR_MESSAGE_ID_CHAR,
                        action.Id,
                        cursor.motionType == MotionType.Linear ? INST_MOVEL : INST_MOVEJ,
                        Math.Round(cursor.position.X, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(cursor.position.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(cursor.position.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(cursor.rotation.Q.W, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(cursor.rotation.Q.X, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(cursor.rotation.Q.Y, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(cursor.rotation.Q.Z, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        STR_MESSAGE_END_CHAR));
                    break;

                case ActionType.Axes:
                    // MoveAbsJ J1 J2 J3 J4 J5 J6
                    msgs.Add(string.Format("{0}{1} {2} {3} {4} {5} {6} {7} {8}{9}",
                        STR_MESSAGE_ID_CHAR,
                        action.Id,
                        INST_MOVEABSJ,
                        Math.Round(cursor.joints.J1, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.joints.J2, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.joints.J3, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.joints.J4, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.joints.J5, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.joints.J6, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        STR_MESSAGE_END_CHAR));
                    break;

                case ActionType.Speed:
                    // (setspeed V_TCP[V_ORI V_LEAX V_REAX])
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_SPEED} {cursor.speed}{STR_MESSAGE_END_CHAR}");  // this accepts more velocity params, but those are still not implemented in Machina... 
                    break;

                case ActionType.Acceleration:
                    Logger.Debug("Acceleration not implemented in ABBCommunicationProtocol");
                    break;

                case ActionType.Precision:
                    // (setzone FINE TCP[ORI EAX ORI LEAX REAX])
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_ZONE} {cursor.precision}{STR_MESSAGE_END_CHAR}");  // this accepts more zone params, but those are still not implemented in Machina... 
                    break;

                case ActionType.Wait:
                    // !WaitTime T
                    ActionWait aw = (ActionWait)action;
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_WAITTIME} {0.001 * aw.millis}{STR_MESSAGE_END_CHAR}");
                    break;

                case ActionType.Message:
                    // !TPWrite "MSG"
                    ActionMessage am = (ActionMessage)action;
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_TPWRITE} \"{am.message}\"{STR_MESSAGE_END_CHAR}");
                    break;

                case ActionType.AttachTool:
                    // !(settool X Y Z QW QX QY QZ KG CX CY CZ)
                    //ActionAttachTool aa = (ActionAttachTool)action;
                    //Tool t = aa.tool;

                    Tool t = cursor.tool;  // @TODO: should I just pull from the library? need to rethink the general approach: take info from cursor state (like motion actions) or action data...

                    msgs.Add(string.Format("{0}{1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}{14}",
                        STR_MESSAGE_ID_CHAR,
                        action.Id,
                        INST_TOOL,
                        Math.Round(t.TCPPosition.X, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.TCPPosition.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.TCPPosition.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.TCPOrientation.Q.W, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(t.TCPOrientation.Q.X, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(t.TCPOrientation.Q.Y, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(t.TCPOrientation.Q.Z, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(t.Weight, Geometry.STRING_ROUND_DECIMALS_KG),
                        Math.Round(t.CenterOfGravity.X, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.CenterOfGravity.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.CenterOfGravity.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                        STR_MESSAGE_END_CHAR));
                    break;

                case ActionType.DetachTool:
                    // !(settool0)
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_NOTOOL}{STR_MESSAGE_END_CHAR}");
                    break;

                case ActionType.IODigital:
                    // !SetDO "NAME" ON
                    ActionIODigital aiod = (ActionIODigital)action;
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_SETDO} \"{aiod.pinName}\" {(aiod.on ? 1 : 0)}{STR_MESSAGE_END_CHAR}");
                    break;

                case ActionType.IOAnalog:
                    // !SetAO "NAME" V
                    ActionIOAnalog aioa = (ActionIOAnalog)action;
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_SETAO} \"{aioa.pinName}\" {aioa.value}{STR_MESSAGE_END_CHAR}");
                    break;

                case ActionType.PushPop:
                    ActionPushPop app = action as ActionPushPop;
                    if (app.push)
                    {
                        return null;
                    }
                    else
                    {
                        // Only precision, speed and acceleration are states kept on the controller
                        Settings beforePop = cursor.settingsBuffer.SettingsBeforeLastPop;
                        if (beforePop.Speed != cursor.speed)
                        {
                            msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_SPEED} {cursor.speed}{STR_MESSAGE_END_CHAR}");
                        }
                        if (beforePop.Acceleration != cursor.acceleration)
                        {
                            msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_ACCELERATION} {cursor.acceleration}{STR_MESSAGE_END_CHAR}");
                        }
                        if (beforePop.Precision != cursor.precision)
                        {
                            msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_ZONE} {cursor.precision}{STR_MESSAGE_END_CHAR}");
                        }
                    }
                    break;

                case ActionType.Coordinates:
                    throw new NotImplementedException();  // @TODO: this should also change the WObj, but not on it yet...

                case ActionType.ExternalAxis:
                    string msg = $"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_EXT_JOINTS} ";

                    for (int i = 0; i < cursor.externalAxes.Length; i++)
                    {
                        // RAPID's StrToVal() will parse 9E9 into a 9E+9 num value, and ignore that axis on motions
                        msg += cursor.externalAxes[i]?.ToString() ?? "9E9";
                        if (i < cursor.externalAxes.Length - 1)
                        {
                            msg += " ";
                        }
                    }

                    msg += STR_MESSAGE_END_CHAR;

                    msgs.Add(msg);

                    break;

                // CustomCode --> is non-streamable

                // If the Action wasn't on the list above, it doesn't have a message representation...
                default:
                    return null;
            }

            return msgs;
        }

    }
}
