using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using BRobot;
using SysQuat = System.Numerics.Quaternion;
using SysVec = System.Numerics.Vector3;
using SysMatrix44 = System.Numerics.Matrix4x4;


namespace RobotTests
{
    public class RobotTests
    {
        /// <summary>
        /// Precision for floating-point comparisons.
        /// </summary>
        internal static readonly double EPSILON = 0.000001;

        /// <summary>
        /// A more permissive precision factor.
        /// </summary>
        internal static readonly double EPSILON2 = 0.001;

        /// <summary>
        /// A more restrictive precision factor.
        /// </summary>
        internal static readonly double EPSILON3 = 0.000000001;

        internal static Random rnd = new System.Random();

        public double Random(double min, double max)
        {
            return Lerp(min, max, rnd.NextDouble());
        }

        public double Random()
        {
            return Random(0, 1);
        }

        public double Random(double max)
        {
            return Random(0, max);
        }

        public int RandomInt(int min, int max)
        {
            return rnd.Next(min, max + 1);
        }

        public double Lerp(double start, double end, double norm)
        {
            return start + (end - start) * norm;
        }

        public double Normalize(double value, double start, double end)
        {
            return (value - start) / (end - start);
        }

        public double Map(double value, double sourceStart, double sourceEnd, double targetStart, double targetEnd)
        {
            //double n = Normalize(value, sourceStart, sourceEnd);
            //return targetStart + n * (targetEnd - targetStart);
            return targetStart + (targetEnd - targetStart) * (value - sourceStart) / (sourceEnd - sourceStart);
        }

        /// <summary>
        /// Compare under EPSILON2 (floats have little precision)
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="sysvec"></param>
        /// <returns></returns>
        public static bool AreSimilar(Vector vec, SysVec sysvec)
        {
            return Math.Abs(vec.X - (double)sysvec.X) < EPSILON2
                && Math.Abs(vec.Y - (double)sysvec.Y) < EPSILON2
                && Math.Abs(vec.Z - (double)sysvec.Z) < EPSILON2;
        }

    }
}
