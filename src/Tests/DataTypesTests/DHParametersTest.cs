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
                BasePlane = Matrix4x4.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                TransformedPlane = Matrix4x4.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                RobotJointType = RobotJointType.Static,
                JointRange = new Interval(0, 0),
                MaxSpeed = 0
            };

            // Joint 1
            RobotJoint Joint1 = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                TransformedPlane = Matrix4x4.CreateFromPlane(70, 0, 352, 1, 0, 0, 0, 0, -1),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-180, 180),
                MaxSpeed = 200
            };

            DHParameters dh1 = DHParameters.CreateFromJoint(Joint1);
            Assert.AreEqual(dh1.D, 352, Geometry.EPSILON2);
            Assert.AreEqual(dh1.R, 70, Geometry.EPSILON2);
            Assert.AreEqual(dh1.Theta, 0, Geometry.EPSILON2);
            Assert.AreEqual(dh1.Alpha, -90, Geometry.EPSILON2);

            // Joint 2
            RobotJoint Joint2 = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(70, 0, 352, 1, 0, 0, 0, 0, -1),
                TransformedPlane = Matrix4x4.CreateFromPlane(70, 0, 712, 0, 0, 1, 1, 0, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-90, 110),
                MaxSpeed = 200
            };

            DHParameters dh2 = DHParameters.CreateFromJoint(Joint2);
            Assert.AreEqual(dh2.D, 0, Geometry.EPSILON2);
            Assert.AreEqual(dh2.R, 360, Geometry.EPSILON2);
            Assert.AreEqual(dh2.Theta, -90, Geometry.EPSILON2);
            Assert.AreEqual(dh2.Alpha, 0, Geometry.EPSILON2);

            // Joint 3
            RobotJoint Joint3 = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(70, 0, 712, 0, 0, 1, 1, 0, 0),
                TransformedPlane = Matrix4x4.CreateFromPlane(70, 0, 712, 0, 0, 1, 0, -1, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-230, 50),
                MaxSpeed = 260
            };

            DHParameters dh3 = DHParameters.CreateFromJoint(Joint3);
            Assert.AreEqual(dh3.D, 0, Geometry.EPSILON2);
            Assert.AreEqual(dh3.R, 0, Geometry.EPSILON2);
            Assert.AreEqual(dh3.Theta, 0, Geometry.EPSILON2);
            Assert.AreEqual(dh3.Alpha, -90, Geometry.EPSILON2);

            // Joint 4
            RobotJoint Joint4 = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(70, 0, 712, 0, 0, 1, 0, -1, 0),
                TransformedPlane = Matrix4x4.CreateFromPlane(450, 0, 712, 0, 0, 1, 1, 0, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-200, 200),
                MaxSpeed = 360
            };

            DHParameters dh4 = DHParameters.CreateFromJoint(Joint4);
            Assert.AreEqual(dh4.D, 380, Geometry.EPSILON2);
            Assert.AreEqual(dh4.R, 0, Geometry.EPSILON2);
            Assert.AreEqual(dh4.Theta, 0, Geometry.EPSILON2);
            Assert.AreEqual(dh4.Alpha, 90, Geometry.EPSILON2);

            // Joint 5
            RobotJoint Joint5 = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(450, 0, 712, 0, 0, 1, 1, 0, 0),
                TransformedPlane = Matrix4x4.CreateFromPlane(450, 0, 712, 0, 0, 1, 0, -1, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-115, 115),
                MaxSpeed = 360
            };

            DHParameters dh5 = DHParameters.CreateFromJoint(Joint5);
            Assert.AreEqual(dh5.D, 0, Geometry.EPSILON2);
            Assert.AreEqual(dh5.R, 0, Geometry.EPSILON2);
            Assert.AreEqual(dh5.Theta, 0, Geometry.EPSILON2);
            Assert.AreEqual(dh5.Alpha, -90, Geometry.EPSILON2);

            // Joint 6
            RobotJoint Joint6 = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(450, 0, 712, 0, 0, 1, 0, -1, 0),
                TransformedPlane = Matrix4x4.CreateFromPlane(515, 0, 712, 0, 0, -1, 0, 1, 0),
                RobotJointType = RobotJointType.Revolute,
                JointRange = new Interval(-400, 400),
                MaxSpeed = 450
            };

            DHParameters dh6 = DHParameters.CreateFromJoint(Joint6);
            Assert.AreEqual(dh6.D, 65, Geometry.EPSILON2);
            Assert.AreEqual(dh6.R, 0, Geometry.EPSILON2);
            Assert.AreEqual(dh6.Theta, -180, Geometry.EPSILON2);
            Assert.AreEqual(dh6.Alpha, 0, Geometry.EPSILON2);
        }

        [TestMethod]
        public void SampleUR10()
        {
            RobotSixAxesArm bot = new RobotSixAxesArm();

            // The Base (this is currently reduntant, but we will need it down the road?).
            bot.Base = new RobotJoint
            {
                BasePlane = Matrix4x4.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                TransformedPlane = Matrix4x4.CreateFromPlane(0, 0, 0, 1, 0, 0, 0, 1, 0),
                RobotJointType = RobotJointType.Static,
                JointRange = new Interval(0, 0),
                MaxSpeed = 0
            };

            bot.Joint1 = RobotJoint.CreateFromDHParameters(
                bot.Base,
                127.3, 0, 90, 0,
                RobotJointType.Revolute, Interval.Zero, 0);

            // Values from GH tests
            Matrix4x4 j1 = Matrix4x4.CreateFromPlane(0, 0, 127.3f, 1, 0, 0, 0, 0, 1);
            Assert.IsTrue(j1.IsSimilar(bot.Joint1.TransformedPlane, Geometry.EPSILON2));
            
            bot.Joint2 = RobotJoint.CreateFromDHParameters(
                bot.Joint1,
                0, -612, 0, 0,
                RobotJointType.Revolute, Interval.Zero, 0);
            Matrix4x4 j2 = Matrix4x4.CreateFromPlane(-612, 0, 127.3f, 1, 0, 0, 0, 0, 1);
            Assert.IsTrue(j2.IsSimilar(bot.Joint2.TransformedPlane, Geometry.EPSILON2));

            bot.Joint3 = RobotJoint.CreateFromDHParameters(
                bot.Joint2,
                0, -572.3, 0, 0,
                RobotJointType.Revolute, Interval.Zero, 0);
            Matrix4x4 j3 = Matrix4x4.CreateFromPlane(-1184.3f, 0, 127.3f, 1, 0, 0, 0, 0, 1);
            Assert.IsTrue(j3.IsSimilar(bot.Joint3.TransformedPlane, Geometry.EPSILON2));

            bot.Joint4 = RobotJoint.CreateFromDHParameters(
                bot.Joint3,
                163.941, 0, 90, 0,
                RobotJointType.Revolute, Interval.Zero, 0);
            Matrix4x4 j4 = Matrix4x4.CreateFromPlane(-1184.3f, -163.941f, 127.3f, 1, 0, 0, 0, -1, 0);
            Assert.IsTrue(j4.IsSimilar(bot.Joint4.TransformedPlane, Geometry.EPSILON2));

            bot.Joint5 = RobotJoint.CreateFromDHParameters(
                bot.Joint4,
                115.7, 0, -90, 0,
                RobotJointType.Revolute, Interval.Zero, 0);
            Matrix4x4 j5 = Matrix4x4.CreateFromPlane(-1184.3f, -163.941f, 11.6f, 1, 0, 0, 0, 0, 1);
            Console.WriteLine(j5);
            Console.WriteLine(bot.Joint5.TransformedPlane);
            Assert.IsTrue(j5.IsSimilar(bot.Joint5.TransformedPlane, Geometry.EPSILON2));

            bot.Joint6 = RobotJoint.CreateFromDHParameters(
                bot.Joint5,
                92.2, 0, 0, 0,
                RobotJointType.Revolute, Interval.Zero, 0);
            Matrix4x4 j6 = Matrix4x4.CreateFromPlane(-1184.3f, -256.141f, 11.6f, 1, 0, 0, 0, 0, 1);
            Assert.IsTrue(j6.IsSimilar(bot.Joint6.TransformedPlane, Geometry.EPSILON2));

        }
    }
}
