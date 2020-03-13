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
    /// <remarks>Some methods are borrowed from Rhinocommon.</remarks>
    public class Vector : IEpsilonComparable<Vector>, ISerializableArray, ISerializableJSON
    {

        #region fields
        private double _x;
        private double _y;
        private double _z;
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
            _x = x;
            _y = y;
            _z = z;
        }
        
        /// <summary>
        /// Initializes a new instance of a vector, copying the three components from a vector.
        /// </summary>
        /// <param name="vector">A double-precision vector.</param>
        public Vector(Vector vector)
        {
            _x = vector._x;
            _y = vector._y;
            _z = vector._z;
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
            return a._x == b._x && a._y == b._y && a._z == b._z;
        }

        /// <summary>
        /// Are any of components of these vectors numerically not equal?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Vector a, Vector b)
        {
            return a._x != b._x || a._y != b._y || a._z == b._z;
        }


        /// <summary>
        /// Add two vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a._x + b._x, a._y + b._y, a._z + b._z);
        }

        /// <summary>
        /// Reverse a vector.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector operator -(Vector v)
        {
            return new Vector(-v._x, -v._y, -v._z);
        }

        /// <summary>
        /// Subtract two vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a._x - b._x, a._y - b._y, a._z - b._z);
        }

        /// <summary>
        /// Vector scalar multiplication.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector operator *(double s, Vector v)
        {
            return new Vector(s * v._x, s * v._y, s * v._z);
        }

        /// <summary>
        /// Vector scalar multiplication.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Vector operator *(Vector v, double s)
        {
            return new Vector(s * v._x, s * v._y, s * v._z);
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
            return a._x * b._x + a._y * b._y + a._z * b._z;
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
            return a._x * b._x + a._y * b._y + a._z * b._z;
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

        // JL: from Rhinocmmon, adapt when Frame structure is in!
        ///// <summary>
        ///// Computes the angle on a plane between two vectors.
        ///// </summary>
        ///// <param name="a">First vector.</param>
        ///// <param name="b">Second vector.</param>
        ///// <param name="plane">Two-dimensional plane on which to perform the angle measurement.</param>
        ///// <returns>On success, the angle (in radians) between a and b as projected onto the plane; RhinoMath.UnsetValue on failure.</returns>
        //public static double AngleBetween(Vector3d a, Vector3d b, Plane plane)
        //{
        //    { // Project vectors onto plane.
        //        Point3d pA = plane.Origin + a;
        //        Point3d pB = plane.Origin + b;

        //        pA = plane.ClosestPoint(pA);
        //        pB = plane.ClosestPoint(pB);

        //        a = pA - plane.Origin;
        //        b = pB - plane.Origin;
        //    }

        //    // Abort on invalid cases.
        //    if (!a.Unitize()) { return RhinoMath.UnsetValue; }
        //    if (!b.Unitize()) { return RhinoMath.UnsetValue; }

        //    double dot = a * b;
        //    { // Limit dot product to valid range.
        //        if (dot >= 1.0)
        //        { dot = 1.0; }
        //        else if (dot < -1.0)
        //        { dot = -1.0; }
        //    }

        //    double angle = Math.Acos(dot);
        //    { // Special case (anti)parallel vectors.
        //        if (Math.Abs(angle) < 1e-64) { return 0.0; }
        //        if (Math.Abs(angle - Math.PI) < 1e-64) { return Math.PI; }
        //    }

        //    Vector3d cross = Vector3d.CrossProduct(a, b);
        //    if (plane.ZAxis.IsParallelTo(cross) == +1)
        //        return angle;
        //    return 2.0 * Math.PI - angle;
        //}

        /// <summary>
        /// Returns the <a href="https://en.wikipedia.org/wiki/Cross_product">Cross Product</a>
        /// of specified Vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector CrossProduct(Vector a, Vector b)
        {
            return new Vector(
                a._y * b._z - a._z * b._y,
                a._z * b._x - a._x * b._z,
                a._x * b._y - a._y * b._x);
        }

        /// <summary>
        /// Given two vectors, this method outputs three new orthogonal vectors where the first one is 
        /// parallel to the original (although normalized), and the second one is perpendicular to the 
        /// first, maintaining the orientation of the first one. 
        /// Returns false if operation could not be processed (both vectors have the same direction)
        /// </summary>
        /// <param name="refX"></param>
        /// <param name="refY"></param>
        /// <param name="orthoX"></param>
        /// <param name="orthoY"></param>
        /// <param name="orthoZ"></param>
        public static bool Orthogonalize(Vector refX, Vector refY, out Vector orthoX, out Vector orthoY, out Vector orthoZ)
        {
            // Some sanity
            Direction dir = Vector.CompareDirections(refX, refY);
            if (dir == Direction.Invalid || dir == Direction.Parallel || dir == Direction.Opposite)
            {
                Logger.Verbose("Invalid vectors for orthogonalization.");
                orthoX = null;
                orthoY = null;
                orthoZ = null;
                return false;
            }

            orthoX = new Vector(refX);
            orthoX.Normalize();

            orthoZ = Vector.CrossProduct(orthoX, refY);
            orthoZ.Normalize();
            orthoY = Vector.CrossProduct(orthoZ, orthoX);

            return true;
        }

        /// <summary>
        /// Creates a vector perpendicular to the source one. The direction of the perpendicular
        /// vector depends on the orientation of the source one.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static bool PerpendicularTo(Vector vec, out Vector perp)
        {
            // Adapted from RC/ON
            int i, j, k;
            double a, b;
            k = 2;
            if (Math.Abs(vec.Y) > Math.Abs(vec.X))
            {
                if (Math.Abs(vec.Z) > Math.Abs(vec.Y))
                {
                    // |v.Z| > |v.Y| > |v.X|
                    i = 2;
                    j = 1;
                    k = 0;
                    a = vec.Z;
                    b = -vec.Y;
                }
                else if (Math.Abs(vec.Z) >= Math.Abs(vec.X))
                {
                    // |v.Y| >= |v.Z| >= |v.X|
                    i = 1;
                    j = 2;
                    k = 0;
                    a = vec.Y;
                    b = -vec.Z;
                }
                else
                {
                    // |v.Y| > |v.X| > |v.Z|
                    i = 1;
                    j = 0;
                    k = 2;
                    a = vec.Y;
                    b = -vec.X;
                }
            }
            else if (Math.Abs(vec.Z) > Math.Abs(vec.X))
            {
                // |v.Z| > |v.X| >= |v.Y|
                i = 2;
                j = 0;
                k = 1;
                a = vec.Z;
                b = -vec.X;
            }
            else if (Math.Abs(vec.Z) > Math.Abs(vec.Y))
            {
                // |v.X| >= |v.Z| > |v.Y|
                i = 0;
                j = 2;
                k = 1;
                a = vec.X;
                b = -vec.Z;
            }
            else
            {
                // |v.X| >= |v.Y| >= |v.Z|
                i = 0;
                j = 1;
                k = 2;
                a = vec.X;
                b = -vec.Y;
            }

            perp = Vector.Zero;
            perp[i] = b;
            perp[j] = a;
            perp[k] = 0.0;
            
            return (a != 0.0) ? true : false;
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
        public string ToArrayString(int decimals)
        {
            if (decimals == -1)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "[{0},{1},{2}]",
                    _x, _y, _z);
            }

            return string.Format(CultureInfo.InvariantCulture,
                "[{0},{1},{2}]",
                Math.Round(_x, decimals),
                Math.Round(_y, decimals),
                Math.Round(_z, decimals));
        }

        /// <summary>
        /// JSON object representation of this Vector.
        /// </summary>
        /// <returns></returns>
        public string ToJSONString(int decimals)
        {
            if (decimals == -1)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "{{\"X\":{0},\"Y\":{1},\"Z\":{2}}}",
                    _x, _y, _z);
            }

            return string.Format(CultureInfo.InvariantCulture,
                "{{\"X\":{0},\"Y\":{1},\"Z\":{2}}}",
                Math.Round(_x, decimals),
                Math.Round(_y, decimals),
                Math.Round(_z, decimals));
        }
        #endregion interfaces



        #region properties
        /// <summary>
        /// X property of the Vector.
        /// </summary>
        public double X { get { return _x; } set { _x = value; } }

        /// <summary>
        /// Y property of the Vector.
        /// </summary>
        public double Y { get { return _y; } set { _y = value; } }

        /// <summary>
        /// Z property of the Vector.
        /// </summary>
        public double Z { get { return _z; } set { _z = value; } }

        /// <summary>
        /// Gets or sets a vector component at the given index.
        /// </summary>
        /// <param name="index">Index of vector component. Valid values are: 
        /// <para>0 = X-component</para>
        /// <para>1 = Y-component</para>
        /// <para>2 = Z-component</para>
        /// .</param>
        public double this[int index]
        {
            // From Rhinocommon
            get
            {
                if (0 == index)
                    return _x;
                if (1 == index)
                    return _y;
                if (2 == index)
                    return _z;
                // IronPython works with indexing when we thrown an IndexOutOfRangeException
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (0 == index)
                    _x = value;
                else if (1 == index)
                    _y = value;
                else if (2 == index)
                    _z = value;
                else
                    throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// Returns the length of this Vector.
        /// </summary>
        /// <returns></returns>
        public double Length => Math.Sqrt(_x * _x + _y * _y + _z * _z);

        /// <summary>
        /// Returns the squared length of this Vector.
        /// </summary>
        /// <returns></returns>
        public double SqLength => _x * _x + _y * _y + _z * _z;

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
            double dx = p1._x - p2._x,
                dy = p1._y - p2._y,
                dz = p1._z - p2._z;

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
            double dx = p1._x - p2._x,
                dy = p1._y - p2._y,
                dz = p1._z - p2._z;

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
        /// -1 if angle is non-computable (any vector is zero?), 0 otherwise.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Direction CompareDirections(Vector a, Vector b)
        {
            double alpha = AngleBetween(a, b);

            if (alpha == MMath.UNSET_VALUE)
            {
                return Direction.Invalid;
            } 
            else if (alpha < MMath.EPSILON2)
            {
                return Direction.Parallel;
            }
            else if (alpha < 0.5 * Math.PI + MMath.EPSILON2 && alpha > 0.5 * Math.PI - MMath.EPSILON2)
            {
                return Direction.Orthogonal;
            }
            else if (Math.Abs(alpha - Math.PI) < MMath.EPSILON2)
            {
                return Direction.Opposite;
            }
            return Direction.Other;
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
            var x1 = p1._x;
            var y1 = p1._y;
            var z1 = p1._z;
            var dx = p2._x - x1;
            var dy = p2._y - y1;
            var dz = p2._z - z1;

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
            _x = clone._x;
            _y = clone._y;
            _z = clone._z;
        }

        /// <summary>
        /// Sets the values of this Vector.
        /// </summary>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        /// <param name="newZ"></param>
        public void Set(double newX, double newY, double newZ)
        {
            _x = newX;
            _y = newY;
            _z = newZ;
        }

        /// <summary>
        /// Add specified values to this Vector.
        /// </summary>
        /// <param name="incX"></param>
        /// <param name="incY"></param>
        /// <param name="incZ"></param>
        public void Add(double incX, double incY, double incZ)
        {
            _x += incX;
            _y += incY;
            _z += incZ;
        }

        /// <summary>
        /// Add the coordinates of specified Vector to this one.
        /// </summary>
        /// <param name="p"></param>
        public void Add(Vector p)
        {
            _x += p._x;
            _y += p._y;
            _z += p._z;
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
            _x /= len;
            _y /= len;
            _z /= len;
            return true;
        }

        /// <summary>
        /// Reverses the direction of this Vector.
        /// </summary>
        public void Reverse()
        {
            _x = -_x;
            _y = -_y;
            _z = -_z;
        }

        /// <summary>
        /// Multiplies this Vector by a scalar. 
        /// </summary>
        /// <param name="factor"></param>
        public void Scale(double factor)
        {
            _x *= factor;
            _y *= factor;
            _y *= factor;
        }

        /// <summary>
        /// Rotates this Vector by speficied Quaternion.
        /// </summary>
        /// <param name="q"></param>
        public bool Rotate(Quaternion q)
        {
            // P.out = Q * P.in * conj(Q);
            // From gl-matrix.js
            double ix = q.W * _x + q.Y * _z - q.Z * _y,
                   iy = q.W * _y + q.Z * _x - q.X * _z,
                   iz = q.W * _z + q.X * _y - q.Y * _x,
                   iw = -q.X * _x - q.Y * _y - q.Z * _z;

            _x = ix * q.W - iw * q.X - iy * q.Z + iz * q.Y;
            _y = iy * q.W - iw * q.Y - iz * q.X + ix * q.Z;
            _z = iz * q.W - iw * q.Z - ix * q.Y + iy * q.X;

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
            double dx = _x - other._x,
                   dy = _y - other._y,
                   dz = _z - other._z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }


        /// <summary>
        /// Computes the hash code for the current vector.
        /// </summary>
        /// <returns>A non-unique number that represents the components of this vector.</returns>
        public override int GetHashCode()
        {
            // MSDN docs recommend XOR'ing the internal values to get a hash code
            return _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();
        }


        /// <summary>
        /// Equality checks.
        /// </summary>
        /// <ref>https://github.com/imshz/simplify-net</ref>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            if (obj.GetType() != typeof(Vector))
                return false;

            if (obj is Vector)
            {
                return Equals((Vector)obj);
            }

            return false;
        }

        /// <summary>
        /// Equality checks.
        /// </summary>
        /// <ref>https://github.com/imshz/simplify-net</ref>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Vector other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            //return other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
            return Math.Abs(X - other.X) < MMath.EPSILON2 &&
                   Math.Abs(Y - other.Y) < MMath.EPSILON2 &&
                   Math.Abs(Z - other.Z) < MMath.EPSILON2;
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
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
