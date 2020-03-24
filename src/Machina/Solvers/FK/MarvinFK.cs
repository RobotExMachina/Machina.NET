using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types.Geometry;
using Machina.Descriptors.Models;
using Machina.Solvers.Errors;

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

        // Remember: this was my first FK solver, I clearly didn't have a 
        // good grasp of DH parameters at this point... :sweat_smile: 
        // Also, relied very heavily on RC types, replicated here for 
        // educational purposes. 
        // Finally, this is super hard-coded for an ABB IRB140, needs to
        // be more generalizable! 
        Vector v01 = new Vector(70, 0, 352);    // o1 in base plane coords
        Vector v12 = new Vector(0, -360, 0);    // o2 in o1 local coords
        Vector v23 = new Vector(0, 0, 0);       
        Vector v34 = new Vector(0, 0, 380);     // o4 in o3 local coords
        Vector v45 = new Vector(0, 0, 0);
        Vector v56 = new Vector(0, 0, 65);      // o6 in o5 local coords


        internal MarvinFK(RobotModel model) : base(model)
        {
            _model = (RobotSixAxesArm) model;  // we want an exception if this doesn't work...
        }

        /// <summary>
        /// Compute the FK of this robot for the end-effector. 
        /// Takes a list of 6 rotations, and returns 7 frames.
        /// </summary>
        /// <param name="jointValues"></param>
        /// <param name="units"></param>
        /// <returns></returns>
        internal override List<Matrix> ForwardKinematics(List<double> jointValues, Units units, out List<SolverError> errors)
        {
            // Sanity
            if (jointValues.Count != 6)
            {
                throw new System.InvalidOperationException("Rotations list for complete Forward Kinematics must contain 6 elements");
            }

            errors = null;

            // Check joint ranges
            for (int i = 1; i < 7; i++)
            {
                if (!_model.Joints[i].IsInRange(jointValues[i - 1], units))
                {
                    if (errors == null) errors = new List<SolverError>();
                    errors.Add(new JointOutOfRangeError($"Joint {i} out of range: {jointValues[i - 1]} {units}"));
                }
            }
            
            // Convert to radians
            List<double> rots = new List<double>();
            if (units == Units.Degrees)
            {
                for (int i = 0; i < jointValues.Count; i++)
                {
                    rots.Add(jointValues[i] * MMath.TO_RADS);
                }
            }
            else if (units == Units.Radians)
            {
                rots = jointValues;
            }
            else
            {
                throw new Exception(units + " units not allowed for this solver.");
            }

            // List of joint frames
            List<Matrix> frames = new List<Matrix>();

            // Base
            frames.Add(_model.Joints[0].BasePlane);

            // FRAME 1
            Plane p1 = Plane.CreateFromMatrix(_model.Joints[0].BasePlane);
            p1.Rotate(rots[0], p1.ZAxis);
            p1.Offset(v01);
            p1.Rotate(-MMath.TAU_4, p1.XAxis);
            frames.Add(p1.ToMatrix());
            
            // FRAME 2
            Plane p2 = p1;
            p2.Rotate(rots[1], p2.ZAxis);
            p2.Offset(v12);
            p2.Rotate(-MMath.TAU_4, p2.ZAxis);
            frames.Add(p2.ToMatrix());

            // FRAME 3
            Plane p3 = p2;
            p3.Rotate(rots[2], p3.ZAxis);
            p3.Offset(v23);
            p3.Rotate(-MMath.TAU_4, p3.XAxis);
            frames.Add(p3.ToMatrix());

            // FRAME 4
            Plane p4 = p3;
            p4.Rotate(rots[3], p4.ZAxis);
            p4.Offset(v34);
            p4.Rotate(MMath.TAU_4, p4.XAxis);
            frames.Add(p4.ToMatrix());

            // FRAME 5
            Plane p5 = p4;
            p5.Rotate(rots[4], p5.ZAxis);
            p5.Offset(v45);
            p5.Rotate(-MMath.TAU_4, p5.XAxis);
            frames.Add(p5.ToMatrix());

            // FRAME 6
            Plane p6 = p5;
            p6.Rotate(rots[5], p6.ZAxis);
            p6.Offset(v56);
            p6.Rotate(MMath.TAU_2, p6.ZAxis);
            frames.Add(p6.ToMatrix());

            return frames;
        }

    }
}
