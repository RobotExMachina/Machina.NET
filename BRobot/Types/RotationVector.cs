using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRobot
{
    //  ██████╗  ██████╗ ████████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔══██╗██╔═══██╗╚══██╔══╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //  ██████╔╝██║   ██║   ██║   ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //  ██╔══██╗██║   ██║   ██║   ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //  ██║  ██║╚██████╔╝   ██║   ██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //  ╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                   
    //  ██╗   ██╗███████╗ ██████╗████████╗ ██████╗ ██████╗               
    //  ██║   ██║██╔════╝██╔════╝╚══██╔══╝██╔═══██╗██╔══██╗              
    //  ██║   ██║█████╗  ██║        ██║   ██║   ██║██████╔╝              
    //  ╚██╗ ██╔╝██╔══╝  ██║        ██║   ██║   ██║██╔══██╗              
    //   ╚████╔╝ ███████╗╚██████╗   ██║   ╚██████╔╝██║  ██║              
    //    ╚═══╝  ╚══════╝ ╚═════╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝              
    //                                                                   
    
    /// <summary>
    /// A class to represent a spatial rotation as a Rotation Vector: an unit rotation
    /// axis multiplied by the rotation angle.
    /// </summary>
    public class RotationVector : Geometry
    {
        double X, Y, Z;

        public RotationVector(double x, double y, double z, double angleDegs)
        {
            Point v = new Point(x, y, z);
            v.Normalize();
            v.Scale(angleDegs);
            this.X = v.X;
            this.Y = v.Y;
            this.Z = v.Z;
        }
    }
}
