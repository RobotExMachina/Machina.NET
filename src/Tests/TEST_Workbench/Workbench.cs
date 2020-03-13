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

            Matrix a = Matrix.CreateFromQuaternion(1, 2, 3, 4);
            Console.WriteLine(a);
            Console.WriteLine(a.IsOrthogonalRotation);
            Console.WriteLine(a.OrthogonalizeRotation());
            Console.WriteLine(a);
            Console.WriteLine(a.IsOrthogonalRotation);


            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }

    }
}
