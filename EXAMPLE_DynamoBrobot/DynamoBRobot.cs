using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

using BRobot;

namespace EXAMPLE_DynamoBRobot
{
    public class DynamoBRobot
    {
        internal DynamoBRobot() { }

        public static Robot NewRobot()
        {
            Robot bot = new Robot();
            return bot;
        }

        public static List<string> GenerateCode(Robot bot, int speed, int zone, List<Plane> planes)
        {
            bot.Mode("offline");

            bot.Speed(speed);
            bot.Zone(zone);
            bot.JointsTo(0, 0, 0, 0, 90, 0);
            foreach (Plane p in planes)
            {
                bot.TransformTo(DynamoPlaneToBRobotRotation(p), DynamoPlaneToBRobotPoint(p));
            }
            bot.JointsTo(0, 0, 0, 0, 90, 0);

            return bot.Export();
        }

        public static string ExportToFile(List<string> code, string filepath)
        {
            string result;
            try
            {
                System.IO.File.WriteAllLines(filepath, code, System.Text.Encoding.ASCII);
                result = "Successfuly saved to " + filepath;
            }
            catch (Exception ex)
            {
                result = "Could not save to file " + filepath + ", ERROR: " + ex;
            }
            return result;
        }


        private static Rotation DynamoPlaneToBRobotRotation(Plane pl)
        {
            return new Rotation(pl.XAxis.X, pl.XAxis.Y, pl.XAxis.Z, pl.YAxis.X, pl.YAxis.Y, pl.YAxis.Z);
        }

        private static BRobot.Point DynamoPlaneToBRobotPoint(Plane pl)
        {
            return new BRobot.Point(pl.Origin.X, pl.Origin.Y, pl.Origin.Z);
        }


    }
}
