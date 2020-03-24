using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina;
using Machina.Types.Geometry;
using Machina.Descriptors.Models;
using Machina.Solvers.Errors;

namespace TEST_Workbench
{
    class Workbench
    {
        static Robot bot;
        static RobotModel robotModel;

        static void Main(string[] args)
        {
            Machina.Logger.SetLogLevel(5);
            Machina.Logger.WriteLine += Console.WriteLine;

            //List<double> target = new List<double> { 0, 0, 0, 0, 90, 0 };
            List<double> target = new List<double> { 259, 67.689, -126.868, 0, -30.821, 287.526 };

            List<SolverError> errors;

            robotModel = RobotModel.CreateABBIRB140();
            var frames = robotModel.ForwardKinematics(target, Units.Degrees, out errors);

            Console.WriteLine(frames.Last());

            foreach (var err in errors)
            {
                Console.WriteLine(err);
            }

            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }


    }
}
