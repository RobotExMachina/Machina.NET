using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRobot
{

    //  ███████╗██╗   ██╗██╗     ███████╗██████╗ ███████╗██╗   ██╗██╗  ██╗
    //  ██╔════╝██║   ██║██║     ██╔════╝██╔══██╗╚══███╔╝╚██╗ ██╔╝╚██╗██╔╝
    //  █████╗  ██║   ██║██║     █████╗  ██████╔╝  ███╔╝  ╚████╔╝  ╚███╔╝ 
    //  ██╔══╝  ██║   ██║██║     ██╔══╝  ██╔══██╗ ███╔╝    ╚██╔╝   ██╔██╗ 
    //  ███████╗╚██████╔╝███████╗███████╗██║  ██║███████╗   ██║   ██╔╝ ██╗
    //  ╚══════╝ ╚═════╝ ╚══════╝╚══════╝╚═╝  ╚═╝╚══════╝   ╚═╝   ╚═╝  ╚═╝
    //                                                                    
    /// <summary>
    /// A class representing a rasdasdotation in Euler Angles over intrinsic
    /// ZY'X'' axes (Tait-Bryan angles). See <see cref="https://en.wikipedia.org/wiki/Euler_angles#Tait.E2.80.93Bryan_angles"/>
    /// </summary>
    public class EulerZYX : Geometry
    {
        /// <summary>
        /// Rotation around the X axis in degrees.
        /// </summary>
        public double XAngle { get; internal set; }

        /// <summary>
        /// Rotation around the Y axis in degrees.
        /// </summary>
        public double YAngle { get; internal set; }

        /// <summary>
        /// Rotation around the Z axis in degrees. 
        /// </summary>
        public double ZAngle { get; internal set; }

        /// <summary>
        /// Alias for rotation around X axis.
        /// </summary>
        public double Roll { get { return this.XAngle; } }

        /// <summary>
        /// Alias for rotation around Y axis.
        /// </summary>
        public double Pitch { get { return this.YAngle; } }

        /// <summary>
        /// Alias for rotation around Z axis.
        /// </summary>
        public double Yaw { get { return this.ZAngle; } }

        /// <summary>
        /// Create an Euler Angles ZY'X'' intrinsic rotation from its constituent components. 
        /// </summary>
        /// <param name="xAngle"></param>
        /// <param name="yAngle"></param>
        /// <param name="zAngle"></param>
        public EulerZYX(double xAngle, double yAngle, double zAngle)
        {
            this.XAngle = xAngle;
            this.YAngle = yAngle;
            this.ZAngle = zAngle;
        }



    }
}
