using Machina.Descriptors.Models;
using Machina.Solvers.Errors;
using Machina.Types.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Solvers.IK
{
    /// <summary>
    /// An IK solver for 6 axis industrial robotic arms with a spherical wrist. 
    /// The first FK solver I ever wrote... <3
    /// </summary>
    internal class MarvinIK : SolverIK
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

        /*
        // define geometry per link in local frame coordinates
        r.v01 = new Vector3d(70, 0, 352);   // in base plane coordinates
        r.v12 = new Vector3d(360, 0, 0);    // o2 in o1 local coordinates
        r.v23 = new Vector3d(380, 0, 0);    // o3 in o2 local coordinates
        r.v56 = new Vector3d(0, 0, 65);     // o6 in o5 local coordinates
        */

        internal MarvinIK(RobotModel model) : base(model)
        {
            _model = (RobotSixAxesArm)model;  // we want an exception if this doesn't work...
        }

        /// <summary>
        /// Compute the IK of this robot for the TCP.
        /// </summary>
        /// <param name="targetTCP"></param>
        /// <param name="prevTCP"></param>
        /// <param name="tool"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        internal override List<double> InverseKinematics(Matrix targetTCP, Matrix? prevTCP, Tool tool, out List<SolverError> errors)
        {
            // Let's start simple: given the target, print out all the solutions.
            errors = null;
            List<Solution> solutions = new List<Solution>();

            Plane target = Plane.CreateFromMatrix(targetTCP);

            // wrist position in robot coordinates
            Vector wrist = wristLocalPosition(target);

            Console.WriteLine("Wrist: " + wrist);

            return null;
        }


        /// <summary>
        /// Given the end effector frame in world coordinates,
        /// returns the wrist point in local robot coordinates.
        /// </summary>
        /// <param name="endEffectorFrame"></param>
        /// <returns></returns>
        private Vector wristLocalPosition(Plane endEffectorFrame)
        {
            Vector eep = endEffectorFrame.PointAt(-v56.X, -v56.Y, -v56.Z);
            _model.Joints[0].BasePlane.ToPlane().RemapToPlaneSpace(eep, out Vector p);
            return p;
        }
    }



    internal class Solution
    {
        public List<double> Rotations;
        public int ArmConfiguration;  // this is a number 0-3 representing 

        // a valid solution
        public Solution(List<double> rots, int _armConfig)
        {
            Rotations = rots;
            ArmConfiguration = _armConfig;
        }

        public override string ToString()
        {
            return $"[{Rotations[0]},{Rotations[1]},{Rotations[2]},{Rotations[3]},{Rotations[4]},{Rotations[5]},{ArmConfiguration}]";
        }
    }
}
