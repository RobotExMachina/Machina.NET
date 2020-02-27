using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Machina.Types;

namespace Machina
{
    //   ██████╗ ██████╗ ███╗   ███╗██████╗ ██╗██╗     ███████╗██████╗ 
    //  ██╔════╝██╔═══██╗████╗ ████║██╔══██╗██║██║     ██╔════╝██╔══██╗
    //  ██║     ██║   ██║██╔████╔██║██████╔╝██║██║     █████╗  ██████╔╝
    //  ██║     ██║   ██║██║╚██╔╝██║██╔═══╝ ██║██║     ██╔══╝  ██╔══██╗
    //  ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║     ██║███████╗███████╗██║  ██║
    //   ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝     ╚═╝╚══════╝╚══════╝╚═╝  ╚═╝
    //
    //  ██╗  ██╗██╗   ██╗██╗  ██╗ █████╗ 
    //  ██║ ██╔╝██║   ██║██║ ██╔╝██╔══██╗
    //  █████╔╝ ██║   ██║█████╔╝ ███████║
    //  ██╔═██╗ ██║   ██║██╔═██╗ ██╔══██║
    //  ██║  ██╗╚██████╔╝██║  ██╗██║  ██║
    //  ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝
    //
    /// <summary>
    /// A compiler for KUKA 6-axis industrial robotic arms.
    /// </summary>
    internal class CompilerKUKA : Compiler
    {

        internal override Encoding Encoding => Encoding.ASCII;

        internal override char CC => ';';

        internal CompilerKUKA() : base() { }


