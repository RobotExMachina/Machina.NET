using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Drivers.Communication.Protocols
{
    class ABBCommunicationProtocol : Base
    {
        // From the Machina_Server.mod file, must be consistent!
        internal static readonly char STR_MESSAGE_END_CHAR = ';';
        internal static readonly char STR_MESSAGE_ID_CHAR = '@';
        internal static readonly char STR_MESSAGE_RESPONSE_CHAR = '>';

        // A RAPID-code oriented API:
        //                                     // INSTRUCTION P1 P2 P3 P4...
        const int INST_MOVEL = 1;              // MoveL X Y Z QW QX QY QZ
        const int INST_MOVEJ = 2;              // MoveJ X Y Z QW QX QY QZ
        const int INST_MOVEABSJ = 3;           // MoveAbsJ J1 J2 J3 J4 J5 J6
        const int INST_SPEED = 4;              // (setspeed V_TCP[V_ORI V_LEAX V_REAX])
        const int INST_ZONE = 5;               // (setzone FINE TCP[ORI EAX ORI LEAX REAX])
        const int INST_WAITTIME = 6;           // WaitTime T
        const int INST_TPWRITE = 7;            // TPWrite "MSG"
        const int INST_TOOL = 8;               // (settool X Y Z QW QX QY QZ KG CX CY CZ)
        const int INST_NOTOOL = 9;             // (settool tool0)
        const int INST_SETDO = 10;             // SetDO "NAME" ON
        const int INST_SETAO = 11;             // SetAO "NAME" V
        const int INST_EXT_JOINTS = 12;        // (setextjoints a1 a2 a3 a4 a5 a6) --> send non-string 9E9 for inactive axes


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

            switch (action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    //// MoveL/J X Y Z QW QX QY QZ
                    msgs.Add(string.Format("{0}{1} {2} {3} {4} {5} {6} {7} {8} {9}{10}",
                        STR_MESSAGE_ID_CHAR,
                        action.id,
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
                        action.id,
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
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.id} {INST_SPEED} {cursor.speed}{STR_MESSAGE_END_CHAR}");  // this accepts more velocity params, but those are still not implemented in Machina... 
                    break;

                case ActionType.Precision:
                    // (setzone FINE TCP[ORI EAX ORI LEAX REAX])
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.id} {INST_ZONE} {cursor.precision}{STR_MESSAGE_END_CHAR}");  // this accepts more zone params, but those are still not implemented in Machina... 
                    break;

                case ActionType.Wait:
                    // !WaitTime T
                    ActionWait aw = (ActionWait)action;
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.id} {INST_WAITTIME} {0.001 * aw.millis}{STR_MESSAGE_END_CHAR}");
                    break;

                case ActionType.Message:
                    // !TPWrite "MSG"
                    ActionMessage am = (ActionMessage)action;
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.id} {INST_TPWRITE} \"{am.message}\"{STR_MESSAGE_END_CHAR}");
                    break;

                case ActionType.Attach:
                    // !(settool X Y Z QW QX QY QZ KG CX CY CZ)
                    ActionAttach aa = (ActionAttach)action;
                    Tool t = aa.tool;

                    msgs.Add(string.Format("{0}{1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}{14}",
                        STR_MESSAGE_ID_CHAR,
                        action.id,
                        INST_TOOL,
                        Math.Round(t.TCPPosition.X, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.TCPPosition.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.TCPPosition.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.TCPOrientation.Q.W, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(t.TCPOrientation.Q.X, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(t.TCPOrientation.Q.Y, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(t.TCPOrientation.Q.Z, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(t.Weight, Geometry.STRING_ROUND_DECIMALS_KG),
                        Math.Round(t.centerOfGravity.X, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.centerOfGravity.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.centerOfGravity.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                        STR_MESSAGE_END_CHAR));
                    break;

                case ActionType.Detach:
                    // !(settool0)
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.id} {INST_NOTOOL}{STR_MESSAGE_END_CHAR}");
                    break;

                case ActionType.IODigital:
                    // !SetDO "NAME" ON
                    ActionIODigital aiod = (ActionIODigital)action;
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.id} {INST_SETDO} \"{aiod.pinName}\" {(aiod.on ? 1 : 0)}{STR_MESSAGE_END_CHAR}");
                    break;

                case ActionType.IOAnalog:
                    // !SetAO "NAME" V
                    ActionIOAnalog aioa = (ActionIOAnalog)action;
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.id} {INST_SETAO} \"{aioa.pinName}\" {aioa.value}{STR_MESSAGE_END_CHAR}");
                    break;

                case ActionType.PushPop:
                    ActionPushPop app = action as ActionPushPop;
                    if (app.push)
                    {
                        return null;
                    }
                    else
                    {
                        // Only precision and speed are states kept on the controller
                        Settings beforePop = cursor.settingsBuffer.SettingsBeforeLastPop;
                        if (beforePop.Speed != cursor.speed)
                        {
                            msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.id} {INST_SPEED} {cursor.speed}{STR_MESSAGE_END_CHAR}");
                        }
                        if (beforePop.Precision != cursor.precision)
                        {
                            msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.id} {INST_ZONE} {cursor.precision}{STR_MESSAGE_END_CHAR}");
                        }
                    }
                    break;

                case ActionType.Coordinates:
                    throw new NotImplementedException();  // @TODO: this should also change the WObj, but not on it yet...

                case ActionType.ExternalAxes:
                    ActionExternalAxes aea = action as ActionExternalAxes;

                    string msg = $"{STR_MESSAGE_ID_CHAR}{aea.id} {INST_EXT_JOINTS} ";

                    for (int i = 0; i < aea.externalAxes.Length; i++)
                    {
                        // RAPID's StrToVal() will parse 9E9 into a 9E+9 num value, and ignore that axis on motions
                        msg += aea.externalAxes[i] == null ? "9E9" : aea.externalAxes[i].ToString();
                        if (i < aea.externalAxes.Length - 1)
                        {
                            msg += " ";
                        }
                    }

                    msg += STR_MESSAGE_END_CHAR;

                    msgs.Add(msg);

                    break;

                // If the Action wasn't on the list above, it doesn't have a message representation...
                default:
                    return null;
            }

            return msgs;
        }

    }
}
