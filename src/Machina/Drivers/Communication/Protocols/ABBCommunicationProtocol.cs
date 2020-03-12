using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.FormattableString;
using Machina.Types.Geometry;


namespace Machina.Drivers.Communication.Protocols
{
    class ABBCommunicationProtocol : Base
    {
        internal static readonly string MACHINA_SERVER_VERSION = "1.4.0";

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
        internal const int INST_NO_TOOL = 9;                    // (settool tool0)
        internal const int INST_SETDO = 10;                     // SetDO "NAME" ON
        internal const int INST_SETAO = 11;                     // SetAO "NAME" V
        internal const int INST_EXT_JOINTS_ALL = 12;            // (setextjoints a1 a2 a3 a4 a5 a6) --> send non-string 9E9 for inactive axes
        internal const int INST_ACCELERATION = 13;              // WorldAccLim \On V (V = 0 sets \Off, any other value sets WorldAccLim \On V)
        internal const int INST_SING_AREA = 14;                 // SingArea bool (sets Wrist or Off)
        internal const int INST_EXT_JOINTS_ROBTARGET = 15;      // (setextjoints a1 a2 a3 a4 a5 a6, applies only to robtarget)
        internal const int INST_EXT_JOINTS_JOINTTARGET = 16;    // (setextjoints a1 a2 a3 a4 a5 a6, applies only to robtarget)
        internal const int INST_CUSTOM_ACTION = 17;             // This is a wildcard for custom user functions that do not really fit in the Machina API (mainly Yumi gripping right now)
        internal const int INST_WOBJ = 18;                      // (setwobj X Y Z QW QX QY QZ) --> defaults to not robot not holding wobj, fixed user coordinate system(ucs), and ucs == worldcs
        internal const int INST_NO_WOBJ = 19;                   // (setwobj wobj0)

        internal const int RES_VERSION = 20;                    // ">20 1 2 1;" Sends version numbers
        internal const int RES_POSE = 21;                       // ">21 400 300 500 0 0 1 0;"
        internal const int RES_JOINTS = 22;                     // ">22 0 0 0 0 90 0;"
        internal const int RES_EXTAX = 23;                      // ">23 1000 9E9 9E9 9E9 9E9 9E9;" Sends external axes values
        internal const int RES_FULL_POSE = 24;                  // ">24 X Y Z QW QX QY QZ J1 J2 J3 J4 J5 J6 A1 A2 A3 A4 A5 A6;" Sends all pose and joint info (probably on split messages)


        // Characters used for buffer parsing
        internal const char STR_MESSAGE_END_CHAR = ';';         // Marks the end of a message
        internal const char STR_MESSAGE_CONTINUE_CHAR = '>';    // Marks the end of an unfinished message, to be continued on next message. Useful when the message is too long and needs to be split in chunks
        internal const char STR_MESSAGE_ID_CHAR = '@';          // Flags a message as an acknowledgment message corresponding to a source id
        internal const char STR_MESSAGE_RESPONSE_CHAR = '$';    // Flags a message as a response to an information request(acknowledgments do not include it)


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
                    // MoveL/J X Y Z QW QX QY QZ
                    msgs.Add(string.Format(CultureInfo.InvariantCulture, 
                        "{0}{1} {2} {3} {4} {5} {6} {7} {8} {9}{10}",
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
                    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0}{1} {2} {3} {4} {5} {6} {7} {8}{9}",
                        STR_MESSAGE_ID_CHAR,
                        action.Id,
                        INST_MOVEABSJ,
                        Math.Round(cursor.axes.J1, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.axes.J2, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.axes.J3, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.axes.J4, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.axes.J5, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.axes.J6, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        STR_MESSAGE_END_CHAR));
                    break;

                case ActionType.Speed:
                    // (setspeed V_TCP[V_ORI V_LEAX V_REAX]) --> this accepts more velocity params, but those are still not implemented in Machina... 
                    msgs.Add(string.Format(CultureInfo.InvariantCulture,  
                        "{0}{1} {2} {3}{4}",
                        STR_MESSAGE_ID_CHAR,
                        action.Id,
                        INST_SPEED,
                        cursor.speed,
                        STR_MESSAGE_END_CHAR));
                    break;

