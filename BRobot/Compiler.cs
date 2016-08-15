using System;
using System.Collections.Generic;

namespace BRobot
{
    //   ██████╗ ██████╗ ███╗   ███╗██████╗ ██╗██╗     ███████╗██████╗ 
    //  ██╔════╝██╔═══██╗████╗ ████║██╔══██╗██║██║     ██╔════╝██╔══██╗
    //  ██║     ██║   ██║██╔████╔██║██████╔╝██║██║     █████╗  ██████╔╝
    //  ██║     ██║   ██║██║╚██╔╝██║██╔═══╝ ██║██║     ██╔══╝  ██╔══██╗
    //  ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║     ██║███████╗███████╗██║  ██║
    //   ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝     ╚═╝╚══════╝╚══════╝╚═╝  ╚═╝
    //                                                                 
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
        /// 
        /// <returns></returns>
        public abstract List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writePointer, bool block);
        

    }


    //   █████╗ ██████╗ ██████╗ 
    //  ██╔══██╗██╔══██╗██╔══██╗
    //  ███████║██████╔╝██████╔╝
    //  ██╔══██║██╔══██╗██╔══██╗
    //  ██║  ██║██████╔╝██████╔╝
    //  ╚═╝  ╚═╝╚═════╝ ╚═════╝ 
    //                          
    internal class CompilerABB : Compiler
    {
        /// <summary>
        /// A Set of ABB's predefined zone values. 
        /// </summary>
        private static HashSet<int> PredefinedZones = new HashSet<int>()
        {
            0, 1, 5, 10, 15, 20, 30, 40, 50, 60, 80, 100, 150, 200 
        };

        /// <summary>
        /// Creates a textual program representation of a set of Actions using a brand-specific RobotCursor.
        /// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        /// robot configuration and assumes the robot controller will figure out the correct one.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="writePointer"></param>
        /// <param name="block">Use actions in waiting queue or buffer?</param>
        /// <returns></returns>
        public override List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writePointer, bool block)
        {
            // Cast the robotPointer to the correct subclass
            RobotCursorABB writer = (RobotCursorABB)writePointer;

            // Which pending Actions are used for this program?
            // Copy them without flushing the buffer.
            List<Action> actions = block ? 
                writePointer.buffer.GetBlockPending(false) : 
                writePointer.buffer.GetAllPending(false);


            // CODE LINES GENERATION
            // VELOCITY & ZONE DECLARATIONS
            // Amount of different VZ types
            Dictionary<int, string> velNames = new Dictionary<int, string>();
            Dictionary<int, string> velDecs = new Dictionary<int, string>();
            Dictionary<int, string> zoneNames = new Dictionary<int, string>();
            Dictionary<int, string> zoneDecs = new Dictionary<int, string>();
            Dictionary<int, bool> zonePredef = new Dictionary<int, bool>();
            // Declarations
            List<string> velocityLines = new List<string>();
            List<string> zoneLines = new List<string>();

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
                    velDecs.Add(writer.speed, writer.GetSpeedDeclaration(writer.speed));
                }

                if (!zoneNames.ContainsKey(writer.zone))
                {
                    bool predef = PredefinedZones.Contains(writer.zone);
                    zonePredef.Add(writer.zone, predef);
                    zoneNames.Add(writer.zone, (predef ? "z" : "zone") + writer.zone);  // use predef syntax or clean new one
                    zoneDecs.Add(writer.zone, predef ? "" : writer.GetZoneDeclaration(writer.zone));
                }

                // Generate lines of code
                if (GenerateVariableDeclaration(a, writer, it, out line))  // there will be a number jump on target-less instructions, but oh well...
                {
                    variableLines.Add(line);
                }

                if (GenerateInstructionDeclaration(a, writer, it, velNames, zoneNames, out line))  // there will be a number jump on target-less instructions, but oh well...
                {
                    instructionLines.Add(line);
                }

                // Move on
                it++;
            }


            // Generate V+Z 
            foreach (int v in velNames.Keys)
            {
                velocityLines.Add(string.Format("  CONST speeddata {0}:={1};", velNames[v], velDecs[v]));
            }
            foreach (int z in zoneNames.Keys)
            {
                if (!zonePredef[z])  // no need to add declarations for predefined zones
                {
                    zoneLines.Add(string.Format("  CONST zonedata {0}:={1};", zoneNames[z], zoneDecs[z]));
                }
            }






            //foreach (Action a in actions)
            //{
            //    if (!velNames.ContainsKey(a.speed))
            //    {
            //        velNames.Add(a.speed, "vel" + a.speed);
            //        velDecs.Add(a.speed, writer.GetSpeedDeclaration(a.speed));
            //    }

            //    if (!zoneNames.ContainsKey(a.zone))
            //    {
            //        bool predef = PredefinedZones.Contains(a.zone);
            //        zonePredef.Add(a.zone, predef);
            //        zoneNames.Add(a.zone, (predef ? "z" : "zone") + a.zone);  // use predef syntax or clean new one
            //        zoneDecs.Add(a.zone, predef ? "" : writer.GetZoneDeclaration(a.zone));
            //    }
            //}

            //List<string> velocityLines = new List<string>();
            //foreach (int v in velNames.Keys)
            //{
            //    velocityLines.Add(string.Format("  CONST speeddata {0}:={1};", velNames[v], velDecs[v]));
            //}

            //List<string> zoneLines = new List<string>();
            //foreach (int z in zoneNames.Keys)
            //{
            //    if (!zonePredef[z])  // no need to add declarations for predefined zones
            //    {
            //        zoneLines.Add(string.Format("  CONST zonedata {0}:={1};", zoneNames[z], zoneDecs[z]));
            //    }
            //}


            //// TARGETS AND INSTRUCTIONS
            //List<string> variableLines = new List<string>();
            //List<string> instructionLines = new List<string>();

            //// Use the write robot pointer to generate the data
            //int it = 0;
            //string line = null;
            //foreach (Action a in actions)
            //{
            //    // Move writerCursor to this action state
            //    writer.ApplyNextAction();  // for the buffer to correctly manage them 

            //    // Generate lines of code
            //    if (GenerateVariableDeclaration(a, writer, it, out line))  // there will be a number jump on target-less instructions, but oh well...
            //    {
            //        //variableLines.Add(it, line);
            //        variableLines.Add(line);
            //    };
            //    if (GenerateInstructionDeclaration(a, writer, it, velNames, zoneNames, out line))  // there will be a number jump on target-less instructions, but oh well...
            //    {
            //        //variableLines.Add(it, line);
            //        instructionLines.Add(line);
            //    };

            //    // Move on
            //    it++;
            //}

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








        static private bool GenerateVariableDeclaration(Action action, RobotCursorABB cursor, int id, out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("  CONST robtarget target{0}:={1};", id, cursor.GetUNSAFERobTargetValue());
                    break;

                case ActionType.Joints:
                    dec = string.Format("  CONST jointtarget target{0}:={1};", id, cursor.GetJointTargetValue());
                    break;
            }

            declaration = dec;
            return dec != null;
        }

        static private bool GenerateInstructionDeclaration(
            Action action, RobotCursorABB cursor, int id, 
            Dictionary<int, string> velNames, Dictionary<int, string> zoneNames, 
            out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    dec = string.Format("    {0} target{1},{2},{3},Tool0\\WObj:=WObj0;",
                        cursor.motionType == MotionType.Joint ? "MoveJ" : "MoveL",
                        id,
                        velNames[cursor.speed],
                        zoneNames[cursor.zone]);
                    break;

                case ActionType.Joints:
                    dec = string.Format("    MoveAbsJ target{0},{1},{2},Tool0\\WObj:=WObj0;",
                        id,
                        velNames[cursor.speed],
                        zoneNames[cursor.zone]);
                    break;

                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("    TPWrite \"{0}\";", am.message);
                    break;

                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format("    WaitTime {0};", 0.001 * aw.millis);
                    break;
            }

            declaration = dec;
            return dec != null;
        }

    }


}
