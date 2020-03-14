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

            //Matrix a = Matrix.CreateFromQuaternion(1, 2, 3, 4);
            //Console.WriteLine(a);
            //Console.WriteLine(a.IsOrthogonalRotation);
            //Console.WriteLine(a.OrthogonalizeRotation());
            //Console.WriteLine(a);
            //Console.WriteLine(a.IsOrthogonalRotation);

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



            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }

    }
}
