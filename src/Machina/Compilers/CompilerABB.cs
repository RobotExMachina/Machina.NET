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

    //   █████╗ ██████╗ ██████╗ 
    //  ██╔══██╗██╔══██╗██╔══██╗
    //  ███████║██████╔╝██████╔╝
    //  ██╔══██║██╔══██╗██╔══██╗
    //  ██║  ██║██████╔╝██████╔╝
    //  ╚═╝  ╚═╝╚═════╝ ╚═════╝                 
    /// <summary>
    /// A compiler for ABB 6-axis industrial robotic arms.
    /// </summary>
    internal class CompilerABB : Compiler
    {
        // @TODO: deprecate all instantiation shit, and make compilers be mostly static, like CompilerUR:
        /*
         * // From the URScript manual
        public static readonly char COMMENT_CHAR = '#';
        public static readonly double DEFAULT_JOINT_ACCELERATION = 1.4;
        public static readonly double DEFAULT_JOINT_SPEED = 1.05;
        public static readonly double DEFAULT_TOOL_ACCELERATION = 1.2;
        public static readonly double DEFAULT_TOOL_SPEED = 0.25;
        */
        
        public static readonly char COMMENT_CHAR = '!';

        internal CompilerABB() : base(COMMENT_CHAR) { }

        /// <summary>
        /// A Set of RAPID's predefined zone values. 
        /// </summary>
        private static HashSet<int> PredefinedZones = new HashSet<int>()
        {
            0, 1, 5, 10, 15, 20, 30, 40, 50, 60, 80, 100, 150, 200
        };

        /// <summary>
        /// Creates a textual program representation of a set of Actions using native RAPID Laguage.
        /// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        /// robot configuration and assumes the robot controller will figure out the correct one.
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
            // VELOCITY & ZONE DECLARATIONS
            // Amount of different VZ types
            var velNames = new Dictionary<double, string>();
            var velDecs = new Dictionary<double, string>();
            var zoneNames = new Dictionary<double, string>();
            var zoneDecs = new Dictionary<double, string>();
            var zonePredef = new Dictionary<double, bool>();

            // TOOL DECLARATIONS
            Dictionary<Tool, string> toolNames = new Dictionary<Tool, string>();
            Dictionary<Tool, string> toolDecs = new Dictionary<Tool, string>();

            // Intro
            List<string> introLines = new List<string>();

            // Declarations
            List<string> velocityLines = new List<string>();
            List<string> zoneLines = new List<string>();
            List<string> toolLines = new List<string>();
            List<string> customLines = new List<string>();

            // TARGETS AND INSTRUCTIONS
            List<string> variableLines = new List<string>();
            List<string> instructionLines = new List<string>();

            // DATA GENERATION
            // Use the write robot pointer to generate the data
            int it = 0;
            string line = null;
            bool usesIO = false;
            foreach (Action a in actions)
            {
                // Move writerCursor to this action state
                writer.ApplyNextAction();  // for the buffer to correctly manage them 

                // For ABB robots, check if any IO command is issued, and display a warning about configuring their names in the controller.
                if (!usesIO && (writer.lastAction.Type == ActionType.IODigital || writer.lastAction.Type == ActionType.IOAnalog))
                {
                    usesIO = true;
                }

                // Check velocity + zone and generate data accordingly
                if (!velNames.ContainsKey(writer.speed))
                {
                    velNames.Add(writer.speed, "vel" + SafeDoubleName(writer.speed));
                    velDecs.Add(writer.speed, GetSpeedValue(writer));
                }

                if (!zoneNames.ContainsKey(writer.precision))
                {
                    // If precision is very close to an integer, make it integer and/or use predefined zones
                    bool predef = false;
                    int roundZone = 0;
                    if (Math.Abs(writer.precision - Math.Round(writer.precision)) < Geometry.EPSILON) {
                        roundZone = (int) Math.Round(writer.precision);
                        predef = PredefinedZones.Contains(roundZone);
                    }
                    zonePredef.Add(writer.precision, predef);
                    zoneNames.Add(writer.precision, predef ? "z" + roundZone : "zone" + SafeDoubleName(writer.precision));  // use predef syntax or clean new one
                    zoneDecs.Add(writer.precision, predef ? "" : GetZoneValue(writer));
                }

                if (writer.tool != null && !toolNames.ContainsKey(writer.tool))
                {
                    toolNames.Add(writer.tool, writer.tool.name);
                    toolDecs.Add(writer.tool, GetToolValue(writer));
                }

                if (a.Type == ActionType.CustomCode && (a as ActionCustomCode).isDeclaration)
                {
                    customLines.Add($"  {(a as ActionCustomCode).statement}");
                }



                // Generate program
                if (inlineTargets)
                {
                    // Generate lines of code
                    if (GenerateInstructionDeclaration(a, writer, velNames, zoneNames, toolNames, out line))
                    {
                        instructionLines.Add(line);
                    }
                }
                else
                {
                    // Generate lines of code
                    if (GenerateVariableDeclaration(a, writer, it, out line))  // there will be a number jump on target-less instructions, but oh well...
                    {
                        variableLines.Add(line);
                    }

                    if (GenerateInstructionDeclarationFromVariable(a, writer, it, velNames, zoneNames, toolNames, out line))  // there will be a number jump on target-less instructions, but oh well...
                    {
                        instructionLines.Add(line);
                    }
                }

                // Move on
                it++;
            }


            // Generate V+Z+T
            foreach (Tool t in toolNames.Keys)
            {
                toolLines.Add(string.Format("  PERS tooldata {0} := {1};", toolNames[t], toolDecs[t]));
            }
            foreach (var v in velNames.Keys)
            {
                velocityLines.Add(string.Format("  CONST speeddata {0} := {1};", velNames[v], velDecs[v]));
            }
            foreach (var z in zoneNames.Keys)
            {
                if (!zonePredef[z])  // no need to add declarations for predefined zones
                {
                    zoneLines.Add(string.Format("  CONST zonedata {0} := {1};", zoneNames[z], zoneDecs[z]));
                }
            }

            // Generate IO warning
            if (usesIO)
            {
                introLines.Add(string.Format("  {0} NOTE: your program is interfacing with the robot's IOs. Make sure to properly configure their names/properties through system preferences in the ABB robot controller.",
                    commChar));
            }



            // PROGRAM ASSEMBLY
            // Initialize a module list
            List<string> module = new List<string>();
            
            // MODULE HEADER
            module.Add("MODULE " + programName);
            module.Add("");

            // Banner (must go after MODULE, or will yield RAPID syntax errors)
            module.AddRange(GenerateDisclaimerHeader(programName));
            module.Add("");

            // INTRO LINES
            if (introLines.Count != 0)
            {
                module.AddRange(introLines);
                module.Add("");
            }

            // VARIABLE DECLARATIONS
            // Tools
            if (toolLines.Count != 0)
            {
                module.AddRange(toolLines);
                module.Add("");
            }

            // Velocities
            if (velocityLines.Count != 0)
            {
                module.AddRange(velocityLines);
                module.Add("");
            }

            // Zones
            if (zoneLines.Count != 0)
            {
                module.AddRange(zoneLines);
                module.Add("");
            }

            // Custom code
            if (customLines.Count != 0)
            {
                module.AddRange(customLines);
                module.Add("");
            }

            // Targets
            if (variableLines.Count != 0)
            {
                module.AddRange(variableLines);
                module.Add("");
            }

            // MAIN PROCEDURE
            module.Add("  PROC main()");
            module.Add(@"    ConfJ \Off;");
            module.Add(@"    ConfL \Off;");
            module.Add("");

            // Instructions
            if (instructionLines.Count != 0)
            {
                module.AddRange(instructionLines);
                module.Add("");
            }

            module.Add("  ENDPROC");
            module.Add("");

            // MODULE FOOTER
            module.Add("ENDMODULE");

            return module;
        }




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
                    dec = string.Format("  CONST robtarget target{0} := {1};", id, GetUNSAFERobTargetValue(cursor));
                    break;

                case ActionType.Axes:
                    dec = string.Format("  CONST jointtarget target{0} := {1};", id, GetJointTargetValue(cursor));
                    break;
            }

            declaration = dec;
            return dec != null;
        }

        internal bool GenerateInstructionDeclarationFromVariable(
            Action action,
            RobotCursor cursor,
            int id,
            Dictionary<double, string> velNames,
            Dictionary<double, string> zoneNames,
            Dictionary<Tool, string> toolNames,
            out string declaration)
        {
            string dec = null;
            switch (action.Type)
            {
                case ActionType.Acceleration:
                    bool zero = cursor.acceleration < Geometry.EPSILON;
                    dec = string.Format("    WorldAccLim {0};",
                        zero ? "\\Off" : "\\On := " + Math.Round(0.001 * cursor.acceleration, Geometry.STRING_ROUND_DECIMALS_M));
                    break;

                //case ActionType.JointSpeed:
                //case ActionType.JointAcceleration:
                //    dec = string.Format("    {0} WARNING: {1}() has no effect in ABB robots.", 
                //        commChar,
                //        action.Type);
                //    break;

                // @TODO: push/pop management should be done PROGRAMMATICALLY, not this CHAPUZA...
                case ActionType.PushPop:
                    // Find if there was a change in acceleration, and set the corresponsing instruction...
                    ActionPushPop app = action as ActionPushPop;
                    if (app.push) break;  // only necessary for pops
                    if (Math.Abs(cursor.acceleration - cursor.settingsBuffer.SettingsBeforeLastPop.Acceleration) < Geometry.EPSILON) break;  // no change
                    // If here, there was a change, so...
                    bool zeroAcc = cursor.acceleration < Geometry.EPSILON;
                    dec = string.Format("    WorldAccLim {0};",
                        zeroAcc ? "\\Off" : "\\On := " + Math.Round(0.001 * cursor.acceleration, Geometry.STRING_ROUND_DECIMALS_M));
                    break;

                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("    {0} target{1}, {2}, {3}, {4}\\{5};",
                        cursor.motionType == MotionType.Joint ? "MoveJ" : "MoveL",
                        id,
                        velNames[cursor.speed],
                        zoneNames[cursor.precision],
                        cursor.tool == null ? "Tool0" : toolNames[cursor.tool],
                        "WObj:=WObj0");
                    break;

                case ActionType.Axes:
                    dec = string.Format("    MoveAbsJ target{0}, {1}, {2}, {3}\\{4};",
                        id,
                        velNames[cursor.speed],
                        zoneNames[cursor.precision],
                        cursor.tool == null ? "Tool0" : toolNames[cursor.tool],
                        "WObj:=WObj0");
                    break;

                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("    TPWrite \"{0}\";",
                        am.message.Length <= 80 ?
                            am.message :
                            am.message.Substring(0, 80));  // ABB TPWrite messages can only be 80 chars long
                    break;

                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format("    WaitTime {0};",
                        0.001 * aw.millis);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("    {0} {1}",
                        commChar,
                        ac.comment);
                    break;

                case ActionType.DefineTool:
                    ActionDefineTool adt = action as ActionDefineTool;
                    dec = string.Format("    {0} Tool \"{1}\" defined",  // this action has no actual RAPID instruction, just add a comment
                        commChar,
                        adt.tool.name);
                    break;

                case ActionType.AttachTool:
                    ActionAttachTool aa = (ActionAttachTool)action;
                    dec = string.Format("    {0} Tool \"{1}\" attached",  // this action has no actual RAPID instruction, just add a comment
                        commChar,
                        aa.toolName);
                    break;

                case ActionType.DetachTool:
                    ActionDetachTool ad = (ActionDetachTool)action;
                    dec = string.Format("    {0} All tools detached",  // this action has no actual RAPID instruction, just add a comment
                        commChar);
                    break;

                case ActionType.IODigital:
                    ActionIODigital aiod = (ActionIODigital)action;
                    //if (aiod.pinName < 0 || aiod.pinName >= cursor.digitalOutputs.Length)
                    //{
                    //    dec = string.Format("    {0} ERROR on \"{1}\": IO number not available",
                    //        commChar,
                    //        aiod.ToString());
                    //}
                    //else
                    //{
                    //    dec = string.Format("    SetDO {0}, {1};",
                    //    cursor.digitalOutputNames[aiod.pinName],
                    //    aiod.on ? "1" : "0");
                    //}
                    dec = $"    SetDO {aiod.pinName}, {(aiod.on ? "1" : "0")};";
                    break;

                case ActionType.IOAnalog:
                    ActionIOAnalog aioa = (ActionIOAnalog)action;
                    //if (aioa.pinName < 0 || aioa.pinName >= cursor.analogOutputs.Length)
                    //{
                    //    dec = string.Format("    {0} ERROR on \"{1}\": IO number not available",
                    //        commChar,
                    //        aioa.ToString());
                    //}
                    //else
                    //{
                    //    dec = string.Format("    SetAO {0}, {1};",
                    //    cursor.analogOutputNames[aioa.pinName],
                    //    aioa.value);
                    //}
                    dec = $"    SetAO {aioa.pinName}, {aioa.value};";
                    break;


                case ActionType.CustomCode:
                    ActionCustomCode acc = action as ActionCustomCode;
                    if (!acc.isDeclaration)
                    {
                        dec = "    " + acc.statement;
                    }
                    break;

                //default:
                //    dec = string.Format("    ! ACTION \"{0}\" NOT IMPLEMENTED", action);
                //    break;

            }

            if (ADD_ACTION_STRING && action.Type != ActionType.Comment)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    commChar,
                    action.ToString());
            }
            else if (ADD_ACTION_ID)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    commChar,
                    action.Id);
            }

            declaration = dec;
            return dec != null;
        }

        internal bool GenerateInstructionDeclaration(
            Action action,
            RobotCursor cursor,
            Dictionary<double, string> velNames,
            Dictionary<double, string> zoneNames,
            Dictionary<Tool, string> toolNames,
            out string declaration)
        {
            string dec = null;
            switch (action.Type)
            {
                case ActionType.Acceleration:
                    bool zero = cursor.acceleration < Geometry.EPSILON;
                    dec = string.Format("    WorldAccLim {0};",
                        zero ? "\\Off" : "\\On := " + Math.Round(0.001 * cursor.acceleration, Geometry.STRING_ROUND_DECIMALS_M));
                    break;

                //case ActionType.JointSpeed:
                //case ActionType.JointAcceleration:
                //    dec = string.Format("    {0} WARNING: {1}() has no effect in ABB robots.",
                //        commChar,
                //        action.Type);
                //    break;

                // @TODO: push/pop management should be done PROGRAMMATICALLY, not this CHAPUZa...
                case ActionType.PushPop:
                    // Find if there was a change in acceleration, and set the corresponsing instruction...
                    ActionPushPop app = action as ActionPushPop;
                    if (app.push) break;  // only necessary for pops
                    if (Math.Abs(cursor.acceleration - cursor.settingsBuffer.SettingsBeforeLastPop.Acceleration) < Geometry.EPSILON) break;  // no change
                    // If here, there was a change, so...
                    bool zeroAcc = cursor.acceleration < Geometry.EPSILON;
                    dec = string.Format("    WorldAccLim {0};",
                        zeroAcc ? "\\Off" : "\\On := " + Math.Round(0.001 * cursor.acceleration, Geometry.STRING_ROUND_DECIMALS_M));
                    break;

                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("    {0} {1}, {2}, {3}, {4}\\{5};",
                        cursor.motionType == MotionType.Joint ? "MoveJ" : "MoveL",
                        GetUNSAFERobTargetValue(cursor),
                        velNames[cursor.speed],
                        zoneNames[cursor.precision],
                        cursor.tool == null ? "Tool0" : toolNames[cursor.tool],
                        "WObj:=WObj0");
                    break;

                case ActionType.Axes:
                    dec = string.Format("    MoveAbsJ {0}, {1}, {2}, {3}\\{4};",
                        GetJointTargetValue(cursor),
                        velNames[cursor.speed],
                        zoneNames[cursor.precision],
                        cursor.tool == null ? "Tool0" : toolNames[cursor.tool],
                        "WObj:=WObj0");
                    break;

                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("    TPWrite \"{0}\";",
                        am.message.Length <= 80 ?
                            am.message :
                            am.message.Substring(0, 80));  // ABB TPWrite messages can only be 80 chars long
                    break;

                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format("    WaitTime {0};",
                        0.001 * aw.millis);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("    {0} {1}",
                        commChar,
                        ac.comment);
                    break;

                case ActionType.DefineTool:
                    ActionDefineTool adt = action as ActionDefineTool;
                    dec = string.Format("    {0} Tool \"{1}\" defined",  // this action has no actual RAPID instruction, just add a comment
                        commChar,
                        adt.tool.name);
                    break;

                case ActionType.AttachTool:
                    ActionAttachTool aa = (ActionAttachTool)action;
                    dec = string.Format("    {0} Tool \"{1}\" attached",  // this action has no actual RAPID instruction, just add a comment
                        commChar,
                        aa.toolName);
                    break;

                case ActionType.DetachTool:
                    ActionDetachTool ad = (ActionDetachTool)action;
                    dec = string.Format("    {0} All tools detached",  // this action has no actual RAPID instruction, just add a comment
                        commChar);
                    break;

                case ActionType.IODigital:
                    ActionIODigital aiod = (ActionIODigital)action;
                    //if (aiod.pinName < 0 || aiod.pinName >= cursor.digitalOutputs.Length)
                    //{
                    //    dec = string.Format("    {0} ERROR on \"{1}\": IO number not available",
                    //        commChar,
                    //        aiod.ToString());
                    //}
                    //else
                    //{
                    //    dec = string.Format("    SetDO {0}, {1};",
                    //    cursor.digitalOutputNames[aiod.pinName],
                    //    aiod.on ? "1" : "0");
                    //}
                    dec = $"    SetDO {aiod.pinName}, {(aiod.on ? "1" : "0")};";
                    break;

                case ActionType.IOAnalog:
                    ActionIOAnalog aioa = (ActionIOAnalog)action;
                    //if (aioa.pinName < 0 || aioa.pinName >= cursor.analogOutputs.Length)
                    //{
                    //    dec = string.Format("    {0} ERROR on \"{1}\": IO number not available",
                    //        commChar,
                    //        aioa.ToString());
                    //}
                    //else
                    //{
                    //    dec = string.Format("    SetAO {0}, {1};",
                    //    cursor.analogOutputNames[aioa.pinName],
                    //    aioa.value);
                    //}
                    dec = $"    SetAO {aioa.pinName}, {aioa.value};";
                    break;

                case ActionType.CustomCode:
                    ActionCustomCode acc = action as ActionCustomCode;
                    if (!acc.isDeclaration)
                    {
                        dec = "    " + acc.statement;
                    }
                    break;

                //default:
                //    dec = string.Format("    ! ACTION \"{0}\" NOT IMPLEMENTED", action);
                //    break;

            }

            if (ADD_ACTION_STRING && action.Type != ActionType.Comment)
            {
                dec = string.Format("{0}{1}  {2} [{3}]",
                    dec,
                    dec == null ? "  " : "",  // add indentation to align with code
                    commChar,
                    action.ToString());
            }
            else if (ADD_ACTION_ID)
            {
                dec = string.Format("{0}{1}  {2} [{3}]",
                    dec,
                    dec == null ? "  " : "",  // add indentation to align with code
                    commChar,
                    action.Id);
            }

            declaration = dec;
            return dec != null;
        }

        /// <summary>
        /// Returns an RAPID robtarget representation of the current state of the cursor.
        /// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        /// robot configuration and assumes the robot controller will figure out the correct one.
        /// </summary>
        /// <returns></returns>
        static internal string GetUNSAFERobTargetValue(RobotCursor cursor)
        {
            return string.Format("[{0}, {1}, {2}, {3}]",
                cursor.position.ToString(false),
                cursor.rotation.Q.ToString(false),
                "[0,0,0,0]",  // no IK at this moment
                GetExternalJointsValue(cursor)); 
        }

        /// <summary>
        /// Returns an RAPID jointtarget representation of the current state of the cursor.
        /// </summary>
        /// <returns></returns>
        static internal string GetJointTargetValue(RobotCursor cursor)
        {
            return string.Format("[{0}, {1}]", cursor.joints, GetExternalJointsValue(cursor));
        }

        /// <summary>
        /// Returns a RAPID representation of cursor speed.
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        static internal string GetSpeedValue(RobotCursor cursor)
        {
            // ABB format: [TCP linear speed in mm/s, TCP reorientation speed in deg/s, linear external axis speed in mm/s, rotational external axis speed in deg/s]
            // Default linear speeddata are [vel, 500, 5000, 1000], which feels like a lot. 
            // Just use the speed data as linear or rotational value, and stay safe. 
            string vel = Math.Round(cursor.speed, Geometry.STRING_ROUND_DECIMALS_MM).ToString();

            return string.Format("[{0},{1},{2},{3}]", vel, vel, vel, vel);
            
            //// Default speed declarations in ABB always use 500 deg/s as rot speed, but it feels too fast (and scary). 
            //// Using either rotationSpeed value or the same value as lin motion here.
            //return string.Format("[{0},{1},{2},{3}]", 
            //    cursor.speed, 
            //    cursor.rotationSpeed > Geometry.EPSILON2 ? cursor.rotationSpeed : cursor.speed, 
            //    5000, 
            //    1000);

        }

        /// <summary>
        /// Returns a RAPID representatiton of cursor zone.
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        static internal string GetZoneValue(RobotCursor cursor)
        {
            if (cursor.precision < Geometry.EPSILON)
                return "fine";

            // Following conventions for default RAPID zones.
            double high = 1.5 * cursor.precision;
            double low = 0.10 * cursor.precision;
            return string.Format("[FALSE,{0},{1},{2},{3},{4},{5}]", cursor.precision, high, high, low, high, low);
        }

        /// <summary>
        /// Returns a RAPID representation of a Tool object.
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        static internal string GetToolValue(RobotCursor cursor)  //TODO: wouldn't it be just better to pass the Tool object? Inconsistent with the rest of the API...
        {
            if (cursor.tool == null)
            {
                throw new Exception("Cursor has no tool attached");
            }

            return string.Format("[TRUE, [{0},{1}], [{2},{3},{4},0,0,0]]",
                cursor.tool.TCPPosition,
                cursor.tool.TCPOrientation.Q.ToString(false),
                cursor.tool.Weight,
                cursor.tool.CenterOfGravity,
                "[1,0,0,0]");  // no internial axes by default
        }

        /// <summary>
        /// Returns a RAPID representation of an extjoints object
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        static internal string GetExternalJointsValue(RobotCursor cursor)
        {
            if (cursor.externalAxes == null)
            {
                return "[9E9,9E9,9E9,9E9,9E9,9E9]";
            }

            string extj = "[";
            double? val;
            for (int i = 0; i < cursor.externalAxes.Length; i++)
            {
                val = cursor.externalAxes[i];
                extj += (val == null) ? "9E9" : val.ToString();
                if (i < cursor.externalAxes.Length - 1)
                    extj += ",";
            }
            extj += "]";
            return extj;
        }

        static internal string SafeDoubleName(double value) => Math.Round(value, Geometry.STRING_ROUND_DECIMALS_MM).ToString().Replace('.', '_');

    }
}
