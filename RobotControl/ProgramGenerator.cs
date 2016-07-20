using System;
using System.Collections.Generic;

namespace RobotControl
{
    /// <summary>
    /// A class that features methods to translate high-level robot actions into
    /// platform-specific programs. 
    /// </summary>
    internal abstract class ProgramGenerator
    {

        public abstract List<string> UNSAFEProgramFromActions(string programName, RobotPointer writePointer, List<Action> actions);


        /// <summary>
        /// Given a Path, and constant velocity and zone for all targets, returns a string representation of a RAPID module. Velocity and zone must comply with predefined types.
        /// WARNING: this method is EXTREMELY UNSAFE, since it performs no IK calculations, assuming all targets are in the positive XYZ octant hence a robot configuration of [0,0,0,0]. To be extended with a proper module creator
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
        /// WARNING: this method is extremely unsafe! It assumes the target is in the positive XYZ octant 
        /// and assigns a [0,0,0,0] robot configuration to the target. Also, it performs no FK/IK nor security checks.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string UNSAFEExplicitRobTargetDeclaration(Frame target)
        {
            //return "[" + target + ",[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]";
            return target.GetUNSAFERobTargetDeclaration();
        }
    }

    internal class ProgramGeneratorABB : ProgramGenerator
    {
        private static HashSet<int> PredefinedZones = new HashSet<int>()
        {
            0, 1, 5, 10, 15, 20, 30, 40, 50, 60, 80, 100, 150, 200 
        };

        private static Dictionary<MotionType, string> MotionInstructions = new Dictionary<MotionType, string>()
        {
            { MotionType.Linear, "MOVEL" },
            { MotionType.Joint, "MOVEJ" },
            { MotionType.Joints, "MoveAbsJ" }
        };


        public override List<string> UNSAFEProgramFromActions(string programName, RobotPointer writePointer, List<Action> actions)
        {
            // Cast the robotPointer to the correct subclass
            RobotPointerABB writer = (RobotPointerABB)writePointer;  // @TODO: ask @PAN

            // Initialize a module list
            List<string> module = new List<string>();

            // MODULE HEADER
            module.Add("MODULE " + programName);
            module.Add("");

            // VELOCITY & ZONE DECLARATIONS
            // Figure out how many different ones are there first
            Dictionary<int, string> velNames = new Dictionary<int, string>();
            Dictionary<int, string> velDecs = new Dictionary<int, string>();
            Dictionary<int, string> zoneNames = new Dictionary<int, string>();
            Dictionary<int, string> zoneDecs = new Dictionary<int, string>();
            Dictionary<int, bool> zonePredef = new Dictionary<int, bool>();
            foreach (Action a in actions)
            {
                if (!velNames.ContainsKey(a.velocity))
                {
                    velNames.Add(a.velocity, "vel" + a.velocity);
                    velDecs.Add(a.velocity, GenerateSpeedDeclaration(a.velocity));
                }

                if (!zoneNames.ContainsKey(a.zone))
                {
                    bool predef = PredefinedZones.Contains(a.zone);
                    zonePredef.Add(a.zone, predef);
                    zoneNames.Add(a.zone, (predef ? "z" : "zone") + a.zone);  // use predef syntax or clean new one
                    zoneDecs.Add(a.zone, predef ? "" : GenerateZoneDeclaration(a.zone));
                }
            }

            foreach(int v in velNames.Keys)
            {
                module.Add(string.Format("  CONST speeddata {0}:={1};", velNames[v], velDecs[v]));
            }
            module.Add("");

            bool anyNonPredef = false;
            foreach (int z in zoneNames.Keys)
            {
                if(!zonePredef[z])  // no need to add declarations for predefined zones
                {
                    module.Add(string.Format("  CONST zonedata {0}:={1};", zoneNames[z], zoneDecs[z]));
                    anyNonPredef = true;
                }
            }
            if (anyNonPredef) module.Add("");

            // TARGET DECLARATIONS
            // Use the write robot pointer to generate instructions
            int it = 0;
            foreach (Action a in actions)
            {
                writer.ApplyAction(a);
                module.Add( string.Format("  CONST robtarget target{0}:={1};", 
                    it++,
                    writer.GetUNSAFERobTargetDeclaration()) );
            }
            module.Add("");

            // MAIN PROCEDURE
            module.Add("  PROC main()");
            module.Add(@"    ConfJ \Off;");
            module.Add(@"    ConfL \Off;");
            it = 0;
            foreach (Action a in actions)
            {
                module.Add(string.Format("    {0} target{1},{2},{3},Tool0\\WObj:=WObj0;",
                    MotionInstructions[a.motionType],
                    it++,
                    velNames[a.velocity],
                    zoneNames[a.zone]
                    ));
            }
            module.Add("  ENDPROC");
            module.Add("");
            
            // MODULE FOOTER
            module.Add("ENDMODULE");

            return module;
        }


        public string GenerateSpeedDeclaration(int velocity)
        {
            // Default speed declarations in ABB always use 500 deg/s as rot speed, but it feels too fast (and scary). 
            // Using the same value as lin motion here.
            return string.Format("[{0},{1},{2},{3}]", velocity, velocity, 5000, 1000);  
        }

        public string GenerateZoneDeclaration(int zone)
        {
            // Following conventions for default RAPID zones.
            double high = 1.5 * zone;
            double low = 0.15 * zone;
            return string.Format("[FALSE,{0},{1},{2},{3},{4},{5}]", zone, high, high, low, high, low);
        }

    }


}
