using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
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
    public class Vector : Geometry
    {

        /// <summary>
        /// X property of the Vector.
        /// </summary>
        public double X { get; internal set; }

        /// <summary>
        /// Y property of the Vector.
        /// </summary>
        public double Y { get; internal set; }

        /// <summary>
        /// Z property of the Vector.
        /// </summary>
        public double Z { get; internal set; }

        /// <summary>
        /// Test if this Vector is approximately equal to another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSimilar(Vector other)
        {
            return Math.Abs(this.X - other.X) < EPSILON
                && Math.Abs(this.Y - other.Y) < EPSILON
                && Math.Abs(this.Z - other.Z) < EPSILON;
        }

        //public static bool operator ==(Vector p1, Vector p2)
        //{
        //    return Math.Abs(p1.X - p2.X) < EPSILON
        //        && Math.Abs(p1.Y - p2.Y) < EPSILON
        //        && Math.Abs(p1.Z - p2.Z) < EPSILON;
        //}

        //public static bool operator !=(Vector p1, Vector p2)
        //{
        //    return Math.Abs(p1.X - p2.X) > EPSILON
        //        || Math.Abs(p1.Y - p2.Y) > EPSILON
        //        || Math.Abs(p1.Z - p2.Z) > EPSILON;
        //}


        /// <summary>
        /// Implicit conversion to Point object.
        /// </summary>
        /// <param name="vec"></param>
        public static implicit operator Point(Vector vec)
        {
            return new Machina.Point(vec.X, vec.Y, vec.Z);
        }

        /// <summary>
        /// Create a Vector from its XYZ coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vector(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Creates a shallow copy of the specified Vector.
        /// </summary>
        /// <param name="p"></param>
        public Vector(Vector p)
        {
            this.X = p.X;
            this.Y = p.Y;
            this.Z = p.Z;
        }

        public Vector()
        {
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }

        public void Set(double newX, double newY, double newz)
        {
            this.X = newX;
            this.Y = newY;
            this.Z = newz;
        }

        /// <summary>
        /// Shallow-copies the values of specified Vector.
        /// </summary>
        /// <param name="p"></param>
        public void Set(Vector p)
        {
            this.X = p.X;
            this.Y = p.Y;
            this.Z = p.Z;
        }

        /// <summary>
        /// Add specified values to this Vector.
        /// </summary>
        /// <param name="incX"></param>
        /// <param name="incY"></param>
        /// <param name="incZ"></param>
        public void Add(double incX, double incY, double incZ)
        {
            this.X += incX;
            this.Y += incY;
            this.Z += incZ;
        }

        /// <summary>
        /// Add the coordinates of specified Vector to this one.
        /// </summary>
        /// <param name="p"></param>
        public void Add(Vector p)
        {
            this.X += p.X;
            this.Y += p.Y;
            this.Z += p.Z;
        }

        /// <summary>
        /// Returns the length of this Vector.
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
        }

        /// <summary>
        /// Returns the squared length of this Vector.
        /// </summary>
        /// <returns></returns>
        public double SqLength()
        {
            return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
        }

        /// <summary>
        /// Unitizes this Vector. Will return false if Vector is not unitizable
        /// (zero length Vector).
        /// </summary>
        /// <returns></returns>
        public bool Normalize()
        {
            double len = this.Length();
            if (len < EPSILON) return false;
            this.X /= len;
            this.Y /= len;
            this.Z /= len;
            return true;
        }

        /// <summary>
        /// Is this a unit Vector?
        /// </summary>
        /// <returns></returns>
        public bool IsUnit()
        {
            return Math.Abs(this.SqLength() - 1) < EPSILON;
        }

        public bool IsZero()
        {
            return Math.Abs(this.SqLength()) < EPSILON;
        }

        /// <summary>
        /// Reverses the direction of this Vector.
        /// </summary>
        public void Invert()
        {
            this.X = -this.X;
            this.Y = -this.Y;
            this.Z = -this.Z;
        }

        /// <summary>
        /// An alias for Invert().
        /// </summary>
        public void Flip()
        {
            Invert();
        }

        /// <summary>
        /// An alias for Invert().
        /// </summary>
        public void Reverse()
        {
            Invert();
        }

        /// <summary>
        /// Multiplies this Vector by a scalar. 
        /// </summary>
        /// <param name="factor"></param>
        public void Scale(double factor)
        {
            this.X *= factor;
            this.Y *= factor;
            this.Z *= factor;
        }

        /// <summary>
        /// Rotates this Vector by speficied Quaterion.
        /// </summary>
        /// <param name="q"></param>
        public bool Rotate(Quaternion q)
        {
            // P.out = Q * P.in * conj(Q);
            // From gl-matrix.js
            double ix = q.W * X + q.Y * Z - q.Z * Y,
                   iy = q.W * Y + q.Z * X - q.X * Z,
                   iz = q.W * Z + q.X * Y - q.Y * X,
                   iw = -q.X * X - q.Y * Y - q.Z * Z;

            this.X = ix * q.W - iw * q.X - iy * q.Z + iz * q.Y;
            this.Y = iy * q.W - iw * q.Y - iz * q.X + ix * q.Z;
            this.Z = iz * q.W - iw * q.Z - ix * q.Y + iy * q.X;

            return true;
        }

        public bool Rotate(Rotation r)
        {
            return this.Rotate(r.Q);
        }


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
        /// <param name="vec"></param>
        /// <param name="angDegs"></param>
        /// <returns></returns>
        public bool Rotate(double vecX, double vecY, double vecZ, double angDegs)
        {
            return this.Rotate(new AxisAngle(vecX, vecY, vecZ, angDegs).ToQuaternion());
        }



        /// <summary>
        /// Unit X Vector.
        /// </summary>
        public static Vector XAxis = new Vector(1, 0, 0);

        /// <summary>
        /// Unit Y Vector.
        /// </summary>
        public static Vector YAxis = new Vector(0, 1, 0);

        /// <summary>
        /// Unit Z Vector.
        /// </summary>
        public static Vector ZAxis = new Vector(0, 0, 1);

        public static Vector operator +(Vector p1, Vector p2)
        {
            return new Vector(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        public static Vector operator -(Vector p)
        {
            return new Vector(-p.X, -p.Y, -p.Z);
        }

        public static Vector operator -(Vector p1, Vector p2)
        {
            return new Vector(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        public static Vector operator *(Double s, Vector p)
        {
            return new Vector(s * p.X, s * p.Y, s * p.Z);
        }

        public static Vector operator *(Vector p, Double s)
        {
            return new Vector(s * p.X, s * p.Y, s * p.Z);
        }

        /// <summary>
        /// Returns the dot product of specified Vectors. 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double operator *(Vector p1, Vector p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z;
        }

        /// <summary>
        /// Returns the <a href="https://en.wikipedia.org/wiki/Dot_product">Dot product</a> 
        /// of specified Points (Vectors).
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double DotProduct(Vector p1, Vector p2)
        {
            // A · B = Ax * Bx + Ay * By + Az * Bz
            return p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z;
        }

        /// <summary>
        /// Returns the angle between two vectors in radians.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double AngleBetween(Vector p1, Vector p2)
        {
            // A · B = ||A|| * ||B|| * cos(ang);
            double lens = p1.Length() * p2.Length();

            // Why was I returning 90 degs...? 
            //if (lens < EPSILON)
            //    return 0.5 * Math.PI;

            if (lens < EPSILON)
                return 0;  // should convert to nullable "double?" ? 

            double div = DotProduct(p1, p2) / lens;

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
                p1.Y * p2.Z - p1.Z * p2.Y,
                p1.Z * p2.X - p1.X * p2.Z,
                p1.X * p2.Y - p1.Y * p2.X);
        }

        ///// <summary>
        ///// Returns a unit Vector orthogonal to specified guiding Vector, contained
        ///// in the plane defined by guiding Vector and Vector. The direction of the 
        ///// resulting Vector will be on the side of the guiding Vector.
        ///// </summary>
        ///// <param name="vec"></param>
        ///// <param name="p"></param>
        ///// <returns></returns>
        //public static Vector OrthogonalTo(Vector vec, Vector p)
        //{
        //    return new CoordinateSystem(vec, p).YAxis;
        //}

        /// <summary>
        /// Returns the distance between two Points.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double Distance(Vector p1, Vector p2)
        {
            double dx = p1.X - p2.X,
                dy = p1.Y - p2.Y,
                dz = p1.Z - p2.Z;

            return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }

        /// <summary>
        /// Returns the squarde distance between two Points.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double SqDistance(Vector p1, Vector p2)
        {
            double dx = p1.X - p2.X,
                dy = p1.Y - p2.Y,
                dz = p1.Z - p2.Z;

            return (dx * dx) + (dy * dy) + (dz * dz);
        }

        /// <summary>
        /// Are specified vectors parallel? This includes vectors with opposite directions, 
        /// even with different magnitudes.
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static bool AreParallel(Vector vec1, Vector vec2)
        {
            double alpha = AngleBetween(vec1, vec2);
            return alpha < EPSILON || (alpha < Math.PI + EPSILON && alpha > Math.PI - EPSILON);
        }

        /// <summary>
        /// Are specified vectors orthogonal?
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static bool AreOrthogonal(Vector vec1, Vector vec2)
        {
            double alpha = AngleBetween(vec1, vec2);
            return alpha < 0.5 * Math.PI + EPSILON && alpha > 0.5 * Math.PI - EPSILON;
        }

        /// <summary>
        /// Are these vectors parallel but in opposite directions? They can have different magnitudes.
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static bool AreOpposite(Vector vec1, Vector vec2)
        {
            double alpha = AngleBetween(vec1, vec2);
            return alpha < Math.PI + EPSILON && alpha > Math.PI - EPSILON;
        }

        /// <summary>
        /// Compares the directions of two vectors, regardless of their magnitude.
        /// Returns 1 if parallel (same direction), 2 if orthogonal (perpendicular
        /// directions), 3 if opposite (parallel in opposite directions), 
        /// 0 otherwise
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static int CompareDirections(Vector vec1, Vector vec2)
        {
            double alpha = AngleBetween(vec1, vec2);

            if (alpha < EPSILON)
            {
                return 1;
            }
            else if (alpha < 0.5 * Math.PI + EPSILON && alpha > 0.5 * Math.PI - EPSILON)
            {
                return 2;
            }
            else if (Math.Abs(alpha - Math.PI) < EPSILON)
            {
                return 3;
            }
            return 0;
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
            return new Machina.Vector(Random(min, max), Random(min, max), Random(min, max));
        }

        public static Vector RandomFromInts(int min, int max)
        {
            return new Machina.Vector(RandomInt(min, max), RandomInt(min, max), RandomInt(min, max));
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
            var x = p1.X;
            var y = p1.Y;
            var z = p1.Z;
            var dx = p2.X - x;
            var dy = p2.Y - y;
            var dz = p2.Z - z;

            if (!dx.Equals(0.0) || !dy.Equals(0.0) || !dz.Equals(0.0))
            {
                var t = ((p.X - x) * dx + (p.Y - y) * dy + (p.Z - z) * dz) / (dx * dx + dy * dy + dz * dz);

                if (t > 1)
                {
                    x = p2.X;
                    y = p2.Y;
                    z = p2.Z;
                }
                else if (t > 0)
                {
                    x += dx * t;
                    y += dy * t;
                    z += dz * t;
                }
            }

            dx = p.X - x;
            dy = p.Y - y;
            dz = p.Z - z;

            return (dx * dx) + (dy * dy) + (dz * dz);
        }

        /// <summary>
        /// Given two vectors, this method outputs two new orthogonal vectors, where the first one is 
        /// parallel to the original (although normalized), and the second one is perpendicular to the 
        /// first, maintaining the orientation of the first one. 
        /// Returns false if operation could not be processed (both vectors have the same direction)
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="nX"></param>
        /// <param name="nY"></param>
        public static bool Orthogonalize(Vector X, Vector Y, out Vector nX, out Vector nY, out Vector nZ)
        {
            // Some sanity
            int dir = Vector.CompareDirections(X, Y);
            if (dir == 1 || dir == 3)
            {
                Console.WriteLine("Cannot orthogonalize vectors with the same direction");
                nX = null;
                nY = null;
                nZ = null;
                return false;
            }

            nX = new Vector(X);  // shallow copy
            nX.Normalize();

            nZ = Vector.CrossProduct(nX, Y);
            nZ.Normalize();
            nY = Vector.CrossProduct(nZ, nX);

            return true;
        }

        /// <summary>
        /// Simplifies the path using a combination of radial distance and 
        /// Ramer–Douglas–Peucker algorithm. 
        /// </summary>
        /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <param name="highQuality"></param>
        /// <returns></returns>
        internal static List<Vector> SimplifyPointList(List<Vector> points, double tolerance, bool highQuality)
        {
            if (points.Count < 1)
            {
                Console.WriteLine("List contains no points.");
            }

            int prev = points.Count;
            List<Vector> simplified;

            double sqTolerance = tolerance * tolerance;

            if (!highQuality)
            {
                simplified = Vector.SimplifyRadialDistance(points, sqTolerance);
                simplified = Vector.SimplifyDouglasPeucker(simplified, sqTolerance);
            }
            else
            {
                simplified = SimplifyDouglasPeucker(points, sqTolerance);
            }

            Console.WriteLine("Vector list simplified from " + prev + " to " + simplified.Count + " Points.");

            return simplified;
        }

        /// <summary>
        /// The Ramer-Douglas-Peucker algorithm.
        /// </summary>
        /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
        /// <param name="points"></param>
        /// <param name="sqTolerance"></param>
        /// <returns></returns>
        internal static List<Vector> SimplifyDouglasPeucker(List<Vector> points, double sqTolerance)
        {
            var len = points.Count;
            var markers = new int?[len];
            int? first = 0;
            int? last = len - 1;
            int? index = 0;
            var stack = new List<int?>();
            var newPoints = new List<Vector>();

            markers[first.Value] = markers[last.Value] = 1;

            while (last != null)
            {
                var maxSqDist = 0.0d;

                for (int? i = first + 1; i < last; i++)
                {
                    var sqDist = Vector.SqSegmentDistance(points[i.Value], points[first.Value], points[last.Value]);

                    if (sqDist > maxSqDist)
                    {
                        index = i;
                        maxSqDist = sqDist;
                    }
                }

                if (maxSqDist > sqTolerance)
                {
                    markers[index.Value] = 1;
                    stack.AddRange(new[] { first, index, index, last });
                }

                if (stack.Count > 0)
                {
                    last = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                {
                    last = null;
                }

                if (stack.Count > 0)
                {
                    first = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                {
                    first = null;
                }
            }

            for (int i = 0; i < len; i++)
            {
                if (markers[i] != null)
                {
                    newPoints.Add(points[i]);
                }
            }

            return newPoints;
        }

        /// <summary>
        /// Simple distance-based simplification. Consecutive points under 
        /// threshold distance are removed. 
        /// </summary>
        /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
        /// <param name="points"></param>
        /// <param name="sqTolerance"></param>
        /// <returns></returns>
        internal static List<Vector> SimplifyRadialDistance(List<Vector> points, double sqTolerance)
        {
            Vector prevPoint = points[0];
            List<Vector> newPoints = new List<Vector> { prevPoint };
            Vector pt = null;

            for (int i = 1; i < points.Count; i++)
            {
                pt = points[i];

                if (Vector.SqDistance(pt, prevPoint) > sqTolerance)
                {
                    newPoints.Add(pt);
                    prevPoint = pt;
                }
            }

            // Add the last frame of the path (?)
            if (pt != null && !prevPoint.Equals(pt))
            {
                newPoints.Add(pt);
            }

            return newPoints;
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
            //if (obj.GetType() != typeof(Vector) && obj.GetType() != typeof(Vector))
            if (obj.GetType() != typeof(Vector))
                return false;
            return Equals(obj as Vector);
        }

        /// <summary>
        /// Equality checks.
        /// </summary>
        /// <ref>https://github.com/imshz/simplify-net</ref>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Equals(Vector other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            //return other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
            return Math.Abs(X - other.X) < EPSILON &&
                   Math.Abs(Y - other.Y) < EPSILON &&
                   Math.Abs(Z - other.Z) < EPSILON;
        }

        public override string ToString()
        {
            return this.ToString(false);
        }

        public string ToString(bool labels)
        {
            return string.Format("{0}[{1}{2}, {3}{4}, {5}{6}]",
                labels ? "Vector" : "",
                labels ? "X:" : "",
                Math.Round(X, STRING_ROUND_DECIMALS_MM),
                labels ? "Y:" : "",
                Math.Round(Y, STRING_ROUND_DECIMALS_MM),
                labels ? "Z:" : "",
                Math.Round(Z, STRING_ROUND_DECIMALS_MM));
        }
    }
}
