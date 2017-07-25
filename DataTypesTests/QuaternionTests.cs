using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using BRobot;
using SysQuat = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;
using SysMatrix44 = System.Numerics.Matrix4x4;
using System.Collections.Generic;

namespace DataTypesTests
{
    [TestClass]
    public class QuaternionTests : DataTypesTests
    {

        public static double TAU = 2 * Math.PI;
        

        //   ██████╗ ██╗   ██╗ █████╗ ████████╗███████╗██████╗ ███╗   ██╗██╗ ██████╗ ███╗   ██╗
        //  ██╔═══██╗██║   ██║██╔══██╗╚══██╔══╝██╔════╝██╔══██╗████╗  ██║██║██╔═══██╗████╗  ██║
        //  ██║   ██║██║   ██║███████║   ██║   █████╗  ██████╔╝██╔██╗ ██║██║██║   ██║██╔██╗ ██║
        //  ██║▄▄ ██║██║   ██║██╔══██║   ██║   ██╔══╝  ██╔══██╗██║╚██╗██║██║██║   ██║██║╚██╗██║
        //  ╚██████╔╝╚██████╔╝██║  ██║   ██║   ███████╗██║  ██║██║ ╚████║██║╚██████╔╝██║ ╚████║
        //   ╚══▀▀═╝  ╚═════╝ ╚═╝  ╚═╝   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝
        //                                                                                     

