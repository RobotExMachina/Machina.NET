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
        /// Alias for rotation around X axis.
        /// </summary>
        public double Bank { get { return this.XAngle; } }

        /// <summary>
        /// Alias for rotation around Y axis.
        /// </summary>
        public double Attitude { get { return this.YAngle; } }

        /// <summary>
        /// Alias for rotation around Z axis.
        /// </summary>
        public double Heading { get { return this.ZAngle; } }

        /// <summary>
        /// Create an Euler Angles ZY'X'' intrinsic rotation from its constituent components in degrees.
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


        //public RotationMatrix ToRotationMatrix()
        //{

        //}

        
        public Quaternion ToQuaternion()
        {
            // From Shoemake, Ken. "Animating rotation with quaternion curves." ACM SIGGRAPH computer graphics. Vol. 19. No. 3. ACM, 1985.
            // (using a different Euler convention than EuclideanSpace)
            double cX = Math.Cos(0.5 * TO_RADS * this.XAngle),
                   cY = Math.Cos(0.5 * TO_RADS * this.YAngle),
                   cZ = Math.Cos(0.5 * TO_RADS * this.ZAngle),
                   sX = Math.Sin(0.5 * TO_RADS * this.XAngle),
                   sY = Math.Sin(0.5 * TO_RADS * this.YAngle),
                   sZ = Math.Sin(0.5 * TO_RADS * this.ZAngle);

            return new Quaternion(cX * cY * cZ + sX * sY * sZ,
                                  sX * cY * cZ - cX * sY * sZ,
                                  cX * sY * cZ + sX * cY * sZ,
                                  cX * cY * sZ - sX * sY * cZ);
        }

        /// <summary>
        /// Returns the Rotation Matrix representation os this rotation.
        /// </summary>
        /// <returns></returns>
        public RotationMatrix ToRotationMatrix()
        {
            double cX = Math.Cos(TO_RADS * this.XAngle),
                   cY = Math.Cos(TO_RADS * this.YAngle),
                   cZ = Math.Cos(TO_RADS * this.ZAngle),
                   sX = Math.Sin(TO_RADS * this.XAngle),
                   sY = Math.Sin(TO_RADS * this.YAngle),
                   sZ = Math.Sin(TO_RADS * this.ZAngle);

            return new RotationMatrix(cY * cZ,  -cX * sZ + sX * sY * cZ,     sX * sZ + cX * sY * cZ,
                                      cY * sZ,   cX * cZ + sX * sY * sZ,    -sX * cZ + cX * sY * sZ,
                                          -sY,                 sX * cY,                     cX * cY, false);
        }

        public override string ToString()
        {
            return string.Format("EulerZYX[Z:{0}, Y:{1}, X:{2}]",
                Math.Round(this.ZAngle, STRING_ROUND_DECIMALS_RADS),
                Math.Round(this.YAngle, STRING_ROUND_DECIMALS_RADS),
                Math.Round(this.XAngle, STRING_ROUND_DECIMALS_RADS));
        }

    }
}
