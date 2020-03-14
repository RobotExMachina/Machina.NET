using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina;
using Machina.Types.Geometry;


namespace TEST_Workbench
{
    class Workbench
    {
        static void Main(string[] args)
        {
            Machina.Logger.SetLogLevel(5);
            Machina.Logger.WriteLine += Console.WriteLine;

            Plane p = new Plane(0, 0, 0, 1, 0, 0, 0, 1, 0);
            Console.WriteLine(p);
            
            Matrix t10 = Matrix.CreateTranslation(0, 0, 10);
            p.Transform(t10);
            Console.WriteLine(p);
            
            Matrix rx90 = Matrix.CreateRotationX(90);
            p.Transform(rx90);
            Console.WriteLine(p);

            Matrix rz90 = Matrix.CreateRotationZ(90);
            p.Transform(rz90);
            Console.WriteLine(p);

            Matrix r45 = Matrix.CreateRotation(Vector.ZAxis, 45, new Vector(10, 0, 0));
            p.Transform(r45);
            Console.WriteLine(p);

            Matrix s10 = Matrix.CreateScale(10);
            p.Transform(s10);
            Console.WriteLine(p);
            Console.WriteLine(p.ToArrayString(-1));



            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }

    }
}
