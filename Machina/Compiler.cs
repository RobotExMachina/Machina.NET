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
    /// <summary>
    /// A class that features methods to translate high-level robot actions into
    /// platform-specific programs. 
    /// </summary>
    internal abstract class Compiler
    {
        /// <summary>
        /// Creates a textual program representation of a set of Actions using a brand-specific RobotCursor.
        /// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        /// robot configuration and assumes the robot controller will figure out the correct one.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="writePointer"></param>
        /// <returns></returns>
        public abstract List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets);
        
    }



    //  ██╗  ██╗██╗   ██╗███╗   ███╗ █████╗ ███╗   ██╗
    //  ██║  ██║██║   ██║████╗ ████║██╔══██╗████╗  ██║
    //  ███████║██║   ██║██╔████╔██║███████║██╔██╗ ██║
    //  ██╔══██║██║   ██║██║╚██╔╝██║██╔══██║██║╚██╗██║
    //  ██║  ██║╚██████╔╝██║ ╚═╝ ██║██║  ██║██║ ╚████║
    //  ╚═╝  ╚═╝ ╚═════╝ ╚═╝     ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝
    /// <summary>
    /// A quick compiler for human-readable instructions.
    /// </summary>
    internal class CompilerHuman : Compiler
    {
        public override List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets)
        {
            // Which pending Actions are used for this program?
            // Copy them without flushing the buffer.
            List<Action> actions = block ?
                writer.actionBuffer.GetBlockPending(false) :
                writer.actionBuffer.GetAllPending(false);


            // ACTION LINES GENERATION
            List<string> actionLines = new List<string>();
            
            // DATA GENERATION
            // Use the write RobotCursor to generate the data
            int it = 0;
            string line = null;
            foreach (Action a in actions)
            {
                // Move writerCursor to this action state
                writer.ApplyNextAction();  // for the buffer to correctly manage them 

                line = string.Format("[{0}] {1}", it, a.ToString());
                actionLines.Add(line);

                // Move on
                it++;
            }


            // PROGRAM ASSEMBLY
            // Initialize a module list
            List<string> module = new List<string>();

            // Code lines
            module.Add("// Machina code for '" + programName + "'");
            module.Add("");
            module.AddRange(actionLines);
            
            return module;
        }
    }




    //   █████╗ ██████╗ ██████╗ 
    //  ██╔══██╗██╔══██╗██╔══██╗
    //  ███████║██████╔╝██████╔╝
    //  ██╔══██║██╔══██╗██╔══██╗
    //  ██║  ██║██████╔╝██████╔╝
    //  ╚═╝  ╚═╝╚═════╝ ╚═════╝                         
    internal class CompilerABB : Compiler
    {
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
        public override List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets)
        {
            // Which pending Actions are used for this program?
            // Copy them without flushing the buffer.
            List<Action> actions = block ?
                writer.actionBuffer.GetBlockPending(false) :
                writer.actionBuffer.GetAllPending(false);


            // CODE LINES GENERATION
            // VELOCITY & ZONE DECLARATIONS
            // Amount of different VZ types
            Dictionary<int, string> velNames = new Dictionary<int, string>();
            Dictionary<int, string> velDecs = new Dictionary<int, string>();
            Dictionary<int, string> zoneNames = new Dictionary<int, string>();
            Dictionary<int, string> zoneDecs = new Dictionary<int, string>();
            Dictionary<int, bool> zonePredef = new Dictionary<int, bool>();

            // TOOL DECLARATIONS
            Dictionary<Tool, string> toolNames = new Dictionary<Tool, string>();
            Dictionary<Tool, string> toolDecs = new Dictionary<Tool, string>();

            // Declarations
            List<string> velocityLines = new List<string>();
            List<string> zoneLines = new List<string>();
            List<string> toolLines = new List<string>();

            // TARGETS AND INSTRUCTIONS
            List<string> variableLines = new List<string>();
            List<string> instructionLines = new List<string>();

            // DATA GENERATION
            // Use the write robot pointer to generate the data
            int it = 0;
            string line = null;
            foreach (Action a in actions)
            {
                // Move writerCursor to this action state
                writer.ApplyNextAction();  // for the buffer to correctly manage them 

                // Check velocity + zone and generate data accordingly
                if (!velNames.ContainsKey(writer.speed))
                {
                    velNames.Add(writer.speed, "vel" + writer.speed);
                    velDecs.Add(writer.speed, GetSpeedValue(writer));
                }

                if (!zoneNames.ContainsKey(writer.zone))
                {
                    bool predef = PredefinedZones.Contains(writer.zone);
                    zonePredef.Add(writer.zone, predef);
                    zoneNames.Add(writer.zone, (predef ? "z" : "zone") + writer.zone);  // use predef syntax or clean new one
                    zoneDecs.Add(writer.zone, predef ? "" : GetZoneValue(writer));
                }

                if (writer.tool != null && !toolNames.ContainsKey(writer.tool))
                {
                    toolNames.Add(writer.tool, writer.tool.name);
                    toolDecs.Add(writer.tool, GetToolValue(writer));
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
                zoneLines.Add(string.Format("  PERS tooldata {0} := {1};", toolNames[t], toolDecs[t]));
            }
            foreach (int v in velNames.Keys)
            {
                velocityLines.Add(string.Format("  CONST speeddata {0} := {1};", velNames[v], velDecs[v]));
            }
            foreach (int z in zoneNames.Keys)
            {
                if (!zonePredef[z])  // no need to add declarations for predefined zones
                {
                    zoneLines.Add(string.Format("  CONST zonedata {0} := {1};", zoneNames[z], zoneDecs[z]));
                }
            }
            


            // PROGRAM ASSEMBLY
            // Initialize a module list
            List<string> module = new List<string>();

            // MODULE HEADER
            module.Add("MODULE " + programName);
            module.Add("");

            // VARIABLE DECLARATIONS
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
        static private bool GenerateVariableDeclaration(Action action, RobotCursor cursor, int id, out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  CONST robtarget target{0} := {1};", id, GetUNSAFERobTargetValue(cursor));
                    break;

                case ActionType.Joints:
                    dec = string.Format("  CONST jointtarget target{0} := {1};", id, GetJointTargetValue(cursor));
                    break;
            }

            declaration = dec;
            return dec != null;
        }

        static private bool GenerateInstructionDeclarationFromVariable(
            Action action, 
            RobotCursor cursor, 
            int id,
            Dictionary<int, string> velNames, 
            Dictionary<int, string> zoneNames,
            Dictionary<Tool, string> toolNames,
            out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("    {0} target{1}, {2}, {3}, {4}\\{5};  ! [{6}]",
                        cursor.motionType == MotionType.Joint ? "MoveJ" : "MoveL",
                        id,
                        velNames[cursor.speed],
                        zoneNames[cursor.zone],
                        cursor.tool == null ? "Tool0" : toolNames[cursor.tool],
                        "WObj:=WObj0",
                        action.id);
                    break;

                case ActionType.Joints:
                    dec = string.Format("    MoveAbsJ target{0}, {1}, {2}, {3}\\{4};  ! [{5}]",
                        id,
                        velNames[cursor.speed],
                        zoneNames[cursor.zone],
                        cursor.tool == null ? "Tool0" : toolNames[cursor.tool],
                        "WObj:=WObj0",
                        action.id);
                    break;

                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("    TPWrite \"{0}\";", am.message);
                    break;

                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format("    WaitTime {0};", 0.001 * aw.millis);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("    ! {0}", ac.comment);
                    break;

                case ActionType.Attach:
                    ActionAttach aa = (ActionAttach)action;
                    dec = string.Format("    ! Tool \"{0}\" attached", aa.tool.name);  // this action has no actual RAPID instruction, just add a comment
                    break;

                case ActionType.Detach:
                    ActionDetach ad = (ActionDetach)action;
                    dec = string.Format("    ! Tool detached");  // this action has no actual RAPID instruction, just add a comment
                    break;

                //default:
                //    dec = string.Format("    ! ACTION \"{0}\" NOT IMPLEMENTED", action);
                //    break;
            }

            declaration = dec;
            return dec != null;
        }

        static private bool GenerateInstructionDeclaration(
            Action action, 
            RobotCursor cursor,
            Dictionary<int, string> velNames, 
            Dictionary<int, string> zoneNames,
            Dictionary<Tool, string> toolNames,
            out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("    {0} {1}, {2}, {3}, {4}\\{5};  ! [{6}]",
                        cursor.motionType == MotionType.Joint ? "MoveJ" : "MoveL",
                        GetUNSAFERobTargetValue(cursor),
                        velNames[cursor.speed],
                        zoneNames[cursor.zone],
                        cursor.tool == null ? "Tool0" : toolNames[cursor.tool],
                        "WObj:=WObj0",
                        action.id);
                    break;

                case ActionType.Joints:
                    dec = string.Format("    MoveAbsJ {0}, {1}, {2}, {3}\\{4};  ! [{5}]",
                        GetJointTargetValue(cursor),
                        velNames[cursor.speed],
                        zoneNames[cursor.zone],
                        cursor.tool == null ? "Tool0" : toolNames[cursor.tool],
                        "WObj:=WObj0",
                        action.id);
                    break;

                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("    TPWrite \"{0}\";  ! [{1}]", 
                        am.message,
                        action.id);
                    break;

                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format("    WaitTime {0};  ! [{1}]", 
                        0.001 * aw.millis,
                        action.id);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("    ! {0}", ac.comment);
                    break;

                case ActionType.Attach:
                    ActionAttach aa = (ActionAttach)action;
                    dec = string.Format("    ! Tool \"{0}\" attached [{1}]",  // this action has no actual RAPID instruction, just add a comment
                        aa.tool.name,
                        aa.id);  
                    break;

                case ActionType.Detach:
                    ActionDetach ad = (ActionDetach)action;
                    dec = string.Format("    ! Tools detached [{0}]",   // this action has no actual RAPID instruction, just add a comment
                        ad.id); 
                    break;

                //default:
                //    dec = string.Format("    ! ACTION \"{0}\" NOT IMPLEMENTED", action);
                //    break;

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
        static public string GetUNSAFERobTargetValue(RobotCursor cursor)
        {
            return string.Format("[{0}, {1}, {2}, {3}]",
                cursor.position.ToString(false),
                cursor.rotation.Q.ToString(false),
                "[0,0,0,0]",  // no IK at this moment
                "[0,9E9,9E9,9E9,9E9,9E9]");  // no external axes at this moment
        }

        /// <summary>
        /// Returns an RAPID jointtarget representation of the current state of the cursor.
        /// </summary>
        /// <returns></returns>
        static public string GetJointTargetValue(RobotCursor cursor)
        {
            return string.Format("[{0}, [0,9E9,9E9,9E9,9E9,9E9]]", cursor.joints);
        }

        /// <summary>
        /// Returns a RAPID representation of cursor speed.
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        static public string GetSpeedValue(RobotCursor cursor)
        {
            // Default speed declarations in ABB always use 500 deg/s as rot speed, but it feels too fast (and scary). 
            // Using the same value as lin motion here.
            return string.Format("[{0},{1},{2},{3}]", cursor.speed, cursor.speed, 5000, 1000);
        }

        /// <summary>
        /// Returns a RAPID representatiton of cursor zone.
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        static public string GetZoneValue(RobotCursor cursor)
        {
            // Following conventions for default RAPID zones.
            double high = 1.5 * cursor.zone;
            double low = 0.15 * cursor.zone;
            return string.Format("[FALSE,{0},{1},{2},{3},{4},{5}]", cursor.zone, high, high, low, high, low);
        }

        /// <summary>
        /// Returns a RAPID representation of a Tool object.
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        static public string GetToolValue(RobotCursor cursor)  //TODO: wouldn't it be just better to pass the Tool object? Inconsistent with the rest of the API...
        {
            if (cursor.tool == null)
            {
                throw new Exception("Cursor has no tool attached");
            }

            return string.Format("[TRUE, [{0},{1}], [{2},{3},{4},0,0,0]]",
                cursor.tool.TCPPosition,
                cursor.tool.TCPOrientation.Q.ToString(false),
                cursor.tool.weight,
                cursor.tool.centerOfGravity,
                "[1,0,0,0]");  // no internial axes by default
        }

    }





    //  ██╗   ██╗██████╗ 
    //  ██║   ██║██╔══██╗
    //  ██║   ██║██████╔╝
    //  ██║   ██║██╔══██╗
    //  ╚██████╔╝██║  ██║
    //   ╚═════╝ ╚═╝  ╚═╝
    internal class CompilerUR : Compiler
    {
        /// <summary>
        /// Creates a textual program representation of a set of Actions using native UR Script.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="writePointer"></param>
        /// <param name="block">Use actions in waiting queue or buffer?</param>
        /// <returns></returns>
        //public override List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writePointer, bool block)
        public override List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets)
        {
            // Which pending Actions are used for this program?
            // Copy them without flushing the buffer.
            List<Action> actions = block ?
                writer.actionBuffer.GetBlockPending(false) :
                writer.actionBuffer.GetAllPending(false);


            // CODE LINES GENERATION
            // TARGETS AND INSTRUCTIONS
            List<string> variableLines = new List<string>();
            List<string> instructionLines = new List<string>();

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
                    if (GenerateInstructionDeclaration(a, writer, out line))  // there will be a number jump on target-less instructions, but oh well...
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

            // MODULE HEADER
            module.Add("def " + programName + "():");
            module.Add("");

            // Targets
            if (variableLines.Count != 0)
            {
                module.AddRange(variableLines);
                module.Add("");
            }

            // MAIN PROCEDURE
            // Instructions
            if (instructionLines.Count != 0)
            {
                module.AddRange(instructionLines);
                module.Add("");
            }

            module.Add("end");
            module.Add("");

            // MODULE KICKOFF
            module.Add(programName + "()");

            return module;
        }




        //  ╦ ╦╔╦╗╦╦  ╔═╗
        //  ║ ║ ║ ║║  ╚═╗
        //  ╚═╝ ╩ ╩╩═╝╚═╝
        static public bool GenerateVariableDeclaration(Action action, RobotCursor cursor, int id, out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  target{0}={1}", id, GetPoseTargetValue(cursor));
                    break;

                case ActionType.Joints:
                    dec = string.Format("  target{0}={1}", id, GetJointTargetValue(cursor));
                    break;
            }

            declaration = dec;
            return dec != null;
        }

        static public bool GenerateInstructionDeclarationFromVariable(
            Action action, RobotCursor cursor, int id,
            out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  {0}(target{1}, a=1, v={2}, r={3})",
                        cursor.motionType == MotionType.Joint ? "movej" : "movel",
                        id,
                        Math.Round(0.001 * cursor.speed, 3 + Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(0.001 * cursor.zone, 3 + Geometry.STRING_ROUND_DECIMALS_MM));
                    break;

                case ActionType.Joints:
                    // HAL generates a "set_tcp(p[0,0,0,0,0,0])" call here which I find confusing... 
                    dec = string.Format("  {0}(target{1}, a=1, v={2}, r={3})",
                        "movej",
                        id,
                        Math.Round(0.001 * cursor.speed, 3 + Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(0.001 * cursor.zone, 3 + Geometry.STRING_ROUND_DECIMALS_MM));
                    break;

                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("  popup(\"{0}\", title=\"{0}\", warning=False, error=False)",
                        am.message);
                    break;

                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format("  sleep({0})",
                        0.001 * aw.millis);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("  # {0}", ac.comment);
                    break;

                case ActionType.Attach:
                    ActionAttach aa = (ActionAttach)action;
                    dec = string.Format("  set_tcp({0})  # [{1}]",  // @TODO: should need to add a "set_payload(m, CoG)" dec afterwards...
                        GetToolValue(cursor),
                        aa.id);
                    break;

                case ActionType.Detach:
                    ActionDetach ad = (ActionDetach)action;
                    dec = string.Format("  set_tcp(p[0,0,0,0,0,0])  # [{0}]",  // @TODO: should need to add a "set_payload(m, CoG)" dec afterwards...
                        ad.id);
                    break;

                    //default:
                    //    dec = string.Format("  # ACTION \"{0}\" NOT IMPLEMENTED", action);
                    //    break;
            }

            declaration = dec;
            return dec != null;
        }

        static public bool GenerateInstructionDeclaration(
            Action action, RobotCursor cursor,
            out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  {0}({1}, a=1, v={2}, r={3})  # [{4}]",
                        cursor.motionType == MotionType.Joint ? "movej" : "movel",
                        GetPoseTargetValue(cursor),
                        Math.Round(0.001 * cursor.speed, 3 + Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(0.001 * cursor.zone, 3 + Geometry.STRING_ROUND_DECIMALS_MM),
                        action.id);
                    break;

                case ActionType.Joints:
                    // HAL generates a "set_tcp(p[0,0,0,0,0,0])" call here which I find confusing... 
                    dec = string.Format("  {0}({1}, a=1, v={2}, r={3})  # [{4}]",
                        "movej",
                        GetJointTargetValue(cursor),
                        Math.Round(0.001 * cursor.speed, 3 + Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(0.001 * cursor.zone, 3 + Geometry.STRING_ROUND_DECIMALS_MM),
                        action.id);
                    break;

                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("  popup(\"{0}\", title=\"{0}\", warning=False, error=False)  # [{1}]",
                        am.message,
                        am.id);
                    break;

                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format("  sleep({0})  # [{1}]",
                        0.001 * aw.millis,
                        aw.id);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("  # {0}", 
                        ac.comment);
                    break;

                case ActionType.Attach:
                    ActionAttach aa = (ActionAttach)action;
                    dec = string.Format("  set_tcp({0})  # [{1}]",   // @TODO: should need to add a "set_payload(m, CoG)" dec afterwards...
                        GetToolValue(cursor),
                        aa.id);
                    break;

                case ActionType.Detach:
                    ActionDetach ad = (ActionDetach)action;
                    dec = string.Format("  set_tcp(p[0,0,0,0,0,0])  # [{0}]",   // @TODO: should need to add a "set_payload(m, CoG)" dec afterwards...
                        ad.id);
                    break;

                    //default:
                    //    dec = string.Format("  # ACTION \"{0}\" NOT IMPLEMENTED", action);
                    //    break;
            }

            declaration = dec;
            return dec != null;
        }




        /// <summary>
        /// Returns an UR pose representation of the current state of the cursor.
        /// </summary>
        /// <returns></returns>
        static public string GetPoseTargetValue(RobotCursor cursor)
        {
            RotationVector axisAng = cursor.rotation.GetRotationVector(true);
            return string.Format("p[{0},{1},{2},{3},{4},{5}]",
                Math.Round(0.001 * cursor.position.X, Geometry.STRING_ROUND_DECIMALS_M),
                Math.Round(0.001 * cursor.position.Y, Geometry.STRING_ROUND_DECIMALS_M),
                Math.Round(0.001 * cursor.position.Z, Geometry.STRING_ROUND_DECIMALS_M),
                Math.Round(axisAng.X, Geometry.STRING_ROUND_DECIMALS_RADS),
                Math.Round(axisAng.Y, Geometry.STRING_ROUND_DECIMALS_RADS),
                Math.Round(axisAng.Z, Geometry.STRING_ROUND_DECIMALS_RADS));
        }

        /// <summary>
        /// Returns a UR joint representation of the current state of the cursor.
        /// </summary>
        /// <returns></returns>
        static public string GetJointTargetValue(RobotCursor cursor)
        {
            Joints jrad = new Joints(cursor.joints);  // use a shallow copy
            Console.WriteLine(jrad);
            jrad.Scale(Math.PI / 180);  // convert to radians
            Console.WriteLine(jrad);
            return string.Format("[{0},{1},{2},{3},{4},{5}]",
                Math.Round(jrad.J1, Geometry.STRING_ROUND_DECIMALS_RADS),
                Math.Round(jrad.J2, Geometry.STRING_ROUND_DECIMALS_RADS),
                Math.Round(jrad.J3, Geometry.STRING_ROUND_DECIMALS_RADS),
                Math.Round(jrad.J4, Geometry.STRING_ROUND_DECIMALS_RADS),
                Math.Round(jrad.J5, Geometry.STRING_ROUND_DECIMALS_RADS),
                Math.Round(jrad.J6, Geometry.STRING_ROUND_DECIMALS_RADS));
        }

        /// <summary>
        /// Returns a UR representation of a Tool object.
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        static public string GetToolValue(RobotCursor cursor)  //TODO: wouldn't it be just better to pass the Tool object? Inconsistent with the rest of the API...
        {
            if (cursor.tool == null)
            {
                throw new Exception("Cursor has no tool attached");
            }

            RotationVector axisAng = cursor.tool.TCPOrientation.Q.ToRotationVector(true);

            return string.Format("p[{0},{1},{2},{3},{4},{5}]",
                Math.Round(0.001 * cursor.tool.TCPPosition.X, Geometry.STRING_ROUND_DECIMALS_M),
                Math.Round(0.001 * cursor.tool.TCPPosition.Y, Geometry.STRING_ROUND_DECIMALS_M),
                Math.Round(0.001 * cursor.tool.TCPPosition.Z, Geometry.STRING_ROUND_DECIMALS_M),
                Math.Round(axisAng.X, Geometry.STRING_ROUND_DECIMALS_RADS),
                Math.Round(axisAng.Y, Geometry.STRING_ROUND_DECIMALS_RADS),
                Math.Round(axisAng.Z, Geometry.STRING_ROUND_DECIMALS_RADS));


        }

    }




    //  ██╗  ██╗██╗   ██╗██╗  ██╗ █████╗ 
    //  ██║ ██╔╝██║   ██║██║ ██╔╝██╔══██╗
    //  █████╔╝ ██║   ██║█████╔╝ ███████║
    //  ██╔═██╗ ██║   ██║██╔═██╗ ██╔══██║
    //  ██║  ██╗╚██████╔╝██║  ██╗██║  ██║
    //  ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝
    internal class CompilerKUKA : Compiler
    {
        /// <summary>
        /// Creates a textual program representation of a set of Actions using native KUKA Robot Language.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="writePointer"></param>
        /// <param name="block">Use actions in waiting queue or buffer?</param>
        /// <returns></returns>
        //public override List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writePointer, bool block)
        public override List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets)
        {
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
            declarationLines.Add("  EXT BAS (BAS_COMMAND :IN, REAL :IN)");  // import BAS sys function

            initializationLines.Add("  GLOBAL INTERRUPT DECL 3 WHEN $STOPMESS==TRUE DO IR_STOPM()");  // legacy support for user-programming safety
            initializationLines.Add("  INTERRUPT ON 3");
            initializationLines.Add("  BAS (#INITMOV, 0)");  // use base function to initialize sys vars to defaults
            initializationLines.Add("  $TOOL = {X 0, Y 0, Z 0, A 0, B 0, C 0}");  // no tool
            initializationLines.Add("  $LOAD.M = 1");
            initializationLines.Add("  $LOAD.CM = {X 0, Y 0, Z 0, A 0, B 0, C 0}");

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

            // MODULE HEADER
            module.Add("DEF " + programName + "():");
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
        static public bool GenerateVariableDeclaration(Action action, RobotCursor cursor, int id, out string declaration)
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

        static public bool GenerateVariableInitialization(Action action, RobotCursor cursor, int id, out string declaration)
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

        static public bool GenerateInstructionDeclarationFromVariable(
            Action action, RobotCursor cursor, int id,
            out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                // KUKA does explicit setting of velocities and approximate positioning, so these actions make sense as instructions
                case ActionType.Speed:
                    dec = string.Format("  $VEL = {{CP {0}, ORI1 100, ORI2 100}}  ; [{1}]",
                        Math.Round(0.001 * cursor.speed, 3 + Geometry.STRING_ROUND_DECIMALS_MM),
                        action.id);
                    break;

                case ActionType.Zone:
                    dec = string.Format("  $APO.CDIS = {0}  ; [{1}]",
                        cursor.zone,
                        action.id);
                    break;

                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  {0} target{1} {2}  ; [{3}]",
                        cursor.motionType == MotionType.Joint ? "PTP" : "LIN",
                        id,
                        cursor.zone >= 1 ? "C_DIS" : "",
                        action.id);
                    break;

                case ActionType.Joints:
                    dec = string.Format("  {0} target{1} {2}  ; [{3}]",
                        "PTP",
                        id,
                        cursor.zone >= 1 ? "C_DIS" : "",  // @TODO: figure out how to turn this into C_PTP
                        action.id);
                    break;

                // @TODO: apparently, messages in KRL are kind fo tricky, with several manuals just dedicated to it.
                // Will figure this out later.
                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("  ; MESSAGE: \"{0}\" [{1}] (messages in KRL currently not supported in Machina)",
                        am.message,
                        am.id);
                    break;

                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format("  WAIT SEC {0}  ; [{1}]",
                        0.001 * aw.millis,
                        aw.id);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("  ; {0} [{1}]",
                        ac.comment,
                        ac.id);
                    break;

                case ActionType.Attach:
                    ActionAttach at = (ActionAttach)action;
                    dec = string.Format("  $TOOL = {0}  ; [{1}]",
                        GetToolValue(cursor),
                        at.id);
                    break;

                case ActionType.Detach:
                    ActionDetach ad = (ActionDetach)action;
                    dec = string.Format("  $TOOL = $NULLFRAME  ; [{0}]",
                        ad.id);
                    break;

                    //default:
                    //    dec = string.Format("  ; ACTION \"{0}\" NOT IMPLEMENTED", action);
                    //    break;
            }

            declaration = dec;
            return dec != null;
        }


        static public bool GenerateInstructionDeclaration(
            Action action, RobotCursor cursor,
            out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                // KUKA does explicit setting of velocities and approximate positioning, so these actions make sense as instructions
                case ActionType.Speed:
                    dec = string.Format("  $VEL = {{CP {0}, ORI1 100, ORI2 100}}  ; [{1}]",
                        Math.Round(0.001 * cursor.speed, 3 + Geometry.STRING_ROUND_DECIMALS_MM),
                        action.id);
                    break;

                case ActionType.Zone:
                    dec = string.Format("  $APO.CDIS = {0}  ; [{1}]",
                        cursor.zone,
                        action.id);
                    break;

                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  {0} {1} {2}  ; [{3}]",
                        cursor.motionType == MotionType.Joint ? "PTP" : "LIN",
                        GetPositionTargetValue(cursor),
                        cursor.zone >= 1 ? "C_DIS" : "",
                        action.id);
                    break;

                case ActionType.Joints:
                    dec = string.Format("  {0} {1} {2}  ; [{3}]",
                        "PTP",
                        GetAxisTargetValue(cursor),
                        cursor.zone >= 1 ? "C_DIS" : "",  // @TODO: figure out how to turn this into C_PTP
                        action.id);
                    break;

                // @TODO: apparently, messages in KRL are kind fo tricky, with several manuals just dedicated to it.
                // Will figure this out later.
                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("  ; MESSAGE: \"{0}\" [{1}] (messages in KRL currently not supported in Machina)",
                        am.message,
                        am.id);
                    break;

                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format("  WAIT SEC {0}  ; [{1}]",
                        0.001 * aw.millis,
                        aw.id);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("  ; {0} [{1}]",
                        ac.comment,
                        ac.id);
                    break;


                case ActionType.Attach:
                    ActionAttach at = (ActionAttach)action;
                    dec = string.Format("  $TOOL = {0}  ; [{1}]",
                        GetToolValue(cursor),
                        at.id);
                    break;

                case ActionType.Detach:
                    ActionDetach ad = (ActionDetach)action;
                    dec = string.Format("  $TOOL = $NULLFRAME  ; [{0}]",
                        ad.id);
                    break;

                    //default:
                    //    dec = string.Format("  ; ACTION \"{0}\" NOT IMPLEMENTED", action);
                    //    break;
            }

            declaration = dec;
            return dec != null;
        }




        /// <summary>
        /// Returns a KRL FRAME representation of the current state of the cursor.
        /// Note POS also accept T and S parameters for unambiguous arm configuration def. @TODO: implement?
        /// </summary>
        /// <returns></returns>
        static public string GetPositionTargetValue(RobotCursor cursor)
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
        static public string GetAxisTargetValue(RobotCursor cursor)
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
        static public string GetToolValue(RobotCursor cursor)
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
