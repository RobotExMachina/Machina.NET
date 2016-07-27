using System;
using System.Collections.Generic;

namespace RobotControl
{
    //  ██████╗  ██████╗ ███████╗███╗   ██╗
    //  ██╔══██╗██╔════╝ ██╔════╝████╗  ██║
    //  ██████╔╝██║  ███╗█████╗  ██╔██╗ ██║
    //  ██╔═══╝ ██║   ██║██╔══╝  ██║╚██╗██║
    //  ██║     ╚██████╔╝███████╗██║ ╚████║
    //  ╚═╝      ╚═════╝ ╚══════╝╚═╝  ╚═══╝
    //                                     
    /// <summary>
    /// A class that features methods to translate high-level robot actions into
    /// platform-specific programs. 
    /// </summary>
    internal abstract class ProgramGenerator
    {
        /// <summary>
        /// Creates a textual program representation of a set of Actions using a brand-specific RobotCursor.
        /// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        /// robot configuration and assumes the robot controller will figure out the correct one.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="writePointer"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public abstract List<string> UNSAFEProgramFromActions(string programName, RobotCursor writePointer, List<Action> actions);

        /// <summary>
        /// Given a Path, and constant velocity and zone for all targets, returns a string representation of a RAPID module. Velocity and zone must comply with predefined types.
        /// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        /// robot configuration and assumes the robot controller will figure out the correct one.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="velocity"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        public static List<string> UNSAFEModuleFromPath(Path path, int velocity, int zone)
        {
            string vel = "v" + velocity;
            string zon = "z" + zone;

            List<string> module = new List<string>();

            module.Add("MODULE " + path.Name);
            module.Add("");

            for (int i = 0; i < path.Count; i++)
            {
                Frame t = path.GetTarget(i);
                module.Add("  CONST robtarget Target_" + i
                    + ":=" + UNSAFEExplicitRobTargetDeclaration(path.GetTarget(i)) + ";");
            }

            module.Add("");
            module.Add("  PROC main()");
            module.Add(@"    ConfJ \Off;");
            module.Add(@"    ConfL \Off;");

            for (int i = 0; i < path.Count; i++)
            {
                module.Add("    MoveL Target_" + i
                    + "," + vel
                    + "," + zon
                    + @",Tool0\WObj:=WObj0;");
            }

            module.Add("  ENDPROC");
            module.Add("ENDMODULE");

            return module;
        }

        /// <summary>
        /// Returns a quick and dirty RobTarget declaration out of a Frame object.
        /// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        /// robot configuration and assumes the robot controller will figure out the correct one.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string UNSAFEExplicitRobTargetDeclaration(Frame target)
        {
            //return "[" + target + ",[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]";
            return target.GetUNSAFERobTargetDeclaration();
        }
    }


    //   █████╗ ██████╗ ██████╗ 
    //  ██╔══██╗██╔══██╗██╔══██╗
    //  ███████║██████╔╝██████╔╝
    //  ██╔══██║██╔══██╗██╔══██╗
    //  ██║  ██║██████╔╝██████╔╝
    //  ╚═╝  ╚═╝╚═════╝ ╚═════╝ 
    //                          
    internal class ProgramGeneratorABB : ProgramGenerator
    {
        /// <summary>
        /// A Set of ABB's predefined zone values. 
        /// </summary>
        private static HashSet<int> PredefinedZones = new HashSet<int>()
        {
            0, 1, 5, 10, 15, 20, 30, 40, 50, 60, 80, 100, 150, 200 
        };

        ///// <summary>
        ///// ABB's correspondance of MotionTypes to datatypes.
        ///// </summary>
        //private static Dictionary<MotionType, string> MotionData = new Dictionary<MotionType, string>()
        //{
        //    { MotionType.Linear, "robtarget" },
        //    { MotionType.Joint, "robtarget" },
        //    { MotionType.Joints, "jointtarget" }
        //};

        ///// <summary>
        ///// ABB's correspondance of ActionTypes to datatypes.
        ///// </summary>
        //private static Dictionary<ActionType, string> ActionData = new Dictionary<ActionType, string>()
        //{
        //    { ActionType.Translation, "robtarget" },
        //    { ActionType.Rotation, "robtarget" },
        //    { ActionType.TranslationAndRotation, "robtarget" },
        //    { ActionType.RotationAndTranslation, "robtarget" },
        //    { ActionType.Joints, "jointtarget" },
        //    { ActionType.Message, null },
        //    { ActionType.Wait, null },
        //};

        ///// <summary>
        ///// ABB's correspondance of MotionTypes to instructions.
        ///// </summary>
        //private static Dictionary<MotionType, string> ActionInstructions = new Dictionary<MotionType, string>()
        //{
        //    { MotionType.Linear, "MOVEL" },
        //    { MotionType.Joint, "MOVEJ" },
        //    { MotionType.Joints, "MoveAbsJ" }
        //};

        //static private Dictionary<ActionType, string> ActionInstructions = new Dictionary<ActionType, string>()
        //{
        //    { ActionType.Translation, "MOVEL" },
        //    { ActionType.Rotation, "MOVEL" },
        //    { ActionType.TranslationAndRotation, "MOVEL" },
        //    { ActionType.RotationAndTranslation, "MOVEL" },
        //    { ActionType.Joints, "MoveAbsJ" },
        //    { ActionType.Message, "TPWrite" },
        //    { ActionType.Wait, null },
        //}


        

