using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types.Data;

namespace Machina.Types.Geometry
{
    //  ███╗   ███╗ █████╗ ████████╗██████╗ ██╗██╗  ██╗██╗  ██╗██╗  ██╗██╗  ██╗
    //  ████╗ ████║██╔══██╗╚══██╔══╝██╔══██╗██║╚██╗██╔╝██║  ██║╚██╗██╔╝██║  ██║
    //  ██╔████╔██║███████║   ██║   ██████╔╝██║ ╚███╔╝ ███████║ ╚███╔╝ ███████║
    //  ██║╚██╔╝██║██╔══██║   ██║   ██╔══██╗██║ ██╔██╗ ╚════██║ ██╔██╗ ╚════██║
    //  ██║ ╚═╝ ██║██║  ██║   ██║   ██║  ██║██║██╔╝ ██╗     ██║██╔╝ ██╗     ██║
    //  ╚═╝     ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝╚═╝╚═╝  ╚═╝     ╚═╝╚═╝  ╚═╝     ╚═╝
    //                                                                         
    /// <summary>
    /// A 4x4 matrix for spatial transformations. Matrix is in vector-column format, 
    /// i.e. vector multiplication is M' = Mv 
    /// </summary>
    /// <remarks>
    /// This is a modified version of System.Numerics.Matrix4x4. 
    /// Why reinvent the wheel, and not use the original instead? A couple reasons:
    /// - System.Numerics.Matrix4x4 is only available .NET Framework >4.6. 
    ///     If I ever want to use this in Unity, it would not work. 
    ///     Also, it uses a row-vector convention, which is clearer to understand 
    ///     when doing transformation operations, but that's precisely why I dislike:
    ///     if you are reading this, you should know your matrix algebra well... :) 
    /// - To learn/practice/expand. "What I cannot create, I do not understand..." Richard Feynmann. 
    /// - This also contributes to the educational character of this open-source project.
    /// - To integrate it with other Machina data types.
    /// Will bring a lot of code from the System.Numerics version anyway!
    /// </remarks>
    public struct Matrix4x4 : IEquatable<Matrix4x4>, IEpsilonComparable<Matrix4x4>, ISerializableArray, ISerializableJSON
    {
        #region Public Fields
        /// <summary>
        /// Value at row 1, column 1 of the matrix.
        /// </summary>
        public double M11;
        /// <summary>
        /// Value at row 1, column 2 of the matrix.
        /// </summary>
        public double M12;
        /// <summary>
        /// Value at row 1, column 3 of the matrix.
        /// </summary>
        public double M13;
        /// <summary>
        /// Value at row 1, column 4 of the matrix.
        /// </summary>
        public double M14;

        /// <summary>
        /// Value at row 2, column 1 of the matrix.
        /// </summary>
        public double M21;
        /// <summary>
        /// Value at row 2, column 2 of the matrix.
        /// </summary>
        public double M22;
        /// <summary>
        /// Value at row 2, column 3 of the matrix.
        /// </summary>
        public double M23;
        /// <summary>
        /// Value at row 2, column 4 of the matrix.
        /// </summary>
        public double M24;

        /// <summary>
        /// Value at row 3, column 1 of the matrix.
        /// </summary>
        public double M31;
        /// <summary>
        /// Value at row 3, column 2 of the matrix.
        /// </summary>
        public double M32;
        /// <summary>
        /// Value at row 3, column 3 of the matrix.
        /// </summary>
        public double M33;
        /// <summary>
        /// Value at row 3, column 4 of the matrix.
        /// </summary>
        public double M34;

        /// <summary>
        /// Value at row 4, column 1 of the matrix.
        /// </summary>
        public double M41;
        /// <summary>
        /// Value at row 4, column 2 of the matrix.
        /// </summary>
        public double M42;
        /// <summary>
        /// Value at row 4, column 3 of the matrix.
        /// </summary>
        public double M43;
        /// <summary>
        /// Value at row 4, column 4 of the matrix.
        /// </summary>
        public double M44;

        #endregion Public Fields


        #region constructors
        /// <summary>
        /// Constructs a Matrix4x4 from the given components.
        /// </summary>
        public Matrix4x4(double m11, double m12, double m13, double m14,
                         double m21, double m22, double m23, double m24,
                         double m31, double m32, double m33, double m34,
                         double m41, double m42, double m43, double m44)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M14 = m14;

            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M24 = m24;

            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
            this.M34 = m34;

            this.M41 = m41;
            this.M42 = m42;
            this.M43 = m43;
            this.M44 = m44;
        }

        /// <summary>
        /// Constructs a Matrix4x4 from the given components.
        /// </summary>
        public Matrix4x4(double[] values)
        {
            this.M11 = values[0];
            this.M12 = values[1];
            this.M13 = values[2];
            this.M14 = values[3];

            this.M21 = values[4];
            this.M22 = values[5];
            this.M23 = values[6];
            this.M24 = values[7];

            this.M31 = values[8];
            this.M32 = values[9];
            this.M33 = values[10];
            this.M34 = values[11];

            this.M41 = values[12];
            this.M42 = values[13];
            this.M43 = values[14];
            this.M44 = values[15];
        }

        private static readonly Matrix4x4 _identity = new Matrix4x4
        (
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        );

