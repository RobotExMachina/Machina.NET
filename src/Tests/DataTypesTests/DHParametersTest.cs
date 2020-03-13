using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Machina;
using Machina.Types.Data;
using Machina.Types.Geometry;
using Machina.Descriptors.Components;
using Machina.Descriptors.Models;

namespace DataTypesTests
{
    [TestClass]
    public class DHParametersTest : DataTypesTests
    {
        [TestMethod]
        public void SampleABBIRB140()
        {
            // The Base (this is currently reduntant, but we will need it down the road?).
            RobotJoint Base = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                TransformedPlane = Matrix.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                RobotJointType = RobotJointType.Static,
                JointRange = new Interval(0, 0),
                MaxSpeed = 0
            };

            // Joint 1
            RobotJoint Joint1 = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                TransformedPlane = Matrix.CreateFromPlane(70, 0, 352, 1, 0, 0, 0, 0, -1),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-180, 180),
                MaxSpeed = 200
            };

            DHParameters dh1 = DHParameters.CreateFromJoint(Joint1);
            Assert.AreEqual(dh1.D, 352, MMath.EPSILON2);
            Assert.AreEqual(dh1.R, 70, MMath.EPSILON2);
            Assert.AreEqual(dh1.Theta, 0, MMath.EPSILON2);
            Assert.AreEqual(dh1.Alpha, -90, MMath.EPSILON2);

            // Joint 2
            RobotJoint Joint2 = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(70, 0, 352, 1, 0, 0, 0, 0, -1),
                TransformedPlane = Matrix.CreateFromPlane(70, 0, 712, 0, 0, 1, 1, 0, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-90, 110),
                MaxSpeed = 200
            };

            DHParameters dh2 = DHParameters.CreateFromJoint(Joint2);
            Assert.AreEqual(dh2.D, 0, MMath.EPSILON2);
            Assert.AreEqual(dh2.R, 360, MMath.EPSILON2);
            Assert.AreEqual(dh2.Theta, -90, MMath.EPSILON2);
            Assert.AreEqual(dh2.Alpha, 0, MMath.EPSILON2);

            // Joint 3
            RobotJoint Joint3 = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(70, 0, 712, 0, 0, 1, 1, 0, 0),
                TransformedPlane = Matrix.CreateFromPlane(70, 0, 712, 0, 0, 1, 0, -1, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-230, 50),
                MaxSpeed = 260
            };

            DHParameters dh3 = DHParameters.CreateFromJoint(Joint3);
            Assert.AreEqual(dh3.D, 0, MMath.EPSILON2);
            Assert.AreEqual(dh3.R, 0, MMath.EPSILON2);
            Assert.AreEqual(dh3.Theta, 0, MMath.EPSILON2);
            Assert.AreEqual(dh3.Alpha, -90, MMath.EPSILON2);

            // Joint 4
            RobotJoint Joint4 = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(70, 0, 712, 0, 0, 1, 0, -1, 0),
                TransformedPlane = Matrix.CreateFromPlane(450, 0, 712, 0, 0, 1, 1, 0, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-200, 200),
                MaxSpeed = 360
            };

            DHParameters dh4 = DHParameters.CreateFromJoint(Joint4);
            Assert.AreEqual(dh4.D, 380, MMath.EPSILON2);
            Assert.AreEqual(dh4.R, 0, MMath.EPSILON2);
            Assert.AreEqual(dh4.Theta, 0, MMath.EPSILON2);
            Assert.AreEqual(dh4.Alpha, 90, MMath.EPSILON2);

            // Joint 5
            RobotJoint Joint5 = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(450, 0, 712, 0, 0, 1, 1, 0, 0),
                TransformedPlane = Matrix.CreateFromPlane(450, 0, 712, 0, 0, 1, 0, -1, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-115, 115),
                MaxSpeed = 360
            };

            DHParameters dh5 = DHParameters.CreateFromJoint(Joint5);
            Assert.AreEqual(dh5.D, 0, MMath.EPSILON2);
            Assert.AreEqual(dh5.R, 0, MMath.EPSILON2);
            Assert.AreEqual(dh5.Theta, 0, MMath.EPSILON2);
            Assert.AreEqual(dh5.Alpha, -90, MMath.EPSILON2);

            // Joint 6
            RobotJoint Joint6 = new RobotJoint
            {
                BasePlane = Matrix.CreateFromPlane(450, 0, 712, 0, 0, 1, 0, -1, 0),
                TransformedPlane = Matrix.CreateFromPlane(515, 0, 712, 0, 0, -1, 0, 1, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-400, 400),
                MaxSpeed = 450
            };

            DHParameters dh6 = DHParameters.CreateFromJoint(Joint6);
            Assert.AreEqual(dh6.D, 65, MMath.EPSILON2);
            Assert.AreEqual(dh6.R, 0, MMath.EPSILON2);
            Assert.AreEqual(dh6.Theta, -180, MMath.EPSILON2);
            Assert.AreEqual(dh6.Alpha, 0, MMath.EPSILON2);
        }

        [TestMethod]
        public void SampleUR10()
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
                RobotJointType.Revolute, Interval.Zero, 0);

            // Values from GH tests
            Matrix j1 = Matrix.CreateFromPlane(0, 0, 127.3, 1, 0, 0, 0, 0, 1);
            Assert.IsTrue(j1.IsSimilarTo(bot.Joint1.TransformedPlane, MMath.EPSILON2));
            
            bot.Joint2 = RobotJoint.CreateFromDHParameters(
                bot.Joint1,
                0, -612, 0, 0,
                RobotJointType.Revolute, Interval.Zero, 0);
            Matrix j2 = Matrix.CreateFromPlane(-612, 0, 127.3, 1, 0, 0, 0, 0, 1);
            Assert.IsTrue(j2.IsSimilarTo(bot.Joint2.TransformedPlane, MMath.EPSILON2));

            bot.Joint3 = RobotJoint.CreateFromDHParameters(
                bot.Joint2,
                0, -572.3, 0, 0,
                RobotJointType.Revolute, Interval.Zero, 0);
            Matrix j3 = Matrix.CreateFromPlane(-1184.3, 0, 127.3, 1, 0, 0, 0, 0, 1);
            Assert.IsTrue(j3.IsSimilarTo(bot.Joint3.TransformedPlane, MMath.EPSILON2));

            bot.Joint4 = RobotJoint.CreateFromDHParameters(
                bot.Joint3,
                163.941, 0, 90, 0,
                RobotJointType.Revolute, Interval.Zero, 0);
            Matrix j4 = Matrix.CreateFromPlane(-1184.3, -163.941, 127.3, 1, 0, 0, 0, -1, 0);
            Assert.IsTrue(j4.IsSimilarTo(bot.Joint4.TransformedPlane, MMath.EPSILON2));

            bot.Joint5 = RobotJoint.CreateFromDHParameters(
                bot.Joint4,
                115.7, 0, -90, 0,
                RobotJointType.Revolute, Interval.Zero, 0);
            Matrix j5 = Matrix.CreateFromPlane(-1184.3, -163.941, 11.6, 1, 0, 0, 0, 0, 1);
            Assert.IsTrue(j5.IsSimilarTo(bot.Joint5.TransformedPlane, MMath.EPSILON2));

            bot.Joint6 = RobotJoint.CreateFromDHParameters(
                bot.Joint5,
                92.2, 0, 0, 0,
                RobotJointType.Revolute, Interval.Zero, 0);
            Matrix j6 = Matrix.CreateFromPlane(-1184.3, -256.141, 11.6, 1, 0, 0, 0, 0, 1);
            Assert.IsTrue(j6.IsSimilarTo(bot.Joint6.TransformedPlane, MMath.EPSILON2));

        }
    }
}
