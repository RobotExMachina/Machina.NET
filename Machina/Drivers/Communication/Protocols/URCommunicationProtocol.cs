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

        // Not implemented yet
        internal static readonly char STR_MESSAGE_END_CHAR = ';';
        internal static readonly char STR_MESSAGE_ID_CHAR = '@';
        internal static readonly char STR_MESSAGE_RESPONSE_CHAR = '>';

        // Instruction data will be sent to the socket in the form of 32 signed integers. 
        // To allow for float precision, the original values must be 'puffed' by these factors. 
        // This works de facto as the maximum precision for unit value types.
        const double FACTOR_M = 10000.0;
        const double FACTOR_RAD = 10000.0;
        const double FACTOR_SEC = 1000.0;
        const double FACTOR_KG = 1000.0;
        const double FACTOR_VOLT = 1000000.0;

        // Instruction codes. 
        // Instruction buffers start with an ID that will be sent back on the acknowledgement response (use -1 if not interested),
        // then a numeric code which determines the instruction to perform, and variable number of parameters for the instruction.
        // Please note that, with the exception of strings (WIP), all parameters must be integers that have been premultiplied by
        // their corresponding unit factor (see above).
        // INCOMING BUFFER:
        const int INST_MOVEL = 1;                   // [ID, CODE, X, Y, Z, RX, RY, RZ] (in (int) M * FACTOR_M, RAD * FACTOR_RAD)
        const int INST_MOVEJ_P = 2;                 // [ID, CODE, X, Y, Z, RX, RY, RZ] (in (int) M * FACTOR_M, RAD * FACTOR_RAD)
        const int INST_MOVEJ_Q = 3;                 // [ID, CODE, J1, J2, J3, J4, J5, J6] (in (int) RAD * FACTOR_RAD)
        const int INST_TCP_SPEED = 4;               // [ID, CODE, VEL] (in (int) M/S * FACTOR_M)
        const int INST_TCP_ACC = 5;                 // [ID, CODE, ACC] (in (int) M/S^2 * FACTOR_M)
        const int INST_Q_SPEED = 6;                 // [ID, CODE, VEL] (in (int) RAD/S * FACTOR_RAD)
        const int INST_Q_ACC = 7;                   // [ID, CODE, ACC] (in (int) RAD/S^2 * FACTOR_RAD)
        const int INST_BLEND = 8;                   // [ID, CODE, RADIUS] (in (int) M * FACTOR_M)
        const int INST_SLEEP = 9;                   // [ID, CODE, TIME] (in (int) S * FACTOR_SEC)
        //   const int INST_TEXTMSG = 10                 // [ID, CODE, MSG] (in (string) "msg" + STR_MESSAGE_END_CHAR) (NOT IMPLEMENTED)
        //   const int INST_POPUP = 11                   // [ID, CODE, MSG] (in (string) "msg" + STR_MESSAGE_END_CHAR) (NOT IMPLEMENTED)
        const int INST_SET_TOOL = 12;               // [ID, CODE, X, Y, Z, RX, RY, RZ, KG] (in (int) M * FACTOR_M, RAD * FACTOR_RAD, KG * FACTOR_KG)
        const int INST_SET_DIGITAL_OUT = 13;        // [ID, CODE, PIN, ON] (in (int)) 
        const int INST_SET_ANALOG_OUT = 14;         // [ID, CODE, PIN, VOLTAGE] (in (int) VOLTAGE * FACTOR_VOLT)

        // For compilation reuse
        private byte[] _buffer;
        private int[] _params = null;
        private Action _action;

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
            //List<string> msgs = new List<string>();
            //string dec;
            //if (!CompilerUR.GenerateInstructionDeclaration(action, cursor, false, out dec))
            //    return null;

            //// The message type in the response is currently ignored by Machina, but let's send a zero anyway to not break string splitting and other checks... 
            //string res = $"  socket_send_string(\"{STR_MESSAGE_ID_CHAR}{action.id} 0{STR_MESSAGE_END_CHAR}\")";

            //msgs.Add(dec);
            //msgs.Add(res);
            //return msgs;
            return null;
        }

        public override byte[] GetBytesForNextAction(RobotCursor cursor)
        {
            if (!cursor.ApplyNextAction(out _action)) return null;  // cursor buffer is empty

            _params = null;

            switch (_action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    RotationVector rv = cursor.rotation.AA.ToRotationVector();
                    _params = new int[]
                    {
                        _action.id,
                        cursor.motionType == MotionType.Joint ? INST_MOVEJ_P : INST_MOVEL,
                        (int) Math.Round(cursor.position.X * 0.001 * FACTOR_M),
                        (int) Math.Round(cursor.position.Y * 0.001 * FACTOR_M),
                        (int) Math.Round(cursor.position.Z * 0.001 * FACTOR_M),
                        (int) Math.Round(rv.X * Geometry.TO_RADS * FACTOR_RAD),
                        (int) Math.Round(rv.Y * Geometry.TO_RADS * FACTOR_RAD),
                        (int) Math.Round(rv.Z * Geometry.TO_RADS * FACTOR_RAD)
                    };
                    break;

                case ActionType.Axes:
                    _params = new int[]
                    {
                        _action.id,
                        INST_MOVEJ_Q,
                        (int) Math.Round(cursor.joints.J1 * Geometry.TO_RADS * FACTOR_RAD),
                        (int) Math.Round(cursor.joints.J2 * Geometry.TO_RADS * FACTOR_RAD),
                        (int) Math.Round(cursor.joints.J3 * Geometry.TO_RADS * FACTOR_RAD),
                        (int) Math.Round(cursor.joints.J4 * Geometry.TO_RADS * FACTOR_RAD),
                        (int) Math.Round(cursor.joints.J5 * Geometry.TO_RADS * FACTOR_RAD),
                        (int) Math.Round(cursor.joints.J6 * Geometry.TO_RADS * FACTOR_RAD),
                    };
                    break;

                case ActionType.Wait:
                    ActionWait aa = _action as ActionWait;
                    _params = new int[]
                    {
                        _action.id,
                        INST_SLEEP,
                        (int) Math.Round(aa.millis * 0.001 * FACTOR_SEC)
                    };
                    break;

                // Not implemented yet
                //case ActionType.Message:
                //    ActionMessage am = (ActionMessage)action;
                //    dec = string.Format("  popup(\"{0}\", title=\"Machina Message\", warning=False, error=False)",
                //        am.message);
                //    break;

                case ActionType.Attach:
                    ActionAttach aatt = _action as ActionAttach;
                    Tool t = aatt.tool;
                    RotationVector trv = t.TCPOrientation.ToRotationVector();
                    _params = new int[]
                    {
                        _action.id,
                        INST_SET_TOOL,
                        (int) Math.Round(t.TCPPosition.X * 0.001 * FACTOR_M),
                        (int) Math.Round(t.TCPPosition.Y * 0.001 * FACTOR_M),
                        (int) Math.Round(t.TCPPosition.Z * 0.001 * FACTOR_M),
                        (int) Math.Round(trv.X * Geometry.TO_RADS * FACTOR_RAD),
                        (int) Math.Round(trv.Y * Geometry.TO_RADS * FACTOR_RAD),
                        (int) Math.Round(trv.Z * Geometry.TO_RADS * FACTOR_RAD),
                        (int) Math.Round(t.Weight * FACTOR_M)
                    };
                    break;

                case ActionType.Detach:
                    _params = new int[]
                    {
                        _action.id,
                        INST_SET_TOOL,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0
                    };
                    break;

                case ActionType.IODigital:
                    ActionIODigital aiod = _action as ActionIODigital;
                    _params = new int[]
                    {
                        _action.id,
                        INST_SET_DIGITAL_OUT,
                        aiod.pin,
                        aiod.on ? 1 : 0
                    };
                    break;

                case ActionType.IOAnalog:
                    ActionIOAnalog aioa = _action as ActionIOAnalog;
                    _params = new int[]
                    {
                        _action.id,
                        INST_SET_DIGITAL_OUT,
                        aioa.pin,
                        (int) Math.Round(aioa.value * FACTOR_VOLT)
                    };
                    break;


                //  ╔═╗╔═╗╔╦╗╔╦╗╦╔╗╔╔═╗╔═╗
                //  ╚═╗║╣  ║  ║ ║║║║║ ╦╚═╗
                //  ╚═╝╚═╝ ╩  ╩ ╩╝╚╝╚═╝╚═╝
                case ActionType.Speed:
                    _params = new int[]
                    {
                        _action.id,
                        INST_TCP_SPEED,
                        (int) Math.Round(cursor.speed * 0.001 * FACTOR_M)
                    };
                    break;

                case ActionType.Acceleration:
                    _params = new int[]
                    {
                        _action.id,
                        INST_TCP_ACC,
                        (int) Math.Round(cursor.acceleration * 0.001 * FACTOR_M)
                    };
                    break;

                case ActionType.JointSpeed:
                    _params = new int[]
                    {
                        _action.id,
                        INST_Q_SPEED,
                        (int) Math.Round(cursor.jointSpeed * Geometry.TO_RADS * FACTOR_RAD)
                    };
                    break;

                case ActionType.JointAcceleration:
                    _params = new int[]
                    {
                        _action.id,
                        INST_Q_ACC,
                        (int) Math.Round(cursor.jointAcceleration * Geometry.TO_RADS * FACTOR_RAD)
                    };
                    break;

                case ActionType.Precision:
                    _params = new int[]
                    {
                        _action.id,
                        INST_BLEND,
                        (int) Math.Round(cursor.precision * 0.001 * FACTOR_M)
                    };
                    break;

                case ActionType.PushPop:
                    ActionPushPop app = _action as ActionPushPop;
                    if (app.push) break;

                    Settings beforePop = cursor.settingsBuffer.SettingsBeforeLastPop;
                    Dictionary<int, int> poppedSettings = new Dictionary<int, int>();

                    // These are the states kept in the controller as of v1.0 of the driver
                    if (beforePop.Speed != cursor.speed)
                        poppedSettings.Add(INST_TCP_SPEED, (int)Math.Round(cursor.speed * 0.001 * FACTOR_M));

                    if (beforePop.Acceleration != cursor.acceleration)
                        poppedSettings.Add(INST_TCP_ACC, (int)Math.Round(cursor.acceleration * 0.001 * FACTOR_M));

                    if (beforePop.JointSpeed != cursor.jointSpeed)
                        poppedSettings.Add(INST_Q_SPEED, (int)Math.Round(cursor.jointSpeed * Geometry.TO_RADS * FACTOR_RAD));

                    if (beforePop.JointAcceleration != cursor.jointAcceleration)
                        poppedSettings.Add(INST_Q_ACC, (int)Math.Round(cursor.jointAcceleration * Geometry.TO_RADS * FACTOR_RAD));

                    if (beforePop.Precision != cursor.precision)
                        poppedSettings.Add(INST_BLEND, (int)Math.Round(cursor.precision * 0.001 * FACTOR_M));

                    // Generate a buffer with all instructions, ids of -1 except for the last one.
                    _params = new int[3 * poppedSettings.Count];
                    int it = 0;
                    foreach (var setting in poppedSettings)
                    {
                        _params[3 * it] = it == poppedSettings.Count - 1 ? app.id : -1;  // only attach the real id to the last instruction
                        _params[3 * it + 1] = setting.Key;
                        _params[3 * it + 2] = setting.Value;
                        it++;
                    }

                    break;

                case ActionType.Coordinates:
                    throw new NotImplementedException();  // @TODO: this should also change the WObj, but not on it yet...

            }

            if (_params == null) return null;

            _buffer = Util.Int32ArrayToByteArray(_params, false);

            return _buffer;
        }



    }
}
