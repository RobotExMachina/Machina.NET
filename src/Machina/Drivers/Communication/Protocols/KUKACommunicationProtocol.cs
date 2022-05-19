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
    class KUKACommunicationProtocol : Base
    {
        internal static readonly string MACHINA_SERVER_VERSION = "0.1";

        // A KUKA-KRL-code oriented API:
        //                                                      // INSTRUCTION: V1 V2 V3 V4...
        internal const int Linear_MotionMode = 0;                  // PTP or LIN motion: X Y Z A B C
        internal const int PTP_MotionMode = 1;                  // PTP or LIN motion: X Y Z A B C
        internal const int Default_Value = 0;                  // PTP or LIN motion: X Y Z A B C
        internal const int Message_Max_Length = 80;                  // PTP or LIN motion: X Y Z A B C
        internal const int Message_Notification = 1;                  // PTP or LIN motion: X Y Z A B C
        internal const int Message_Warning = 2;                  // PTP or LIN motion: X Y Z A B C
        internal const int Message_Critical_Stop = 3;                  // PTP or LIN motion: X Y Z A B C
        internal const int Message_State = 4;                  // PTP or LIN motion: X Y Z A B C
        internal const int Detach_Tool = 16;
        internal const double Speed_MMS_To_MS = 0.001;

        internal const int INST_TRANSFORM = 3;                  // PTP or LIN motion: X Y Z A B C
        internal const int INST_AXES = 4;                       // PTP: A1 A2 A3 A4 A5 A6

        internal const int INST_PTP = 20;
        internal const int INST_LIN = 21;

        private string Sending_Message = "";

        // @TODO: fill in the API of instruction codes...


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

            //// TEMP: this needs to be streamlined...
            //string xml = "<DT><DC>1</DC><DR>";
            string xml = "";


            switch (action.Type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:

                    YawPitchRoll euler = cursor.rotation.Q.ToYawPitchRoll();  // @TODO: does this actually work...?

                    // PTP/LIN X Y Z A B C 
                    xml += string.Format(CultureInfo.InvariantCulture,
                        "<A ID=\"{0}\" T=\"{1}\" V1=\"{2}\" V2=\"{3}\" V3=\"{4}\" V4=\"{5}\" V5=\"{6}\" V6=\"{7}\" V7=\"{8}\"/>",
                        action.Id,
                        (int) action.Type,
                        cursor.motionType == MotionType.Linear ? Linear_MotionMode : PTP_MotionMode,
                        Math.Round(cursor.position.X, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(cursor.position.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(cursor.position.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                        // note reversed ZYX order
                        Math.Round(euler.ZAngle, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(euler.YAngle, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(euler.XAngle, Geometry.STRING_ROUND_DECIMALS_DEGS));
                    break;

                // @TODO: complete rest of actions

                case ActionType.Axes:
                    // PTP A1 A2 A3 A4 A5 A6 C_PTP
                    xml += string.Format(CultureInfo.InvariantCulture,
                        "<A ID=\"{0}\" T=\"{1}\" V1=\"{2}\" V2=\"{3}\" V3=\"{4}\" V4=\"{5}\" V5=\"{6}\" V6=\"{7}\" V7=\"{8}\"/>",
                        action.Id,
                        (int)action.Type,
                        Default_Value,  // the frame values should be shifted to start on V1! 
                        Math.Round(cursor.axes.J1, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.axes.J2, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.axes.J3, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.axes.J4, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.axes.J5, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.axes.J6, Geometry.STRING_ROUND_DECIMALS_DEGS));
                    break;

                case ActionType.Message:

                    ActionMessage am = (ActionMessage)action;
                    Sending_Message = am.message;

                    int stringLength = Sending_Message.Length;
                    if (stringLength > Message_Max_Length)
                    {
                        stringLength = Message_Max_Length; // The maximum character length of a message to send to a KUKA robot is 80 characters
                    }
                    Sending_Message = Sending_Message.Substring(0, stringLength);


                    xml += string.Format(CultureInfo.InvariantCulture,
                        "<A ID=\"{0}\" T=\"{1}\" V1=\"{2}\" V2=\"{3}\" V3=\"{4}\" V4=\"{5}\" V5=\"{6}\" V6=\"{7}\" V7=\"{8}\"/>",
                        action.Id,
                        (int)action.Type,
                        Message_Notification,  // the frame values should be shifted to start on V1! 
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value);

                    break;

                case ActionType.Wait:
                    // WaitTime T
                    ActionWait aw = (ActionWait)action;

                    xml += string.Format(CultureInfo.InvariantCulture,
                        "<A ID=\"{0}\" T=\"{1}\" V1=\"{2}\" V2=\"{3}\" V3=\"{4}\" V4=\"{5}\" V5=\"{6}\" V6=\"{7}\" V7=\"{8}\"/>",
                        action.Id,
                        (int)action.Type,
                        0.001 * aw.millis,  // the frame values should be shifted to start on V1! 
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value);

                    break;

                case ActionType.Speed:

                    xml += string.Format(CultureInfo.InvariantCulture,
                        "<A ID=\"{0}\" T=\"{1}\" V1=\"{2}\" V2=\"{3}\" V3=\"{4}\" V4=\"{5}\" V5=\"{6}\" V6=\"{7}\" V7=\"{8}\"/>",
                        action.Id,
                        (int)action.Type,
                        cursor.speed * Speed_MMS_To_MS,  // the frame values should be shifted to start on V1! 
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value);

                    break;

                case ActionType.Acceleration:

                    xml += string.Format(CultureInfo.InvariantCulture,
                        "<A ID=\"{0}\" T=\"{1}\" V1=\"{2}\" V2=\"{3}\" V3=\"{4}\" V4=\"{5}\" V5=\"{6}\" V6=\"{7}\" V7=\"{8}\"/>",
                        action.Id,
                        (int)action.Type,
                        cursor.acceleration * Speed_MMS_To_MS,  // the frame values should be shifted to start on V1! 
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value);

                    break;

                case ActionType.Precision:

                    xml += string.Format(CultureInfo.InvariantCulture,
                        "<A ID=\"{0}\" T=\"{1}\" V1=\"{2}\" V2=\"{3}\" V3=\"{4}\" V4=\"{5}\" V5=\"{6}\" V6=\"{7}\" V7=\"{8}\"/>",
                        action.Id,
                        (int)action.Type,
                        cursor.precision,  // the frame values should be shifted to start on V1! 
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value);

                    break;


                case ActionType.DefineTool:
                    // Add a log here to avoid a confusing default warning.
                    Logger.Verbose("`DefineTool()` doesn't need to be streamed.");
                    xml += string.Format(CultureInfo.InvariantCulture,
                        "<A ID=\"{0}\" T=\"{1}\" V1=\"{2}\" V2=\"{3}\" V3=\"{4}\" V4=\"{5}\" V5=\"{6}\" V6=\"{7}\" V7=\"{8}\"/>",
                        action.Id,
                        (int)action.Type,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value,
                        Default_Value);
                    break;
                    break;

                case ActionType.AttachTool:
                    // !(settool X Y Z QW QX QY QZ KG CX CY CZ)
                    Tool t = cursor.tool;  // @TODO: should I just pull from the library? need to rethink the general approach: take info from cursor state (like motion actions) or action data...
                    euler = cursor.tool.TCPOrientation.ToQuaternion().ToYawPitchRoll();
                    // PTP/LIN X Y Z A B C 
                    xml += string.Format(CultureInfo.InvariantCulture,
                        "<A ID=\"{0}\" T=\"{1}\" V1=\"{2}\" V2=\"{3}\" V3=\"{4}\" V4=\"{5}\" V5=\"{6}\" V6=\"{7}\" V7=\"{8}\"/>",
                        action.Id,
                        (int)action.Type,
                        Default_Value,
                        Math.Round(t.TCPPosition.X, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.TCPPosition.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.TCPPosition.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                        // note reversed ZYX order
                        Math.Round(euler.ZAngle, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(euler.YAngle, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(euler.XAngle, Geometry.STRING_ROUND_DECIMALS_DEGS));
                    break;

                case ActionType.DetachTool:

                    xml += string.Format(CultureInfo.InvariantCulture,
                       "<A ID=\"{0}\" T=\"{1}\" V1=\"{2}\" V2=\"{3}\" V3=\"{4}\" V4=\"{5}\" V5=\"{6}\" V6=\"{7}\" V7=\"{8}\"/>",
                       action.Id,
                       (int)action.Type,
                       Detach_Tool,  // the frame values should be shifted to start on V1! 
                       Default_Value,
                       Default_Value,
                       Default_Value,
                       Default_Value,
                       Default_Value,
                       Default_Value);

                    break;

                case ActionType.IODigital:

                    // !SetDO "NAME" ON
                    ActionIODigital aiod = (ActionIODigital)action;

                    xml += string.Format(CultureInfo.InvariantCulture,
                       "<A ID=\"{0}\" T=\"{1}\" V1=\"{2}\" V2=\"{3}\" V3=\"{4}\" V4=\"{5}\" V5=\"{6}\" V6=\"{7}\" V7=\"{8}\"/>",
                       action.Id,
                       (int)action.Type,
                       aiod.pinName,  // the frame values should be shifted to start on V1! 
                       aiod.on ? 1 : 0,
                       Default_Value,
                       Default_Value,
                       Default_Value,
                       Default_Value,
                       Default_Value);

                    break;

                //case ActionType.IOAnalog:
                //    // !SetAO "NAME" V
                //    ActionIOAnalog aioa = (ActionIOAnalog)action;
                //    msgs.Add($"{STR_MESSAGE_ID_CHAR}{action.Id} {INST_SETAO} \"{aioa.pinName}\" {aioa.value}{STR_MESSAGE_END_CHAR}");
                //    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                //        "{0}{1} {2} \"{3}\" {4}{5}",
                //        STR_MESSAGE_ID_CHAR,
                //        action.Id,
                //        INST_SETAO,
                //        aioa.pinName,
                //        aioa.value,
                //        STR_MESSAGE_END_CHAR));
                //    break;

                //case ActionType.PushPop:
                //    ActionPushPop app = action as ActionPushPop;
                //    if (app.push)
                //    {
                //        return null;
                //    }
                //    else
                //    {
                //        // Only precision, speed and acceleration are states kept on the controller
                //        Settings beforePop = cursor.settingsBuffer.SettingsBeforeLastPop;
                //        if (beforePop.Speed != cursor.speed)
                //        {
                //            msgs.Add(string.Format(CultureInfo.InvariantCulture,
                //                "{0}{1} {2} {3}{4}",
                //                STR_MESSAGE_ID_CHAR,
                //                action.Id,
                //                INST_SPEED,
                //                cursor.speed,
                //                STR_MESSAGE_END_CHAR));
                //        }
                //        if (beforePop.Acceleration != cursor.acceleration)
                //        {
                //            msgs.Add(string.Format(CultureInfo.InvariantCulture,
                //                "{0}{1} {2} {3}{4}",
                //                STR_MESSAGE_ID_CHAR,
                //                action.Id,
                //                INST_ACCELERATION,
                //                cursor.acceleration,
                //                STR_MESSAGE_END_CHAR));
                //        }
                //        if (beforePop.Precision != cursor.precision)
                //        {
                //            msgs.Add(string.Format(CultureInfo.InvariantCulture,
                //                "{0}{1} {2} {3}{4}",
                //                STR_MESSAGE_ID_CHAR,
                //                action.Id,
                //                INST_ZONE,
                //                cursor.precision,
                //                STR_MESSAGE_END_CHAR));
                //        }
                //    }
                //    break;

                //case ActionType.Coordinates:
                //    throw new NotImplementedException();  // @TODO: this should also change the WObj, but not on it yet...

                //case ActionType.ExternalAxis:
                //    string @params, id;
                //    ActionExternalAxis aea = action as ActionExternalAxis;

                //    // Cartesian msg
                //    if (aea.target == ExternalAxesTarget.All || aea.target == ExternalAxesTarget.Cartesian)
                //    {
                //        if (aea.target == ExternalAxesTarget.All)
                //        {
                //            id = "0";  // If will need to send two messages, only add real id to the last one.
                //        }
                //        else
                //        {
                //            id = aea.Id.ToString();
                //        }

                //        @params = ExternalAxesToParameters(cursor.externalAxesCartesian);

                //        msgs.Add(string.Format(CultureInfo.InvariantCulture,
                //            "{0}{1} {2} {3}{4}",
                //            STR_MESSAGE_ID_CHAR,
                //            id,
                //            INST_EXT_JOINTS_ROBTARGET,
                //            @params,
                //            STR_MESSAGE_END_CHAR));
                //    }

                //    // Joints msg
                //    if (aea.target == ExternalAxesTarget.All || aea.target == ExternalAxesTarget.Joint)
                //    {
                //        id = aea.Id.ToString();  // now use the real id in any case

                //        @params = ExternalAxesToParameters(cursor.externalAxesJoints);

                //        msgs.Add(string.Format(CultureInfo.InvariantCulture,
                //            "{0}{1} {2} {3}{4}",
                //            STR_MESSAGE_ID_CHAR,
                //            id,
                //            INST_EXT_JOINTS_JOINTTARGET,
                //            @params,
                //            STR_MESSAGE_END_CHAR));
                //    }

                //    break;

                //case ActionType.ArmAngle:
                //    // Send a request to change only the robtarget portion of the external axes for next motion
                //    ActionArmAngle aaa = action as ActionArmAngle;
                //    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                //        "{0}{1} {2} {3} 9E9 9E9 9E9 9E9 9E9{4}",
                //        STR_MESSAGE_ID_CHAR,
                //        action.Id,
                //        INST_EXT_JOINTS_ROBTARGET,
                //        Math.Round(aaa.angle, Geometry.STRING_ROUND_DECIMALS_DEGS),
                //        STR_MESSAGE_END_CHAR));
                //    break;

                //// When connected live, this is used as a way to stream a message directly to the robot. Unsafe, but oh well...
                //// ~~@TODO: should the protocol add the action id, or is this also left to the user to handle?~~
                //// --> Let's add the action id and statement terminator for the moment (using this mainly for Yumi gripping)
                //case ActionType.CustomCode:
                //    ActionCustomCode acc = action as ActionCustomCode;
                //    msgs.Add(string.Format(CultureInfo.InvariantCulture,
                //        "{0}{1} {2}{3}",
                //        STR_MESSAGE_ID_CHAR,
                //        acc.Id,
                //        acc.statement,
                //        STR_MESSAGE_END_CHAR));
                //    break;

                // If the Action wasn't on the list above, it doesn't have a message representation...
                default:
                    Logger.Debug("Action `" + action + "` is not streamable...");
                    return null;
            }

           // Close the XML
           //xml += "</DR><Msg><Str M01=\"" + Sending_Message + "\"/><Con>1</Con></Msg></DT>";

            msgs.Add(xml);

            return msgs;
        }


    }
}