                case ActionType.Acceleration:
                    // WorldAccLim \On V (V = 0 sets \Off, any other value sets WorldAccLim \On V)
                    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0}{1} {2} {3}{4}",
                        STR_MESSAGE_ID_CHAR,
                        action.Id,
                        INST_ACCELERATION,
                        cursor.acceleration,
                        STR_MESSAGE_END_CHAR));
                    break;

                case ActionType.Precision:
                    // (setzone FINE TCP[ORI EAX ORI LEAX REAX])  --> this accepts more zone params, but those are still not implemented in Machina... 
                    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0}{1} {2} {3}{4}",
                        STR_MESSAGE_ID_CHAR,
                        action.Id,
                        INST_ZONE,
                        cursor.precision,
                        STR_MESSAGE_END_CHAR));
                    break;

                case ActionType.Wait:
                    // WaitTime T
                    ActionWait aw = (ActionWait)action;
                    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0}{1} {2} {3}{4}",
                        STR_MESSAGE_ID_CHAR,
                        action.Id,
                        INST_WAITTIME,
                        0.001 * aw.millis,
                        STR_MESSAGE_END_CHAR));
                    break;

                case ActionType.Message:
                    // TPWrite "MSG"
                    ActionMessage am = (ActionMessage)action;
                    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0}{1} {2} \"{3}\"{4}",
                        STR_MESSAGE_ID_CHAR,
                        action.Id,
                        INST_TPWRITE,
                        am.message,
                        STR_MESSAGE_END_CHAR));
                    break;

                case ActionType.DefineTool:
                    // Add a log here to avoid a confusing default warning.
                    Logger.Verbose("`DefineTool()` doesn't need to be streamed.");
                    break;

                case ActionType.AttachTool:
                    // !(settool X Y Z QW QX QY QZ KG CX CY CZ)
                    Tool t = cursor.tool;  // @TODO: should I just pull from the library? need to rethink the general approach: take info from cursor state (like motion actions) or action data...

                    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0}{1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}{14}",
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

                        //// Was getting messages longer than 80 chars, temporal worksround
                        //Math.Round(t.TCPPosition.X, 1),
                        //Math.Round(t.TCPPosition.Y, 1),
                        //Math.Round(t.TCPPosition.Z, 1),
                        //Math.Round(t.TCPOrientation.Q.W, 3),
                        //Math.Round(t.TCPOrientation.Q.X, 3),
                        //Math.Round(t.TCPOrientation.Q.Y, 3),
                        //Math.Round(t.TCPOrientation.Q.Z, 3),
                        //Math.Round(t.Weight, 1),
                        //Math.Round(t.CenterOfGravity.X, 1),
                        //Math.Round(t.CenterOfGravity.Y, 1),
                        //Math.Round(t.CenterOfGravity.Z, 1),

                        STR_MESSAGE_END_CHAR));
                    break;

                case ActionType.DetachTool:
                    // !(settool0)
                    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0}{1} {2}{3}",
                        STR_MESSAGE_ID_CHAR,
                        action.Id,
                        INST_NO_TOOL,
                        STR_MESSAGE_END_CHAR));
                    break;

                case ActionType.IODigital:
                    // !SetDO "NAME" ON
                    ActionIODigital aiod = (ActionIODigital)action;
                    //msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_SETDO} \"{aiod.pinName}\" {(aiod.on ? 1 : 0)}{STR_MESSAGE_END_CHAR}");
                    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0}{1} {2} \"{3}\" {4}{5}",
                        STR_MESSAGE_ID_CHAR,
                        action.Id,
                        INST_SETDO,
                        aiod.pinName,
                        aiod.on ? 1 : 0,
                        STR_MESSAGE_END_CHAR));
                    break;

                case ActionType.IOAnalog:
                    // !SetAO "NAME" V
                    ActionIOAnalog aioa = (ActionIOAnalog)action;
                    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_SETAO} \"{aioa.pinName}\" {aioa.value}{STR_MESSAGE_END_CHAR}");
                    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0}{1} {2} \"{3}\" {4}{5}",
                        STR_MESSAGE_ID_CHAR,
                        action.Id,
                        INST_SETAO,
                        aioa.pinName,
                        aioa.value,
                        STR_MESSAGE_END_CHAR));
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
                            msgs.Add(string.Format(CultureInfo.InvariantCulture,
                                "{0}{1} {2} {3}{4}",
                                STR_MESSAGE_ID_CHAR,
                                action.Id,
                                INST_SPEED,
                                cursor.speed,
                                STR_MESSAGE_END_CHAR));
                        }
                        if (beforePop.Acceleration != cursor.acceleration)
                        {
                            msgs.Add(string.Format(CultureInfo.InvariantCulture,
                                "{0}{1} {2} {3}{4}",
                                STR_MESSAGE_ID_CHAR,
                                action.Id,
                                INST_ACCELERATION,
                                cursor.acceleration,
                                STR_MESSAGE_END_CHAR));
                        }
                        if (beforePop.Precision != cursor.precision)
                        {
                            msgs.Add(string.Format(CultureInfo.InvariantCulture,
                                "{0}{1} {2} {3}{4}",
                                STR_MESSAGE_ID_CHAR,
                                action.Id,
                                INST_ZONE,
                                cursor.precision,
                                STR_MESSAGE_END_CHAR));
                        }
                    }
                    break;

                case ActionType.Coordinates:
                    throw new NotImplementedException();  // @TODO: this should also change the WObj, but not on it yet...

                case ActionType.ExternalAxis:
                    string @params, id;
                    ActionExternalAxis aea = action as ActionExternalAxis;
                    
                    // Cartesian msg
                    if (aea.target == ExternalAxesTarget.All || aea.target == ExternalAxesTarget.Cartesian)
                    {
                        if (aea.target == ExternalAxesTarget.All)
                        {
                            id = "0";  // If will need to send two messages, only add real id to the last one.
                        }
                        else
                        {
                            id = aea.Id.ToString();
                        }

                        @params = ExternalAxesToParameters(cursor.externalAxesCartesian);

                        msgs.Add(string.Format(CultureInfo.InvariantCulture,
                            "{0}{1} {2} {3}{4}",
                            STR_MESSAGE_ID_CHAR,
                            id,
                            INST_EXT_JOINTS_ROBTARGET,
                            @params,
                            STR_MESSAGE_END_CHAR));
                    }

                    // Joints msg
                    if (aea.target == ExternalAxesTarget.All || aea.target == ExternalAxesTarget.Joint)
                    {
                        id = aea.Id.ToString();  // now use the real id in any case

                        @params = ExternalAxesToParameters(cursor.externalAxesJoints);

                        msgs.Add(string.Format(CultureInfo.InvariantCulture,
                            "{0}{1} {2} {3}{4}",
                            STR_MESSAGE_ID_CHAR,
                            id,
                            INST_EXT_JOINTS_JOINTTARGET,
                            @params,
                            STR_MESSAGE_END_CHAR));
                    }
                    
                    break;

                case ActionType.ArmAngle:
                    // Send a request to change only the robtarget portion of the external axes for next motion
                    ActionArmAngle aaa = action as ActionArmAngle;
                    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0}{1} {2} {3} 9E9 9E9 9E9 9E9 9E9{4}",
                        STR_MESSAGE_ID_CHAR,
                        action.Id,
                        INST_EXT_JOINTS_ROBTARGET,
                        Math.Round(aaa.angle, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        STR_MESSAGE_END_CHAR));
                    break;

                // When connected live, this is used as a way to stream a message directly to the robot. Unsafe, but oh well...
                // ~~@TODO: should the protocol add the action id, or is this also left to the user to handle?~~
                // --> Let's add the action id and statement terminator for the moment (using this mainly for Yumi gripping)
                case ActionType.CustomCode:
                    ActionCustomCode acc = action as ActionCustomCode;
                    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0}{1} {2}{3}",
                        STR_MESSAGE_ID_CHAR,
                        acc.Id,
                        acc.statement,
                        STR_MESSAGE_END_CHAR));
                    break;
                    
                // If the Action wasn't on the list above, it doesn't have a message representation...
                default:
                    Logger.Debug("Action `" + action + "` is not streamable...");
                    return null;
            }

            return msgs;
        }

        internal static string ExternalAxesToParameters(ExternalAxes extax)
        {
            string param = "";
            for (int i = 0; i < extax.Length; i++)
            {
                if (extax[i] == null)
                {
                    // RAPID's StrToVal() will parse 9E9 into a 9E+9 num value, and ignore that axis on motions
                    param += "9E9";
                }
                else
                {
                    param += Math.Round((double) extax[i], Geometry.STRING_ROUND_DECIMALS_MM)
                        .ToString(CultureInfo.InvariantCulture);
                }

                if (i < extax.Length - 1)
                {
                    param += " ";
                }
            }

            return param;
        }

    }
}