        /// <summary>
        /// Returns the multiplicative identity matrix.
        /// </summary>
        public static Matrix4x4 Identity
        {
            get { return _identity; }
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="position">The amount to translate in each axis.</param>
        /// <returns>The translation matrix.</returns>
        public static Matrix4x4 CreateTranslation(Vector position)
        {
            Matrix4x4 result;

            result.M11 = 1;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = position.X;

            result.M21 = 0;
            result.M22 = 1;
            result.M23 = 0;
            result.M24 = position.Y;

            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
            result.M34 = position.Z;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="xPosition">The amount to translate on the X-axis.</param>
        /// <param name="yPosition">The amount to translate on the Y-axis.</param>
        /// <param name="zPosition">The amount to translate on the Z-axis.</param>
        /// <returns>The translation matrix.</returns>
        public static Matrix4x4 CreateTranslation(double xPosition, double yPosition, double zPosition)
        {
            Matrix4x4 result;

            result.M11 = 1;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = xPosition;

            result.M21 = 0;
            result.M22 = 1;
            result.M23 = 0;
            result.M24 = yPosition;

            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
            result.M34 = zPosition;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Creates a scaling matrix.
        /// </summary>
        /// <param name="xScale">Value to scale by on the X-axis.</param>
        /// <param name="yScale">Value to scale by on the Y-axis.</param>
        /// <param name="zScale">Value to scale by on the Z-axis.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix4x4 CreateScale(double xScale, double yScale, double zScale)
        {
            Matrix4x4 result;

            result.M11 = xScale;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;

            result.M21 = 0;
            result.M22 = yScale;
            result.M23 = 0;
            result.M24 = 0;

            result.M31 = 0;
            result.M32 = 0;
            result.M33 = zScale;
            result.M34 = 0;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Creates a scaling matrix.
        /// </summary>
        /// <param name="scales">The vector containing the amount to scale by on each axis.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix4x4 CreateScale(Vector scales)
        {
            Matrix4x4 result;

            result.M11 = scales.X;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;

            result.M21 = 0;
            result.M22 = scales.Y;
            result.M23 = 0;
            result.M24 = 0;

            result.M31 = 0;
            result.M32 = 0;
            result.M33 = scales.Z;
            result.M34 = 0;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Creates a uniform scaling matrix that scales equally on each axis.
        /// </summary>
        /// <param name="scale">The uniform scaling factor.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix4x4 CreateScale(double scale)
        {
            Matrix4x4 result;

            result.M11 = scale;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;

            result.M21 = 0;
            result.M22 = scale;
            result.M23 = 0;
            result.M24 = 0;

            result.M31 = 0;
            result.M32 = 0;
            result.M33 = scale;
            result.M34 = 0;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Creates a scaling matrix with a center point.
        /// </summary>
        /// <param name="xScale">Value to scale by on the X-axis.</param>
        /// <param name="yScale">Value to scale by on the Y-axis.</param>
        /// <param name="zScale">Value to scale by on the Z-axis.</param>
        /// <param name="centerPoint">The center point.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix4x4 CreateScale(double xScale, double yScale, double zScale, Vector centerPoint)
        {
            Matrix4x4 result;

            double tx = centerPoint.X * (1 - xScale);
            double ty = centerPoint.Y * (1 - yScale);
            double tz = centerPoint.Z * (1 - zScale);

            result.M11 = xScale;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = tx;

            result.M21 = 0;
            result.M22 = yScale;
            result.M23 = 0;
            result.M24 = ty;

            result.M31 = 0;
            result.M32 = 0;
            result.M33 = zScale;
            result.M34 = tz;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Creates a matrix for rotating points around the X-axis.
        /// </summary>
        /// <param name="degrees">The amount, in degrees, by which to rotate around the X-axis.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix4x4 CreateRotationX(double degrees)
        {
            Matrix4x4 result;

            double radians = degrees * MMath.TO_RADS;
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);

            // [  1  0  0  0 ]
            // [  0  c -s  0 ]
            // [  0  s  c  0 ]
            // [  0  0  0  1 ]
            result.M11 = 1;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;

            result.M21 = 0;
            result.M22 = c;
            result.M23 = -s;
            result.M24 = 0;

            result.M31 = 0;
            result.M32 = s;
            result.M33 = c;
            result.M34 = 0;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Creates a matrix for rotating points around the X-axis, from a center point.
        /// </summary>
        /// <param name="degrees">The amount, in degrees, by which to rotate around the X-axis.</param>
        /// <param name="centerPoint">The center point.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix4x4 CreateRotationX(double degrees, Vector centerPoint)
        {
            Matrix4x4 result;

            double radians = degrees * MMath.TO_RADS;
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);

            double y = centerPoint.Y * (1 - c) + centerPoint.Z * s;
            double z = centerPoint.Z * (1 - c) - centerPoint.Y * s;

            // [  1  0  0  0 ]
            // [  0  c -s  y ]
            // [  0  s  c  z ]
            // [  0  0  0  1 ]
            result.M11 = 1;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;

            result.M21 = 0;
            result.M22 = c;
            result.M23 = -s;
            result.M24 = y;

            result.M31 = 0;
            result.M32 = s;
            result.M33 = c;
            result.M34 = z;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Creates a matrix for rotating points around the Y-axis.
        /// </summary>
        /// <param name="degrees">The amount, in degrees, by which to rotate around the Y-axis.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix4x4 CreateRotationY(double degrees)
        {
            Matrix4x4 result;

            double radians = degrees * MMath.TO_RADS;
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);

            // [  c  0  s  0 ]
            // [  0  1  0  0 ]
            // [ -s  0  c  0 ]
            // [  0  0  0  1 ]
            result.M11 = c;
            result.M12 = 0;
            result.M13 = s;
            result.M14 = 0;

            result.M21 = 0;
            result.M22 = 1;
            result.M23 = 0;
            result.M24 = 0;

            result.M31 = -s;
            result.M32 = 0;
            result.M33 = c;
            result.M34 = 0;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Creates a matrix for rotating points around the Y-axis, from a center point.
        /// </summary>
        /// <param name="degrees">The amount, in degrees, by which to rotate around the Y-axis.</param>
        /// <param name="centerPoint">The center point.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix4x4 CreateRotationY(double degrees, Vector centerPoint)
        {
            Matrix4x4 result;

            double radians = degrees * MMath.TO_RADS;
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);

            double x = centerPoint.X * (1 - c) - centerPoint.Z * s;
            double z = centerPoint.Z * (1 - c) + centerPoint.X * s;

            // [  c  0  s  x ]
            // [  0  1  0  0 ]
            // [ -s  0  c  z ]
            // [  0  0  0  1 ]
            result.M11 = c;
            result.M12 = 0;
            result.M13 = s;
            result.M14 = x;

            result.M21 = 0;
            result.M22 = 1;
            result.M23 = 0;
            result.M24 = 0;

            result.M31 = -s;
            result.M32 = 0;
            result.M33 = c;
            result.M34 = z;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Creates a matrix for rotating points around the Z-axis.
        /// </summary>
        /// <param name="degrees">The amount, in radians, by which to rotate around the Z-axis.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix4x4 CreateRotationZ(double degrees)
        {
            Matrix4x4 result;

            double radians = degrees * MMath.TO_RADS;
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);

            // [  c -s  0  0 ]
            // [  s  c  0  0 ]
            // [  0  0  1  0 ]
            // [  0  0  0  1 ]
            result.M11 = c;
            result.M12 = -s;
            result.M13 = 0;
            result.M14 = 0;

            result.M21 = s;
            result.M22 = c;
            result.M23 = 0;
            result.M24 = 0;

            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
            result.M34 = 0;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Creates a matrix for rotating points around the Z-axis, from a center point.
        /// </summary>
        /// <param name="degrees">The amount, in radians, by which to rotate around the Z-axis.</param>
        /// <param name="centerPoint">The center point.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix4x4 CreateRotationZ(double degrees, Vector centerPoint)
        {
            Matrix4x4 result;

            double radians = degrees * MMath.TO_RADS;
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);

            double x = centerPoint.X * (1 - c) + centerPoint.Y * s;
            double y = centerPoint.Y * (1 - c) - centerPoint.X * s;

            // [  c -s  0  x ]
            // [  s  c  0  y ]
            // [  0  0  1  0 ]
            // [  0  0  0  1 ]
            result.M11 = c;
            result.M12 = -s;
            result.M13 = 0;
            result.M14 = x;

            result.M21 = s;
            result.M22 = c;
            result.M23 = 0;
            result.M24 = y;

            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
            result.M34 = 0;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Creates a matrix that rotates around an arbitrary vector.
        /// </summary>
        /// <param name="axis">The normalized axis to rotate around.</param>
        /// <param name="angleDegs">The angle to rotate around the given axis, in degrees.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix4x4 CreateFromAxisAngle(Vector axis, double angleDegs)
        {
            // Not using the original System.Numerics.Matrix4x4 implementation, but yields same results. 
            // Based on http://www.euclideanspace.com/maths/geometry/rotations/conversions/angleToMatrix/index.htm
            // This conversion assumes the rotation vector is normalized.
            //      
            //      [ t*x*x+c    t*x*y-z*s  t*x*z+y*s  0]
            // M =  [ t*x*y+z*s  t*y*y+c    t*y*z-x*s  0]
            //      [ t*x*z-y*s  t*y*z+x*s  t*z*z+c    0] 
            //      [ 0          0          0          1]
            //

            double ang = angleDegs * MMath.TO_RADS;
            double c = Math.Cos(ang);
            double s = Math.Sin(ang);
            double t = 1 - c;
            double x = axis.X, y = axis.Y, z = axis.Z;
            double xx = x * x, yy = y * y, zz = z * z;
            double xy = x * y, xz = x * z, yz = y * z;

            Matrix4x4 result;

            result.M11 = t * xx + c;
            result.M12 = t * xy - z * s;
            result.M13 = t * xz + y * s;
            result.M14 = 0;

            result.M21 = t * xy + z * s;
            result.M22 = t * yy + c;
            result.M23 = t * yz - x * s;
            result.M24 = 0;

            result.M31 = t * xz - y * s;
            result.M32 = t * yz + x * s;
            result.M33 = t * zz + c;
            result.M34 = 0;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        public static Matrix4x4 CreateRotation(Vector axis, double angleDegs, Vector center)
        {
            // @TODO: this should be optimized to use an algebraic form.
            Matrix4x4 T1 = Matrix4x4.CreateTranslation(-center);
            Matrix4x4 R = Matrix4x4.CreateFromAxisAngle(axis, angleDegs);
            Matrix4x4 T2 = Matrix4x4.CreateTranslation(center);

            return T2 * R * T1;
        }


        /// <summary>
        /// Creates a matrix that rotates around an arbitrary vector.
        /// </summary>
        /// <param name="axisAngle">The AxisAngle object</param>
        /// <returns></returns>
        public static Matrix4x4 CreateFromAxisAngle(AxisAngle axisAngle)
        {
            return CreateFromAxisAngle(axisAngle.Axis, axisAngle.Angle);
        }

        /// <summary>
        /// Creates a matrix that represents a Plane object.
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        public static Matrix4x4 CreateFromPlane(Plane plane)
        {
            // This needs a LOT of optimization...
            Matrix4x4 result;

            result.M11 = plane.XAxis.X;
            result.M12 = plane.YAxis.X;
            result.M13 = plane.ZAxis.X;
            result.M14 = plane.Origin.X;

            result.M21 = plane.XAxis.Y;
            result.M22 = plane.YAxis.Y;
            result.M23 = plane.ZAxis.Y;
            result.M24 = plane.Origin.Y;

            result.M31 = plane.XAxis.Z;
            result.M32 = plane.YAxis.Z;
            result.M33 = plane.ZAxis.Z;
            result.M34 = plane.Origin.Z;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Creates a matrix that represents a Plane object. 
        /// This method assumes input vectors are unitary and orthogonal.
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="originZ"></param>
        /// <param name="x0"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y0"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static Matrix4x4 CreateFromPlane(
                double originX, double originY, double originZ,
                double x0, double x1, double x2,
                double y0, double y1, double y2)
        {
            // Cross product
            double z0 = x1 * y2 - x2 * y1;
            double z1 = x2 * y0 - x0 * y2;
            double z2 = x0 * y1 - x1 * y0;

            Matrix4x4 m;

            m.M11 = x0;
            m.M12 = y0;
            m.M13 = z0;
            m.M14 = originX;

            m.M21 = x1;
            m.M22 = y1;
            m.M23 = z1;
            m.M24 = originY;

            m.M31 = x2;
            m.M32 = y2;
            m.M33 = z2;
            m.M34 = originZ;

            m.M41 = 0;
            m.M42 = 0;
            m.M43 = 0;
            m.M44 = 1;

            return m;
        }

        /// <summary>
        /// Creates a matrix with an orthogonal special rotation part, for any two input vectors.
        /// If failed (vectors were parallel or zero), the identity matrix will be returned).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>True if </returns>
        public static bool CreateOrthogonal(double x0, double x1, double x2, double y0, double y1, double y2, out Matrix4x4 m)
        {
            m.M11 = x0;
            m.M21 = x1;
            m.M31 = x2;
            m.M41 = 0;

            m.M12 = y0;
            m.M22 = y1;
            m.M32 = y2;
            m.M42 = 0;

            m.M13 = 0;
            m.M23 = 0;
            m.M33 = 1;
            m.M43 = 0;

            m.M14 = 0;
            m.M24 = 0;
            m.M34 = 0;
            m.M44 = 1;

            bool success = m.OrthogonalizeRotation();

            if (!success)
            {
                m = Identity;
            }

            return success;
        }

        /// <summary>
        /// Creates a matrix with an orthogonal special rotation part, for any two input vectors.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool  CreateOrthogonal(Vector x, Vector y, out Matrix4x4 m)
        {
            return CreateOrthogonal(x.X, x.Y, x.Z, y.X, y.Y, y.Z, out m);
        }


        /// <summary>
        /// Creates a rotation matrix from the given Quaternion rotation value.
        /// </summary>
        /// <param name="quaternion">The source Quaternion.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix4x4 CreateFromQuaternion(Quaternion quaternion)
        {
            return CreateFromQuaternion(quaternion.W, quaternion.X, quaternion.Y, quaternion.Z);
        }

        /// <summary>
        /// Creates a rotation matrix from the given Quaternion rotation value.
        /// </summary>
        /// <param name="w">The scalar part.</param>
        /// <param name="x">The i part.</param>
        /// <param name="y">The j part.</param>
        /// <param name="z">The k part.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix4x4 CreateFromQuaternion(double w, double x, double y, double z)
        {
            // double implementation of Quaternion.ToRotationMatrix()
            // Based on http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToMatrix/

            double xx2 = 2 * x * x,
                xy2 = 2 * x * y,
                xz2 = 2 * x * z,
                xw2 = 2 * x * w,
                yy2 = 2 * y * y,
                yz2 = 2 * y * z,
                yw2 = 2 * y * w,
                zz2 = 2 * z * z,
                zw2 = 2 * z * w;

            Matrix4x4 result;

            result.M11 = 1 - yy2 - zz2;
            result.M12 = xy2 - zw2;
            result.M13 = xz2 + yw2;
            result.M14 = 0;

            result.M21 = xy2 + zw2;
            result.M22 = 1 - xx2 - zz2;
            result.M23 = yz2 - xw2;
            result.M24 = 0;

            result.M31 = xz2 - yw2;
            result.M32 = yz2 + xw2;
            result.M33 = 1 - xx2 - yy2;
            result.M34 = 0;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }



        /// <summary>
        /// Creates a transformation matrix from Denavit-Hartenberg parameters.
        /// </summary>
        /// <param name="distance">Depth along base Z axis to the common normal.</param>
        /// <param name="radius">Distance between axes, or length of the common normal. </param>
        /// <param name="alpha">Rotation in degrees around base X to align base Z to new Z.</param>
        /// <param name="theta">Angle in degrees from base X to align with new common normal. </param>
        /// <returns></returns>
        public static Matrix4x4 CreateFromDHParameters(double distance, double radius, double alpha, double theta)
        {
            // The DH convention makes it very easy/fast to create the transformation
            // matrix between a Joint's base axis plane and its transformed one. 
            // See https://en.wikipedia.org/wiki/Denavit%E2%80%93Hartenberg_parameters#Denavit%E2%80%93Hartenberg_matrix
            // 
            // (n-1)T(n) = Trans.d * Rot.θ * Trans.r * Rot.α;
            // 
            //             [ cθ  -sθ*cα   sθ*sα  r*cθ ]
            // (n-1)T(n) = [ sθ   cθ*cα  -cθ*sα  r*sθ ]
            //             [ 0    sα      cα     d    ]  
            //             [ 0    0       0      1    ]
            //
            // NOTE: because of transformation order, this matrix should be POST-multiplied
            // to the previous joint matrix:
            //
            // M(2) = M(1) * (1)T(2)
            //

            double a = alpha * MMath.TO_RADS,
                t = theta * MMath.TO_RADS;
            double sa = Math.Sin(a),
                ca = Math.Cos(a),
                st = Math.Sin(t),
                ct = Math.Cos(t);

            Matrix4x4 result;

            result.M11 = ct;
            result.M12 = -st * ca;
            result.M13 = st * sa;
            result.M14 = radius * ct;

            result.M21 = st;
            result.M22 = ct * ca;
            result.M23 = -ct * sa;
            result.M24 = radius * st;

            result.M31 = 0;
            result.M32 = sa;
            result.M33 = ca;
            result.M34 = distance;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }



        /// <summary>
        /// Creates a transformation matrix from Denavit-Hartenberg parameters.
        /// See https://en.wikipedia.org/wiki/Denavit%E2%80%93Hartenberg_parameters#Denavit%E2%80%93Hartenberg_matrix
        /// </summary>
        /// <param name="dh">DHParameters object.</param>
        /// <returns></returns>
        public static Matrix4x4 CreateFromDHParameters(DHParameters dh)
        {
            return CreateFromDHParameters(dh.D, dh.R, dh.Alpha, dh.Theta);
        }



        #endregion constructors


        #region operators

        /// <summary>
        /// Returns a new matrix with the negated elements of the given matrix.
        /// </summary>
        /// <param name="value">The source matrix.</param>
        /// <returns>The negated matrix.</returns>
        public static Matrix4x4 operator -(Matrix4x4 value)
        {
            Matrix4x4 m;

            m.M11 = -value.M11;
            m.M12 = -value.M12;
            m.M13 = -value.M13;
            m.M14 = -value.M14;
            m.M21 = -value.M21;
            m.M22 = -value.M22;
            m.M23 = -value.M23;
            m.M24 = -value.M24;
            m.M31 = -value.M31;
            m.M32 = -value.M32;
            m.M33 = -value.M33;
            m.M34 = -value.M34;
            m.M41 = -value.M41;
            m.M42 = -value.M42;
            m.M43 = -value.M43;
            m.M44 = -value.M44;

            return m;
        }

        /// <summary>
        /// Adds two matrices together.
        /// </summary>
        /// <param name="value1">The first source matrix.</param>
        /// <param name="value2">The second source matrix.</param>
        /// <returns>The resulting matrix.</returns>
        public static Matrix4x4 operator +(Matrix4x4 value1, Matrix4x4 value2)
        {
            Matrix4x4 m;

            m.M11 = value1.M11 + value2.M11;
            m.M12 = value1.M12 + value2.M12;
            m.M13 = value1.M13 + value2.M13;
            m.M14 = value1.M14 + value2.M14;
            m.M21 = value1.M21 + value2.M21;
            m.M22 = value1.M22 + value2.M22;
            m.M23 = value1.M23 + value2.M23;
            m.M24 = value1.M24 + value2.M24;
            m.M31 = value1.M31 + value2.M31;
            m.M32 = value1.M32 + value2.M32;
            m.M33 = value1.M33 + value2.M33;
            m.M34 = value1.M34 + value2.M34;
            m.M41 = value1.M41 + value2.M41;
            m.M42 = value1.M42 + value2.M42;
            m.M43 = value1.M43 + value2.M43;
            m.M44 = value1.M44 + value2.M44;

            return m;
        }

        /// <summary>
        /// Subtracts the second matrix from the first.
        /// </summary>
        /// <param name="value1">The first source matrix.</param>
        /// <param name="value2">The second source matrix.</param>
        /// <returns>The result of the subtraction.</returns>
        public static Matrix4x4 operator -(Matrix4x4 value1, Matrix4x4 value2)
        {
            Matrix4x4 m;

            m.M11 = value1.M11 - value2.M11;
            m.M12 = value1.M12 - value2.M12;
            m.M13 = value1.M13 - value2.M13;
            m.M14 = value1.M14 - value2.M14;
            m.M21 = value1.M21 - value2.M21;
            m.M22 = value1.M22 - value2.M22;
            m.M23 = value1.M23 - value2.M23;
            m.M24 = value1.M24 - value2.M24;
            m.M31 = value1.M31 - value2.M31;
            m.M32 = value1.M32 - value2.M32;
            m.M33 = value1.M33 - value2.M33;
            m.M34 = value1.M34 - value2.M34;
            m.M41 = value1.M41 - value2.M41;
            m.M42 = value1.M42 - value2.M42;
            m.M43 = value1.M43 - value2.M43;
            m.M44 = value1.M44 - value2.M44;

            return m;
        }

        /// <summary>
        /// Multiplies a matrix by another matrix.
        /// </summary>
        /// <param name="value1">The first source matrix.</param>
        /// <param name="value2">The second source matrix.</param>
        /// <returns>The result of the multiplication.</returns>
        public static Matrix4x4 operator *(Matrix4x4 value1, Matrix4x4 value2)
        {
            Matrix4x4 m;

            // First row
            m.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31 + value1.M14 * value2.M41;
            m.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32 + value1.M14 * value2.M42;
            m.M13 = value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33 + value1.M14 * value2.M43;
            m.M14 = value1.M11 * value2.M14 + value1.M12 * value2.M24 + value1.M13 * value2.M34 + value1.M14 * value2.M44;

            // Second row
            m.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31 + value1.M24 * value2.M41;
            m.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32 + value1.M24 * value2.M42;
            m.M23 = value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33 + value1.M24 * value2.M43;
            m.M24 = value1.M21 * value2.M14 + value1.M22 * value2.M24 + value1.M23 * value2.M34 + value1.M24 * value2.M44;

            // Third row
            m.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31 + value1.M34 * value2.M41;
            m.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32 + value1.M34 * value2.M42;
            m.M33 = value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33 + value1.M34 * value2.M43;
            m.M34 = value1.M31 * value2.M14 + value1.M32 * value2.M24 + value1.M33 * value2.M34 + value1.M34 * value2.M44;

            // Fourth row
            m.M41 = value1.M41 * value2.M11 + value1.M42 * value2.M21 + value1.M43 * value2.M31 + value1.M44 * value2.M41;
            m.M42 = value1.M41 * value2.M12 + value1.M42 * value2.M22 + value1.M43 * value2.M32 + value1.M44 * value2.M42;
            m.M43 = value1.M41 * value2.M13 + value1.M42 * value2.M23 + value1.M43 * value2.M33 + value1.M44 * value2.M43;
            m.M44 = value1.M41 * value2.M14 + value1.M42 * value2.M24 + value1.M43 * value2.M34 + value1.M44 * value2.M44;

            return m;
        }

        /// <summary>
        /// Multiplies a matrix by a scalar value.
        /// </summary>
        /// <param name="value1">The source matrix.</param>
        /// <param name="value2">The scaling factor.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4x4 operator *(Matrix4x4 value1, double value2)
        {
            Matrix4x4 m;

            m.M11 = value1.M11 * value2;
            m.M12 = value1.M12 * value2;
            m.M13 = value1.M13 * value2;
            m.M14 = value1.M14 * value2;
            m.M21 = value1.M21 * value2;
            m.M22 = value1.M22 * value2;
            m.M23 = value1.M23 * value2;
            m.M24 = value1.M24 * value2;
            m.M31 = value1.M31 * value2;
            m.M32 = value1.M32 * value2;
            m.M33 = value1.M33 * value2;
            m.M34 = value1.M34 * value2;
            m.M41 = value1.M41 * value2;
            m.M42 = value1.M42 * value2;
            m.M43 = value1.M43 * value2;
            m.M44 = value1.M44 * value2;
            return m;
        }

        /// <summary>
        /// Returns a boolean indicating whether the given two matrices are equal.
        /// </summary>
        /// <param name="value1">The first matrix to compare.</param>
        /// <param name="value2">The second matrix to compare.</param>
        /// <returns>True if the given matrices are equal; False otherwise.</returns>
        public static bool operator ==(Matrix4x4 value1, Matrix4x4 value2)
        {
            return (value1.M11 == value2.M11 && value1.M22 == value2.M22 && value1.M33 == value2.M33 && value1.M44 == value2.M44 && // Check diagonal element first for early out.
                                                value1.M12 == value2.M12 && value1.M13 == value2.M13 && value1.M14 == value2.M14 &&
                    value1.M21 == value2.M21 && value1.M23 == value2.M23 && value1.M24 == value2.M24 &&
                    value1.M31 == value2.M31 && value1.M32 == value2.M32 && value1.M34 == value2.M34 &&
                    value1.M41 == value2.M41 && value1.M42 == value2.M42 && value1.M43 == value2.M43);
        }

        /// <summary>
        /// Returns a boolean indicating whether the given two matrices are not equal.
        /// </summary>
        /// <param name="value1">The first matrix to compare.</param>
        /// <param name="value2">The second matrix to compare.</param>
        /// <returns>True if the given matrices are not equal; False if they are equal.</returns>
        public static bool operator !=(Matrix4x4 value1, Matrix4x4 value2)
        {
            return (value1.M11 != value2.M11 || value1.M12 != value2.M12 || value1.M13 != value2.M13 || value1.M14 != value2.M14 ||
                    value1.M21 != value2.M21 || value1.M22 != value2.M22 || value1.M23 != value2.M23 || value1.M24 != value2.M24 ||
                    value1.M31 != value2.M31 || value1.M32 != value2.M32 || value1.M33 != value2.M33 || value1.M34 != value2.M34 ||
                    value1.M41 != value2.M41 || value1.M42 != value2.M42 || value1.M43 != value2.M43 || value1.M44 != value2.M44);
        }

        /// <summary>
        /// Attempts to calculate the inverse of the given matrix. If successful, result will contain the inverted matrix.
        /// </summary>
        /// <param name="matrix">The source matrix to invert.</param>
        /// <param name="result">If successful, contains the inverted matrix.</param>
        /// <returns>True if the source matrix could be inverted; False otherwise.</returns>
        public static bool Invert(Matrix4x4 matrix, out Matrix4x4 result)
        {
            //                                       -1
            // If you have matrix M, inverse Matrix M   can compute
            //
            //     -1       1      
            //    M   = --------- A
            //            det(M)
            //
            // A is adjugate (adjoint) of M, where,
            //
            //      T
            // A = C
            //
            // C is Cofactor matrix of M, where,
            //           i + j
            // C   = (-1)      * det(M  )
            //  ij                    ij
            //
            //     [ a b c d ]
            // M = [ e f g h ]
            //     [ i j k l ]
            //     [ m n o p ]
            //
            // First Row
            //           2 | f g h |
            // C   = (-1)  | j k l | = + ( f ( kp - lo ) - g ( jp - ln ) + h ( jo - kn ) )
            //  11         | n o p |
            //
            //           3 | e g h |
            // C   = (-1)  | i k l | = - ( e ( kp - lo ) - g ( ip - lm ) + h ( io - km ) )
            //  12         | m o p |
            //
            //           4 | e f h |
            // C   = (-1)  | i j l | = + ( e ( jp - ln ) - f ( ip - lm ) + h ( in - jm ) )
            //  13         | m n p |
            //
            //           5 | e f g |
            // C   = (-1)  | i j k | = - ( e ( jo - kn ) - f ( io - km ) + g ( in - jm ) )
            //  14         | m n o |
            //
            // Second Row
            //           3 | b c d |
            // C   = (-1)  | j k l | = - ( b ( kp - lo ) - c ( jp - ln ) + d ( jo - kn ) )
            //  21         | n o p |
            //
            //           4 | a c d |
            // C   = (-1)  | i k l | = + ( a ( kp - lo ) - c ( ip - lm ) + d ( io - km ) )
            //  22         | m o p |
            //
            //           5 | a b d |
            // C   = (-1)  | i j l | = - ( a ( jp - ln ) - b ( ip - lm ) + d ( in - jm ) )
            //  23         | m n p |
            //
            //           6 | a b c |
            // C   = (-1)  | i j k | = + ( a ( jo - kn ) - b ( io - km ) + c ( in - jm ) )
            //  24         | m n o |
            //
            // Third Row
            //           4 | b c d |
            // C   = (-1)  | f g h | = + ( b ( gp - ho ) - c ( fp - hn ) + d ( fo - gn ) )
            //  31         | n o p |
            //
            //           5 | a c d |
            // C   = (-1)  | e g h | = - ( a ( gp - ho ) - c ( ep - hm ) + d ( eo - gm ) )
            //  32         | m o p |
            //
            //           6 | a b d |
            // C   = (-1)  | e f h | = + ( a ( fp - hn ) - b ( ep - hm ) + d ( en - fm ) )
            //  33         | m n p |
            //
            //           7 | a b c |
            // C   = (-1)  | e f g | = - ( a ( fo - gn ) - b ( eo - gm ) + c ( en - fm ) )
            //  34         | m n o |
            //
            // Fourth Row
            //           5 | b c d |
            // C   = (-1)  | f g h | = - ( b ( gl - hk ) - c ( fl - hj ) + d ( fk - gj ) )
            //  41         | j k l |
            //
            //           6 | a c d |
            // C   = (-1)  | e g h | = + ( a ( gl - hk ) - c ( el - hi ) + d ( ek - gi ) )
            //  42         | i k l |
            //
            //           7 | a b d |
            // C   = (-1)  | e f h | = - ( a ( fl - hj ) - b ( el - hi ) + d ( ej - fi ) )
            //  43         | i j l |
            //
            //           8 | a b c |
            // C   = (-1)  | e f g | = + ( a ( fk - gj ) - b ( ek - gi ) + c ( ej - fi ) )
            //  44         | i j k |
            //
            // Cost of operation
            // 53 adds, 104 muls, and 1 div.
            double a = matrix.M11, b = matrix.M12, c = matrix.M13, d = matrix.M14;
            double e = matrix.M21, f = matrix.M22, g = matrix.M23, h = matrix.M24;
            double i = matrix.M31, j = matrix.M32, k = matrix.M33, l = matrix.M34;
            double m = matrix.M41, n = matrix.M42, o = matrix.M43, p = matrix.M44;

            double kp_lo = k * p - l * o;
            double jp_ln = j * p - l * n;
            double jo_kn = j * o - k * n;
            double ip_lm = i * p - l * m;
            double io_km = i * o - k * m;
            double in_jm = i * n - j * m;

            double a11 = +(f * kp_lo - g * jp_ln + h * jo_kn);
            double a12 = -(e * kp_lo - g * ip_lm + h * io_km);
            double a13 = +(e * jp_ln - f * ip_lm + h * in_jm);
            double a14 = -(e * jo_kn - f * io_km + g * in_jm);

            double det = a * a11 + b * a12 + c * a13 + d * a14;

            if (Math.Abs(det) < double.Epsilon)
            {
                result = new Matrix4x4(double.NaN, double.NaN, double.NaN, double.NaN,
                                       double.NaN, double.NaN, double.NaN, double.NaN,
                                       double.NaN, double.NaN, double.NaN, double.NaN,
                                       double.NaN, double.NaN, double.NaN, double.NaN);
                return false;
            }

            double invDet = 1 / det;

            result.M11 = a11 * invDet;
            result.M21 = a12 * invDet;
            result.M31 = a13 * invDet;
            result.M41 = a14 * invDet;

            result.M12 = -(b * kp_lo - c * jp_ln + d * jo_kn) * invDet;
            result.M22 = +(a * kp_lo - c * ip_lm + d * io_km) * invDet;
            result.M32 = -(a * jp_ln - b * ip_lm + d * in_jm) * invDet;
            result.M42 = +(a * jo_kn - b * io_km + c * in_jm) * invDet;

            double gp_ho = g * p - h * o;
            double fp_hn = f * p - h * n;
            double fo_gn = f * o - g * n;
            double ep_hm = e * p - h * m;
            double eo_gm = e * o - g * m;
            double en_fm = e * n - f * m;

            result.M13 = +(b * gp_ho - c * fp_hn + d * fo_gn) * invDet;
            result.M23 = -(a * gp_ho - c * ep_hm + d * eo_gm) * invDet;
            result.M33 = +(a * fp_hn - b * ep_hm + d * en_fm) * invDet;
            result.M43 = -(a * fo_gn - b * eo_gm + c * en_fm) * invDet;

            double gl_hk = g * l - h * k;
            double fl_hj = f * l - h * j;
            double fk_gj = f * k - g * j;
            double el_hi = e * l - h * i;
            double ek_gi = e * k - g * i;
            double ej_fi = e * j - f * i;

            result.M14 = -(b * gl_hk - c * fl_hj + d * fk_gj) * invDet;
            result.M24 = +(a * gl_hk - c * el_hi + d * ek_gi) * invDet;
            result.M34 = -(a * fl_hj - b * el_hi + d * ej_fi) * invDet;
            result.M44 = +(a * fk_gj - b * ek_gi + c * ej_fi) * invDet;

            return true;
        }

        /// <summary>
        /// Transforms the given matrix by applying the given Quaternion rotation.
        /// The quaternion will be pre-multiplied to the Matrix.
        /// </summary>
        /// <param name="value">The source matrix to transform.</param>
        /// <param name="rotation">The rotation to apply.</param>
        /// <returns>The transformed matrix.</returns>
        public static Matrix4x4 Transform(Matrix4x4 value, Quaternion rotation)
        {
            // This is basically computing the 3x3 matrix out of the quaternion 
            // and premultiplying it by the matrix...

            // Compute rotation matrix.
            double x = rotation.X, y = rotation.Y,
                z = rotation.Z, w = rotation.W;

            double x2 = x + x;
            double y2 = y + y;
            double z2 = z + z;

            double wx2 = w * x2;
            double wy2 = w * y2;
            double wz2 = w * z2;
            double xx2 = x * x2;
            double xy2 = x * y2;
            double xz2 = x * z2;
            double yy2 = y * y2;
            double yz2 = y * z2;
            double zz2 = z * z2;

            double q11 = 1 - yy2 - zz2;
            double q12 = xy2 - wz2;
            double q13 = xz2 + wy2;

            double q21 = xy2 + wz2;
            double q22 = 1 - xx2 - zz2;
            double q23 = yz2 - wx2;

            double q31 = xz2 - wy2;
            double q32 = yz2 + wx2;
            double q33 = 1 - xx2 - yy2;

            Matrix4x4 result;

            // First row
            result.M11 = q11 * value.M11 + q12 * value.M21 + q13 * value.M31;
            result.M12 = q11 * value.M12 + q12 * value.M22 + q13 * value.M32;
            result.M13 = q11 * value.M13 + q12 * value.M23 + q13 * value.M33;
            result.M14 = q11 * value.M14 + q12 * value.M24 + q13 * value.M34;

            // Second row
            result.M21 = q21 * value.M11 + q22 * value.M21 + q23 * value.M31;
            result.M22 = q21 * value.M12 + q22 * value.M22 + q23 * value.M32;
            result.M23 = q21 * value.M13 + q22 * value.M23 + q23 * value.M33;
            result.M24 = q21 * value.M14 + q22 * value.M24 + q23 * value.M34;

            // Third row
            result.M31 = q31 * value.M11 + q32 * value.M21 + q33 * value.M31;
            result.M32 = q31 * value.M12 + q32 * value.M22 + q33 * value.M32;
            result.M33 = q31 * value.M13 + q32 * value.M23 + q33 * value.M33;
            result.M34 = q31 * value.M14 + q32 * value.M24 + q33 * value.M34;

            // Fourth row
            result.M41 = value.M41;
            result.M42 = value.M42;
            result.M43 = value.M43;
            result.M44 = value.M44;

            return result;
        }

        /// <summary>
        /// Transposes the rows and columns of a matrix.
        /// </summary>
        /// <param name="matrix">The source matrix.</param>
        /// <returns>The transposed matrix.</returns>
        public static Matrix4x4 Transpose(Matrix4x4 matrix)
        {
            Matrix4x4 result;

            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M14 = matrix.M41;
            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M24 = matrix.M42;
            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;
            result.M34 = matrix.M43;
            result.M41 = matrix.M14;
            result.M42 = matrix.M24;
            result.M43 = matrix.M34;
            result.M44 = matrix.M44;

            return result;
        }

        /// <summary>
        /// Linearly interpolates between the corresponding values of two matrices.
        /// </summary>
        /// <param name="matrix1">The first source matrix.</param>
        /// <param name="matrix2">The second source matrix.</param>
        /// <param name="amount">The relative weight of the second source matrix.</param>
        /// <returns>The interpolated matrix.</returns>
        public static Matrix4x4 Lerp(Matrix4x4 matrix1, Matrix4x4 matrix2, double amount)
        {
            Matrix4x4 result;

            // First row
            result.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount;
            result.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount;
            result.M13 = matrix1.M13 + (matrix2.M13 - matrix1.M13) * amount;
            result.M14 = matrix1.M14 + (matrix2.M14 - matrix1.M14) * amount;

            // Second row
            result.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount;
            result.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount;
            result.M23 = matrix1.M23 + (matrix2.M23 - matrix1.M23) * amount;
            result.M24 = matrix1.M24 + (matrix2.M24 - matrix1.M24) * amount;

            // Third row
            result.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount;
            result.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount;
            result.M33 = matrix1.M33 + (matrix2.M33 - matrix1.M33) * amount;
            result.M34 = matrix1.M34 + (matrix2.M34 - matrix1.M34) * amount;

            // Fourth row
            result.M41 = matrix1.M41 + (matrix2.M41 - matrix1.M41) * amount;
            result.M42 = matrix1.M42 + (matrix2.M42 - matrix1.M42) * amount;
            result.M43 = matrix1.M43 + (matrix2.M43 - matrix1.M43) * amount;
            result.M44 = matrix1.M44 + (matrix2.M44 - matrix1.M44) * amount;

            return result;
        }

        /// <summary>
        /// Returns a new matrix with the negated elements of the given matrix.
        /// </summary>
        /// <param name="value">The source matrix.</param>
        /// <returns>The negated matrix.</returns>
        public static Matrix4x4 Negate(Matrix4x4 value)
        {
            Matrix4x4 result;

            result.M11 = -value.M11;
            result.M12 = -value.M12;
            result.M13 = -value.M13;
            result.M14 = -value.M14;
            result.M21 = -value.M21;
            result.M22 = -value.M22;
            result.M23 = -value.M23;
            result.M24 = -value.M24;
            result.M31 = -value.M31;
            result.M32 = -value.M32;
            result.M33 = -value.M33;
            result.M34 = -value.M34;
            result.M41 = -value.M41;
            result.M42 = -value.M42;
            result.M43 = -value.M43;
            result.M44 = -value.M44;

            return result;
        }

        /// <summary>
        /// Adds two matrices together.
        /// </summary>
        /// <param name="value1">The first source matrix.</param>
        /// <param name="value2">The second source matrix.</param>
        /// <returns>The resulting matrix.</returns>
        public static Matrix4x4 Add(Matrix4x4 value1, Matrix4x4 value2)
        {
            Matrix4x4 result;

            result.M11 = value1.M11 + value2.M11;
            result.M12 = value1.M12 + value2.M12;
            result.M13 = value1.M13 + value2.M13;
            result.M14 = value1.M14 + value2.M14;
            result.M21 = value1.M21 + value2.M21;
            result.M22 = value1.M22 + value2.M22;
            result.M23 = value1.M23 + value2.M23;
            result.M24 = value1.M24 + value2.M24;
            result.M31 = value1.M31 + value2.M31;
            result.M32 = value1.M32 + value2.M32;
            result.M33 = value1.M33 + value2.M33;
            result.M34 = value1.M34 + value2.M34;
            result.M41 = value1.M41 + value2.M41;
            result.M42 = value1.M42 + value2.M42;
            result.M43 = value1.M43 + value2.M43;
            result.M44 = value1.M44 + value2.M44;

            return result;
        }

        /// <summary>
        /// Subtracts the second matrix from the first.
        /// </summary>
        /// <param name="value1">The first source matrix.</param>
        /// <param name="value2">The second source matrix.</param>
        /// <returns>The result of the subtraction.</returns>
        public static Matrix4x4 Subtract(Matrix4x4 value1, Matrix4x4 value2)
        {
            Matrix4x4 result;

            result.M11 = value1.M11 - value2.M11;
            result.M12 = value1.M12 - value2.M12;
            result.M13 = value1.M13 - value2.M13;
            result.M14 = value1.M14 - value2.M14;
            result.M21 = value1.M21 - value2.M21;
            result.M22 = value1.M22 - value2.M22;
            result.M23 = value1.M23 - value2.M23;
            result.M24 = value1.M24 - value2.M24;
            result.M31 = value1.M31 - value2.M31;
            result.M32 = value1.M32 - value2.M32;
            result.M33 = value1.M33 - value2.M33;
            result.M34 = value1.M34 - value2.M34;
            result.M41 = value1.M41 - value2.M41;
            result.M42 = value1.M42 - value2.M42;
            result.M43 = value1.M43 - value2.M43;
            result.M44 = value1.M44 - value2.M44;

            return result;
        }


        /// <summary>
        /// Multiplies a matrix by another matrix.
        /// </summary>
        /// <param name="value1">The first source matrix.</param>
        /// <param name="value2">The second source matrix.</param>
        /// <returns>The result of the multiplication.</returns>
        public static Matrix4x4 Multiply(Matrix4x4 value1, Matrix4x4 value2)
        {
            Matrix4x4 result;

            // First row
            result.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31 + value1.M14 * value2.M41;
            result.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32 + value1.M14 * value2.M42;
            result.M13 = value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33 + value1.M14 * value2.M43;
            result.M14 = value1.M11 * value2.M14 + value1.M12 * value2.M24 + value1.M13 * value2.M34 + value1.M14 * value2.M44;

            // Second row
            result.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31 + value1.M24 * value2.M41;
            result.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32 + value1.M24 * value2.M42;
            result.M23 = value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33 + value1.M24 * value2.M43;
            result.M24 = value1.M21 * value2.M14 + value1.M22 * value2.M24 + value1.M23 * value2.M34 + value1.M24 * value2.M44;

            // Third row
            result.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31 + value1.M34 * value2.M41;
            result.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32 + value1.M34 * value2.M42;
            result.M33 = value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33 + value1.M34 * value2.M43;
            result.M34 = value1.M31 * value2.M14 + value1.M32 * value2.M24 + value1.M33 * value2.M34 + value1.M34 * value2.M44;

            // Fourth row
            result.M41 = value1.M41 * value2.M11 + value1.M42 * value2.M21 + value1.M43 * value2.M31 + value1.M44 * value2.M41;
            result.M42 = value1.M41 * value2.M12 + value1.M42 * value2.M22 + value1.M43 * value2.M32 + value1.M44 * value2.M42;
            result.M43 = value1.M41 * value2.M13 + value1.M42 * value2.M23 + value1.M43 * value2.M33 + value1.M44 * value2.M43;
            result.M44 = value1.M41 * value2.M14 + value1.M42 * value2.M24 + value1.M43 * value2.M34 + value1.M44 * value2.M44;

            return result;
        }


        /// <summary>
        /// Multiplies a matrix by a scalar value.
        /// </summary>
        /// <param name="value1">The source matrix.</param>
        /// <param name="value2">The scaling factor.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4x4 Multiply(Matrix4x4 value1, double value2)
        {
            Matrix4x4 result;

            result.M11 = value1.M11 * value2;
            result.M12 = value1.M12 * value2;
            result.M13 = value1.M13 * value2;
            result.M14 = value1.M14 * value2;
            result.M21 = value1.M21 * value2;
            result.M22 = value1.M22 * value2;
            result.M23 = value1.M23 * value2;
            result.M24 = value1.M24 * value2;
            result.M31 = value1.M31 * value2;
            result.M32 = value1.M32 * value2;
            result.M33 = value1.M33 * value2;
            result.M34 = value1.M34 * value2;
            result.M41 = value1.M41 * value2;
            result.M42 = value1.M42 * value2;
            result.M43 = value1.M43 * value2;
            result.M44 = value1.M44 * value2;

            return result;
        }

       

        #endregion operators




        #region interfaces

        /// <summary>
        /// Checks if all the values of the matrices are below a threshold.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public bool IsSimilarTo(Matrix4x4 other, double epsilon)
        {
            return (
                // Check diagonal element first for early out.
                Math.Abs(M11 - other.M11) < epsilon && Math.Abs(M22 - other.M22) < epsilon &&
                Math.Abs(M33 - other.M33) < epsilon && Math.Abs(M44 - other.M44) < epsilon &&

                Math.Abs(M12 - other.M12) < epsilon && Math.Abs(M13 - other.M13) < epsilon && Math.Abs(M14 - other.M14) < epsilon &&
                Math.Abs(M21 - other.M21) < epsilon && Math.Abs(M23 - other.M23) < epsilon && Math.Abs(M24 - other.M24) < epsilon &&
                Math.Abs(M31 - other.M31) < epsilon && Math.Abs(M32 - other.M32) < epsilon && Math.Abs(M34 - other.M34) < epsilon &&
                Math.Abs(M41 - other.M41) < epsilon && Math.Abs(M42 - other.M42) < epsilon && Math.Abs(M43 - other.M43) < epsilon);

        }

        /// <summary>
        /// JSON array representation of this object with rounding decimals. Use -1 for no rounding.
        /// </summary>
        /// <returns></returns>
        public string ToArrayString(int decimals)
        {
            if (decimals == -1)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "[{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}]",
                    M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44);
            }

            return string.Format(CultureInfo.InvariantCulture,
                "[{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}]",
                Math.Round(M11, decimals),
                Math.Round(M12, decimals),
                Math.Round(M13, decimals),
                Math.Round(M14, decimals),
                Math.Round(M21, decimals),
                Math.Round(M22, decimals),
                Math.Round(M23, decimals),
                Math.Round(M24, decimals),
                Math.Round(M31, decimals),
                Math.Round(M32, decimals),
                Math.Round(M33, decimals),
                Math.Round(M34, decimals),
                Math.Round(M41, decimals),
                Math.Round(M42, decimals),
                Math.Round(M43, decimals),
                Math.Round(M44, decimals));
        }

        /// <summary>
        /// JSON representation of this object, with rounding decimals. Use -1 for no rounding.
        /// </summary>
        /// <returns></returns>
        public string ToJSONString(int decimals)
        {

            if (decimals == -1)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "{{\"M11\":{0},\"M12\":{1},\"M13\":{2},\"M14\":{3},\"M21\":{4},\"M22\":{5},\"M23\":{6},\"M24\":{7},\"M31\":{8},\"M32\":{9},\"M33\":{10},\"M34\":{11},\"M41\":{12},\"M42\":{13},\"M43\":{14},\"M44\":{15}}}",
                    M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44);
            }

            return String.Format(CultureInfo.InvariantCulture,
                "{{\"M11\":{0},\"M12\":{1},\"M13\":{2},\"M14\":{3},\"M21\":{4},\"M22\":{5},\"M23\":{6},\"M24\":{7},\"M31\":{8},\"M32\":{9},\"M33\":{10},\"M34\":{11},\"M41\":{12},\"M42\":{13},\"M43\":{14},\"M44\":{15}}}",
                Math.Round(M11, decimals),
                Math.Round(M12, decimals),
                Math.Round(M13, decimals),
                Math.Round(M14, decimals),
                Math.Round(M21, decimals),
                Math.Round(M22, decimals),
                Math.Round(M23, decimals),
                Math.Round(M24, decimals),
                Math.Round(M31, decimals),
                Math.Round(M32, decimals),
                Math.Round(M33, decimals),
                Math.Round(M34, decimals),
                Math.Round(M41, decimals),
                Math.Round(M42, decimals),
                Math.Round(M43, decimals),
                Math.Round(M44, decimals));
        }

        #endregion interfaces



        #region properties

        /// <summary>
        /// Returns whether the matrix is the identity matrix.
        /// </summary>
        public bool IsIdentity
        {
            get
            {
                return M11 == 1 && M22 == 1 && M33 == 1 && M44 == 1 && // Check diagonal element first for early out.
                       M12 == 0 && M13 == 0 && M14 == 0 &&
                       M21 == 0 && M23 == 0 && M24 == 0 &&
                       M31 == 0 && M32 == 0 && M34 == 0 &&
                       M41 == 0 && M42 == 0 && M43 == 0;
            }
        }

        public bool IsOrthogonalRotation
        {
            get
            {
                // Orthogonal matrices satisfy that:
                // Q * Qt = Qt * Q = I;  
                // (the matrix multiplied by its transpose yields the identity matrix)
                // As a consequence, it also holds that the transpose of an orthogonal matrix equals its inverse:
                // Qt = Q^-1
                Matrix4x4 rot = this.GetRotationMatrix();
                Matrix4x4 trans = Transpose(rot);
                Matrix4x4 ident = this * trans;
                return 
                    ident.IsSimilarTo(Identity, MMath.EPSILON3) && 
                    Math.Abs(this.GetDeterminant() - 1) < MMath.EPSILON2;
            }
        }

        /// <summary>
        /// Gets the X vector component of this matrix (first three elements of first column).
        /// </summary>
        public Vector X
        {
            get
            {
                return new Vector(M11, M21, M31);
            }
        }

        /// <summary>
        /// Gets the Y vector component of this matrix (first three elements of second column).
        /// </summary>
        public Vector Y
        {
            get
            {
                return new Vector(M12, M22, M32);
            }
        }

        /// <summary>
        /// Gets the Z vector component of this matrix (first three elements of third column).
        /// </summary>
        public Vector Z
        {
            get
            {
                return new Vector(M13, M23, M33);
            }
        }

        /// <summary>
        /// Gets or sets the translation component of this matrix.
        /// </summary>
        public Vector Translation
        {
            get
            {
                return new Vector(M14, M24, M34);
            }
            set
            {
                // @TODO remove casting when Vector uses doubles
                M14 = value.X;
                M24 = value.Y;
                M34 = value.Z;
            }
        }

        #endregion properties






        #region methods

        /// <summary>
        /// Force the orthogonalization of this matrix. This means that the rotation part of the matrix
        /// will become an orthogonal coordinate system of three unit vectors. 
        /// </summary>
        /// <returns></returns>
        public bool OrthogonalizeRotation()
        {
            // This algorithm will orthogonalize this matrix by
            // maintaining the main X direction and XY plane, 
            // and recomputing the Y and Z axes to comply with this condition.
            Vector oldX = new Vector(M11, M21, M31),
                   oldY = new Vector(M12, M22, M32);

            if (Vector.Orthogonalize(oldX, oldY, out Vector newX, out Vector newY, out Vector newZ) == false)
            {
                Logger.Verbose("Cannot orthogonalize a Matrix with X & Y parallel vectors");
                return false;
            }

            M11 = newX.X;
            M21 = newX.Y;
            M31 = newX.Z;

            M12 = newY.X;
            M22 = newY.Y;
            M32 = newY.Z;

            M13 = newZ.X;
            M23 = newZ.Y;
            M33 = newZ.Z;

            return true;
        }

        /// <summary>
        /// Transposes the rows and columns of this matrix.
        /// </summary>
        public void Transpose()
        {
            Matrix4x4 trans = Matrix4x4.Transpose(this);
            
            M11 = trans.M11;
            M12 = trans.M12;
            M13 = trans.M13;
            M14 = trans.M14;
            M21 = trans.M21;
            M22 = trans.M22;
            M23 = trans.M23;
            M24 = trans.M24;
            M31 = trans.M31;
            M32 = trans.M32;
            M33 = trans.M33;
            M34 = trans.M34;
            M41 = trans.M41;
            M42 = trans.M42;
            M43 = trans.M43;
            M44 = trans.M44;
        }

        /// <summary>
        /// Returns a new matrix with only the 3x3 rotational part of the original one.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Matrix4x4 GetRotationMatrix()
        {
            Matrix4x4 result;

            result.M11 = M11;
            result.M12 = M12;
            result.M13 = M13;
            result.M14 = 0;

            result.M21 = M21;
            result.M22 = M22;
            result.M23 = M23;
            result.M24 = 0;

            result.M31 = M31;
            result.M32 = M32;
            result.M33 = M33;
            result.M34 = 0;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;
        }

        /// <summary>
        /// Returns the rotational part of this matrix as a Quaternion.
        /// </summary>
        /// <returns></returns>
        public bool GetQuaternion(out Quaternion q)
        {
            q = new Quaternion();

            // This conversion assumes the rotation matrix is special orthogonal.
            // As a result, the returned Quaternion will be a versor.
            // Based on http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
            if (!IsOrthogonalRotation)
            {
                return false;
            }

            double trace = M11 + M22 + M33;
            double s;

            // Compute a regular conversion
            if (trace > 0)
            {
                s = 2 * Math.Sqrt(trace + 1);
                q.W = 0.25 * s;
                q.X = (M32 - M23) / s;
                q.Y = (M13 - M31) / s;
                q.Z = (M21 - M12) / s;
            }

            // If trace is zero or negative, avoid division by zero, square root of negative and floating-point degeneracy
            // by searching which major diagonal element has the greatest value:
            else
            {
                if (M11 > M22 && M11 > M33)
                {
                    s = 2 * Math.Sqrt(1 + M11 - M22 - M33);
                    q.W = (M32 - M23) / s;
                    q.X = 0.25 * s;
                    q.Y = (M12 + M21) / s;
                    q.Z = (M13 + M31) / s;
                }
                else if (M22 > M33)
                {
                    s = 2 * Math.Sqrt(1 + M22 - M11 - M33);
                    q.W = (M13 - M31) / s;
                    q.X = (M12 + M21) / s;
                    q.Y = 0.25 * s;
                    q.Z = (M23 + M32) / s;
                }
                else
                {
                    s = 2 * Math.Sqrt(1 + M33 - M11 - M22);
                    q.W = (M21 - M12) / s;
                    q.X = (M13 + M31) / s;
                    q.Y = (M23 + M32) / s;
                    q.Z = 0.25 * s;
                }
            }

            return true;

            //// Alternative method?
            ////http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/christian.htm
            //Quaternion q = new Quaternion();
            //q.W = 0.5 * Math.Sqrt(Math.Max(0, 1 + m00 + m11 + m22));
            //q.X = Copysign(0.5 * Math.Sqrt(Math.Max(0, 1 + m00 - m11 - m22)), m21 - m12);
            //q.Y = Copysign(0.5 * Math.Sqrt(Math.Max(0, 1 - m00 + m11 - m22)), m02 - m20);
            //q.Z = Copysign(0.5 * Math.Sqrt(Math.Max(0, 1 - m00 - m11 + m22)), m10 - m01);

            //// From the ABB Rapid manual p.1151
            //double w = 0.5 * Math.Sqrt(1 + XAxis.X + YAxis.Y + ZAxis.Z);
            //double x = 0.5 * Math.Sqrt(1 + XAxis.X - YAxis.Y - ZAxis.Z) * (YAxis.Z - ZAxis.Y >= 0 ? 1 : -1);
            //double y = 0.5 * Math.Sqrt(1 - XAxis.X + YAxis.Y - ZAxis.Z) * (ZAxis.X - XAxis.Z >= 0 ? 1 : -1);
            //double z = 0.5 * Math.Sqrt(1 - XAxis.X - YAxis.Y + ZAxis.Z) * (XAxis.Y - YAxis.X >= 0 ? 1 : -1);
            //return new Quaternion(w, x, y, z, true);
        }


        /// <summary>
        /// Returns an Axis Angle representation of the rotation part Matrix. 
        /// Note that the returned AxisAngle will always represent a positive rotation between [0, 180]
        /// </summary>
        /// <returns></returns>
        public AxisAngle GetAxisAngle()
        {
            // Taken from http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToAngle/index.htm
            double x, y, z, angle;
            double sinv = 1 / Math.Sqrt(2);  // 0.7071...

            // Check for singularities
            if (Math.Abs(M12 - M21) < MMath.EPSILON2
                && Math.Abs(M13 - M31) < MMath.EPSILON2
                && Math.Abs(M23 - M32) < MMath.EPSILON2)
            {
                // If identity matrix (angle = 0), return a non rotation AxisAngle
                if (this.IsIdentity)
                {
                    return new AxisAngle();
                }
                // Otherwise, angle is 180
                angle = 180;
                double xx = 0.5 * M11 + 0.5;
                double yy = 0.5 * M22 + 0.5;
                double zz = 0.5 * M33 + 0.5;
                double xy = 0.25 * (M12 + M21);
                double xz = 0.25 * (M13 + M31);
                double yz = 0.25 * (M23 + M32);

                // If m00 is the largest diagonal
                if (xx > yy && xx > zz)
                {
                    if (xx < MMath.EPSILON2)
                    {
                        x = 0;
                        y = sinv;
                        z = sinv;
                    }
                    else
                    {
                        x = Math.Sqrt(xx);
                        y = xy / x;
                        z = xz / x;
                    }
                }
                // If m11 is the largest diagonal
                else if (yy > zz)
                {
                    if (yy < MMath.EPSILON2)
                    {
                        x = sinv;
                        y = 0;
                        z = sinv;
                    }
                    else
                    {
                        y = Math.Sqrt(yy);
                        x = xy / y;
                        z = yz / y;
                    }
                }
                // m22 is the largest diagonal
                else
                {
                    if (zz < MMath.EPSILON2)
                    {
                        x = sinv;
                        y = sinv;
                        z = 0;
                    }
                    else
                    {
                        z = Math.Sqrt(zz);
                        x = xz / z;
                        y = yz / z;
                    }
                }

                return new AxisAngle(x, y, z, angle, false);
            }

            // No singularities then, proceed normally
            double s = Math.Sqrt((M32 - M23) * (M32 - M23)
                    + (M13 - M31) * (M13 - M31)
                    + (M21 - M12) * (M21 - M12));  // for normalization (is this necessary here?)
            if (Math.Abs(s) < MMath.EPSILON2) s = 1;  // "prevent divide by zero, should not happen if matrix is orthogonal and should be caught by singularity test above, but I've left it in just in case"
            angle = MMath.TO_DEGS * Math.Acos(0.5 * (M11 + M22 + M33 - 1));
            x = (M32 - M23) / s;
            y = (M13 - M31) / s;
            z = (M21 - M12) / s;

            return new AxisAngle(x, y, z, angle, false);
        }
        

        /// <summary>
        /// Return the YawPitchRoll representation of this matrix.s
        /// </summary>
        /// <returns></returns>
        public YawPitchRoll ToYawPitchRoll()
        {
            /**
             * Adapted from http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToEuler/index.htm with:
             *      - Machina conventions: ES uses different heading/pitch/bank axes, fixed here
             *      - More singularity precision: using 0.001 triggers on angles above 86.3 degrees, 
             *        resulting in imprecise sets of conversions in those areas, like:
             *              Quaternion[0.027125, 0.691126, -0.058642, 0.71984]
                            EulerZYX[Z:175.504902, Y:-90, X:0]
                            Quaternion[0.027731, 0.706563, -0.027731, 0.706563]
                            EulerZYX[Z:175.504902, Y:-90, X:0]
                      instead of the following for 0.000001:
                            [Quaternion[0.027125, 0.691126, -0.058642, 0.71984]]
                            [EulerZYX[Z:-135.814015, Y:-86.544758, X:-51.142989]]
                            [Quaternion[-0.027125, -0.691126, 0.058642, -0.71984]]
             *      - Consistency adjustment on singularities: as a result of the 2 * atan2 operation, singularities
             *        may yield Z rotations between [-TAU, TAU], instead of the regular [-PI, PI]. This is not a big deal, 
             *        but for the sake of consistency in the Euler Angle representation, this has been readapted. 
             **/

            double xAng, yAng, zAng;

            // North pole singularity (yAng ~ 90degs)? Note m02 is -sin(y) = -sin(90) = -1
            if (this.M31 < -1 + MMath.EPSILON3)
            {
                xAng = 0;
                yAng = 0.5 * Math.PI;
                zAng = Math.Atan2(this.M23, this.M13);
                if (zAng < -Math.PI) zAng += MMath.TAU;  // remap to [-180, 180]
                else if (zAng > Math.PI) zAng -= MMath.TAU;
            }

            // South pole singularity (yAng ~ -90degs)? Note m02 is -sin(y) = -sin(-90) = 1
            else if (this.M31 > 1 - MMath.EPSILON3)
            {
                xAng = 0;
                yAng = -0.5 * Math.PI;
                zAng = Math.Atan2(-this.M23, -this.M13);
                if (zAng < -Math.PI) zAng += MMath.TAU;  // remap to [-180, 180]
                else if (zAng > Math.PI) zAng -= MMath.TAU;
            }

            // Regular derivation
            else
            {
                xAng = Math.Atan2(this.M32, this.M33);
                yAng = -Math.Asin(this.M31);
                zAng = Math.Atan2(this.M21, this.M11);
            }

            return new YawPitchRoll(MMath.TO_DEGS * xAng, MMath.TO_DEGS * yAng, MMath.TO_DEGS * zAng);
        }


        /// <summary>
        /// Calculates the determinant of the matrix.
        /// </summary>
        /// <returns>The determinant of the matrix.</returns>
        public double GetDeterminant()
        {
            // | a b c d |     | f g h |     | e g h |     | e f h |     | e f g |
            // | e f g h | = a | j k l | - b | i k l | + c | i j l | - d | i j k |
            // | i j k l |     | n o p |     | m o p |     | m n p |     | m n o |
            // | m n o p |
            //
            //   | f g h |
            // a | j k l | = a ( f ( kp - lo ) - g ( jp - ln ) + h ( jo - kn ) )
            //   | n o p |
            //
            //   | e g h |     
            // b | i k l | = b ( e ( kp - lo ) - g ( ip - lm ) + h ( io - km ) )
            //   | m o p |     
            //
            //   | e f h |
            // c | i j l | = c ( e ( jp - ln ) - f ( ip - lm ) + h ( in - jm ) )
            //   | m n p |
            //
            //   | e f g |
            // d | i j k | = d ( e ( jo - kn ) - f ( io - km ) + g ( in - jm ) )
            //   | m n o |
            //
            // Cost of operation
            // 17 adds and 28 muls.
            //
            // add: 6 + 8 + 3 = 17
            // mul: 12 + 16 = 28

            double a = M11, b = M12, c = M13, d = M14;
            double e = M21, f = M22, g = M23, h = M24;
            double i = M31, j = M32, k = M33, l = M34;
            double m = M41, n = M42, o = M43, p = M44;

            double kp_lo = k * p - l * o;
            double jp_ln = j * p - l * n;
            double jo_kn = j * o - k * n;
            double ip_lm = i * p - l * m;
            double io_km = i * o - k * m;
            double in_jm = i * n - j * m;

            return a * (f * kp_lo - g * jp_ln + h * jo_kn) -
                   b * (e * kp_lo - g * ip_lm + h * io_km) +
                   c * (e * jp_ln - f * ip_lm + h * in_jm) -
                   d * (e * jo_kn - f * io_km + g * in_jm);
        }

        /// <summary>
        /// Returns a boolean indicating whether this matrix instance is equal to the other given matrix.
        /// </summary>
        /// <param name="other">The matrix to compare this instance to.</param>
        /// <returns>True if the matrices are equal; False otherwise.</returns>
        public bool Equals(Matrix4x4 other)
        {
            return (M11 == other.M11 && M22 == other.M22 && M33 == other.M33 && M44 == other.M44 && // Check diagonal element first for early out.
                                        M12 == other.M12 && M13 == other.M13 && M14 == other.M14 &&
                    M21 == other.M21 && M23 == other.M23 && M24 == other.M24 &&
                    M31 == other.M31 && M32 == other.M32 && M34 == other.M34 &&
                    M41 == other.M41 && M42 == other.M42 && M43 == other.M43);
        }

        /// <summary>
        /// Returns a boolean indicating whether the given Object is equal to this matrix instance.
        /// </summary>
        /// <param name="obj">The Object to compare against.</param>
        /// <returns>True if the Object is equal to this matrix; False otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Matrix4x4)
            {
                return Equals((Matrix4x4)obj);
            }

            return false;
        }

        /// <summary>
        /// Returns a String representing this matrix instance.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            //CultureInfo ci = CultureInfo.InvariantCulture;

            //return String.Format(ci, "{{ {{M11:{0} M12:{1} M13:{2} M14:{3}}} {{M21:{4} M22:{5} M23:{6} M24:{7}}} {{M31:{8} M32:{9} M33:{10} M34:{11}}} {{M41:{12} M42:{13} M43:{14} M44:{15}}} }}",
            //                     M11.ToString(ci), M12.ToString(ci), M13.ToString(ci), M14.ToString(ci),
            //                     M21.ToString(ci), M22.ToString(ci), M23.ToString(ci), M24.ToString(ci),
            //                     M31.ToString(ci), M32.ToString(ci), M33.ToString(ci), M34.ToString(ci),
            //                     M41.ToString(ci), M42.ToString(ci), M43.ToString(ci), M44.ToString(ci));

            return string.Format(CultureInfo.InvariantCulture,
                "[{0}, {1}, {2}, {3}]\n[{4}, {5}, {6}, {7}]\n[{8}, {9}, {10}, {11}]\n[{12}, {13}, {14}, {15}]",
                Math.Round(M11, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M12, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M13, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M14, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M21, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M22, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M23, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M24, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M31, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M32, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M33, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M34, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M41, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M42, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M43, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(M44, MMath.STRING_ROUND_DECIMALS_MM));
        }


        #endregion methods

































        

        

        

        
    }
}