        /// <summary>
        /// A general test that goes over Quaternions and checks basic Normalization,
        /// Length, and versor and null checks.
        /// </summary>
        [TestMethod]
        public void Quaternion_Normalization_Lengths_Units_Zeros()
        {
            Quaternion q;

            double w, x, y, z;
            double len, len2;
            bool norm;

            // Test random quaternions
            for (var i = 0; i < 50; i++)
            {
                w = Random(-100, 100);
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                len = Math.Sqrt(w * w + x * x + y * y + z * z);

                Trace.WriteLine("");
                Trace.WriteLine(w + " " + x + " " + y + " " + z);
                Trace.WriteLine(len);

                q = new Quaternion(w, x, y, z);
                Trace.WriteLine(q);

                // Did the quaternion get normalized? It should probably be different than the randomly gen on
                len2 = Math.Sqrt(q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
                Trace.WriteLine(q + " " + len2);
                Assert.AreNotEqual(len, len2, 0.00000001);

                // Is Length working?
                len = q.Length();
                Trace.WriteLine(q + " " + len2);
                Assert.AreEqual(len2, len, 0.0000001);

                // In Normalize working?
                norm = q.Normalize();
                len2 = q.Length();
                Assert.AreEqual(1, len2, 0.0000001);
            }

            // Test all permutations of unitary components quaternions (including zero)
            for (w = -1; w <= 1; w++)
            {
                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        for (z = -1; z <= 1; z++)
                        {
                            Trace.WriteLine("");
                            Trace.WriteLine(w + " " + x + " " + y + " " + z);

                            q = new Quaternion(w, x, y, z);
                            Trace.WriteLine(q);

                            //// THIS IS OBSOLETE, SINCE A ZERO QUATERNION SHOULD ALWAYS TURN INTO AN IDENTITY ONE
                            //// Is Zero Quaternion properly handled?
                            //if (w == 0 && x == 0 && y == 0 && z == 0)
                            //{
                            //    Assert.IsTrue(q.IsZero());
                            //}
                            //else
                            //{
                            //    Assert.IsFalse(q.IsZero());
                            //}

                            // Is Zero/Identity Quaternion properly handled?
                            if ((w == 0 || w == 1 || w == -1) && x == 0 && y == 0 && z == 0)
                            {
                                Assert.IsTrue(q.IsIdentity(), "Failed IsIdentity()");
                            }
                            else
                            {
                                Assert.IsFalse(q.IsIdentity(), "Failed IsIdentity()");
                            }

                            // Is Length working?
                            len = Math.Sqrt(q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
                            Trace.WriteLine(q + " " + len);
                            Assert.AreEqual(len, q.Length(), 0.0000001, "Failed Length() for " + q);

                            // Is Unit() working?
                            if (Math.Abs(len - 1) < 0.0000001)
                            {
                                Assert.IsTrue(q.IsUnit());
                            }
                            else
                            {
                                Assert.IsFalse(q.IsUnit());
                            }

                            // In Normalize working?
                            norm = q.Normalize();
                            Trace.WriteLine(q + " " + len);

                            len = Math.Sqrt(q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
                            Assert.AreEqual(1, len, 0.0000001, "Failed Normalize() for " + q);


                            // Is Length working?
                            Trace.WriteLine(q + " " + len);
                            Assert.AreEqual(len, q.Length(), 0.0000001, "Failed normalized Length()");

                        }
                    }
                }
            }
        }

        // Any Quaternion is correctly normalized
        [TestMethod]
        public void Quaternion_Normalization()
        {
            Quaternion q;

            double w, x, y, z;
            double len;

            // Test random quaternions
            for (var i = 0; i < 50; i++)
            {
                w = Random(-100, 100);
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);

                Trace.WriteLine("");
                Trace.WriteLine(w + " " + x + " " + y + " " + z);

                q = new Quaternion(w, x, y, z);  // gets automatically normalized
                Trace.WriteLine(q);

                // Raw check
                len = Math.Sqrt(q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
                Trace.WriteLine(len);
                Assert.AreEqual(1, len, 0.000001);

                // .Length() check
                len = q.Length();
                Trace.WriteLine(len);
                Assert.AreEqual(1, len, 0.0000001);
            }

            // Test all permutations of unitary components quaternions (including zero)
            for (w = -2; w <= 2; w += 0.25)
            {
                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        for (z = -1; z <= 1; z++)
                        {
                            Trace.WriteLine("");
                            Trace.WriteLine(w + " " + x + " " + y + " " + z);

                            q = new Quaternion(w, x, y, z);  // gets automatically normalized
                            Trace.WriteLine(q);

                            // Raw check
                            len = Math.Sqrt(q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
                            Trace.WriteLine(len);
                            Assert.AreEqual(1, len, 0.000001);

                            // .Length() check
                            len = q.Length();
                            Trace.WriteLine(len);
                            Assert.AreEqual(1, len, 0.0000001);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The result of normalizing a zero-quaternion should be the positive identity quaternion.
        /// </summary>
        [TestMethod]
        public void Quaternion_VectorNormalizeZeroQuaternion()
        {
            Quaternion q;

            q = new Quaternion(0, 0, 0, 0);

            Trace.WriteLine("");
            Trace.WriteLine("0 0 0 0");
            Trace.WriteLine(q);

            Assert.AreEqual(1, q.W, 0.00001);
            Assert.AreEqual(0, q.X, 0.00001);
            Assert.AreEqual(0, q.Y, 0.00001);
            Assert.AreEqual(0, q.Z, 0.00001);
        }

        /// <summary>
        /// Identity quaternions should remain the same on creation.
        /// </summary>
        [TestMethod]
        public void Quaternion_VectorNormalizeIdentityQuaternions()
        {
            Quaternion q;

            q = new Quaternion(1, 0, 0, 0);
            Trace.WriteLine("");
            Trace.WriteLine(q);

            Assert.AreEqual(1, q.W, 0.00001);
            Assert.AreEqual(0, q.X, 0.00001);
            Assert.AreEqual(0, q.Y, 0.00001);
            Assert.AreEqual(0, q.Z, 0.00001);
            Assert.IsTrue(q.NormalizeVector());

            q = new Quaternion(-1, 0, 0, 0);
            Trace.WriteLine("");
            Trace.WriteLine(q);
            Assert.AreEqual(-1, q.W, 0.00001);
            Assert.AreEqual(0, q.X, 0.00001);
            Assert.AreEqual(0, q.Y, 0.00001);
            Assert.AreEqual(0, q.Z, 0.00001);
            Assert.IsTrue(q.NormalizeVector());
        }

        /// <summary>
        /// Quaternions with no rotation vector should normalize to identity vector.
        /// </summary>
        [TestMethod]
        public void Quaternion_VectorNormalizeZeroAxisQuaternions()
        {
            Quaternion q;
            double w;

            for (var i = 0; i < 10; i++)
            {
                w = Random(-2, 2);
                Trace.WriteLine("");
                Trace.WriteLine(w + " 0 0 0");
                q = new Quaternion(w, 0, 0, 0);
                Trace.WriteLine(q);
                Assert.AreEqual(w >= 0 ? 1 : -1, q.W, 0.00001);
                Assert.AreEqual(0, q.X, 0.00001);
                Assert.AreEqual(0, q.Y, 0.00001);
                Assert.AreEqual(0, q.Z, 0.00001);
                Assert.IsTrue(q.NormalizeVector());
            }
        }



        /// <summary>
        /// Tests Q -> AA -> Q
        /// </summary>
        [TestMethod]
        public void Quaternion_ToAxisAngle_ToQuaternion()
        {
            Quaternion q1, q2;
            AxisAngle aa;

            double w, x, y, z;

            // Test random quaternions
            for (var i = 0; i < 50; i++)
            {
                w = Random(-1, 1);  // force vector-normalization
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);

                Trace.WriteLine("");
                Trace.WriteLine(w + " " + x + " " + y + " " + z);

                q1 = new Quaternion(w, x, y, z);  // gets automatically normalized
                Trace.WriteLine(q1);

                aa = q1.ToAxisAngle();
                Trace.WriteLine(aa);

                q2 = aa.ToQuaternion();
                Trace.WriteLine(q2);

                Assert.IsTrue(q1 == q2, "Booo! :(");
            }

            // Test all permutations of unitary components quaternions (including zero)
            for (w = -2; w <= 2; w += 0.5)  // test vector + non-vector normalization
            {
                for (x = -1; x <= 1; x += 0.5)
                {
                    for (y = -1; y <= 1; y += 0.5)
                    {
                        for (z = -1; z <= 1; z += 0.5)
                        {
                            Trace.WriteLine("");
                            Trace.WriteLine(w + " " + x + " " + y + " " + z);

                            q1 = new Quaternion(w, x, y, z);  // gets automatically normalized
                            Trace.WriteLine(q1);

                            aa = q1.ToAxisAngle();
                            Trace.WriteLine(aa);

                            q2 = aa.ToQuaternion();
                            Trace.WriteLine(q2);
                            
                            if (q1.IsIdentity())
                            {
                                // Make this pass:
                                /* Make this pass:
                                   -1 0 0 0
                                   Quaternion[-1, 0, 0, 0]
                                   AxisAngle[0, 0, 0, 360]
                                   Quaternion[1, 0, 0, 0]
                                 */
                                Assert.IsTrue(q2.IsIdentity());  
                            }
                            else
                            {
                                Assert.IsTrue(q1 == q2, "Booo!");
                            }
                            
                        }
                    }
                }
            }

        }

        [TestMethod]
        public void Quaternion_ToRotationMatrix_ToQuaternion()
        {
            Quaternion q1, q2;
            RotationMatrix m;

            SysQuat sq1, sq2;
            SysMatrix44 sm;

            double w, x, y, z;
            int method;

            List<string> quatsIn = new List<string>();
            quatsIn.Add("W,X,Y,Z,FLIPPED,METHOD");
            List<string> quatsOut = new List<string>();
            quatsOut.Add("W,X,Y,Z,FLIPPED,METHOD");

            // Test random quaternions
            for (var i = 0; i < 2000; i++)
            {
                w = Random(-1, 1);  // force vector-normalization
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);

                q1 = new Quaternion(w, x, y, z);  // gets automatically normalized
                m = q1.ToRotationMatrix();
                q2 = m.ToQuaternion(out method);

                // Compare with System quaternions
                sq1 = new SysQuat((float)q1.X, (float)q1.Y, (float)q1.Z, (float)q1.W);  // use same q1 normalization
                sm = SysMatrix44.CreateFromQuaternion(sq1);
                sq2 = SysQuat.CreateFromRotationMatrix(sm);

                Trace.WriteLine("");
                Trace.WriteLine(w + " " + x + " " + y + " " + z);
                Trace.WriteLine(q1);
                Trace.WriteLine(m);
                Trace.WriteLine(q2);
                Trace.WriteLine(sq1);
                Trace.WriteLine(sm);
                Trace.WriteLine(sq2);

                // TEMP: create a dump CSV file with these quats
                    quatsIn.Add(string.Format("{0},{1},{2},{3},{4},{5}", q1.W, q1.X, q1.Y, q1.Z, q1 == q2 ? 0 : 1, method));
                    quatsOut.Add(string.Format("{0},{1},{2},{3},{4},{5}", q2.W, q2.X, q2.Y, q2.Z, q1 == q2 ? 0 : 1, method));

                System.IO.File.WriteAllLines(@"quaternionsIn.csv", quatsIn, System.Text.Encoding.UTF8);
                System.IO.File.WriteAllLines(@"quaternionsOut.csv", quatsOut, System.Text.Encoding.UTF8);

                //if (HasAnyComponentBelowOrEqual(q1, -0.5))
                //{
                //    q2.Scale(-1);
                //}

                //Assert.IsTrue(q1 == q2, "Booo! :(");
            }

            //Console.WriteLine("flipped:");
            //foreach(Quaternion q in flipped)
            //{
            //    Console.WriteLine(q);
            //}
            //Console.WriteLine("nonflipped");
            //foreach (Quaternion q in nonflipped)
            //{
            //    Console.WriteLine(q);
            //}

            //// Test all permutations of unitary components quaternions (including zero)
            //for (w = -2; w <= 2; w += 0.5)  // test vector + non-vector normalization
            //{
            //    for (x = -1; x <= 1; x += 0.5)
            //    {
            //        for (y = -1; y <= 1; y += 0.5)
            //        {
            //            for (z = -1; z <= 1; z += 0.5)
            //            {
            //                q1 = new Quaternion(w, x, y, z);  // gets automatically normalized
            //                m = q1.ToRotationMatrix();
            //                q2 = m.ToQuaternion();

            //                Trace.WriteLine("");
            //                Trace.WriteLine(w + " " + x + " " + y + " " + z);
            //                Trace.WriteLine(q1);
            //                Trace.WriteLine(m);
            //                Trace.WriteLine(q2);

            //                if (q1.IsIdentity())
            //                {
            //                    //Assert.IsTrue(q2.IsIdentity());
            //                }
            //                else
            //                {
            //                    //Assert.IsTrue(q1 == q2, "Booo!");
            //                }

            //            }
            //        }
            //    }
            //}
        }


        public bool HasAnyComponentBelowOrEqual(Quaternion q, double threshold)
        {
            return q.W <= threshold || q.X <= threshold || q.Y <= threshold || q.Z <= threshold;
        }

    }
}
