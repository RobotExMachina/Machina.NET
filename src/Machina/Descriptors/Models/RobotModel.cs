using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types.Geometry;
using Machina.Solvers.FK;
using Machina.Descriptors.Components;

namespace Machina.Descriptors.Models
{
    /// <summary>
    /// A base class for the description of machine models.
    /// </summary>
    public abstract class RobotModel
    {
        internal SolverFK solverFK;

        /// <summary>
        /// Compute Forward Kinematics for this RobotModel, given a list of joint values.
        /// </summary>
        /// <param name="jointValues"></param>
        /// <param name="units"></param>
        /// <returns></returns>
        public List<Matrix> ForwardKinematics(List<double> jointValues, Units units)
        {
            return solverFK.ForwardKinematics(jointValues, units);
        }

        /// <summary>
        /// Links an instance of a SolveFK to this RobotModel. 
        /// </summary>
        /// <param name="solverFK"></param>
        /// <returns></returns>
        internal bool AssignSolverFK(SolverFK solverFK)
        {
            this.solverFK = solverFK;
            return true;
        }

        /// <summary>
        /// A hard-coded factory creator for testing purposes. 
        /// </summary>
        /// <returns></returns>
        public static RobotModel CreateABBIRB140()
        {
            RobotSixAxesArm bot = new RobotSixAxesArm();

            // The Base (this is currently reduntant, but we will need it down the road?)
            bot.Base = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                TransformedPlane = Matrix.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                RobotJointType = RobotJointType.Static,
                JointRange = new Interval(0, 0),
                MaxSpeed = 0
            };

            // Joint 1
            bot.Joint1 = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                TransformedPlane = Matrix.CreateFromPlane(70, 0, 352, 1, 0, 0, 0, 0, -1),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-180, 180),
                MaxSpeed = 200
            };

            // Joint 2
            bot.Joint2 = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(70, 0, 352, 1, 0, 0, 0, 0, -1),
                TransformedPlane = Matrix.CreateFromPlane(70, 0, 712, 0, 0, 1, 1, 0, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-90, 110),
                MaxSpeed = 200
            };

            // Joint 3
            bot.Joint3 = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(70, 0, 712, 0, 0, 1, 1, 0, 0),
                TransformedPlane = Matrix.CreateFromPlane(70, 0, 712, 0, 0, 1, 0, -1, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-230, 50),
                MaxSpeed = 260
            };

            // Joint 4
            bot.Joint4 = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(70, 0, 712, 0, 0, 1, 0, -1, 0),
                TransformedPlane = Matrix.CreateFromPlane(450, 0, 712, 0, 0, 1, 1, 0, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-200, 200),
                MaxSpeed = 360
            };

            // Joint 5
            bot.Joint5 = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(450, 0, 712, 0, 0, 1, 1, 0, 0),
                TransformedPlane = Matrix.CreateFromPlane(450, 0, 712, 0, 0, 1, 0, -1, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-115, 115),
                MaxSpeed = 360
            };

            // Joint 6
            bot.Joint6 = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(450, 0, 712, 0, 0, 1, 0, -1, 0),
                TransformedPlane = Matrix.CreateFromPlane(515, 0, 712, 0, 0, -1, 0, 1, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-400, 400),
                MaxSpeed = 450
            };

            // Assign a FK solver.
            bot.AssignSolverFK(new MarvinFK(bot));

            return bot;
        }

        /// <summary>
        /// A hard-coded factory creator for testing purposes. 
        /// </summary>
        /// <returns></returns>
        public static RobotModel CreateUR10()
        {
            RobotSixAxesArm bot = new RobotSixAxesArm();

            // The Base (this is currently reduntant, but we will need it down the road?).
            bot.Base = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                TransformedPlane = Matrix.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                RobotJointType = RobotJointType.Static,
                JointRange = new Interval(0, 0),
                MaxSpeed = 0
            };

            bot.Joint1 = RobotJoint.CreateFromDHParameters(
                bot.Base,
                127.3, 0, 90, 0,
                RobotJointType.Revolute, new Interval(-360, 360), 120);

            bot.Joint2 = RobotJoint.CreateFromDHParameters(
                bot.Joint1,
                0, -612, 0, 0,
                RobotJointType.Revolute, new Interval(-360, 360), 120);

            bot.Joint3 = RobotJoint.CreateFromDHParameters(
                bot.Joint2,
                0, -572.3, 0, 0,
                RobotJointType.Revolute, new Interval(-360, 360), 180);

            bot.Joint4 = RobotJoint.CreateFromDHParameters(
                bot.Joint3,
                163.941, 0, 90, 0,
                RobotJointType.Revolute, new Interval(-360, 360), 180);

            bot.Joint5 = RobotJoint.CreateFromDHParameters(
                bot.Joint4,
                115.7, 0, -90, 0,
                RobotJointType.Revolute, new Interval(-360, 360), 180);

            bot.Joint6 = RobotJoint.CreateFromDHParameters(
                bot.Joint5,
                92.2, 0, 0, 0,
                RobotJointType.Revolute, new Interval(-360, 360), 180);

            return bot;
        }
    }
}
