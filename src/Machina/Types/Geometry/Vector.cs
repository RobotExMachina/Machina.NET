using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Types.Geometry
{

    //  ██╗   ██╗███████╗ ██████╗████████╗ ██████╗ ██████╗ 
    //  ██║   ██║██╔════╝██╔════╝╚══██╔══╝██╔═══██╗██╔══██╗
    //  ██║   ██║█████╗  ██║        ██║   ██║   ██║██████╔╝
    //  ╚██╗ ██╔╝██╔══╝  ██║        ██║   ██║   ██║██╔══██╗
    //   ╚████╔╝ ███████╗╚██████╗   ██║   ╚██████╔╝██║  ██║
    //    ╚═══╝  ╚══════╝ ╚═════╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝
    //                                                     
    /// <summary>
    /// Represents a three dimensional vector.
    /// </summary>
    public class Vector : IEpsilonComparable<Vector>, ISerializableArray, ISerializableJSON
    {

        #region fields
        internal double x;
        internal double y;
        internal double z;
        #endregion fields


        #region constructors
        /// <summary>
        /// Initializes a new instance of a vector, using its three components.
        /// </summary>
        /// <param name="x">The X (first) component.</param>
        /// <param name="y">The Y (second) component.</param>
        /// <param name="z">The Z (third) component.</param>
        public Vector(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        /// <summary>
        /// Initializes a new instance of a vector, copying the three components from a vector.
        /// </summary>
        /// <param name="vector">A double-precision vector.</param>
        public Vector(Vector vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        /// <summary>
        /// Gets the value of the vector with components 0,0,0.
        /// </summary>
        public static Vector Zero
        {
            get { return new Vector(0.0, 0.0, 0.0); }
        }

        /// <summary>
        /// Gets the value of the vector with components 1,0,0.
        /// </summary>
        public static Vector XAxis
        {
            get { return new Vector(1.0, 0.0, 0.0); }
        }

        /// <summary>
        /// Gets the value of the vector with components 0,1,0.
        /// </summary>
        public static Vector YAxis
        {
            get { return new Vector(0.0, 1.0, 0.0); }
        }

        /// <summary>
        /// Gets the value of the vector with components 0,0,1.
        /// </summary>
        public static Vector ZAxis
        {
            get { return new Vector(0.0, 0.0, 1.0); }
        }

        /// <summary>
        /// Gets the value of the vector with each component set to RhinoMath.UnsetValue.
        /// </summary>
        public static Vector Unset
        {
            get { return new Vector(MMath.UNSET_VALUE, MMath.UNSET_VALUE, MMath.UNSET_VALUE); }
        }

        /// <summary>
        /// Returns a random Vector with coordinates between specified double ranges. 
        /// Useful for testing and debugging.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Vector RandomFromDoubles(double min, double max)
        {
            return new Vector(MMath.Random(min, max), MMath.Random(min, max), MMath.Random(min, max));
        }

        /// <summary>
        /// Returns a rancom Vector with int coordinates between specified ranges.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Vector RandomFromInts(int min, int max)
        {
            return new Vector(MMath.RandomInt(min, max), MMath.RandomInt(min, max), MMath.RandomInt(min, max));
        }

        /// <summary>
        /// Returns a new Vector as the rotation of Vector 'p' by Rotation 'r'
        /// </summary>
        /// <param name="p"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Vector Rotation(Vector p, Rotation r)
        {
            //Vector v = new Vector(p);
            //v.Rotate(r);
            //return v;

            Vector v = new Vector(p);
            v.Rotate(r.Q);
            return v;
        }

        #endregion



        #region operators
        /// <summary>
        /// Are all the components of these vectors numerically equal?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Vector a, Vector b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        /// <summary>
        /// Are any of components of these vectors numerically not equal?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Vector a, Vector b)
        {
            return a.x != b.x || a.y != b.y || a.z == b.z;
        }


        /// <summary>
        /// Add two vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        /// <summary>
        /// Reverse a vector.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector operator -(Vector v)
        {
            return new Vector(-v.x, -v.y, -v.z);
        }

        /// <summary>
        /// Subtract two vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        /// <summary>
        /// Vector scalar multiplication.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector operator *(double s, Vector v)
        {
            return new Vector(s * v.x, s * v.y, s * v.z);
        }

        /// <summary>
        /// Vector scalar multiplication.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Vector operator *(Vector v, double s)
        {
            return new Vector(s * v.x, s * v.y, s * v.z);
        }

        /// <summary>
        /// Returns the <a href="https://en.wikipedia.org/wiki/Dot_product">Dot product</a> 
        /// of two Vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double operator *(Vector a, Vector b)
        {
            return a.x * b.x + a.y * b.y + a.x * b.y;
        }

        /// <summary>
        /// Returns the <a href="https://en.wikipedia.org/wiki/Dot_product">Dot product</a> 
        /// of two Vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double DotProduct(Vector a, Vector b)
        {
            // A · B = Ax * Bx + Ay * By + Az * Bz
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }


        /// <summary>
        /// Returns the angle between two vectors in radians.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double AngleBetween(Vector a, Vector b)
        {
            // A · B = ||A|| * ||B|| * cos(ang);
            double lens = a.Length * b.Length;

            if (lens < MMath.EPSILON_SQRT)
                return MMath.UNSET_VALUE;

            double div = (a * b) / lens;

            // Cap floating-point decimal precision on orthogonal angles to avoid NaN problems
            // https://stackoverflow.com/questions/8961952/c-sharp-math-acos-how-prevent-nan
            if (div > 1) div = 1;
            if (div < -1) div = -1;

            return Math.Acos(div);
        }

        /// <summary>
        /// Returns the <a href="https://en.wikipedia.org/wiki/Cross_product">Cross Product</a>
        /// of specified Vectors (Points).
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Vector CrossProduct(Vector p1, Vector p2)
        {
            return new Vector(
                p1.y * p2.z - p1.z * p2.y,
                p1.z * p2.x - p1.x * p2.z,
                p1.x * p2.y - p1.y * p2.x);
        }


        ///// <summary>
        ///// Returns a unit Vector orthogonal to specified guiding Vector, contained
        ///// in the plane defined by guiding Vector and reference Vector. The direction of the 
        ///// resulting Vector will be on the side of the reference Vector.
        ///// </summary>
        ///// <param name="guiding"></param>
        ///// <param name="reference"></param>
        ///// <returns></returns>
        //public static Vector OrthogonalTo(Vector guiding, Vector reference)
        //{
        //    return new CoordinateSystem(guiding, reference).YAxis;
        //}

        /// <summary>
        /// Given two vectors, this method outputs three new orthogonal vectors where the first one is 
        /// parallel to the original (although normalized), and the second one is perpendicular to the 
        /// first, maintaining the orientation of the first one. 
        /// Returns false if operation could not be processed (both vectors have the same direction)
        /// </summary>
        /// <param name="refX"></param>
        /// <param name="Y"></param>
        /// <param name="orthoX"></param>
        /// <param name="orthoY"></param>
        /// <param name="orthoZ"></param>
        public static bool Orthogonalize(Vector refX, Vector refY, out Vector orthoX, out Vector orthoY, out Vector orthoZ)
        {
            // Some sanity
            int dir = Vector.CompareDirections(refX, refY);
            if (dir == 1 || dir == 3)
            {
                Logger.Verbose("Cannot orthogonalize vectors with the same direction");
                orthoX = Unset;
                orthoY = Unset;
                orthoZ = Unset;
                return false;
            }

            orthoX = new Vector(refX);
            orthoX.Normalize();

            orthoZ = Vector.CrossProduct(orthoX, refY);
            orthoZ.Normalize();
            orthoY = Vector.CrossProduct(orthoZ, orthoX);

            return true;
        }

        #endregion operators


        #region interfaces
        /// <summary>
        /// Compare to other Vector under a numeric threshold.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public bool IsSimilarTo(Vector other, double epsilon)
        {
            return
                Math.Abs(this.X - other.X) < epsilon &&
                Math.Abs(this.Y - other.Y) < epsilon &&
                Math.Abs(this.Z - other.Z) < epsilon;
        }

        /// <summary>
        /// Array representation of this Vector.
        /// </summary>
        /// <returns></returns>
        public string ToArrayString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "[{0},{1},{2}]",
                Math.Round(X, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(Y, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(Z, MMath.STRING_ROUND_DECIMALS_MM));
        }

        /// <summary>
        /// JSON object representation of this Vector.
        /// </summary>
        /// <returns></returns>
        public string ToJSONString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{{\"x\":{0},\"y\":{1},\"z\":{2}}}",
                Math.Round(X, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(Y, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(Z, MMath.STRING_ROUND_DECIMALS_MM));
        }
        #endregion interfaces



        #region properties
        /// <summary>
        /// X property of the Vector.
        /// </summary>
        public double X { get { return x; } set { x = value; } }

        /// <summary>
        /// Y property of the Vector.
        /// </summary>
        public double Y { get { return y; } set { y = value; } }

        /// <summary>
        /// Z property of the Vector.
        /// </summary>
        public double Z { get { return z; } set { z = value; } }

        /// <summary>
        /// Returns the length of this Vector.
        /// </summary>
        /// <returns></returns>
        public double Length => Math.Sqrt(x * x + y * y + z * z);

        /// <summary>
        /// Returns the squared length of this Vector.
        /// </summary>
        /// <returns></returns>
        public double SqLength => x * x + y * y + z * z;

        /// <summary>
        /// Is this a unit Vector?
        /// </summary>
        /// <returns></returns>
        public bool IsUnit => Math.Abs(this.SqLength - 1) < MMath.EPSILON_SQRT;

        /// <summary>
        /// Is this a zero Vector?
        /// </summary>
        /// <returns></returns>
        public bool IsZero => Math.Abs(this.SqLength) < MMath.EPSILON_SQRT;



        #endregion properties





        #region methods
        /// <summary>
        /// Returns the distance between two Points represented by Vectors
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double Distance(Vector p1, Vector p2)
        {
            double dx = p1.x - p2.x,
                dy = p1.y - p2.y,
                dz = p1.z - p2.z;

            return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }

        /// <summary>
        /// Returns the squarde distance between two Points represented by Vectors
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double SqDistance(Vector p1, Vector p2)
        {
            double dx = p1.x - p2.x,
                dy = p1.y - p2.y,
                dz = p1.z - p2.z;

            return (dx * dx) + (dy * dy) + (dz * dz);
        }
        
        /// <summary>
        /// Are specified vectors parallel? This includes vectors with opposite directions, 
        /// even with different magnitudes.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AreParallel(Vector a, Vector b)
        {
            double alpha = AngleBetween(a, b);
            return alpha < MMath.EPSILON2 || 
                (alpha < Math.PI + MMath.EPSILON2 && alpha > Math.PI - MMath.EPSILON2);
        }

        /// <summary>
        /// Are specified vectors orthogonal?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AreOrthogonal(Vector a, Vector b)
        {
            double alpha = AngleBetween(a, b);
            return alpha < 0.5 * Math.PI + MMath.EPSILON2 && alpha > 0.5 * Math.PI - MMath.EPSILON2;
        }

        /// <summary>
        /// Are these vectors parallel but in opposite directions? They can have different magnitudes.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AreOpposite(Vector a, Vector b)
        {
            double alpha = AngleBetween(a, b);
            return alpha < Math.PI + MMath.EPSILON2 && alpha > Math.PI - MMath.EPSILON2;
        }

        /// <summary>
        /// Compares the directions of two vectors, regardless of their magnitude.
        /// Returns 1 if parallel (same direction), 2 if orthogonal (perpendicular
        /// directions), 3 if opposite (parallel in opposite directions), 
        /// 0 otherwise
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int CompareDirections(Vector a, Vector b)
        {
            double alpha = AngleBetween(a, b);

            if (alpha < MMath.EPSILON2)
            {
                return 1;
            }
            else if (alpha < 0.5 * Math.PI + MMath.EPSILON2 && alpha > 0.5 * Math.PI - MMath.EPSILON2)
            {
                return 2;
            }
            else if (Math.Abs(alpha - Math.PI) < MMath.EPSILON2)
            {
                return 3;
            }
            return 0;
        }

        /// <summary>
        /// Returns the squared distance from 'p' to the segment 'p1-p2'.
        /// </summary>
        /// <ref>https://github.com/imshz/simplify-net</ref>
        /// <param name="p"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double SqSegmentDistance(Vector p, Vector p1, Vector p2)
        {
            var x1 = p1.x;
            var y1 = p1.y;
            var z1 = p1.z;
            var dx = p2.x - x1;
            var dy = p2.y - y1;
            var dz = p2.z - z1;

            if (!dx.Equals(0.0) || !dy.Equals(0.0) || !dz.Equals(0.0))
            {
                var t = ((p.X - x1) * dx + (p.Y - y1) * dy + (p.Z - z1) * dz) / (dx * dx + dy * dy + dz * dz);

                if (t > 1)
                {
                    x1 = p2.X;
                    y1 = p2.Y;
                    z1 = p2.Z;
                }
                else if (t > 0)
                {
                    x1 += dx * t;
                    y1 += dy * t;
                    z1 += dz * t;
                }
            }

            dx = p.X - x1;
            dy = p.Y - y1;
            dz = p.Z - z1;

            return (dx * dx) + (dy * dy) + (dz * dz);
        }


        ///// <summary>
        ///// Simplifies the path using a combination of radial distance and 
        ///// Ramer–Douglas–Peucker algorithm. 
        ///// </summary>
        ///// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
        ///// <param name="points"></param>
        ///// <param name="tolerance"></param>
        ///// <param name="highQuality"></param>
        ///// <returns></returns>
        //internal static List<Vector> SimplifyPointList(List<Vector> points, double tolerance, bool highQuality)
        //{
        //    if (points.Count < 1)
        //    {
        //        Logger.Verbose("List contains no points.");
        //    }

        //    int prev = points.Count;
        //    List<Vector> simplified;

        //    double sqTolerance = tolerance * tolerance;

        //    if (!highQuality)
        //    {
        //        simplified = Vector.SimplifyRadialDistance(points, sqTolerance);
        //        simplified = Vector.SimplifyDouglasPeucker(simplified, sqTolerance);
        //    }
        //    else
        //    {
        //        simplified = SimplifyDouglasPeucker(points, sqTolerance);
        //    }

        //    Logger.Verbose("Vector list simplified from " + prev + " to " + simplified.Count + " Points.");

        //    return simplified;
        //}

        ///// <summary>
        ///// Simple distance-based simplification. Consecutive points under 
        ///// threshold distance are removed. 
        ///// </summary>
        ///// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
        ///// <param name="points"></param>
        ///// <param name="sqTolerance"></param>
        ///// <returns></returns>
        //internal static List<Vector> SimplifyRadialDistance(List<Vector> points, double sqTolerance)
        //{
        //    Vector prevPoint = points[0];
        //    List<Vector> newPoints = new List<Vector> { prevPoint };
        //    Vector pt = Unset;

        //    for (int i = 1; i < points.Count; i++)
        //    {
        //        pt = points[i];

        //        if (Vector.SqDistance(pt, prevPoint) > sqTolerance)
        //        {
        //            newPoints.Add(pt);
        //            prevPoint = pt;
        //        }
        //    }

        //    // Add the last frame of the path (?)
        //    if (pt != Unset && !prevPoint.Equals(pt))
        //    {
        //        newPoints.Add(pt);
        //    }

        //    return newPoints;
        //}

        ///// <summary>
        ///// The Ramer-Douglas-Peucker algorithm.
        ///// </summary>
        ///// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
        ///// <param name="points"></param>
        ///// <param name="sqTolerance"></param>
        ///// <returns></returns>
        //internal static List<Vector> SimplifyDouglasPeucker(List<Vector> points, double sqTolerance)
        //{
        //    var len = points.Count;
        //    var markers = new int?[len];
        //    int? first = 0;
        //    int? last = len - 1;
        //    int? index = 0;
        //    var stack = new List<int?>();
        //    var newPoints = new List<Vector>();

        //    markers[first.Value] = markers[last.Value] = 1;

        //    while (last != null)
        //    {
        //        var maxSqDist = 0.0d;

        //        for (int? i = first + 1; i < last; i++)
        //        {
        //            var sqDist = Vector.SqSegmentDistance(points[i.Value], points[first.Value], points[last.Value]);

        //            if (sqDist > maxSqDist)
        //            {
        //                index = i;
        //                maxSqDist = sqDist;
        //            }
        //        }

        //        if (maxSqDist > sqTolerance)
        //        {
        //            markers[index.Value] = 1;
        //            stack.AddRange(new[] { first, index, index, last });
        //        }

        //        if (stack.Count > 0)
        //        {
        //            last = stack[stack.Count - 1];
        //            stack.RemoveAt(stack.Count - 1);
        //        }
        //        else
        //        {
        //            last = null;
        //        }

        //        if (stack.Count > 0)
        //        {
        //            first = stack[stack.Count - 1];
        //            stack.RemoveAt(stack.Count - 1);
        //        }
        //        else
        //        {
        //            first = null;
        //        }
        //    }

        //    for (int i = 0; i < len; i++)
        //    {
        //        if (markers[i] != null)
        //        {
        //            newPoints.Add(points[i]);
        //        }
        //    }

        //    return newPoints;
        //}


        /// <summary>
        /// Sets the values of this Vector from another Vector (shallow copy if this is a class).
        /// </summary>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        /// <param name="newZ"></param>
        public void Set(Vector clone)
        {
            x = clone.x;
            y = clone.y;
            z = clone.z;
        }

        /// <summary>
        /// Sets the values of this Vector.
        /// </summary>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        /// <param name="newZ"></param>
        public void Set(double newX, double newY, double newZ)
        {
            x = newX;
            y = newY;
            z = newZ;
        }

        /// <summary>
        /// Add specified values to this Vector.
        /// </summary>
        /// <param name="incX"></param>
        /// <param name="incY"></param>
        /// <param name="incZ"></param>
        public void Add(double incX, double incY, double incZ)
        {
            x += incX;
            y += incY;
            z += incZ;
        }

        /// <summary>
        /// Add the coordinates of specified Vector to this one.
        /// </summary>
        /// <param name="p"></param>
        public void Add(Vector p)
        {
            x += p.x;
            y += p.y;
            z += p.z;
        }

        /// <summary>
        /// Unitizes this Vector. Will return false if Vector is not unitizable
        /// (zero length Vector).
        /// </summary>
        /// <returns></returns>
        public bool Normalize()
        {
            double len = this.Length;
            if (len < MMath.EPSILON2) return false;
            x /= len;
            y /= len;
            z /= len;
            return true;
        }

        /// <summary>
        /// Reverses the direction of this Vector.
        /// </summary>
        public void Reverse()
        {
            x = -x;
            y = -y;
            z = -z;
        }

        /// <summary>
        /// Multiplies this Vector by a scalar. 
        /// </summary>
        /// <param name="factor"></param>
        public void Scale(double factor)
        {
            x *= factor;
            y *= factor;
            y *= factor;
        }

        /// <summary>
        /// Rotates this Vector by speficied Quaternion.
        /// </summary>
        /// <param name="q"></param>
        public bool Rotate(Quaternion q)
        {
            // P.out = Q * P.in * conj(Q);
            // From gl-matrix.js
            double ix = q.W * x + q.Y * z - q.Z * y,
                   iy = q.W * y + q.Z * x - q.X * z,
                   iz = q.W * z + q.X * y - q.Y * x,
                   iw = -q.X * x - q.Y * y - q.Z * z;

            x = ix * q.W - iw * q.X - iy * q.Z + iz * q.Y;
            y = iy * q.W - iw * q.Y - iz * q.X + ix * q.Z;
            z = iz * q.W - iw * q.Z - ix * q.Y + iy * q.X;

            return true;
        }

        /// <summary>
        /// Rotates this vector by a specified Rotation object.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public bool Rotate(Rotation r)
        {
            return this.Rotate(r.Q);
        }

        /// <summary>
        /// Rotates this Vector by a specified AxisAngle.
        /// </summary>
        /// <param name="aa"></param>
        /// <returns></returns>
        public bool Rotate(AxisAngle aa)
        {
            return this.Rotate(aa.ToQuaternion());
        }

        /// <summary>
        /// Rotates this Vector specified degrees around specified vector. 
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="angDegs"></param>
        /// <returns></returns>
        public bool Rotate(Vector vec, double angDegs)
        {
            return this.Rotate(new AxisAngle(vec, angDegs).ToQuaternion());
        }

        /// <summary>
        /// Rotates this Vector specified degrees around specified vector. 
        /// </summary>
        /// <param name="vecX"></param>
        /// <param name="vecY"></param>
        /// <param name="vecZ"></param>
        /// <param name="angDegs"></param>
        /// <returns></returns>
        public bool Rotate(double vecX, double vecY, double vecZ, double angDegs)
        {
            // @TODO: optimize this...
            return this.Rotate(new AxisAngle(vecX, vecY, vecZ, angDegs).ToQuaternion());
        }


        /// <summary>
        /// Returns the euclidean distance from this Vector to another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double DistanceTo(Vector other)
        {
            double dx = x - other.x,
                   dy = y - other.y,
                   dz = z - other.z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }


        ///// <summary>
        ///// Equality checks.
        ///// </summary>
        ///// <ref>https://github.com/imshz/simplify-net</ref>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //public override bool Equals(object obj)
        //{
        //    if (ReferenceEquals(null, obj)) return false;
        //    if (ReferenceEquals(this, obj)) return true;

        //    if (obj.GetType() != typeof(Vector))
        //        return false;

        //    if (obj is Vector)
        //    {
        //        return Equals((Vector)obj);
        //    }

        //    return false;
        //}

        ///// <summary>
        ///// Equality checks.
        ///// </summary>
        ///// <ref>https://github.com/imshz/simplify-net</ref>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //public bool Equals(Vector other)
        //{
        //    if (ReferenceEquals(null, other)) return false;
        //    if (ReferenceEquals(this, other)) return true;
        //    //return other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
        //    return Math.Abs(X - other.X) < EPSILON2 &&
        //           Math.Abs(Y - other.Y) < EPSILON2 &&
        //           Math.Abs(Z - other.Z) < EPSILON2;
        //}

        public override string ToString()
        {
            return this.ToString(false);
        }

        /// <summary>
        /// Converts 
        /// </summary>
        /// <param name="labels"></param>
        /// <returns></returns>
        public string ToString(bool labels)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0}[{1}{2}, {3}{4}, {5}{6}]",
                labels ? "Vector" : "",
                labels ? "X:" : "",
                Math.Round(X, MMath.STRING_ROUND_DECIMALS_MM),
                labels ? "Y:" : "",
                Math.Round(Y, MMath.STRING_ROUND_DECIMALS_MM),
                labels ? "Z:" : "",
                Math.Round(Z, MMath.STRING_ROUND_DECIMALS_MM));
        }

        

        #endregion methods

    }
}
