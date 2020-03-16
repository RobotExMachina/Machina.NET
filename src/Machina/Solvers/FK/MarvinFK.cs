using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types.Geometry;
using Machina.Descriptors.Models;

namespace Machina.Solvers.FK
{
    //  ███╗   ███╗ █████╗ ██████╗ ██╗   ██╗██╗███╗   ██╗███████╗██╗  ██╗
    //  ████╗ ████║██╔══██╗██╔══██╗██║   ██║██║████╗  ██║██╔════╝██║ ██╔╝
    //  ██╔████╔██║███████║██████╔╝██║   ██║██║██╔██╗ ██║█████╗  █████╔╝ 
    //  ██║╚██╔╝██║██╔══██║██╔══██╗╚██╗ ██╔╝██║██║╚██╗██║██╔══╝  ██╔═██╗ 
    //  ██║ ╚═╝ ██║██║  ██║██║  ██║ ╚████╔╝ ██║██║ ╚████║██║     ██║  ██╗
    //  ╚═╝     ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝  ╚═══╝  ╚═╝╚═╝  ╚═══╝╚═╝     ╚═╝  ╚═╝
    //                                                                   
    /// <summary>
    /// An FK solver for 6 axis industrial robotic arms with a spherical wrist. 
    /// The first FK solver I ever wrote... <3
    /// </summary>
    internal class MarvinFK : SolverFK
    {
        /// <summary>
        /// A copy of the base model class casted to the specific subclass required by this solver. 
        /// Is this the most elegant way to deal with such inheritance?
        /// </summary>
        private RobotSixAxesArm _model;

        internal MarvinFK(RobotModelBase model) : base(model)
        {
            _model = (RobotSixAxesArm) model;  // we want an exception if this doesn't work...
        }

        internal override List<Matrix> ForwardKinematics(List<double> jointValues, Units units)
        {
            // Sanity
            if (jointValues.Count != 6)
            {
                throw new System.InvalidOperationException("Rotations list for complete Forward Kinematics must contain 6 elements");
            }

            // Convert to radians
            List<double> rotsRad = new List<double>();
            if (units == Units.Degrees)
            {
                for (int i = 0; i < jointValues.Count; i++)
                {
                    rotsRad.Add(jointValues[i] * MMath.TO_RADS);
                }
            }
            else if (units == Units.Radians)
            {
                rotsRad = jointValues;
            }
            else
            {
                throw new Exception(units + " units not allowed for this solver.");
            }


            // List of joint frames
            List<Matrix> frames = new List<Matrix>();

            // Base
            frames.Add(_model.Base.BasePlane);

            // Arm portion
            List<Matrix> armFrames = ForwardKinematicsArm(rotsRad);
            frames.AddRange(armFrames);

            //// Wrist portion
            //List<Plane> wristFrames = forwardKinematicsWrist(rotsRad.GetRange(3, 3), frames.Last(), false);
            //frames.AddRange(wristFrames);

            return frames;
        }

         /// <summary>
        /// Compute the forward kinematics of this robot for the arm portion. 
        /// </summary>
        /// <param name="rots">The list of the full arm rotations in radians.</param>
        /// <returns>List of the three transforms for the arm axes.</returns>
        private List<Matrix> ForwardKinematicsArm(List<double> rots)
        {
            // List of joint frames
            List<Matrix> frames = new List<Matrix>();
            
            // Remember: this was my first FK solver, I clearly didn't have a 
            // good grasp of DH parameters at this point... :sweat_smile: 
            // Also, relied very heavily on RC types, replicated here for 
            // educational purposes. 
            // Finally, this is super hard-coded for an ABB IRB140, needs to
            // be more generalizable! 
            Vector v01 = new Vector(70, 0, 352);    // o1 in base plane coords
            Vector v12 = new Vector(0, -360, 0);    // o2 in o1 local coords
            Vector v34 = new Vector(0, 0, 380);     // o4 in o3 local coords
            Vector v56 = new Vector(0, 0, 65);      // o6 in o5 local coords

            /*
             // define geometry per link in local frame coordinates
             r.v01 = new Vector3d(70, 0, 352);   // in base plane coordinates
             r.v12 = new Vector3d(360, 0, 0);    // o2 in o1 local coordinates
             r.v23 = new Vector3d(380, 0, 0);    // o3 in o2 local coordinates
             r.v56 = new Vector3d(0, 0, 65);     // o6 in o5 local coordinates
            */
            
            // FRAME 1
            Plane p1 = Plane.CreateFromMatrix(_model.Base.BasePlane);
            p1.Rotate(rots[0], p1.ZAxis);
            //Vector origin1 = plane1.PointAt(v01.X, v01.Y, v01.Z);
            //Vector vector1 = origin1 - plane1.Origin;
            //plane1.Translate(vector1);
            p1.Translate(v01);
            p1.Rotate(-MMath.TAU_4, p1.XAxis);
            frames.Add(p1.ToMatrix());

            ////FRAME 1
            //Plane p1 = basePlane;
            //p1.Rotate(rots[0], p1.ZAxis);
            //Point3d o1 = p1.PointAt(v01.X, v01.Y, v01.Z);
            //Vector3d v1 = o1 - p1.Origin;
            //p1.Translate(v1);
            //p1.Rotate(-PI_2, p1.ZAxis);
            //p1.Rotate(-PI_2, p1.YAxis);
            //frames.Add(p1);

            // FRAME 2
            Plane p2 = p1;
            p2.Rotate(rots[1], p2.ZAxis);
            p2.Translate(v12);
            p2.Rotate(-MMath.TAU_4, p2.ZAxis);
            frames.Add(p2.ToMatrix());

            //// FRAME 2
            //Plane p2 = p1;
            //p2.Rotate(rots[1], p2.ZAxis);
            //Point3d o2 = p2.PointAt(v12.X, v12.Y, v12.Z);
            //Vector3d v2 = o2 - p2.Origin;
            //p2.Translate(v2);
            //p2.Rotate(PI_2, p2.ZAxis);
            //frames.Add(p2);

            // FRAME 3
            Plane p3 = p2;
            p3.Rotate(rots[2], p3.ZAxis);
            p3.Rotate(-MMath.TAU_4, p3.XAxis);
            frames.Add(p3.ToMatrix());

            //// FRAME    
            //Plane p3 = p2;
            //p3.Rotate(rots[2], p3.ZAxis);
            //Point3d o3 = p3.PointAt(v23.X, v23.Y, v23.Z);
            //Vector3d v3 = o3 - p3.Origin;
            //p3.Translate(v3);
            //p3.Rotate(PI_2, p3.YAxis);
            //p3.Rotate(PI_2, p3.ZAxis);
            //frames.Add(p3);

            // FRAME 4
            Plane p4 = p3;
            p4.Rotate(rots[3], p4.ZAxis);
            p4.Translate(v34);
            p4.Rotate(MMath.TAU_4, p4.XAxis);
            frames.Add(p4.ToMatrix());

            return frames;
        }
    }
}
