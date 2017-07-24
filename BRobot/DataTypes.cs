using System;
using System.Collections.Generic;


//██████╗  █████╗ ████████╗ █████╗ ████████╗██╗   ██╗██████╗ ███████╗███████╗
//██╔══██╗██╔══██╗╚══██╔══╝██╔══██╗╚══██╔══╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔════╝
//██║  ██║███████║   ██║   ███████║   ██║    ╚████╔╝ ██████╔╝█████╗  ███████╗
//██║  ██║██╔══██║   ██║   ██╔══██║   ██║     ╚██╔╝  ██╔═══╝ ██╔══╝  ╚════██║
//██████╔╝██║  ██║   ██║   ██║  ██║   ██║      ██║   ██║     ███████╗███████║
//╚═════╝ ╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝   ╚═╝      ╚═╝   ╚═╝     ╚══════╝╚══════╝
/// <summary>
/// A bunch of utility classes mostly representing geometry and robot instructions.
/// </summary>


namespace BRobot
{
    public abstract class Geometry
    {
        /// <summary>
        /// Precision for floating-point comparisons.
        /// </summary>
        internal static readonly double EPSILON = 0.000001;

        /// <summary>
        /// Amount of digits for floating-point comparisons precision.
        /// </summary>
        internal static readonly int EPSILON_DECIMALS = 10;

        /// <summary>
        /// Amount of decimals for rounding on ToString() operations.
        /// </summary>
        internal static readonly int STRING_ROUND_DECIMALS_MM = 3;
        internal static readonly int STRING_ROUND_DECIMALS_DEGS = 3;
        internal static readonly int STRING_ROUND_DECIMALS_RADS = 6;

        // Angle conversion
        internal static readonly double TO_DEGS = 180.0 / Math.PI;
        internal static readonly double TO_RADS = Math.PI / 180.0;

        internal static readonly double TAU = 2 * Math.PI;



        //  ╦ ╦╔╦╗╦╦  ╦╔╦╗╦ ╦  ╔═╗╦ ╦╔╗╔╔═╗╔╦╗╦╔═╗╔╗╔╔═╗
        //  ║ ║ ║ ║║  ║ ║ ╚╦╝  ╠╣ ║ ║║║║║   ║ ║║ ║║║║╚═╗
        //  ╚═╝ ╩ ╩╩═╝╩ ╩  ╩   ╚  ╚═╝╝╚╝╚═╝ ╩ ╩╚═╝╝╚╝╚═╝
        public static double Length(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        public static double Length(double x, double y, double z)
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        public static double Length(double w, double x, double y, double z)
        {
            return Math.Sqrt(w * w + x * x + y * y + z * z);
        }

        public static void Normalize(double x, double y, double z,
            out double newX, out double newY, out double newZ)
        {
            double len = Length(x, y, z);
            newX = x / len;
            newY = y / len;
            newZ = z / len;
        }

        internal static Random rnd = new System.Random();

        public static double Random(double min, double max)
        {
            return Lerp(min, max, rnd.NextDouble());
        }

        public static double Random()
        {
            return Random(0, 1);
        }

        public static double Random(double max)
        {
            return Random(0, max);
        }

        public static int RandomInt(int min, int max)
        {
            return rnd.Next(min, max + 1);
        }

        public static double Lerp(double start, double end, double norm)
        {
            return start + (end - start) * norm;
        }

        public static double Normalize(double value, double start, double end)
        {
            return (value - start) / (end - start);
        }

        public static double Map(double value, double sourceStart, double sourceEnd, double targetStart, double targetEnd)
        {
            //double n = Normalize(value, sourceStart, sourceEnd);
            //return targetStart + n * (targetEnd - targetStart);
            return targetStart + (targetEnd - targetStart) * (value - sourceStart) / (sourceEnd - sourceStart);
        }


    }

    //██████╗  ██████╗ ██╗███╗   ██╗████████╗
    //██╔══██╗██╔═══██╗██║████╗  ██║╚══██╔══╝
    //██████╔╝██║   ██║██║██╔██╗ ██║   ██║   
    //██╔═══╝ ██║   ██║██║██║╚██╗██║   ██║   
    //██║     ╚██████╔╝██║██║ ╚████║   ██║   
    //╚═╝      ╚═════╝ ╚═╝╚═╝  ╚═══╝   ╚═╝                                  
    /// <summary>
    /// Represents three coordinates in space.
    /// </summary>
    public class Point : Geometry
    {
       
        public double X, Y, Z;
        

        public static bool operator ==(Point p1, Point p2) {
            return Math.Abs(p1.X - p2.X) < EPSILON
                && Math.Abs(p1.Y - p2.Y) < EPSILON
                && Math.Abs(p1.Z - p2.Z) < EPSILON;
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) > EPSILON
                || Math.Abs(p1.Y - p2.Y) > EPSILON
                || Math.Abs(p1.Z - p2.Z) > EPSILON;
        }

        /// <summary>
        /// Create a Point from its XYZ coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Point(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Creates a shallow copy of the specified Point.
        /// </summary>
        /// <param name="p"></param>
        public Point(Point p)
        {
            this.X = p.X;
            this.Y = p.Y;
            this.Z = p.Z;
        }

        public Point()
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
        /// Shallow-copies the values of specified Point.
        /// </summary>
        /// <param name="p"></param>
        public void Set(Point p)
        {
            this.X = p.X;
            this.Y = p.Y;
            this.Z = p.Z;
        }

        /// <summary>
        /// Add specified values to this Point.
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
        /// Add the coordinates of specified Point to this one.
        /// </summary>
        /// <param name="p"></param>
        public void Add(Point p)
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
        public double LengthSq()
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
            double zero = Math.Abs(this.Length() - 1);
            return zero < EPSILON;
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
        /// Rotates this Point by speficied Quaterion.
        /// </summary>
        /// <param name="r"></param>
        public bool Rotate(Rotation r)
        {
            if (!r.IsUnit())
            {
                Console.WriteLine("Please use unit Quaternions to perform rotations");
                return false;
            }

            // P.out = Q * P.in * conj(Q);
            // From gl-matrix.js
            double ix =  r.W * X + r.Y * Z - r.Z * Y,
                   iy =  r.W * Y + r.Z * X - r.X * Z,
                   iz =  r.W * Z + r.X * Y - r.Y * X,
                   iw = -r.X * X - r.Y * Y - r.Z * Z;

            this.X = ix * r.W - iw * r.X - iy * r.Z + iz * r.Y;
            this.Y = iy * r.W - iw * r.Y - iz * r.X + ix * r.Z;
            this.Z = iz * r.W - iw * r.Z - ix * r.Y + iy * r.X;

            return true;
        }

        /// <summary>
        /// Rotates this Point specified degrees around specified vector. 
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="angDegs"></param>
        /// <returns></returns>
        public bool Rotate(Point vec, double angDegs)
        {
            Rotation r = new Rotation(vec, angDegs);
            return this.Rotate(r);
        }

        /// <summary>
        /// Rotates this Point specified degrees around specified vector. 
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="angDegs"></param>
        /// <returns></returns>
        public bool Rotate(double vecX, double vecY, double vecZ, double angDegs)
        {
            Rotation r = new Rotation(new Point(vecX, vecY, vecZ), angDegs);
            return this.Rotate(r);
        }











        /// <summary>
        /// Unit X Vector.
        /// </summary>
        public static Point XAxis = new Point(1, 0, 0);

