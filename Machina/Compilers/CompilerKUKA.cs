using System;
using System.Collections.Generic;

namespace Machina
{
    //   ██████╗ ██████╗ ███╗   ███╗██████╗ ██╗██╗     ███████╗██████╗ 
    //  ██╔════╝██╔═══██╗████╗ ████║██╔══██╗██║██║     ██╔════╝██╔══██╗
    //  ██║     ██║   ██║██╔████╔██║██████╔╝██║██║     █████╗  ██████╔╝
    //  ██║     ██║   ██║██║╚██╔╝██║██╔═══╝ ██║██║     ██╔══╝  ██╔══██╗
    //  ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║     ██║███████╗███████╗██║  ██║
    //   ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝     ╚═╝╚══════╝╚══════╝╚═╝  ╚═╝

    //  ██╗  ██╗██╗   ██╗██╗  ██╗ █████╗ 
    //  ██║ ██╔╝██║   ██║██║ ██╔╝██╔══██╗
    //  █████╔╝ ██║   ██║█████╔╝ ███████║
    //  ██╔═██╗ ██║   ██║██╔═██╗ ██╔══██║
    //  ██║  ██╗╚██████╔╝██║  ██╗██║  ██║
    //  ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝
    internal class CompilerKUKA : Compiler
    {

        internal CompilerKUKA() : base(";") { }

        /// <summary>
        /// Creates a textual program representation of a set of Actions using native KUKA Robot Language.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="writePointer"></param>
        /// <param name="block">Use actions in waiting queue or buffer?</param>
        /// <returns></returns>
        //public override List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writePointer, bool block)
        public override List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets, bool humanComments)
        {
            ADD_ACTION_STRING = humanComments;

            // Which pending Actions are used for this program?
            // Copy them without flushing the buffer.
            List<Action> actions = block ?
                writer.actionBuffer.GetBlockPending(false) :
                writer.actionBuffer.GetAllPending(false);


            // CODE LINES GENERATION
            // TARGETS AND INSTRUCTIONS
            List<string> declarationLines = new List<string>();
            List<string> initializationLines = new List<string>();
            List<string> instructionLines = new List<string>();

            //KUKA INITIALIZATION BOILERPLATE
            declarationLines.Add("  ; @TODO: consolidate all same datatypes into single inline declarations...");
            declarationLines.Add("  EXT BAS (BAS_COMMAND :IN, REAL :IN)");              // import BAS sys function

            initializationLines.Add("  GLOBAL INTERRUPT DECL 3 WHEN $STOPMESS==TRUE DO IR_STOPM()");  // legacy support for user-programming safety
            initializationLines.Add("  INTERRUPT ON 3");
            initializationLines.Add("  BAS (#INITMOV, 0)");  // use base function to initialize sys vars to defaults
            initializationLines.Add("");
            initializationLines.Add("  $TOOL = {X 0, Y 0, Z 0, A 0, B 0, C 0}");  // no tool
            initializationLines.Add("  $LOAD.M = 0");   // no mass
            initializationLines.Add("  $LOAD.CM = {X 0, Y 0, Z 0, A 0, B 0, C 0}");  // no CoG

            // DATA GENERATION
            // Use the write RobotCursor to generate the data
            int it = 0;
            string line = null;
            foreach (Action a in actions)
            {
                // Move writerCursor to this action state
                writer.ApplyNextAction();  // for the buffer to correctly manage them

                if (inlineTargets)
                {
                    if (GenerateInstructionDeclaration(a, writer, out line))
                    {
                        instructionLines.Add(line);
                    }
                }
                else
                {
                    if (GenerateVariableDeclaration(a, writer, it, out line))  // there will be a number jump on target-less instructions, but oh well...
                    {
                        declarationLines.Add(line);
                    }

                    if (GenerateVariableInitialization(a, writer, it, out line))
                    {
                        initializationLines.Add(line);
                    }

                    if (GenerateInstructionDeclarationFromVariable(a, writer, it, out line))  // there will be a number jump on target-less instructions, but oh well...
                    {
                        instructionLines.Add(line);
                    }
                }

                // Move on
                it++;
            }


            // PROGRAM ASSEMBLY
            // Initialize a module list
            List<string> module = new List<string>();

            // SOME INTERFACE INITIALIZATION
            // These are all for interface handling, ignored by the compiler (?)
            module.Add(@"&ACCESS RVP");  // read-write permissions
            module.Add(@"&REL 1");       // release number (increments on file changes)
            module.Add(@"&COMMENT MACHINA PROGRAM");
            module.Add(@"&PARAM TEMPLATE = C:\KRC\Roboter\Template\vorgabe");
            module.Add(@"&PARAM EDITMASK = *");
            module.Add("");

            // MODULE HEADER
            module.Add("DEF " + programName + "()");
            module.Add("");

            // Declarations
            if (declarationLines.Count != 0)
            {
                module.AddRange(declarationLines);
                module.Add("");
            }

            // Initializations
            if (initializationLines.Count != 0)
            {
                module.AddRange(initializationLines);
                module.Add("");
            }

            // MAIN PROCEDURE
            // Instructions
            if (instructionLines.Count != 0)
            {
                module.AddRange(instructionLines);
                module.Add("");
            }

            module.Add("END");
            module.Add("");

            //// MODULE KICKOFF
            //module.Add(programName + "()");  // no need for this in KRL if file name is same as module name --> what if user exports them with different names?

            return module;
        }