        /// <summary>
        /// Creates a textual program representation of a set of Actions using a brand-specific RobotCursor.
        /// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        /// robot configuration and assumes the robot controller will figure out the correct one.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="writePointer"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public override List<string> UNSAFEProgramFromActions(string programName, RobotCursor writePointer, List<Action> actions)
        {
            // Cast the robotPointer to the correct subclass
            RobotCursorABB writer = (RobotCursorABB)writePointer;  // @TODO: ask @PAN

            // CODE LINES GENERATION

            // VELOCITY & ZONE DECLARATIONS
            // Figure out how many different ones are there first
            Dictionary<int, string> velNames = new Dictionary<int, string>();
            Dictionary<int, string> velDecs = new Dictionary<int, string>();
            Dictionary<int, string> zoneNames = new Dictionary<int, string>();
            Dictionary<int, string> zoneDecs = new Dictionary<int, string>();
            Dictionary<int, bool> zonePredef = new Dictionary<int, bool>();
            foreach (Action a in actions)
            {
                if (!velNames.ContainsKey(a.speed))
                {
                    velNames.Add(a.speed, "vel" + a.speed);
                    velDecs.Add(a.speed, GenerateSpeedDeclaration(a.speed));
                }

                if (!zoneNames.ContainsKey(a.zone))
                {
                    bool predef = PredefinedZones.Contains(a.zone);
                    zonePredef.Add(a.zone, predef);
                    zoneNames.Add(a.zone, (predef ? "z" : "zone") + a.zone);  // use predef syntax or clean new one
                    zoneDecs.Add(a.zone, predef ? "" : GenerateZoneDeclaration(a.zone));
                }
            }

            List<string> velocityLines = new List<string>();
            foreach (int v in velNames.Keys)
            {
                velocityLines.Add(string.Format("  CONST speeddata {0}:={1};", velNames[v], velDecs[v]));
            }

            List<string> zoneLines = new List<string>();
            foreach (int z in zoneNames.Keys)
            {
                if (!zonePredef[z])  // no need to add declarations for predefined zones
                {
                    zoneLines.Add(string.Format("  CONST zonedata {0}:={1};", zoneNames[z], zoneDecs[z]));
                }
            }


            // TARGETS AND INSTRUCTIONS
            //Dictionary<int, string> variableLines = new Dictionary<int, string>();
            //Dictionary<int, string> instructionLines = new Dictionary<int, string>();
            List<string> variableLines = new List<string>();
            List<string> instructionLines = new List<string>();

            // Use the write robot pointer to generate the data
            int it = 0;
            string line = null;
            foreach (Action a in actions)
            {
                // Move writerCursor to this action state
                writer.ApplyAction(a);

                // Generate lines of code
                if (GenerateVariableDeclaration(a, writer, it, out line))  // there will be a number jump on target-less instructions, but oh well...
                {
                    //variableLines.Add(it, line);
                    variableLines.Add(line);
                };
                if (GenerateInstructionDeclaration(a, writer, it, velNames, zoneNames, out line))  // there will be a number jump on target-less instructions, but oh well...
                {
                    //variableLines.Add(it, line);
                    instructionLines.Add(line);
                };

                // Move on
                it++;
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

        /// <summary>
        /// Returns a speeddata value. 
        /// </summary>
        /// <param name="velocity"></param>
        /// <returns></returns>
        public string GenerateSpeedDeclaration(int velocity)
        {
            // Default speed declarations in ABB always use 500 deg/s as rot speed, but it feels too fast (and scary). 
            // Using the same value as lin motion here.
            return string.Format("[{0},{1},{2},{3}]", velocity, velocity, 5000, 1000);  
        }

        /// <summary>
        /// Returns a zonedata value.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public string GenerateZoneDeclaration(int zone)
        {
            // Following conventions for default RAPID zones.
            double high = 1.5 * zone;
            double low = 0.15 * zone;
            return string.Format("[FALSE,{0},{1},{2},{3},{4},{5}]", zone, high, high, low, high, low);
        }


        static private bool GenerateVariableDeclaration(Action action, RobotCursorABB cursor, int id, out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.TranslationAndRotation:
                case ActionType.RotationAndTranslation:
                    dec = string.Format("  CONST robtarget target{0}:={1};", id, cursor.GetUNSAFERobTargetDeclaration());
                    break;

                case ActionType.Joints:
                    dec = string.Format("  CONST jointtarget target{0}:={1};", id, cursor.GetJointTargetDeclaration());
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
                case ActionType.TranslationAndRotation:
                case ActionType.RotationAndTranslation:
                    dec = string.Format("    {0} target{1},{2},{3},Tool0\\WObj:=WObj0;",
                        action.motionType == MotionType.Joint ? "MoveJ" : "MoveL",
                        id,
                        velNames[action.speed],
                        zoneNames[action.zone]);
                    break;

                case ActionType.Joints:
                    dec = string.Format("    MoveAbsJ target{0},{1},{2},Tool0\\WObj:=WObj0;",
                        id,
                        velNames[action.speed],
                        zoneNames[action.zone]);
                    break;

                case ActionType.Message:
                    dec = string.Format("    TPWrite \"{0}\";", action.message);
                    break;

                case ActionType.Wait:
                    dec = string.Format("    WaitTime {0};", 0.001 * action.waitMillis);
                    break;
            }

            declaration = dec;
            return dec != null;
        }

    }


}
