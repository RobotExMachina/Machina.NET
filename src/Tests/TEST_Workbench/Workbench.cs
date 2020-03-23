using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina;
using Machina.Types.Geometry;
using Machina.Descriptors.Models;


namespace TEST_Workbench
{
    class Workbench
    {
        static void Main(string[] args)
        {
            Machina.Logger.SetLogLevel(5);
            Machina.Logger.WriteLine += Console.WriteLine;


            List<double> target = new List<double> { 0, 0, 0, 0, 90, 0 };

            RobotModel bot = RobotModel.CreateABBIRB140();
            var frames = bot.ForwardKinematics(target, Units.Degrees);

            var it = 0;
            foreach (var m in frames)
            {
                Console.WriteLine(it);
                Console.WriteLine(m);
                it++;
            }




            //Plane p = new Plane(0, 0, 0, 1, 0, 0, 0, 1, 0);
            //Console.WriteLine(p);
            
            //Matrix t10 = Matrix.CreateTranslation(0, 0, 10);
            //p.Transform(t10);
            //Console.WriteLine(p);
            
            //Matrix rx90 = Matrix.CreateRotationX(90);
            //p.Transform(rx90);
            //Console.WriteLine(p);

            //Matrix rz90 = Matrix.CreateRotationZ(90);
            //p.Transform(rz90);
            //Console.WriteLine(p);

            //Matrix r45 = Matrix.CreateRotation(Vector.ZAxis, 90, new Vector(10, 0, 0));
            //p.Transform(r45);
            //Console.WriteLine(p);

            //Matrix s10 = Matrix.CreateScale(10);
            //p.Transform(s10);
            //Console.WriteLine(p);
            //Console.WriteLine(p.ToArrayString(-1));


            //p.Origin = new Vector(4, 5, 6);
            //Console.WriteLine(p);

            //Matrix rr = Matrix.CreateRotation(new Vector(1, 2, 3), 37, new Vector(15, 24, 10));
            //Console.WriteLine(rr);

            //p.Transform(rr);
            //Console.WriteLine(p);


            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }

        static void Scale(List<double> rots, int factor)
        {
            for (int i = 0; i < rots.Count; i++)
            {
                rots[i] *= factor;
            }
        }

    }
}
