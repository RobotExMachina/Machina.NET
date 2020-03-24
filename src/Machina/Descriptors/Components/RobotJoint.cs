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
        public Matrix BasePlane { get; set; }

        /// <summary>
        /// The end plane this joint transforms. 
        /// </summary>
        public Matrix TransformedPlane { get; set; }

        /// <summary>
        /// Is this a linear a revolution joint?
        /// </summary>
        public RobotJointType RobotJointType { get; set; }

        private Interval _jointRange;
        /// <summary>
        /// The linear/angular limitations for this joint in degrees.
        /// </summary>
        public Interval JointRange {
            get
            {
                return _jointRange;
            }
            set
            {
                _jointRange = value;
                JointRangeRadians = MMath.TO_RADS * _jointRange;
            }
        }

        /// <summary>
        /// The linear/angular limitations for this joint in radians.
        /// </summary>
        public Interval JointRangeRadians { get; private set; }

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
            //// This would be the "manual" way, which the DH matrix simplifies.
            //Matrix4x4 m = baseJoint.TransformedPlane;
            //m = Matrix4x4.CreateTranslation(m.Z * distance) * m;
            //m = Matrix4x4.CreateFromAxisAngle(m.Z, (float)theta, m.Translation) * m;
            //m = Matrix4x4.CreateTranslation(m.X * radius) * m;
            //m = Matrix4x4.CreateFromAxisAngle(m.X, (float)alpha, m.Translation) * m;

            // Faster way with DH matrix:
            Matrix mm = baseJoint.TransformedPlane;
            Matrix dhm = Matrix.CreateFromDHParameters(distance, radius, alpha, theta);
            mm = mm * dhm;  // remember the DH is post-multiplied

            return new RobotJoint
            {
                BasePlane = baseJoint.TransformedPlane,
                TransformedPlane = mm,
                RobotJointType = robotJointType,
                JointRange = jointRange,
                MaxSpeed = maxSpeed
            };
        }

        /// <summary>
        /// Checks if a particular angle value is within the range of this Joint.
        /// </summary>
        /// <param name="angleValue"></param>
        /// <param name="units"></param>
        /// <returns></returns>
        public bool IsInRange(double angleValue, Units units)
        {
            switch(units)
            {
                case Units.Degrees:
                    return JointRange.IncludesParameter(angleValue);

                case Units.Radians:
                    return JointRangeRadians.IncludesParameter(angleValue);

                default:
                    throw new Exception(units + " units not allowed here.");
            }
        }

    }
}