        //  ╦ ╦╔╦╗╦╦  ╔═╗
        //  ║ ║ ║ ║║  ╚═╗
        //  ╚═╝ ╩ ╩╩═╝╚═╝
        internal bool GenerateVariableDeclaration(Action action, RobotCursor cursor, int id, out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  POS target{0}",
                        id);
                    break;

                case ActionType.Joints:
                    dec = string.Format("  AXIS target{0}",
                        id);
                    break;
            }

            declaration = dec;
            return dec != null;
        }

        internal bool GenerateVariableInitialization(Action action, RobotCursor cursor, int id, out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  target{0} = {1}",
                        id,
                        GetPositionTargetValue(cursor));
                    break;

                case ActionType.Joints:
                    dec = string.Format("  target{0} = {1}",
                        id,
                        GetAxisTargetValue(cursor));
                    break;
            }

            declaration = dec;
            return dec != null;
        }

        internal bool GenerateInstructionDeclarationFromVariable(Action action, RobotCursor cursor, int id,
            out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                // KUKA does explicit setting of velocities and approximate positioning, so these actions make sense as instructions
                case ActionType.Speed:
                    dec = string.Format("  $VEL = {{CP {0}, ORI1 100, ORI2 100}}",
                        Math.Round(0.001 * cursor.speed, 3 + Geometry.STRING_ROUND_DECIMALS_MM));
                    break;

                case ActionType.Zone:
                    dec = string.Format("  $APO.CDIS = {0}",
                        cursor.zone);
                    break;

                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  {0} target{1} {2}",
                        cursor.motionType == MotionType.Joint ? "PTP" : "LIN",
                        id,
                        cursor.zone >= 1 ? "C_DIS" : "");
                    break;

                case ActionType.Joints:
                    dec = string.Format("  {0} target{1} {2}",
                        "PTP",
                        id,
                        cursor.zone >= 1 ? "C_DIS" : "");  // @TODO: figure out how to turn this into C_PTP
                    break;

                // @TODO: apparently, messages in KRL are kind fo tricky, with several manuals just dedicated to it.
                // Will figure this out later.
                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("  ; MESSAGE: \"{0}\" (messages in KRL currently not supported in Machina)",
                        am.message);
                    break;

                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format("  WAIT SEC {0}",
                        0.001 * aw.millis);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("  {0} {1}",
                        cursor.compiler.commentCharacter,
                        ac.comment);
                    break;

                case ActionType.Attach:
                    ActionAttach at = (ActionAttach)action;
                    dec = string.Format("  $TOOL = {0}",
                        GetToolValue(cursor));
                    break;

                case ActionType.Detach:
                    ActionDetach ad = (ActionDetach)action;
                    dec = string.Format("  $TOOL = $NULLFRAME");
                    break;

                case ActionType.IODigital:
                    ActionIODigital aiod = (ActionIODigital)action;
                    if (aiod.pin < 1 || aiod.pin >= cursor.digitalOutputs.Length)  // KUKA starts counting pins by 1
                    {
                        dec = string.Format("  {0} ERROR on \"{1}\": IO number not available",
                            commentCharacter,
                            aiod.ToString());
                    }
                    else
                    {
                        dec = string.Format("  $OUT[{0}] = {1}",
                            aiod.pin,
                            aiod.on ? "TRUE" : "FALSE");
                    }
                    break;

                case ActionType.IOAnalog:
                    ActionIOAnalog aioa = (ActionIOAnalog)action;
                    if (aioa.pin < 1 || aioa.pin >= cursor.analogOutputNames.Length || aioa.pin > 16)    // KUKA: analog pins [1 to 16]
                    {
                        dec = string.Format("  {0} ERROR on \"{1}\": IO number not available",
                            commentCharacter,
                            aioa.ToString());
                    }
                    else if (aioa.value < -1 || aioa.value > 1)
                    {
                        dec = string.Format("  {0} ERROR on \"{1}\": value out of range [-1.0, 1.0]",
                            commentCharacter,
                            aioa.ToString());
                    }
                    else
                    {
                        dec = string.Format("  $ANOUT[{0}] = {1}",
                            aioa.pin,
                            Math.Round(aioa.value, Geometry.STRING_ROUND_DECIMALS_VOLTAGE));
                    }
                    break;

                    //default:
                    //    dec = string.Format("  ; ACTION \"{0}\" NOT IMPLEMENTED", action);
                    //    break;
            }

            if (ADD_ACTION_STRING && action.type != ActionType.Comment)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    commentCharacter,
                    action.ToString());
            }
            else if (ADD_ACTION_ID)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    commentCharacter,
                    action.id);
            }

            declaration = dec;
            return dec != null;
        }


        internal bool GenerateInstructionDeclaration(
            Action action, RobotCursor cursor,
            out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                // KUKA does explicit setting of velocities and approximate positioning, so these actions make sense as instructions
                case ActionType.Speed:
                    dec = string.Format("  $VEL = {{CP {0}, ORI1 100, ORI2 100}}",
                        Math.Round(0.001 * cursor.speed, 3 + Geometry.STRING_ROUND_DECIMALS_MM));
                    break;

                case ActionType.Zone:
                    dec = string.Format("  $APO.CDIS = {0}",
                        cursor.zone);
                    break;

                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  {0} {1} {2}",
                        cursor.motionType == MotionType.Joint ? "PTP" : "LIN",
                        GetPositionTargetValue(cursor),
                        cursor.zone >= 1 ? "C_DIS" : "");
                    break;

                case ActionType.Joints:
                    dec = string.Format("  {0} {1} {2}",
                        "PTP",
                        GetAxisTargetValue(cursor),
                        cursor.zone >= 1 ? "C_DIS" : "");  // @TODO: figure out how to turn this into C_PTP
                    break;

                // @TODO: apparently, messages in KRL are kind fo tricky, with several manuals just dedicated to it.
                // Will figure this out later.
                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("  {0} MESSAGE: \"{1}\" (messages in KRL currently not supported in Machina)",
                        commentCharacter,
                        am.message);
                    break;

                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format("  WAIT SEC {0}",
                        0.001 * aw.millis);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("  {0} {1}",
                        commentCharacter,
                        ac.comment);
                    break;


                case ActionType.Attach:
                    ActionAttach at = (ActionAttach)action;
                    dec = string.Format("  $TOOL = {0}",
                        GetToolValue(cursor));
                    break;

                case ActionType.Detach:
                    ActionDetach ad = (ActionDetach)action;
                    dec = string.Format("  $TOOL = $NULLFRAME");
                    break;

                case ActionType.IODigital:
                    ActionIODigital aiod = (ActionIODigital)action;
                    if (aiod.pin < 1 || aiod.pin >= cursor.digitalOutputs.Length)  // KUKA starts counting pins by 1
                    {
                        dec = string.Format("  {0} ERROR on \"{1}\": IO number not available",
                            commentCharacter,
                            aiod.ToString());
                    }
                    else
                    {
                        dec = string.Format("  $OUT[{0}] = {1}",
                            aiod.pin,
                            aiod.on ? "TRUE" : "FALSE");
                    }
                    break;

                case ActionType.IOAnalog:
                    ActionIOAnalog aioa = (ActionIOAnalog)action;
                    if (aioa.pin < 1 || aioa.pin >= cursor.analogOutputNames.Length || aioa.pin > 16)    // KUKA: analog pins [1 to 16]
                    {
                        dec = string.Format("  {0} ERROR on \"{1}\": IO number not available",
                            commentCharacter,
                            aioa.ToString());
                    }
                    else if (aioa.value < -1 || aioa.value > 1)
                    {
                        dec = string.Format("  {0} ERROR on \"{1}\": value out of range [-1.0, 1.0]",
                            commentCharacter,
                            aioa.ToString());
                    }
                    else
                    {
                        dec = string.Format("  $ANOUT[{0}] = {1}",
                            aioa.pin,
                            Math.Round(aioa.value, Geometry.STRING_ROUND_DECIMALS_VOLTAGE));
                    }
                    break;

                    //default:
                    //    dec = string.Format("  ; ACTION \"{0}\" NOT IMPLEMENTED", action);
                    //    break;
            }

            if (ADD_ACTION_STRING && action.type != ActionType.Comment)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    commentCharacter,
                    action.ToString());
            }
            else if (ADD_ACTION_ID)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    commentCharacter,
                    action.id);
            }

            declaration = dec;
            return dec != null;
        }




        /// <summary>
        /// Returns a KRL FRAME representation of the current state of the cursor.
        /// Note POS also accept T and S parameters for unambiguous arm configuration def. @TODO: implement?
        /// </summary>
        /// <returns></returns>
        internal string GetPositionTargetValue(RobotCursor cursor)
        {
            YawPitchRoll euler = cursor.rotation.Q.ToYawPitchRoll();  // @TODO: does this actually work...?

            return string.Format("{{POS: X {0}, Y {1}, Z {2}, A {3}, B {4}, C {5}}}",
                Math.Round(cursor.position.X, Geometry.STRING_ROUND_DECIMALS_MM),
                Math.Round(cursor.position.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                Math.Round(cursor.position.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                // note reversed ZYX order
                Math.Round(euler.ZAngle, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(euler.YAngle, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(euler.XAngle, Geometry.STRING_ROUND_DECIMALS_DEGS));
        }

        /// <summary>
        /// Returns a KRL AXIS joint representation of the current state of the cursor.
        /// </summary>
        /// <returns></returns>
        internal string GetAxisTargetValue(RobotCursor cursor)
        {
            return string.Format("{{AXIS: A1 {0}, A2 {1}, A3 {2}, A4 {3}, A5 {4}, A6 {5}}}",
                Math.Round(cursor.joints.J1, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(cursor.joints.J2, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(cursor.joints.J3, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(cursor.joints.J4, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(cursor.joints.J5, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(cursor.joints.J6, Geometry.STRING_ROUND_DECIMALS_DEGS));
        }

        /// <summary>
        /// Returns a KRL representation of a Tool object
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        internal string GetToolValue(RobotCursor cursor)
        {
            if (cursor.tool == null)
            {
                throw new Exception("Cursor has no tool attached");
            }

            YawPitchRoll euler = cursor.tool.TCPOrientation.Q.ToYawPitchRoll();

            return string.Format("{{X {0}, Y {1}, Z {2}, A {3}, B {4}, C {5}}}",
                Math.Round(cursor.tool.TCPPosition.X, Geometry.STRING_ROUND_DECIMALS_MM),
                Math.Round(cursor.tool.TCPPosition.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                Math.Round(cursor.tool.TCPPosition.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                // note reversed ZYX order
                Math.Round(euler.ZAngle, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(euler.YAngle, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(euler.XAngle, Geometry.STRING_ROUND_DECIMALS_DEGS));
        }

    }
}
