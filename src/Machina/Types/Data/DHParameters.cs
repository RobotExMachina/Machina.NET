using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types.Geometry;
using Machina.Descriptors.Components;
using System.Globalization;

namespace Machina.Types.Data
{
    //  ██████╗ ███████╗███╗   ██╗ █████╗ ██╗   ██╗██╗████████╗                             
    //  ██╔══██╗██╔════╝████╗  ██║██╔══██╗██║   ██║██║╚══██╔══╝                             
    //  ██║  ██║█████╗  ██╔██╗ ██║███████║██║   ██║██║   ██║█████╗                          
    //  ██║  ██║██╔══╝  ██║╚██╗██║██╔══██║╚██╗ ██╔╝██║   ██║╚════╝                          
    //  ██████╔╝███████╗██║ ╚████║██║  ██║ ╚████╔╝ ██║   ██║                                
    //  ╚═════╝ ╚══════╝╚═╝  ╚═══╝╚═╝  ╚═╝  ╚═══╝  ╚═╝   ╚═╝                                
    //                                                                                      
    //  ██╗  ██╗ █████╗ ██████╗ ████████╗███████╗███╗   ██╗██████╗ ███████╗██████╗  ██████╗ 
    //  ██║  ██║██╔══██╗██╔══██╗╚══██╔══╝██╔════╝████╗  ██║██╔══██╗██╔════╝██╔══██╗██╔════╝ 
    //  ███████║███████║██████╔╝   ██║   █████╗  ██╔██╗ ██║██████╔╝█████╗  ██████╔╝██║  ███╗
    //  ██╔══██║██╔══██║██╔══██╗   ██║   ██╔══╝  ██║╚██╗██║██╔══██╗██╔══╝  ██╔══██╗██║   ██║
    //  ██║  ██║██║  ██║██║  ██║   ██║   ███████╗██║ ╚████║██████╔╝███████╗██║  ██║╚██████╔╝
    //  ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝   ╚══════╝╚═╝  ╚═══╝╚═════╝ ╚══════╝╚═╝  ╚═╝ ╚═════╝ 
    //                                                                                      
    //  ██████╗  █████╗ ██████╗  █████╗ ███╗   ███╗███████╗████████╗███████╗██████╗ ███████╗
    //  ██╔══██╗██╔══██╗██╔══██╗██╔══██╗████╗ ████║██╔════╝╚══██╔══╝██╔════╝██╔══██╗██╔════╝
    //  ██████╔╝███████║██████╔╝███████║██╔████╔██║█████╗     ██║   █████╗  ██████╔╝███████╗
    //  ██╔═══╝ ██╔══██║██╔══██╗██╔══██║██║╚██╔╝██║██╔══╝     ██║   ██╔══╝  ██╔══██╗╚════██║
    //  ██║     ██║  ██║██║  ██║██║  ██║██║ ╚═╝ ██║███████╗   ██║   ███████╗██║  ██║███████║
    //  ╚═╝     ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝   ╚═╝   ╚══════╝╚═╝  ╚═╝╚══════╝
    //                                                                                      

    /// <summary>
    /// Represents a set of Denavit-Hartenberg parameters for a robot joint. 
    /// Note this uses the original DH convention, not any of the modified versions.
    /// See https://en.wikipedia.org/wiki/Denavit%E2%80%93Hartenberg_parameters
    /// </summary>
    public struct DHParameters
    {
        /// <summary>
        /// Depth along base Z axis to the common normal. 
        /// </summary>
        public double D { get; private set; }

        /// <summary>
        /// Distance between axes, or length of the common normal. 
        /// </summary>
        public double R { get; private set; }

        /// <summary>
        /// Rotation in degrees around base X to align base Z to new Z.
        /// </summary>
        public double Alpha { get; private set; }

        /// <summary>
        /// Angle in degrees from base X to align with new common normal. 
        /// </summary>
        public double Theta { get; private set; }

        /// <summary>
        /// Create a set of Denavit-Hartberger parameters. 
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="radius"></param>
        /// <param name="alpha"></param>
        /// <param name="theta"></param>
        public DHParameters(double distance, double radius, double alpha, double theta)
        {
            D = distance;
            R = radius;
            Alpha = alpha;
            Theta = theta;
        }

        /// <summary>
        /// Compute the Denavit-Hartenberg parameters of a robot joint. 
        /// </summary>
        /// <param name="joint"></param>
        /// <returns></returns>
        public static DHParameters CreateFromJoint(RobotJoint joint)
        {
            Vector baseOrigin = joint.BasePlane.Translation,
                baseVX = joint.BasePlane.X,
                baseVZ = joint.BasePlane.Z,
                targetOrigin = joint.TransformedPlane.Translation,
                targetVX = joint.TransformedPlane.X,
                targetVZ = joint.TransformedPlane.Z;

            // Project center of new plane over Z of old.
            Vector u = targetOrigin - baseOrigin;
            double distance = u * baseVZ;

            // Find that point and their distance
            Vector intersection = baseOrigin + (distance * baseVZ);
            double radius = intersection.DistanceTo(targetOrigin);

            // Alpha rotation angle between Z vectors. 
            double angleZ = Vector.AngleBetween(baseVZ, targetVZ);
            Vector crossZ = Vector.CrossProduct(baseVZ, targetVZ);
            int signAngleZ = (crossZ * targetVX) > 0 ? 1 : -1;
            angleZ *= signAngleZ * MMath.TO_DEGS;

            // Theta offset angle between X vectors.
            double angleX = Vector.AngleBetween(baseVX, targetVX);
            Vector crossX = Vector.CrossProduct(baseVX, targetVX);
            int signAngleX = (crossX * baseVZ) > 0 ? 1 : -1;
            angleX *= signAngleX * MMath.TO_DEGS;

            return new DHParameters
            {
                D = distance,
                R = radius,
                Alpha = angleZ,
                Theta = angleX
            };
        }

        public override string ToString()
        {
            CultureInfo ci = CultureInfo.InvariantCulture;

            return String.Format(ci, "{{D:{0}, R:{1}, α:{2}, θ:{3}}}",
                D.ToString(ci),
                R.ToString(ci),
                Alpha.ToString(ci),
                Theta.ToString(ci));
        }
    }
}
