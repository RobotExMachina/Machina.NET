using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machina.Types.Geometry;

namespace Machina.Descriptors.Components
{
    //  ██████╗  ██████╗ ██████╗  ██████╗ ████████╗  ██╗ ██████╗ ██╗███╗   ██╗████████╗
    //  ██╔══██╗██╔═══██╗██╔══██╗██╔═══██╗╚══██╔══╝  ██║██╔═══██╗██║████╗  ██║╚══██╔══╝
    //  ██████╔╝██║   ██║██████╔╝██║   ██║   ██║     ██║██║   ██║██║██╔██╗ ██║   ██║   
    //  ██╔══██╗██║   ██║██╔══██╗██║   ██║   ██║██   ██║██║   ██║██║██║╚██╗██║   ██║   
    //  ██║  ██║╚██████╔╝██████╔╝╚██████╔╝   ██║╚█████╔╝╚██████╔╝██║██║ ╚████║   ██║   
    //  ╚═╝  ╚═╝ ╚═════╝ ╚═════╝  ╚═════╝    ╚═╝ ╚════╝  ╚═════╝ ╚═╝╚═╝  ╚═══╝   ╚═╝   
    //                                                                                 
    /// <summary>
    /// Represents a physical Joint between two axes.
    /// </summary>
    public class RobotJoint
    {
        /// <summary>
        /// The base plane for this joint's transformation (linear or rotary).
        /// </summary>
        public Matrix4x4 BasePlane { get; set; }

        /// <summary>
        /// The end plane this joint transforms. 
        /// </summary>
        public Matrix4x4 TransformedPlane { get; set; }

        /// <summary>
        /// Is this a linear a revolution joint?
        /// </summary>
        public RobotJointType RobotJointType { get; set; }

        /// <summary>
        /// The linear/angular limitations for this joint.
        /// </summary>
        public Interval JointRange { get; set; }

        /// <summary>
        /// Max joint speed in degrees/sec or mm/sec.
        /// </summary>
        public double MaxSpeed { get; set; }
        
        // mesh if applicable?


        /// <summary>
        /// Create Joint based on previous joint + Denavit-Hartenberg parameter description
        /// of this joint. 
        /// </summary>
        /// <param name="baseJoint"></param>
        /// <param name="distance"></param>
        /// <param name="radius"></param>
        /// <param name="alpha"></param>
        /// <param name="theta"></param>
        /// <param name="robotJointType"></param>
        /// <param name="jointRange"></param>
        /// <param name="maxSpeed"></param>
        /// <returns></returns>
        public static RobotJoint CreateFromDHParameters(
            RobotJoint baseJoint, 
            double distance, double radius, double alpha, double theta,
            RobotJointType robotJointType, Interval jointRange, double maxSpeed)
        {
            Matrix4x4 m = baseJoint.TransformedPlane;

            //// This would be the "manual" way, which the DH matrix simplifies.
            //m = Matrix4x4.CreateTranslation(m.Z * distance) * m;
            //m = Matrix4x4.CreateFromAxisAngle(m.Z, (float)theta, m.Translation) * m;
            //m = Matrix4x4.CreateTranslation(m.X * radius) * m;
            //m = Matrix4x4.CreateFromAxisAngle(m.X, (float)alpha, m.Translation) * m;
            //Console.WriteLine("Manual: " + m);

            Matrix4x4 dhm = Matrix4x4.CreateFromDHParameters(distance, radius, alpha, theta);
            m = m * dhm;  // remember the DH is post-multiplied

            return new RobotJoint
            {
                BasePlane = baseJoint.TransformedPlane,
                TransformedPlane = m,
                RobotJointType = robotJointType,
                JointRange = jointRange,
                MaxSpeed = maxSpeed
            };
        }

    }
}
