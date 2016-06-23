using System;
using System.Collections.Generic;

namespace RobotControl
{
    internal class RAPID
    {

        /// <summary>
        /// Given a Path, and constant velocity and zone for all targets, returns a string representation of a RAPID module. Velocity and zone must comply with predefined types.
        /// WARNING: this method is EXTREMELY UNSAFE, since it performs no IK calculations, assuming all targets are in the positive XYZ octant hence a robot configuration of [0,0,0,0]. To be extended with a proper module creator
        /// </summary>
        /// <param name="path"></param>
        /// <param name="velocity"></param>
        /// <param name="zone"></param>
        /// 
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
            return "[" + target + ",[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]";
        }


    }
}