        /// <summary>
        /// Creates a textual program representation of a set of Actions using native KUKA Robot Language.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="writePointer"></param>
        /// <param name="block">Use actions in waiting queue or buffer?</param>
        /// <returns></returns>
        public override RobotProgram UNSAFEFullProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets, bool humanComments)
        {
            // The program files to be returned
            RobotProgram robotProgram = new RobotProgram(programName, CC);

            // HEADER file
            RobotProgramFile datFile = new RobotProgramFile(programName, "dat", Encoding, CC);
            
            List<string> header = new List<string>();
            header.AddRange(GenerateDisclaimerHeader(programName));
            header.Add("&ACCESS RVP");
            header.Add("& REL 1");
            header.Add("& PARAM EDITMASK = *");
            header.Add(@"&PARAM TEMPLATE = C:\KRC\Roboter\Template\vorgabe");
            header.Add(@"& PARAM DISKPATH = KRC:\R1\Program");  // @TODO: this path should be programmatically generated...
            header.Add(string.Format("DEFDAT  {0}", programName));
            header.Add("EXT BAS (BAS_COMMAND: IN, REAL: IN)");
            header.Add("DECL INT SUCCESS");
            header.Add("ENDDAT");

            datFile.SetContent(header);
            robotProgram.Add(datFile);
                        
            // PROGRAM FILE
            addActionString = humanComments;

            // Which pending Actions are used for this program?
            // Copy them without flushing the buffer.
            List<Action> actions = block ?
                writer.actionBuffer.GetBlockPending(false) :
                writer.actionBuffer.GetAllPending(false);

            // CODE LINES GENERATION
            // TARGETS AND INSTRUCTIONS
            List<string> declarationLines = new List<string>();
            List<string> customDeclarationLines = new List<string>();
            List<string> initializationLines = new List<string>();
            List<string> instructionLines = new List<string>();
                       
            //KUKA INITIALIZATION BOILERPLATE
            //declarationLines.Add("  ; @TODO: consolidate all same datatypes into single inline declarations...");
            //declarationLines.Add("  EXT BAS (BAS_COMMAND :IN, REAL :IN)");              // import BAS sys function  --> This needs to move to `.dat` file

            initializationLines.Add("  GLOBAL INTERRUPT DECL 3 WHEN $STOPMESS==TRUE DO IR_STOPM()");  // legacy support for user-programming safety
            initializationLines.Add("  INTERRUPT ON 3");
            initializationLines.Add("  BAS (#INITMOV, 0)");  // use base function to initialize sys vars to defaults
            initializationLines.Add("");

            // This was reported not to work
            //initializationLines.Add("  $TOOL = {X 0, Y 0, Z 0, A 0, B 0, C 0}");  // no tool
            //initializationLines.Add("  $LOAD.M = 0");   // no mass
            //initializationLines.Add("  $LOAD.CM = {X 0, Y 0, Z 0, A 0, B 0, C 0}");  // no CoG

            // This was reported to be needed
            initializationLines.Add("  BASE_DATA[1] = {X 0, Y 0, Z 0, A 0, B 0, C 0}");

            // DATA GENERATION
            // Use the write RobotCursor to generate the data
            int it = 0;
            string line = null;
            foreach (Action a in actions)
            {
                // Move writerCursor to this action state
                writer.ApplyNextAction();  // for the buffer to correctly manage them

                if (a.Type == ActionType.CustomCode && (a as ActionCustomCode).isDeclaration)
                {
                    customDeclarationLines.Add("  " + (a as ActionCustomCode).statement);
                }

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

            // Banner
            module.AddRange(GenerateDisclaimerHeader(programName));

            // SOME INTERFACE INITIALIZATION
            // These are all for interface handling, ignored by the compiler (?)
            module.Add(@"&ACCESS RVP");  // read-write permissions
            module.Add(@"&REL 1");       // release number (increments on file changes)
            //module.Add(@"&COMMENT MACHINA PROGRAM");  // This was reported to not work
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

            // Custom declarations
            if (customDeclarationLines.Count != 0)
            {
                module.AddRange(customDeclarationLines);
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


            RobotProgramFile srcFile = new Types.RobotProgramFile(programName, "src", Encoding, CC);
            srcFile.SetContent(module);
            robotProgram.Add(srcFile);

            return robotProgram;
        }


        ///// <summary>
        ///// Creates a textual program representation of a set of Actions using native KUKA Robot Language.
        ///// </summary>
        ///// <param name="programName"></param>
        ///// <param name="writePointer"></param>
        ///// <param name="block">Use actions in waiting queue or buffer?</param>
        ///// <returns></returns>
        //public override List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets, bool humanComments)
        //{
        //    addActionString = humanComments;

        //    // Which pending Actions are used for this program?
        //    // Copy them without flushing the buffer.
        //    List<Action> actions = block ?
        //        writer.actionBuffer.GetBlockPending(false) :
        //        writer.actionBuffer.GetAllPending(false);

        //    // CODE LINES GENERATION
        //    // TARGETS AND INSTRUCTIONS
        //    List<string> declarationLines = new List<string>();
        //    List<string> customDeclarationLines = new List<string>();
        //    List<string> initializationLines = new List<string>();
        //    List<string> instructionLines = new List<string>();

        //    //KUKA INITIALIZATION BOILERPLATE
        //    //declarationLines.Add("  ; @TODO: consolidate all same datatypes into single inline declarations...");
        //    //declarationLines.Add("  EXT BAS (BAS_COMMAND :IN, REAL :IN)");              // import BAS sys function  --> This needs to move to `.dat` file

        //    initializationLines.Add("  GLOBAL INTERRUPT DECL 3 WHEN $STOPMESS==TRUE DO IR_STOPM()");  // legacy support for user-programming safety
        //    initializationLines.Add("  INTERRUPT ON 3");
        //    initializationLines.Add("  BAS (#INITMOV, 0)");  // use base function to initialize sys vars to defaults
        //    initializationLines.Add("");

        //    // This was reported not to work
        //    //initializationLines.Add("  $TOOL = {X 0, Y 0, Z 0, A 0, B 0, C 0}");  // no tool
        //    //initializationLines.Add("  $LOAD.M = 0");   // no mass
        //    //initializationLines.Add("  $LOAD.CM = {X 0, Y 0, Z 0, A 0, B 0, C 0}");  // no CoG

        //    // This was reported to be needed
        //    initializationLines.Add("  BASE_DATA[1] = {X 0, Y 0, Z 0, A 0, B 0, C 0}");

        //    // DATA GENERATION
        //    // Use the write RobotCursor to generate the data
        //    int it = 0;
        //    string line = null;
        //    foreach (Action a in actions)
        //    {
        //        // Move writerCursor to this action state
        //        writer.ApplyNextAction();  // for the buffer to correctly manage them

        //        if (a.Type == ActionType.CustomCode && (a as ActionCustomCode).isDeclaration)
        //        {
        //            customDeclarationLines.Add("  " + (a as ActionCustomCode).statement);
        //        }

        //        if (inlineTargets)
        //        {
        //            if (GenerateInstructionDeclaration(a, writer, out line))
        //            {
        //                instructionLines.Add(line);
        //            }
        //        }
        //        else
        //        {
        //            if (GenerateVariableDeclaration(a, writer, it, out line))  // there will be a number jump on target-less instructions, but oh well...
        //            {
        //                declarationLines.Add(line);
        //            }

        //            if (GenerateVariableInitialization(a, writer, it, out line))
        //            {
        //                initializationLines.Add(line);
        //            }

        //            if (GenerateInstructionDeclarationFromVariable(a, writer, it, out line))  // there will be a number jump on target-less instructions, but oh well...
        //            {
        //                instructionLines.Add(line);
        //            }
        //        }

        //        // Move on
        //        it++;
        //    }


        //    // PROGRAM ASSEMBLY
        //    // Initialize a module list
        //    List<string> module = new List<string>();

        //    // Banner
        //    module.AddRange(GenerateDisclaimerHeader(programName));
        //    module.Add("");

        //    // SOME INTERFACE INITIALIZATION
        //    // These are all for interface handling, ignored by the compiler (?)
        //    module.Add(@"&ACCESS RVP");  // read-write permissions
        //    module.Add(@"&REL 1");       // release number (increments on file changes)
        //    //module.Add(@"&COMMENT MACHINA PROGRAM");  // This was reported to not work
        //    module.Add(@"&PARAM TEMPLATE = C:\KRC\Roboter\Template\vorgabe");
        //    module.Add(@"&PARAM EDITMASK = *");
        //    module.Add("");

        //    // MODULE HEADER
        //    module.Add("DEF " + programName + "()");
        //    module.Add("");

        //    // Declarations
        //    if (declarationLines.Count != 0)
        //    {
        //        module.AddRange(declarationLines);
        //        module.Add("");
        //    }

        //    // Custom declarations
        //    if (customDeclarationLines.Count != 0)
        //    {
        //        module.AddRange(customDeclarationLines);
        //        module.Add("");
        //    }

        //    // Initializations
        //    if (initializationLines.Count != 0)
        //    {
        //        module.AddRange(initializationLines);
        //        module.Add("");
        //    }

        //    // MAIN PROCEDURE
        //    // Instructions
        //    if (instructionLines.Count != 0)
        //    {
        //        module.AddRange(instructionLines);
        //        module.Add("");
        //    }

        //    module.Add("END");
        //    module.Add("");

        //    //// MODULE KICKOFF
        //    //module.Add(programName + "()");  // no need for this in KRL if file name is same as module name --> what if user exports them with different names?

        //    return module;
        //}




        //  ╦ ╦╔╦╗╦╦  ╔═╗
        //  ║ ║ ║ ║║  ╚═╗
        //  ╚═╝ ╩ ╩╩═╝╚═╝
        internal bool GenerateVariableDeclaration(Action action, RobotCursor cursor, int id, out string declaration)
        {
            string dec = null;
            switch (action.Type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  POS target{0}", id);
                    break;

                case ActionType.Axes:
                    dec = string.Format("  AXIS target{0}", id);
                    break;
            }

            declaration = dec;
            return dec != null;
        }

        internal bool GenerateVariableInitialization(Action action, RobotCursor cursor, int id, out string declaration)
        {
            string dec = null;
            switch (action.Type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  target{0} = {1}",
                        id,
                        GetPositionTargetValue(cursor));
                    break;

                case ActionType.Axes:
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
            switch (action.Type)
            {
                // KUKA does explicit setting of velocities and approximate positioning, so these actions make sense as instructions
                case ActionType.Speed:
                    dec = string.Format(CultureInfo.InvariantCulture,
                        //"  $VEL = {{CP {0}, ORI1 100, ORI2 100}}",  // This was reported to not work
                        "  $VEL.CP = {0}",  // @TODO: figure out how to also incorporate ORI1 and ORI2
                        Math.Round(0.001 * cursor.speed, 3 + Geometry.STRING_ROUND_DECIMALS_MM));
                    break;

                case ActionType.Precision:
                    dec = string.Format(CultureInfo.InvariantCulture, 
                        "  $APO.CDIS = {0}",
                        cursor.precision);
                    break;

                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  {0} target{1} {2}",
                        cursor.motionType == MotionType.Joint ? "PTP" : "LIN",
                        id,
                        cursor.precision >= 1 ? "C_DIS" : "");
                    break;

                case ActionType.Axes:
                    dec = string.Format("  {0} target{1} {2}",
                        "PTP",
                        id,
                        cursor.precision >= 1 ? "C_DIS" : "");  // @TODO: figure out how to turn this into C_PTP
                    break;

                // @TODO: apparently, messages in KRL are kind fo tricky, with several manuals just dedicated to it.
                // Will figure this out later.
                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("  {0} MESSAGE: \"{1}\" (messages in KRL currently not supported in Machina)",
                        CC,
                        am.message);
                    break;

                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format(CultureInfo.InvariantCulture, 
                        "  WAIT SEC {0}",
                        0.001 * aw.millis);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("  {0} {1}",
                        CC,
                        ac.comment);
                    break;

                case ActionType.DefineTool:
                    ActionDefineTool adt = action as ActionDefineTool;
                    dec = string.Format("  {0} Tool \"{1}\" defined",  // this action has no actual instruction, just add a comment
                        CC,
                        adt.tool.name);
                    break;

                case ActionType.AttachTool:
                    ActionAttachTool at = (ActionAttachTool)action;
                    dec = string.Format("  $TOOL = {0}",
                        GetToolValue(cursor));
                    break;

                case ActionType.DetachTool:
                    ActionDetachTool ad = (ActionDetachTool)action;
                    dec = string.Format("  $TOOL = $NULLFRAME");
                    break;

                case ActionType.IODigital:
                    ActionIODigital aiod = (ActionIODigital)action;
                    if (!aiod.isDigit)
                    {
                        dec = $"  {CC} ERROR on \"{aiod}\": only integer pin names are possible";
                    }
                    else if (aiod.pinNum < 1 || aiod.pinNum > 32)  // KUKA starts counting pins by 1
                    {
                        dec = $"  {CC} ERROR on \"{aiod}\": IO number not available";
                    }
                    else
                    {
                        dec = $"  $OUT[{aiod.pinNum}] = {(aiod.on ? "TRUE" : "FALSE")}";
                    }
                    break;

                case ActionType.IOAnalog:
                    ActionIOAnalog aioa = (ActionIOAnalog)action;
                    if (!aioa.isDigit)
                    {
                        dec = $"  {CC} ERROR on \"{aioa}\": only integer pin names are possible";
                    }
                    else if (aioa.pinNum < 1 || aioa.pinNum > 16)    // KUKA: analog pins [1 to 16]
                    {
                        dec = $"  {CC} ERROR on \"{aioa}\": IO number not available";
                    }
                    else if (aioa.value < -1 || aioa.value > 1)
                    {
                        dec = $"  {CC} ERROR on \"{aioa}\": value out of range [-1.0, 1.0]";
                    }
                    else
                    {
                        //dec = "  $ANOUT[{aioa.pinNum}] = {Math.Round(aioa.value, Geometry.STRING_ROUND_DECIMALS_VOLTAGE)}";
                        dec = string.Format(CultureInfo.InvariantCulture,
                            "  $ANOUT[{0}] = {1}",
                            aioa.pinNum,
                            Math.Round(aioa.value, Geometry.STRING_ROUND_DECIMALS_VOLTAGE));

                    }
                    break;

                case ActionType.CustomCode:
                    ActionCustomCode acc = action as ActionCustomCode;
                    if (!acc.isDeclaration)
                    {
                        dec = $"  {acc.statement}";
                    }
                    break;

                    //default:
                    //    dec = string.Format("  ; ACTION \"{0}\" NOT IMPLEMENTED", action);
                    //    break;
            }

            if (addActionString && action.Type != ActionType.Comment)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    CC,
                    action.ToString());
            }
            else if (addActionID)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    CC,
                    action.Id);
            }

            declaration = dec;
            return dec != null;
        }


        internal bool GenerateInstructionDeclaration(
            Action action, RobotCursor cursor,
            out string declaration)
        {
            string dec = null;
            switch (action.Type)
            {
                // KUKA does explicit setting of velocities and approximate positioning, so these actions make sense as instructions
                case ActionType.Speed:
                    dec = string.Format(CultureInfo.InvariantCulture,
                        //"  $VEL = {{CP {0}, ORI1 100, ORI2 100}}",  // This was reported to not work
                        "  $VEL.CP = {0}",  // @TODO: figure out how to also incorporate ORI1 and ORI2
                        Math.Round(0.001 * cursor.speed, 3 + Geometry.STRING_ROUND_DECIMALS_MM));
                    break;

                case ActionType.Precision:
                    dec = string.Format(CultureInfo.InvariantCulture, 
                        "  $APO.CDIS = {0}",
                        cursor.precision);
                    break;

                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  {0} {1} {2}",
                        cursor.motionType == MotionType.Joint ? "PTP" : "LIN",
                        GetPositionTargetValue(cursor),
                        cursor.precision >= 1 ? "C_DIS" : "");
                    break;

                case ActionType.Axes:
                    dec = string.Format("  {0} {1} {2}",
                        "PTP",
                        GetAxisTargetValue(cursor),
                        cursor.precision >= 1 ? "C_DIS" : "");  // @TODO: figure out how to turn this into C_PTP
                    break;

                // @TODO: apparently, messages in KRL are kind fo tricky, with several manuals just dedicated to it.
                // Will figure this out later.
                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("  {0} MESSAGE: \"{1}\" (messages in KRL currently not supported in Machina)",
                        CC,
                        am.message);
                    break;

                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format(CultureInfo.InvariantCulture, 
                        "  WAIT SEC {0}",
                        0.001 * aw.millis);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("  {0} {1}",
                        CC,
                        ac.comment);
                    break;

                case ActionType.DefineTool:
                    ActionDefineTool adt = action as ActionDefineTool;
                    dec = string.Format("  {0} Tool \"{1}\" defined",  // this action has no actual instruction, just add a comment
                        CC,
                        adt.tool.name);
                    break;

                case ActionType.AttachTool:
                    ActionAttachTool at = (ActionAttachTool)action;
                    dec = string.Format("  $TOOL = {0}",
                        GetToolValue(cursor));
                    break;

                case ActionType.DetachTool:
                    ActionDetachTool ad = (ActionDetachTool)action;
                    dec = string.Format("  $TOOL = $NULLFRAME");
                    break;

                case ActionType.IODigital:
                    ActionIODigital aiod = (ActionIODigital)action;
                    if (!aiod.isDigit)
                    {
                        dec = $"  {CC} ERROR on \"{aiod}\": only integer pin names are possible";
                    }
                    else if (aiod.pinNum < 1 || aiod.pinNum > 32)  // KUKA starts counting pins by 1
                    {
                        dec = $"  {CC} ERROR on \"{aiod}\": IO number not available";
                    }
                    else
                    {
                        dec = $"  $OUT[{aiod.pinNum}] = {(aiod.on ? "TRUE" : "FALSE")}";
                    }
                    break;

                case ActionType.IOAnalog:
                    ActionIOAnalog aioa = (ActionIOAnalog)action;
                    if (!aioa.isDigit)
                    {
                        dec = $"  {CC} ERROR on \"{aioa}\": only integer pin names are possible";
                    }
                    else if (aioa.pinNum < 1 || aioa.pinNum > 16)    // KUKA: analog pins [1 to 16]
                    {
                        dec = $"  {CC} ERROR on \"{aioa}\": IO number not available";
                    }
                    else if (aioa.value < -1 || aioa.value > 1)
                    {
                        dec = $"  {CC} ERROR on \"{aioa}\": value out of range [-1.0, 1.0]";
                    }
                    else
                    {
                        //dec = $"  $ANOUT[{aioa.pinNum}] = {Math.Round(aioa.value, Geometry.STRING_ROUND_DECIMALS_VOLTAGE)}";
                        dec = string.Format(CultureInfo.InvariantCulture,
                            "  $ANOUT[{0}] = {1}",
                            aioa.pinNum,
                            Math.Round(aioa.value, Geometry.STRING_ROUND_DECIMALS_VOLTAGE));
                    }
                    break;

                case ActionType.CustomCode:
                    ActionCustomCode acc = action as ActionCustomCode;
                    if (!acc.isDeclaration)
                    {
                        dec = $"  {acc.statement}";
                    }
                    break;

                    //default:
                    //    dec = string.Format("  ; ACTION \"{0}\" NOT IMPLEMENTED", action);
                    //    break;
            }

            if (addActionString && action.Type != ActionType.Comment)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    CC,
                    action.ToString());
            }
            else if (addActionID)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    CC,
                    action.Id);
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

            return string.Format(CultureInfo.InvariantCulture,
                "{{POS: X {0}, Y {1}, Z {2}, A {3}, B {4}, C {5}}}",
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
            return string.Format(CultureInfo.InvariantCulture, 
                "{{AXIS: A1 {0}, A2 {1}, A3 {2}, A4 {3}, A5 {4}, A6 {5}}}",
                Math.Round(cursor.axes.J1, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(cursor.axes.J2, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(cursor.axes.J3, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(cursor.axes.J4, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(cursor.axes.J5, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(cursor.axes.J6, Geometry.STRING_ROUND_DECIMALS_DEGS));
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

            return string.Format(CultureInfo.InvariantCulture, 
                "{{X {0}, Y {1}, Z {2}, A {3}, B {4}, C {5}}}",
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