        /// <summary>
        /// Unit Y Vector.
        /// </summary>
        public static Point YAxis = new Point(0, 1, 0);

        /// <summary>
        /// Unit Z Vector.
        /// </summary>
        public static Point ZAxis = new Point(0, 0, 1);

        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        public static Point operator -(Point p)
        {
            return new Point(-p.X, -p.Y, -p.Z);
        }

        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        public static Point operator *(Double s, Point p)
        {
            return new Point(s * p.X, s * p.Y, s * p.Z);
        }

        public static Point operator *(Point p, Double s)
        {
            return new Point(s * p.X, s * p.Y, s * p.Z);
        }

        /// <summary>
        /// Returns the dot product of specified Vectors. 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double operator *(Point p1, Point p2)
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
        public static double DotProduct(Point p1, Point p2)
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
        public static double AngleBetween(Point p1, Point p2)
        {
            // A · B = ||A|| * ||B|| * cos(ang);
            double lens = p1.Length() * p2.Length();

            if (lens < EPSILON)
                return 0.5 * Math.PI;
            
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
        public static Point CrossProduct(Point p1, Point p2)
        {
            return new Point(
                p1.Y * p2.Z - p1.Z * p2.Y,
                p1.Z * p2.X - p1.X * p2.Z,
                p1.X * p2.Y - p1.Y * p2.X);
        }

        /// <summary>
        /// Returns a unit Vector orthogonal to specified guiding Vector, contained
        /// in the plane defined by guiding Vector and Point. The direction of the 
        /// resulting Vector will be on the side of the guiding Point.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Point OrthogonalTo(Point vec, Point p)
        {
            return new CoordinateSystem(vec, p).YAxis;
        }

        /// <summary>
        /// Returns the distance between two Points.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double Distance(Point p1, Point p2)
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
        public static double SqDistance(Point p1, Point p2)
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
        public static bool AreParallel(Point vec1, Point vec2)
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
        public static bool AreOrthogonal(Point vec1, Point vec2)
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
        public static bool AreOpposite(Point vec1, Point vec2)
        {
            double alpha = AngleBetween(vec1, vec2);
            return alpha < Math.PI + EPSILON && alpha > Math.PI - EPSILON;
        }

        /// <summary>
        /// Compares the directions of two vectors, regardless of their magnitude.
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns>1 if parallel (same direction), 2 if orthogonal (perpendicular
        /// directions), 3 if opposite (parallel in opposite directions), 
        /// 0 otherwise</returns>
        public static int CompareDirections(Point vec1, Point vec2)
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
        /// Returns a random Point with coordinates between specified double ranges. 
        /// Useful for testing and debugging.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Point RandomDouble(double min, double max)
        {
            return new BRobot.Point(Random(min, max), Random(min, max), Random(min, max));
        }

        /// <summary>
        /// Returns a new Point as the rotation of Point 'p' by Rotation 'r'
        /// </summary>
        /// <param name="p"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Point Rotation(Point p, Rotation r)
        {
            Point v = new Point(p);
            v.Rotate(r);
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
        public static double SqSegmentDistance(Point p, Point p1, Point p2)
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
        /// Simplifies the path using a combination of radial distance and 
        /// Ramer–Douglas–Peucker algorithm. 
        /// </summary>
        /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <param name="highQuality"></param>
        /// <returns></returns>
        public static List<Point> SimplifyPointList(List<Point> points, double tolerance, bool highQuality)
        {
            if (points.Count < 1)
            {
                Console.WriteLine("List contains no points.");
            }

            int prev = points.Count;
            List<Point> simplified;

            double sqTolerance = tolerance * tolerance;

            if (!highQuality)
            {
                simplified = Point.SimplifyRadialDistance(points, sqTolerance);
                simplified = Point.SimplifyDouglasPeucker(simplified, sqTolerance);
            }
            else
            {
                simplified = SimplifyDouglasPeucker(points, sqTolerance);
            }

            Console.WriteLine("Point list simplified from " + prev + " to " + simplified.Count + " Points.");

            return simplified;
        }

        /// <summary>
        /// The Ramer-Douglas-Peucker algorithm.
        /// </summary>
        /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
        /// <param name="points"></param>
        /// <param name="sqTolerance"></param>
        /// <returns></returns>
        public static List<Point> SimplifyDouglasPeucker(List<Point> points, double sqTolerance)
        {
            var len = points.Count;
            var markers = new int?[len];
            int? first = 0;
            int? last = len - 1;
            int? index = 0;
            var stack = new List<int?>();
            var newPoints = new List<Point>();

            markers[first.Value] = markers[last.Value] = 1;

            while (last != null)
            {
                var maxSqDist = 0.0d;

                for (int? i = first + 1; i < last; i++)
                {
                    var sqDist = Point.SqSegmentDistance(points[i.Value], points[first.Value], points[last.Value]);

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
        public static List<Point> SimplifyRadialDistance(List<Point> points, double sqTolerance)
        {
            Point prevPoint = points[0];
            List<Point> newPoints = new List<Point> { prevPoint };
            Point pt = null;

            for (int i = 1; i < points.Count; i++)
            {
                pt = points[i];

                if (Point.SqDistance(pt, prevPoint) > sqTolerance)
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
            //if (obj.GetType() != typeof(Point) && obj.GetType() != typeof(Point))
            if (obj.GetType() != typeof(Point))
                return false;
            return Equals(obj as Point);
        }

        /// <summary>
        /// Equality checks.
        /// </summary>
        /// <ref>https://github.com/imshz/simplify-net</ref>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Equals(Point other)
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
            return string.Format("[{0},{1},{2}]",
                Math.Round(X, STRING_ROUND_DECIMALS_MM),
                Math.Round(Y, STRING_ROUND_DECIMALS_MM),
                Math.Round(Z, STRING_ROUND_DECIMALS_MM));
        }

    }


    //██████╗  ██████╗ ████████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //██╔══██╗██╔═══██╗╚══██╔══╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //██████╔╝██║   ██║   ██║   ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //██╔══██╗██║   ██║   ██║   ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //██║  ██║╚██████╔╝   ██║   ██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    /// <summary>
    /// Represents a rotation using quaternions.
    /// </summary>
    public class Rotation : Geometry
    {
        /// <summary>
        /// The orientation of a global XYZ coordinate system (aka an identity rotation).
        /// </summary>
        public static readonly Rotation GlobalXY = new Rotation(1, 0, 0, 0, true);

        /// <summary>
        /// A global XYZ coordinate system rotated 180 degs around its X axis.
        /// </summary>
        public static readonly Rotation FlippedAroundX = new Rotation(0, 1, 0, 0, true);

        /// <summary>
        /// A global XYZ coordinate system rotated 180 degs around its Y axis. 
        /// Recommended as the easiest orientation for the standard robot end effector to reach in positive XY octants.
        /// </summary>
        public static readonly Rotation FlippedAroundY = new Rotation(0, 0, 1, 0, true);

        /// <summary>
        /// A global XYZ coordinate system rotated 180 degs around its Z axis.
        /// </summary>
        public static readonly Rotation FlippedAroundZ = new Rotation(0, 0, 0, 1, true);

        public double W, X, Y, Z;


        public static Rotation operator + (Rotation r1, Rotation r2)
        {
            return Rotation.Addition(r1, r2);
        }

        public static Rotation operator - (Rotation r1, Rotation r2)
        {
            return Rotation.Subtraction(r1, r2);
        }

        /// <summary>
        /// Returns the <a href="https://en.wikipedia.org/wiki/Quaternion#Hamilton_product">Hamilton product</a> 
        /// of the first quaternion by the second.
        /// Remember quaternion multiplication is non-commutative.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rotation operator * (Rotation r1, Rotation r2)
        {
            return Rotation.Multiply(r1, r2);
        }


        /// <summary>
        /// Create a Rotation object from its Quaternion parameters: 
        /// w + x * i + y * j + z * k. 
        /// NOTE: it is very unlikely that any public user will input 
        /// direct quaternion values when specifying rotations, and this
        /// signature could be better used for vector + angle value.
        /// Changed this to internal, and adding a public vectorXYZ + ang signature.
        /// A public static Rotation.FromQuaterion() is added for 
        /// advanced users.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        internal Rotation(double w, double x, double y, double z, bool quaternionRepresentation)
        {
            if (quaternionRepresentation)
            {
                this.W = w;
                this.X = x;
                this.Y = y;
                this.Z = z;
            }
            else
            {
                // TODO: this is awful, must be a better way of choosing between two constructors...
                Rotation r = new Rotation(w, x, y, z);  // use wxyz as vector + ang representation
                this.W = r.W;
                this.X = r.X;
                this.Y = r.Y;
                this.Z = r.Z;
            } 
            
        }

        /// <summary>
        /// Creates a unit Quaternion representing a rotation of n degrees around a
        /// vector, with right-hand positive convention.
        /// </summary>
        /// <param name="vecX"></param>
        /// <param name="vecY"></param>
        /// <param name="vecZ"></param>
        /// <param name="angDegs"></param>
        public Rotation(double vecX, double vecY, double vecZ, double angDegs)
        {
            double halfAngRad = 0.5 * TO_RADS * angDegs;   // ang / 2
            double s = Math.Sin(halfAngRad);
            Point u = new Point(vecX, vecY, vecZ);
            u.Normalize();  // rotation quaternion must be unit

            this.W = Math.Cos(halfAngRad);
            this.X = s * u.X;
            this.Y = s * u.Y;
            this.Z = s * u.Z;
        }

        /// <summary>
        /// Creates a unit Quaternion representing a rotation of n degrees around a 
        /// vector, with right-hand positive convention.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="angDegs"></param>
        public Rotation(Point vec, double angDegs) : this(vec.X, vec.Y, vec.Z, angDegs) { }
        

        /// <summary>
        /// A static constructor for Rotation objects from their Quaternion representation.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Rotation FromQuaternion(double w, double x, double y, double z)
        {
            return new Rotation(w, x, y, z, true);
        }
         
        /// <summary>
        /// Create a Rotation object from a CoordinateSystem defined by
        /// the coordinates of its main X vector and the coordiantes of 
        /// a guiding Y vector.
        /// Vectors don't need to be normalized or orthogonal, the constructor 
        /// will generate the best-fitting CoordinateSystem with this information. 
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y0"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        public Rotation(double x0, double x1, double x2, double y0, double y1, double y2) :
            this(new CoordinateSystem(x0, x1, x2, y0, y1, y2).GetQuaternion())
        { }

        /// <summary>
        /// Create a Rotation object from a CoordinateSystem defined by
        /// the main Vector X and the guiding Vector Y.
        /// Vectors don't need to be normalized or orthogonal, the constructor 
        /// will generate the best-fitting CoordinateSystem with this information.
        /// </summary>
        /// <param name="vecX"></param>
        /// <param name="vecY"></param>
        public Rotation(Point vecX, Point vecY) :
            this(new CoordinateSystem(vecX, vecY).GetQuaternion())
        { }

        /// <summary>
        /// Create a Rotation object from a CoordinateSystem defined by
        /// the coordinates of its main X vector and the coordiantes of 
        /// a guiding Y vector.
        /// Vectors don't need to be normalized or orthogonal, the constructor 
        /// will generate the best-fitting CoordinateSystem with this information.
        /// </summary>
        /// <param name="cs"></param>
        public Rotation(CoordinateSystem cs) :
            this(cs.GetQuaternion())
        { }

        /// <summary>
        /// Creates a new Rotation as a shallow copy of the passed one.
        /// </summary>
        /// <param name="r"></param>
        public Rotation(Rotation r)
        {
            this.W = r.W;
            this.X = r.X;
            this.Y = r.Y;
            this.Z = r.Z;
        }
        
        /// <summary>
        /// Creates an identity Quaternion (no rotation).
        /// </summary>
        public Rotation()
        {
            this.W = 1;
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }

        /// <summary>
        /// Sets the values of this Quaternion's components.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set(double w, double x, double y, double z)
        {
            this.W = w;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Shallow-copies the values of specified Quaternion.
        /// </summary>
        /// <param name="r"></param>
        public void Set(Rotation r)
        {
            this.W = r.W;
            this.X = r.X;
            this.Y = r.Y;
            this.Z = r.Z;
        }



        /// <summary>
        /// Returns the length (norm) of this Quaternion.
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return Math.Sqrt(W * W + X * X + Y * Y + Z * Z);
        }

        /// <summary>
        /// Returns the square length of this Quaternion.
        /// </summary>
        /// <returns></returns>
        public double SqLength()
        {
            return W * W + X * X + Y * Y + Z * Z;
        }

        /// <summary>
        /// Turns this Quaternion into a <a href="https://en.wikipedia.org/wiki/Versor">Versor</a> (unit length quaternion).
        /// </summary>
        public void Normalize()
        {
            double len = this.Length();
            this.W /= len;
            this.X /= len;
            this.Y /= len;
            this.Z /= len;
        }

        /// <summary>
        /// Is this a unit length quaternion?
        /// </summary>
        /// <returns></returns>
        public bool IsUnit()
        {
            double zero = Math.Abs(this.SqLength() - 1);
            return zero < EPSILON;
        }

        /// <summary>
        /// Is this a zero length quaternion?
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            double sqlen = this.SqLength();
            return Math.Abs(sqlen) > EPSILON;
        }

        /// <summary>
        /// Add a Quaternion to this one. 
        /// </summary>
        /// <param name="r"></param>
        public void Add(Rotation r)
        {
            this.W += r.W;
            this.X += r.X;
            this.Y += r.Y;
            this.Z += r.Z;
        }

        /// <summary>
        /// Returns the addition of two quaternions.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rotation Addition(Rotation r1, Rotation r2)
        {
            return new Rotation(
                r1.W + r2.W,
                r1.X + r2.X,
                r1.Y + r2.Y,
                r1.Z + r2.Z, true);
        }
        
        /// <summary>
        /// Subtract a quaternion from this one. 
        /// </summary>
        /// <param name="r"></param>
        public void Subtract(Rotation r)
        {
            this.W -= r.W;
            this.X -= r.X;
            this.Y -= r.Y;
            this.Z -= r.Z;
        }

        /// <summary>
        /// Returns the subtraction of two quaternions.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rotation Subtraction(Rotation r1, Rotation r2)
        {
            return new Rotation(
                r1.W - r2.W,
                r1.X - r2.X,
                r1.Y - r2.Y,
                r1.Z - r2.Z, true);
        }

        /// <summary>
        /// Multiply this Quaternion by the specified one, a.k.a. this = this * r. 
        /// Conceptually, this means that a Rotation 'r' in Local coordinates is applied 
        /// to this Rotation.
        /// See https://en.wikipedia.org/wiki/Quaternion#Hamilton_product
        /// </summary>
        /// <param name="r"></param>
        public void Multiply(Rotation r)
        {
            double w = this.W,
                   x = this.X,
                   y = this.Y,
                   z = this.Z;

            this.W = w * r.W - x * r.X - y * r.Y - z * r.Z;
            this.X = x * r.W + w * r.X + y * r.Z - z * r.Y;
            this.Y = y * r.W + w * r.Y + z * r.X - x * r.Z;
            this.Z = z * r.W + w * r.Z + x * r.Y - y * r.X;
        }

        /// <summary>
        /// Premultiplies this Quaternion by the specified one, a.k.a. this = r * this. 
        /// Conceptually, this means that a Rotation 'r' in Global coordinates is applied 
        /// to this Rotation.
        /// See https://en.wikipedia.org/wiki/Quaternion#Hamilton_product
        /// </summary>
        /// <param name="r"></param>
        public void PreMultiply(Rotation r)
        {
            double w = this.W,
                   x = this.X,
                   y = this.Y,
                   z = this.Z;

            this.W = r.W * w - r.X * x - r.Y * y - r.Z * z;
            this.X = r.X * w + r.W * x + r.Y * z - r.Z * y;
            this.Y = r.Y * w + r.W * y + r.Z * x - r.X * z;
            this.Z = r.Z * w + r.W * z + r.X * y - r.Y * x;
        }

        /// <summary>
        /// Returns the <a href="https://en.wikipedia.org/wiki/Quaternion#Hamilton_product">Hamilton product</a> 
        /// of the first quaternion by the second.
        /// Remember quaternion multiplication is non-commutative.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rotation Multiply(Rotation r1, Rotation r2)
        {
            double w = r1.W * r2.W - r1.X * r2.X - r1.Y * r2.Y - r1.Z * r2.Z;
            double x = r1.X * r2.W + r1.W * r2.X + r1.Y * r2.Z - r1.Z * r2.Y;
            double y = r1.Y * r2.W + r1.W * r2.Y + r1.Z * r2.X - r1.X * r2.Z;
            double z = r1.Z * r2.W + r1.W * r2.Z + r1.X * r2.Y - r1.Y * r2.X;

            return new Rotation(w, x, y, z, true);
        }

        /// <summary>
        /// Divide this Quaternion by another one. 
        /// In reality, this quaternion is post-multiplied by the inverse of the provided one.
        /// </summary>
        /// <param name="r"></param>
        public void Divide(Rotation r)
        {
            Rotation arg = new Rotation(r);
            arg.Invert();
            this.Multiply(arg);
        }

        /// <summary>
        /// Returns the division of r1 by r2.
        /// Under the hood, r1 is post-multiplied by the inverse of r2.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rotation Division(Rotation r1, Rotation r2)
        {
            Rotation a = new Rotation(r1),
                     b = new Rotation(r2);
            b.Invert();
            a.Multiply(b);
            return a;
        }

        /// <summary>
        /// Turns this Rotation into its conjugate.
        /// </summary>
        public void Conjugate()
        {
            // W stays the same
            this.X = -this.X;
            this.Y = -this.Y;
            this.Z = -this.Z;
        }

        /// <summary>
        /// Returns the conjugate of given quaternion.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Rotation Conjugate(Rotation r)
        {
            return new Rotation(r.W, -r.X, -r.Y, -r.Z, true);
        }

        /// <summary>
        /// Inverts this quaternion.
        /// </summary>
        public void Invert()
        {
            if (this.IsUnit())
            {
                // The inverse of unit vectors is its conjugate
                this.X = -this.X;
                this.Y = -this.Y;
                this.Z = -this.Z;
            }
            else if (this.IsZero())
            {
                // The inverse of a zero quat is itself
            }
            else
            {
                // The inverse of a quaternion is its conjugate divided by the sqaured norm.
                double sqlen = this.SqLength();

                this.W /= sqlen;
                this.X /= -sqlen;
                this.Y /= -sqlen;
                this.Z /= -sqlen;
            }
        }

        /// <summary>
        /// Returns a CoordinateSystem representation of current Quaternion
        /// (a 3x3 rotation matrix).
        /// </summary>
        /// <seealso cref="https://en.wikipedia.org/wiki/Rotation_formalisms_in_three_dimensions#Conversion_formulae_between_formalisms"/>
        /// <returns></returns>
        public CoordinateSystem GetCoordinateSystem()
        {
            // From gl-matrix.js
            double x2 = this.X + this.X,
                   y2 = this.Y + this.Y,
                   z2 = this.Z + this.Z;

            double xx2 = this.X * x2,
                   yx2 = this.Y * x2,
                   yy2 = this.Y * y2,
                   zx2 = this.Z * x2,
                   zy2 = this.Z * y2,
                   zz2 = this.Z * z2,
                   wx2 = this.W * x2,
                   wy2 = this.W * y2,
                   wz2 = this.W * z2;

            // @TODO: review the order of these elements, they might be transposed for webgl conventions
            return CoordinateSystem.FromComponents(
                1 - yy2 - zz2,     yx2 + wz2,     zx2 - wy2,
                    yx2 - wz2, 1 - xx2 - zz2,     zy2 + wx2,
                    zx2 + wy2,     zy2 - wx2, 1 - xx2 - yy2);
        }

        /// <summary>
        /// Returns the inverse of given quaternion.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Rotation Inverse(Rotation r)
        {
            Rotation rot = new Rotation(r);
            rot.Invert();
            return rot;
        }

        /// <summary>
        /// Returns the rotation vector in axis-angle representation, 
        /// i.e. the unit vector defining the rotation axis multiplied by the 
        /// angle rotation scalar in degrees.
        /// https://en.wikipedia.org/wiki/Axis%E2%80%93angle_representation
        /// </summary>
        /// <returns></returns>
        public Point GetRotationVector(bool radians)
        {
            Point axisAng = GetRotationAxis() * GetRotationAngle();
            if (radians)
            {
                axisAng.Scale(TO_RADS);
            }

            return axisAng;
        }

        public Point GetRotationVector()
        {
            return GetRotationVector(false);
        }


        /// <summary>
        /// Returns the rotation axis represented by this Quaternion. 
        /// Note it will always return the unit vector corresponding to a positive rotation, 
        /// even if the quaternion was created from a negative one (flipped vector).
        /// </summary>
        /// <returns></returns>
        public Point GetRotationAxis()
        {
            double theta2 = 2 * Math.Acos(W);

            // If angle == 0, no rotation is performed and this Quat is identity
            if (theta2 < EPSILON)
            {
                return new Point();
            }

            double s = Math.Sin(0.5 * theta2);
            return new Point(this.X / s, this.Y / s, this.Z / s);
        }

        /// <summary>
        /// Returns the rotation angle represented by this Quaternion in degrees.
        /// Note it will always yield the positive rotation.
        /// </summary>
        /// <returns></returns>
        public double GetRotationAngle()
        {
            double theta2 = 2 * Math.Acos(W);
            return theta2 < EPSILON ? 0 : Math.Round(theta2 * TO_DEGS, EPSILON_DECIMALS);
        }


        /// <summary>
        /// Rotate this Quaternion by specified Rotation around GLOBAL reference system.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public void RotateGlobal(Rotation r)
        {
            this.PreMultiply(r);
        }

        /// <summary>
        /// Rotate this Quaternion by specified Rotation around LOCAL reference system.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public void RotateLocal(Rotation r)
        {
            this.Multiply(r);
        }

        /// <summary>
        /// Returns the Euler angle representation of this Quaternion in Tait-Bryant convention (ZY'X'' order, aka intrinsic ZYX). Please note each axis rotation is stored in its own XYZ parameter. So for example, to convert this to the ABC representation used by KUKA, ABC maps to the vector's ZYX.
        /// https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#Conversion
        /// </summary>
        /// <returns></returns>
        public Point ToEulerZYX()
        {
            Point eu = new Point();

            double y2, t0, t1, t2, t3, t4;

            y2 = Y * Y;

            t0 = 2 * (W * X + Y * Z);
            t1 = 1 - 2 * (X * X + y2);
            eu.X = Math.Atan2(t0, t1) * TO_DEGS;

            t2 = 2 * (W * Y - Z * X);
            t2 = t2 > 1 ? 1 : t2;
            t2 = t2 < -1 ? -1 : t2;
            eu.Y = Math.Asin(t2) * TO_DEGS;

            t3 = 2 * (W * Z + X * Y);
            t4 = 1 - 2 * (y2 + Z * Z);
            eu.Z = Math.Atan2(t3, t4) * TO_DEGS;

            return eu;
        }

        



        ///// @DEPRECATED
        ///// <summary>
        ///// Borrowed from DynamoTORO
        ///// </summary>
        ///// <param name="XAxis"></param>
        ///// <param name="YAxis"></param>
        ///// <param name="Normal"></param>
        ///// <returns></returns>
        //public static List<double> PlaneToQuaternion(double x1, double x2, double x3, double y1, double y2, double y3, double z1, double z2, double z3)
        //{
        //    double s, trace;
        //    double q1, q2, q3, q4;

        //    trace = x1 + y2 + z3 + 1;

        //    //if (trace > 0.00001)
        //    if (trace > EPSILON)
        //    {
        //        // s = (trace) ^ (1 / 2) * 2
        //        s = Math.Sqrt(trace) * 2;
        //        q1 = s / 4;
        //        q2 = (-z2 + y3) / s;
        //        q3 = (-x3 + z1) / s;
        //        q4 = (-y1 + x2) / s;

        //    }
        //    else if (x1 > y2 && x1 > z3)
        //    {
        //        //s = (x1 - y2 - z3 + 1) ^ (1 / 2) * 2
        //        s = Math.Sqrt(x1 - y2 - z3 + 1) * 2;
        //        q1 = (z2 - y3) / s;
        //        q2 = s / 4;
        //        q3 = (y1 + x2) / s;
        //        q4 = (x3 + z1) / s;

        //    }
        //    else if (y2 > z3)
        //    {
        //        //s = (-x1 + y2 - z3 + 1) ^ (1 / 2) * 2
        //        s = Math.Sqrt(-x1 + y2 - z3 + 1) * 2;
        //        q1 = (x3 - z1) / s;
        //        q2 = (y1 + x2) / s;
        //        q3 = s / 4;
        //        q4 = (z2 + y3) / s;
        //    }

        //    else
        //    {
        //        //s = (-x1 - y2 + z3 + 1) ^ (1 / 2) * 2
        //        s = Math.Sqrt(-x1 - y2 + z3 + 1) * 2;
        //        q1 = (y1 - x2) / s;
        //        q2 = (x3 + z1) / s;
        //        q3 = (z2 + y3) / s;
        //        q4 = s / 4;

        //    }

        //    List<double> quatDoubles = new List<double>();
        //    quatDoubles.Add(q1);
        //    quatDoubles.Add(q2);
        //    quatDoubles.Add(q3);
        //    quatDoubles.Add(q4);
        //    return quatDoubles;

        //    //return new Rotation(q1, q2, q3, q4);
        //}


        public override string ToString()
        {
            return string.Format("[{0},{1},{2},{3}]",
                Math.Round(W, STRING_ROUND_DECIMALS_RADS),
                Math.Round(X, STRING_ROUND_DECIMALS_RADS),
                Math.Round(Y, STRING_ROUND_DECIMALS_RADS),
                Math.Round(Z, STRING_ROUND_DECIMALS_RADS));
        }

    }


    //     ██╗ ██████╗ ██╗███╗   ██╗████████╗███████╗
    //     ██║██╔═══██╗██║████╗  ██║╚══██╔══╝██╔════╝
    //     ██║██║   ██║██║██╔██╗ ██║   ██║   ███████╗
    //██   ██║██║   ██║██║██║╚██╗██║   ██║   ╚════██║
    //╚█████╔╝╚██████╔╝██║██║ ╚████║   ██║   ███████║
    // ╚════╝  ╚═════╝ ╚═╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝
    /// <summary>
    /// Represents the 6 angular rotations of the axes in a 6-axis manipulator, in degrees.
    /// </summary>
    public class Joints : Geometry
    {
        public double J1, J2, J3, J4, J5, J6;

        public double this[int i]
        {   
            get
            {
                if (i < 0 || i > 5)
                {
                    throw new IndexOutOfRangeException();
                }
                switch (i)
                {
                    case 0: return J1;
                    case 1: return J2;
                    case 2: return J3;
                    case 3: return J4;
                    case 4: return J5;
                    case 5: return J6;
                }
                return 0;
            }
            set
            {
                if (i < 0 || i > 5)
                {
                    throw new IndexOutOfRangeException();
                }
                switch (i)
                {
                    case 0: J1 = value; break;
                    case 1: J2 = value; break;
                    case 2: J3 = value; break;
                    case 3: J4 = value; break;
                    case 4: J5 = value; break;
                    case 5: J6 = value; break;
                }
            }
        }


        public Joints()
        {
            this.J1 = 0;
            this.J2 = 0;
            this.J3 = 0;
            this.J4 = 0;
            this.J5 = 0;
            this.J6 = 0;
        }

        /// <summary>
        /// Create a Joints configuration from values.
        /// </summary>
        /// <param name="j1"></param>
        /// <param name="j2"></param>
        /// <param name="j3"></param>
        /// <param name="j4"></param>
        /// <param name="j5"></param>
        /// <param name="j6"></param>
        public Joints(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            this.J1 = j1;
            this.J2 = j2;
            this.J3 = j3;
            this.J4 = j4;
            this.J5 = j5;
            this.J6 = j6;
        }

        public Joints(Joints j)
        {
            this.J1 = j.J1;
            this.J2 = j.J2;
            this.J3 = j.J3;
            this.J4 = j.J4;
            this.J5 = j.J5;
            this.J6 = j.J6;
        }

        public void Add(Joints j)
        {
            this.J1 += j.J1;
            this.J2 += j.J2;
            this.J3 += j.J3;
            this.J4 += j.J4;
            this.J5 += j.J5;
            this.J6 += j.J6;
        }

        public void Set(Joints j)
        {
            this.J1 = j.J1;
            this.J2 = j.J2;
            this.J3 = j.J3;
            this.J4 = j.J4;
            this.J5 = j.J5;
            this.J6 = j.J6;
        }

        public void Scale(double s)
        {
            this.J1 *= s;
            this.J2 *= s;
            this.J3 *= s;
            this.J4 *= s;
            this.J5 *= s;
            this.J6 *= s;
        }

        /// <summary>
        /// Returns the norm (euclidean length) of this joints as a vector.
        /// </summary>
        /// <returns></returns>
        public double Norm()
        {
            return Math.Sqrt(this.NormSq());
        }

        /// <summary>
        /// Returns the square norm of this joints as a vector.
        /// </summary>
        /// <returns></returns>
        public double NormSq()
        {
            return J1 * J1 + J2 * J2 + J3 * J3 + J4 * J4 + J5 * J5 + J6 * J6;
        }


        public override string ToString()
        {
            return string.Format("[{0},{1},{2},{3},{4},{5}]",
                Math.Round(J1, STRING_ROUND_DECIMALS_DEGS),
                Math.Round(J2, STRING_ROUND_DECIMALS_DEGS),
                Math.Round(J3, STRING_ROUND_DECIMALS_DEGS),
                Math.Round(J4, STRING_ROUND_DECIMALS_DEGS),
                Math.Round(J5, STRING_ROUND_DECIMALS_DEGS),
                Math.Round(J6, STRING_ROUND_DECIMALS_DEGS));
        }

    }





    //   ██████╗███████╗██╗   ██╗███████╗
    //  ██╔════╝██╔════╝╚██╗ ██╔╝██╔════╝
    //  ██║     ███████╗ ╚████╔╝ ███████╗
    //  ██║     ╚════██║  ╚██╔╝  ╚════██║
    //  ╚██████╗███████║   ██║   ███████║
    //   ╚═════╝╚══════╝   ╚═╝   ╚══════╝
    //                                   
    /// <summary>
    /// Represents a Coordinate System composed of a triplet of orthogonal XYZ unit vectors
    /// following right-hand rule orientations. Useful for spatial and rotational orientation
    /// operations. 
    /// </summary>
    public class CoordinateSystem : Geometry
    {
        public Point XAxis, YAxis, ZAxis;

        /// <summary>
        /// Creates a global XYZ reference system.
        /// </summary>
        public CoordinateSystem()
        {
            XAxis = new Point(1, 0, 0);
            YAxis = new Point(0, 1, 0);
            ZAxis = new Point(0, 0, 1);
        }

        /// <summary>
        /// Createa a CoordinateSystem based on the specified guiding Vecots. 
        /// Vectors don't need to be normalized or orthogonal, the constructor 
        /// will generate the best-fitting CoordinateSystem with this information. 
        /// </summary>
        /// <param name="vecX"></param>
        /// <param name="vecY"></param>
        public CoordinateSystem(Point vecX, Point vecY)
        {
            // Some sanity
            if (Point.AreParallel(vecX, vecY))
            {
                throw new Exception("Cannot create a CoordinateSystem with two parallel vectors");
            }

            // Create unit X axis
            XAxis = new Point(vecX);
            XAxis.Normalize();
                       
            // Find normal vector to plane
            ZAxis = Point.CrossProduct(vecX, vecY);
            ZAxis.Normalize();

            // Y axis is the cross product of both
            YAxis = Point.CrossProduct(ZAxis, XAxis);
        }

        /// <summary>
        /// Create a CoordinateSystem based on the specified guiding Vecots. 
        /// Vectors don't need to be normalized or orthogonal, the constructor 
        /// will generate the best-fitting CoordinateSystem with this information. 
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y0"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        public CoordinateSystem(double x0, double x1, double x2, double y0, double y1, double y2) :
            this( new Point(x0, x1, x2), new Point(y0, y1, y2) )
        { }

        /// <summary>
        /// A static constructor that returns a CoordinateSystem from specified vector components. 
        /// It will return null if provided components do not form a valid 3x3 rotation matrix.
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y0"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="z0"></param>
        /// <param name="z1"></param>
        /// <param name="z2"></param>
        /// <returns></returns>
        public static CoordinateSystem FromComponents(double x0, double x1, double x2, double y0, double y1, double y2, double z0, double z1, double z2)
        {
            CoordinateSystem cs = new CoordinateSystem();

            cs.XAxis.Set(x0, x1, x2);
            cs.YAxis.Set(y0, y1, y2);
            cs.ZAxis.Set(z0, z1, z2);

            if (cs.IsValid())
                return cs;

            return null;
        }


        /// <summary>
        /// Returns a Quaternion representation of the current CoordinateSystem. 
        /// </summary>
        /// <returns></returns>
        public Rotation GetQuaternion()
        {
            // From the ABB Rapid manual p.1151
            double w = 0.5 * Math.Sqrt(1 + XAxis.X + YAxis.Y + ZAxis.Z);

            double x = 0.5 * Math.Sqrt(1 + XAxis.X - YAxis.Y - ZAxis.Z)
                * (YAxis.Z - ZAxis.Y >= 0 ? 1 : -1);
            double y = 0.5 * Math.Sqrt(1 - XAxis.X + YAxis.Y - ZAxis.Z)
                * (ZAxis.X - XAxis.Z >= 0 ? 1 : -1);
            double z = 0.5 * Math.Sqrt(1 - XAxis.X - YAxis.Y + ZAxis.Z)
                * (XAxis.Y - YAxis.X >= 0 ? 1 : -1);

            return new Rotation(w, x, y, z, true);
        }

        /// <summary>
        /// Are the three axes unit vectors and orthonormal?
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            bool valid = this.XAxis.IsUnit() && this.YAxis.IsUnit() && this.ZAxis.IsUnit();
            //Console.WriteLine("Unit axes: " + valid);
            //if (!valid) Console.WriteLine("{0}-{1} {2}-{3} {4}-{5}", this.XAxis, this.XAxis.IsUnit(), this.YAxis, this.YAxis.IsUnit(), this.ZAxis, this.ZAxis.IsUnit());

            Point z = Point.CrossProduct(this.XAxis, this.YAxis);
            valid = valid && this.ZAxis.Equals(z);
            //Console.WriteLine("Orthonormal axes: " + valid);
            //if (!valid) Console.WriteLine("{0} {1}", this.ZAxis, z);

            return valid;            
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", XAxis, YAxis, ZAxis);
        }

    }


































    //███████╗██████╗  █████╗ ███╗   ███╗███████╗
    //██╔════╝██╔══██╗██╔══██╗████╗ ████║██╔════╝
    //█████╗  ██████╔╝███████║██╔████╔██║█████╗  
    //██╔══╝  ██╔══██╗██╔══██║██║╚██╔╝██║██╔══╝  
    //██║     ██║  ██║██║  ██║██║ ╚═╝ ██║███████╗
    //╚═╝     ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝                                      
    /// <summary>
    /// Represents a location and rotation in 3D space, with some additional
    /// metadata representing speeds, zones, etc.
    /// </summary>
    public class Frame : Geometry
    {
        /// <summary>
        /// This is the default rotation that will be assigned to Frames constructed only with location properties.
        /// </summary>
        public static Rotation DefaultOrientation = Rotation.FlippedAroundY;
        public static double DefaultSpeed = 10;
        public static double DefaultZone = 5;

        public static double DistanceBetween(Frame f1, Frame f2)
        {
            double dx = f2.Position.X - f1.Position.X;
            double dy = f2.Position.Y - f1.Position.Y;
            double dz = f2.Position.Z - f1.Position.Z;

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public Point Position;
        public Rotation Orientation;
        public double Speed;
        public double Zone;

        public Frame(double x, double y, double z)
        {
            this.Position = new Point(x, y, z);
            this.Orientation = DefaultOrientation;
            this.Speed = DefaultSpeed;
            this.Zone = DefaultZone;
        }

        public Frame(double x, double y, double z, double speed, double zone)
        {
            this.Position = new Point(x, y, z);
            this.Orientation = DefaultOrientation;
            this.Speed = speed;
            this.Zone = zone;
        }

        public Frame(double x, double y, double z, double qw, double qx, double qy, double qz)
        {
            this.Position = new Point(x, y, z);
            this.Orientation = new Rotation(qw, qx, qy, qz, true);
            this.Speed = DefaultSpeed;
            this.Zone = DefaultZone;
        }

        public Frame(double x, double y, double z, double qw, double qx, double qy, double qz, double speed, double zone)
        {
            this.Position = new Point(x, y, z);
            this.Orientation = new Rotation(qw, qx, qy, qz, true);
            this.Speed = speed;
            this.Zone = zone;
        }

        public Frame(Point position)
        {
            this.Position = new Point(position.X, position.Y, position.Z);  // shallow copy
            this.Orientation = DefaultOrientation;
            this.Speed = DefaultSpeed;
            this.Zone = DefaultZone;
        }

        public Frame(Point position, double speed, double zone)
        {
            this.Position = new Point(position.X, position.Y, position.Z);  // shallow copy
            this.Orientation = DefaultOrientation;
            this.Speed = speed;
            this.Zone = zone;
        }

        public Frame(Point position, Rotation orientation)
        {
            this.Position = new Point(position.X, position.Y, position.Z);  // shallow copy
            this.Orientation = new Rotation(orientation.W, orientation.X, orientation.Y, orientation.Z, true);  // shallow copy
            this.Speed = DefaultSpeed;
            this.Zone = DefaultZone;
        }

        public Frame(Point position, Rotation orientation, double speed, double zone)
        {
            this.Position = new Point(position.X, position.Y, position.Z);  // shallow copy
            this.Orientation = new Rotation(orientation.W, orientation.X, orientation.Y, orientation.Z, true);  // shallow copy
            this.Speed = speed;
            this.Zone = zone;
        }
        
        public string GetPositionDeclaration()
        {
            return string.Format("[{0},{1},{2}]", Position.X, Position.Y, Position.Z);
        }

        public string GetOrientationDeclaration()
        {
            return string.Format("[{0},{1},{2},{3}]", Orientation.W, Orientation.X, Orientation.Y, Orientation.Z);
        }

        /// <summary>
        /// WARNING: This library still doesn't perform IK calculation, and will always use
        /// a default [0,0,0,0] axis configuration.
        /// </summary>
        /// <returns></returns>
        public string GetUNSAFEConfigurationDeclaration()
        {
            return string.Format("[{0},{1},{2},{3}]", 0, 0, 0, 0);
        }

        /// <summary>
        /// WARNING: no external axes are taken into account here... 
        /// </summary>
        /// <returns></returns>
        public string GetExternalAxesDeclaration()
        {
            return "[9E9,9E9,9E9,9E9,9E9,9E9]";
        }

        public string GetSpeedDeclaration()
        {
            return string.Format("[{0},{1},{2},{3}]", Speed, Speed, 5000, 1000);  // default speed declarations in ABB always use 500 deg/s as rot speed, but it feels too fast (and scary). using the same value as lin motion
        }

        public string GetZoneDeclaration()
        {
            double high = 1.5 * Zone;
            double low = 0.15 * Zone;
            return string.Format("[FALSE,{0},{1},{2},{3},{4},{5}]", Zone, high, high, low, high, low);
        }

        /// <summary>
        /// WARNING: This library still doesn't perform IK calculation, and will always return
        /// a default [0,0,0,0] axis configuration.
        /// </summary>
        /// <returns></returns>
        public string GetUNSAFERobTargetDeclaration()
        {
            return string.Format("[{0},{1},{2},{3}]", 
                GetPositionDeclaration(),
                GetOrientationDeclaration(),
                GetUNSAFEConfigurationDeclaration(),
                GetExternalAxesDeclaration()
            );
        }

        public void FlipXY()
        {
            double x = this.Position.X;
            this.Position.X = this.Position.Y;
            this.Position.Y = x;
        }

        public void FlipYZ()
        {
            double y = this.Position.Y;
            this.Position.Y = this.Position.Z;
            this.Position.Z = y;
        }

        public void FlipXZ()
        {
            double x = this.Position.X;
            this.Position.X = this.Position.Z;
            this.Position.Z = x;
        }

        public void ReverseX()
        {
            this.Position.X = -this.Position.X;
        }

        public void ReverseY()
        {
            this.Position.Y = -this.Position.Y;
        }

        public void ReverseZ()
        {
            this.Position.Z = -this.Position.Z;
        }

        public bool RemapAxis(string axis, double prevMin, double prevMax, double newMin, double newMax)
        {
            string a = axis.ToLower();
            //Some sanity
            if (!a.Equals("x") && !a.Equals("y") && !a.Equals("z"))
            {
                Console.WriteLine("Please use 'x', 'y' or 'z' as arguments");
                return false;
            }

            int axid = a.Equals("x") ? 0 : a.Equals("y") ? 1 : 2;

            switch (axid)
            {
                case 0:
                    this.Position.X = Util.Remap(this.Position.X, prevMin, prevMax, newMin, newMax);
                    break;
                case 1:
                    this.Position.Y = Util.Remap(this.Position.Y, prevMin, prevMax, newMin, newMax);
                    break;
                default:
                    this.Position.Z = Util.Remap(this.Position.Z, prevMin, prevMax, newMin, newMax);
                    break;
            }

            return true;
        }


        public override string ToString()
        {
            //return this.Position + "," + this.Orientation;
            return string.Format("{0},{1},{2},{3}", this.Position, this.Orientation, this.Speed, this.Zone);
        }
    }


    ////██████╗  █████╗ ████████╗██╗  ██╗
    ////██╔══██╗██╔══██╗╚══██╔══╝██║  ██║
    ////██████╔╝███████║   ██║   ███████║
    ////██╔═══╝ ██╔══██║   ██║   ██╔══██║
    ////██║     ██║  ██║   ██║   ██║  ██║
    ////╚═╝     ╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝
    ///// <summary>
    ///// Represents an ordered sequence of target Frames
    ///// </summary>
    //public class Path : Geometry
    //{
    //    public string Name;
    //    private List<Frame> Targets;
    //    public int Count { get; private set; }

    //    public Path() : this("defaultPath") { }

    //    public Path(string name)
    //    {
    //        this.Name = name;
    //        this.Targets = new List<Frame>();
    //        Count = 0;
    //    }

    //    public void Add(Frame target)
    //    {
    //        this.Targets.Add(target);
    //        Count++;
    //    }

    //    public void Add(Point position)
    //    {
    //        this.Add(new Frame(position));
    //    }

    //    public void Add(double x, double y, double z)
    //    {
    //        this.Add(new Frame(x, y, z));
    //    }

    //    public void Add(Point position, Rotation orientation)
    //    {
    //        this.Add(new Frame(position, orientation));
    //    }

    //    public Frame GetTarget(int index)
    //    {
    //        return this.Targets[index];
    //    }

    //    public Frame GetFirstTarget()
    //    {
    //        return this.Targets[0];
    //    }

    //    public Frame GetLastTarget()
    //    {
    //        return this.Targets[this.Targets.Count - 1];
    //    }

    //    private void UpadateTargetCount()
    //    {
    //        Count = Targets.Count;
    //    }

    //    /// <summary>
    //    /// Flips the XY coordinates of all target frames.
    //    /// </summary>
    //    public void FlipXY()
    //    {
    //        foreach (Frame f in Targets)
    //        {
    //            //double x = f.Position.X;
    //            //f.Position.X = f.Position.Y;
    //            //f.Position.Y = x;
    //            f.FlipXY();
    //        }
    //    }

    //    /// <summary>
    //    /// Remaps the coordinates of all target frames from a source to a target domain.
    //    /// </summary>
    //    /// <param name="axis"></param>
    //    /// <param name="prevMin"></param>
    //    /// <param name="prevMax"></param>
    //    /// <param name="newMin"></param>
    //    /// <param name="newMax"></param>
    //    /// <returns></returns>
    //    public bool RemapAxis(string axis, double prevMin, double prevMax, double newMin, double newMax)
    //    {
    //        string a = axis.ToLower();
    //        //Some sanity
    //        if (!a.Equals("x") && !a.Equals("y") && !a.Equals("z"))
    //        {
    //            Console.WriteLine("Please use 'x', 'y' or 'z' as arguments");
    //            return false;
    //        }

    //        int axid = a.Equals("x") ? 0 : a.Equals("y") ? 1 : 2;

    //        switch (axid)
    //        {
    //            case 0:
    //                foreach (Frame f in Targets)
    //                {
    //                    f.Position.X = Util.Remap(f.Position.X, prevMin, prevMax, newMin, newMax);
    //                }
    //                break;
    //            case 1:
    //                foreach (Frame f in Targets)
    //                {
    //                    f.Position.Y = Util.Remap(f.Position.Y, prevMin, prevMax, newMin, newMax);
    //                }
    //                break;
    //            default:
    //                foreach (Frame f in Targets)
    //                {
    //                    f.Position.Z = Util.Remap(f.Position.Z, prevMin, prevMax, newMin, newMax);
    //                }
    //                break;
    //        }

    //        return true;
    //    }

    //    /// <summary>
    //    /// Simplifies the path using a combination of radial distance and 
    //    /// Ramer–Douglas–Peucker algorithm. 
    //    /// </summary>
    //    /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
    //    /// <param name="tolerance"></param>
    //    /// <param name="highQuality"></param>
    //    /// <returns></returns>
    //    public bool Simplify(double tolerance, bool highQuality)
    //    {

    //        if (Count < 1)
    //        {
    //            Console.WriteLine("Path contains no targets.");
    //            return false;
    //        }

    //        int prev = Count;

    //        double sqTolerance = tolerance * tolerance;

    //        if (!highQuality)
    //        {
    //            SimplifyRadialDistance(sqTolerance);
    //        }

    //        SimplifyDouglasPeucker(sqTolerance);

    //        Console.WriteLine("Path " + this.Name + " simplified from " + prev + " to " + Count +" targets.");

    //        return true;
    //    }

    //    /// <summary>
    //    /// The RDP algorithm.
    //    /// </summary>
    //    /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
    //    /// <param name="sqTolerance"></param>
    //    /// <returns></returns>
    //    private void SimplifyDouglasPeucker(double sqTolerance)
    //    {
    //        var len = Count;
    //        var markers = new int?[len];
    //        int? first = 0;
    //        int? last = len - 1;
    //        int? index = 0;
    //        var stack = new List<int?>();
    //        var newTargets = new List<Frame>();

    //        markers[first.Value] = markers[last.Value] = 1;

    //        while (last != null)
    //        {
    //            var maxSqDist = 0.0d;

    //            for (int? i = first + 1; i < last; i++)
    //            {
    //                var sqDist = Point.SqSegmentDistance(Targets[i.Value].Position,
    //                    Targets[first.Value].Position, Targets[last.Value].Position);

    //                if (sqDist > maxSqDist)
    //                {
    //                    index = i;
    //                    maxSqDist = sqDist;
    //                }
    //            }

    //            if (maxSqDist > sqTolerance)
    //            {
    //                markers[index.Value] = 1;
    //                stack.AddRange(new[] { first, index, index, last });
    //            }

    //            if (stack.Count > 0)
    //            {
    //                last = stack[stack.Count - 1];
    //                stack.RemoveAt(stack.Count - 1);
    //            }
    //            else
    //            {
    //                last = null;
    //            }

    //            if (stack.Count > 0)
    //            {
    //                first = stack[stack.Count - 1];
    //                stack.RemoveAt(stack.Count - 1);
    //            }
    //            else
    //            {
    //                first = null;
    //            }
    //        }


    //        for (int i = 0; i < len; i++)
    //        {
    //            if (markers[i] != null)
    //            {
    //                newTargets.Add(Targets[i]);
    //            }
    //        }

    //        Targets = newTargets;
    //        UpadateTargetCount();
    //    }

    //    /// <summary>
    //    /// Simple distance-based simplification. Consecutive points under 
    //    /// threshold distance are removed. 
    //    /// </summary>
    //    /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
    //    /// <param name="sqTolerance"></param>
    //    /// <returns></returns>
    //    private void SimplifyRadialDistance(double sqTolerance)
    //    {
    //        Frame prevFrame = Targets[0];
    //        List<Frame> newTargets = new List<Frame> { prevFrame };
    //        Frame frame = null;

    //        for (int i = 1; i < Targets.Count; i++)
    //        {
    //            frame = Targets[i];

    //            if (Point.SqDistance(frame.Position, prevFrame.Position) > sqTolerance)
    //            {
    //                newTargets.Add(frame);
    //                prevFrame = frame;
    //            }
    //        }

    //        // Add the last frame of the path (?)
    //        if (frame != null && !prevFrame.Position.Equals(frame.Position))
    //        {
    //            newTargets.Add(frame);
    //        }

    //        Targets = newTargets;
    //        UpadateTargetCount();
    //    }
    //}









    //███████╗██████╗  █████╗ ████████╗██╗ █████╗ ██╗         ████████╗██████╗ ██╗ ██████╗  ██████╗ ███████╗██████╗ ███████╗
    //██╔════╝██╔══██╗██╔══██╗╚══██╔══╝██║██╔══██╗██║         ╚══██╔══╝██╔══██╗██║██╔════╝ ██╔════╝ ██╔════╝██╔══██╗██╔════╝
    //███████╗██████╔╝███████║   ██║   ██║███████║██║            ██║   ██████╔╝██║██║  ███╗██║  ███╗█████╗  ██████╔╝███████╗
    //╚════██║██╔═══╝ ██╔══██║   ██║   ██║██╔══██║██║            ██║   ██╔══██╗██║██║   ██║██║   ██║██╔══╝  ██╔══██╗╚════██║
    //███████║██║     ██║  ██║   ██║   ██║██║  ██║███████╗       ██║   ██║  ██║██║╚██████╔╝╚██████╔╝███████╗██║  ██║███████║
    //╚══════╝╚═╝     ╚═╝  ╚═╝   ╚═╝   ╚═╝╚═╝  ╚═╝╚══════╝       ╚═╝   ╚═╝  ╚═╝╚═╝ ╚═════╝  ╚═════╝ ╚══════╝╚═╝  ╚═╝╚══════╝

    public class SpatialTrigger
    {
        virtual public void Check(Robot robot)
        {

        }
    }

    public class SpatialTriggerBox : SpatialTrigger
    {
        Point p0;
        Point p1;

        public bool isTriggered = false;

        public override void Check(Robot robot)
        {

        }
    }

    public class SpatialTriggerPointProximity : SpatialTrigger
    {
        Point p0;
        double r;
        public override void Check(Robot robot)
        {
        }
    }
                                                                   




}
