using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types.Geometry;
using Machina.Descriptors.Components;

namespace Machina.Descriptors.Models
{
    class RobotSixAxesArm : RobotModelBase
    {
        RobotJoint Base;
        RobotJoint Joint1;
        RobotJoint Joint2;
        RobotJoint Joint3;
        RobotJoint Joint4;
        RobotJoint Joint5;
        RobotJoint Joint6;
        
        public RobotSixAxesArm() : base()
        {

        }

        /// <summary>
        /// A hard-coded factory creator for testing purposes. 
        /// </summary>
        /// <returns></returns>
        public static RobotSixAxesArm CreateABBIRB140()
        {
            RobotSixAxesArm bot = new RobotSixAxesArm();

            // The Base (this is currently reduntant, but we will need it down the road?).
            bot.Base = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                TransformedPlane = Matrix4x4.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                RobotJointType = RobotJointType.Static,
                JointRange = new Interval(0, 0),
                MaxSpeed = 200
            };

            // Joint 1
            bot.Joint1 = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                TransformedPlane = Matrix4x4.CreateFromPlane(70, 0, 352, 1, 0, 0, 0, 0, -1),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-180, 180),
                MaxSpeed = 200
            };

            // Joint 2
            bot.Joint2 = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(70, 0, 352, 1, 0, 0, 0, 0, -1),
                TransformedPlane = Matrix4x4.CreateFromPlane(70, 0, 712, 0, 0, 1, 1, 0, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-90, 110),
                MaxSpeed = 200
            };

            // Joint 3
            bot.Joint3 = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(70, 0, 712, 0, 0, 1, 1, 0, 0),
                TransformedPlane = Matrix4x4.CreateFromPlane(70, 0, 712, 0, 0, 1, 0, -1, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-230, 50),
                MaxSpeed = 260
            };

            // Joint 4
            bot.Joint4 = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(70, 0, 712, 0, 0, 1, 0, -1, 0),
                TransformedPlane = Matrix4x4.CreateFromPlane(450, 0, 712, 0, 0, 1, 1, 0, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-200, 200),
                MaxSpeed = 360
            };

            // Joint 5
            bot.Joint5 = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(450, 0, 712, 0, 0, 1, 1, 0, 0),
                TransformedPlane = Matrix4x4.CreateFromPlane(450, 0, 712, 0, 0, 1, 0, -1, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-115, 115),
                MaxSpeed = 360
            };

            // Joint 6
            bot.Joint6 = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(450, 0, 712, 0, 0, 1, 0, -1, 0),
                TransformedPlane = Matrix4x4.CreateFromPlane(515, 0, 712, 0, 0, -1, 0, 1, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-400, 400),
                MaxSpeed = 450
            };

            return bot;
        }
    }
}
