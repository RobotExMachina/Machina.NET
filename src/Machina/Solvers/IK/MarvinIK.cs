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

            // Going back and forth between Planes and Matrices makes no sense, 
            // but let's stick to it here since that was the original implementation. 
            Plane target = Plane.CreateFromMatrix(targetTCP);

            // Wrist position in robot coordinates
            Vector wrist = WristLocalPosition(target);

            // Arm solutions
            var armSolutions = InverseKinematicsArm(wrist);

            // Full solutions
            for (int i = 0; i < 4; i++)
            {
                var armSol = armSolutions[i];

                // If arm could reach here, ad two invalid solutions
                if (armSol == null)
                {
                    solutions.Add(null);
                    solutions.Add(null);
                }
                // Otherwise, compute wrist solutions and add them
                else
                {
                    // @TODO: continue here! 
                    // There is a problem, which is that my robot definition keeps the wrist plane
                    // before the wrist, as opposed to MARVIN. For Joint4, wrist plane is already in place,
                    // but has an extra rotation. The decoupling doesn't work like this, will have to do 
                    // some edits to fix it.
                    //continue here;
                }
            }


            Console.WriteLine("Wrist: " + wrist);
            foreach (var sol in armSolutions)
            {
                
                Console.WriteLine(sol);
            }

            return null;
        }

        /// <summary>
        /// Given a point in local robot coordinates, returns a list of 4 
        /// Solutions representing the rotations of the 4 possible solutions 
        /// (left/right arm, elbow up/down). If invalid solution, null instead. 
        /// </summary>
        /// <param name="wrist"></param>
        /// <returns></returns>
        private List<Solution> InverseKinematicsArm(Vector wrist)
        {
            double j3max = _model.Joints[3].JointRangeRadians.Max;

            // Solve arm IK
            var solutions = new List<Solution>();
            for (int i = 0; i < 4; i++)
            {
                Solution sol = new Solution();
                double r;

                // Q1
                // Left arm
                if (i < 2)
                {
                    sol.Rotations[0] = Math.Atan2(wrist.Y, wrist.X);
                    r = Math.Sqrt(wrist.X * wrist.X + wrist.Y * wrist.Y) - v01.X;
                }
                // Right arm
                else
                {
                    sol.Rotations[0] = MMath.TAU_2 + Math.Atan2(wrist.Y, wrist.X);
                    if (sol.Rotations[0] > MMath.TAU_2) sol.Rotations[0] -= MMath.TAU;
                    r = -(Math.Sqrt(wrist.X * wrist.X + wrist.Y * wrist.Y) + v01.X);
                }

                double s = wrist.Z - v01.Z;
                double D = (v12.Y * v12.Y + v34.Z * v34.Z - r * r - s * s) / (2 * -v12.Y * v34.Z);
                double disc = 1 - D * D;
                if (disc < 0)
                {
                    solutions.Add(null);
                    continue;  // go to next solution (this one is out of reach)
                }
                double sqDisc = Math.Sqrt(disc);

                // Q3
                // elbow up
                if (i % 2 == 0)
                {
                    sol.Rotations[2] = MMath.TAU_4 - Math.Atan2(sqDisc, D);
                }
                // elbow down
                else
                {
                    sol.Rotations[2] = MMath.TAU_4 - Math.Atan2(-sqDisc, D);
                }
                // capping the solution
                if (sol.Rotations[2] > j3max)
                {
                    sol.Rotations[2] -= MMath.TAU;
                }

                // Q2
                sol.Rotations[1] = MMath.TAU_4 - Math.Atan2(s, r) - Math.Atan2(v34.Z * Math.Cos(sol.Rotations[2]), -v12.Y - v34.Z * Math.Sin(sol.Rotations[2]));
                if (sol.Rotations[1] > MMath.PI)
                {
                    sol.Rotations[1] -= MMath.TAU;
                }

                sol.ArmConfiguration = i;
                sol.ToDegrees();

                // add this solution to list
                solutions.Add(sol);
            }

            return solutions;
        }

        /// <summary>
        /// Given the end effector frame in world coordinates,
        /// returns the wrist point in local robot coordinates.
        /// </summary>
        /// <param name="endEffectorFrame"></param>
        /// <returns></returns>
        private Vector WristLocalPosition(Plane endEffectorFrame)
        {
            Vector eep = endEffectorFrame.PointAt(-v56.X, -v56.Y, -v56.Z);
            _model.Joints[0].BasePlane.ToPlane().RemapToPlaneSpace(eep, out Vector p);
            return p;
        }
    }



    internal class Solution
    {
        public List<double> Rotations;
        public int ArmConfiguration;  // this is a number 0-3 representing (left/right arm, elbow up/down)
        public Units Units;


        // A valid solution
        public Solution(List<double> rots, int _armConfig, Units units)
        {
            Rotations = rots;
            ArmConfiguration = _armConfig;
        }

        public Solution()
        {
            Rotations = new List<double>() { 0, 0, 0, 0, 0, 0 };
            ArmConfiguration = -1;
            Units = Units.Undefined;
        }

        public void ToDegrees()
        {
            for (int i = 0; i < Rotations.Count; i++)
            {
                Rotations[i] *= MMath.TO_DEGS;
            }
            Units = Units.Degrees;
        }

        public void ToRadians()
        {
            for (int i = 0; i < Rotations.Count; i++)
            {
                Rotations[i] *= MMath.TO_RADS;
            }
            Units = Units.Radians;
        }

        public override string ToString()
        {
            return $"[{Rotations[0]},{Rotations[1]},{Rotations[2]},{Rotations[3]},{Rotations[4]},{Rotations[5]},{ArmConfiguration},{Units}]";
        }
    }
}
