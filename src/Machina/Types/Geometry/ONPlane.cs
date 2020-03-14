using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Types.Geometry
{
    /// <summary>
    /// A Plane class taken mostly from RhinoCommon/OpenNurbs.
    /// </summary>
    public struct Plane // : IEquatable<Matrix>, IEpsilonComparable<Matrix>, ISerializableArray, ISerializableJSON
    {

        #region members
        internal Vector m_origin;
        internal Vector m_xaxis;
        internal Vector m_yaxis;
        internal Vector m_zaxis;
        #endregion

        #region properties
        /// <summary>
        /// Gets or sets the origin point of this plane.
        /// </summary>
        public Vector Origin
        {
            get { return m_origin; }
            set { m_origin = value; }
        }
        /// <summary>
        /// Gets or sets the X coordinate of the origin of this plane.
        /// </summary>
        public double OriginX
        {
            get { return m_origin.X; }
            set { m_origin.X = value; }
        }
        /// <summary>
        /// Gets or sets the Y coordinate of the origin of this plane.
        /// </summary>
        public double OriginY
        {
            get { return m_origin.Y; }
            set { m_origin.Y = value; }
        }
        /// <summary>
        /// Gets or sets the Z coordinate of the origin of this plane.
        /// </summary>
        public double OriginZ
        {
            get { return m_origin.Z; }
            set { m_origin.Z = value; }
        }
        /// <summary>
        /// Gets or sets the X axis vector of this plane.
        /// </summary>
        public Vector XAxis
        {
            get { return m_xaxis; }
            set { m_xaxis = value; }
        }
        /// <summary>
        /// Gets or sets the Y axis vector of this plane.
        /// </summary>
        public Vector YAxis
        {
            get { return m_yaxis; }
            set { m_yaxis = value; }
        }
        /// <summary>
        /// Gets or sets the Z axis vector of this plane.
        /// </summary>
        public Vector ZAxis
        {
            get { return m_zaxis; }
            set { m_zaxis = value; }
        }
        #endregion


        #region constants
        /// <summary>
        /// plane coincident with the World XY plane.
        /// </summary>
        public static Plane WorldXY
        {
            get
            {
                return new Plane { XAxis = new Vector(1, 0, 0), YAxis = new Vector(0, 1, 0), ZAxis = new Vector(0, 0, 1) };
            }
        }

        /// <summary>
        /// plane coincident with the World YZ plane.
        /// </summary>
        public static Plane WorldYZ
        {
            get
            {
                return new Plane { XAxis = new Vector(0, 1, 0), YAxis = new Vector(0, 0, 1), ZAxis = new Vector(1, 0, 0) };
            }
        }

        /// <summary>
        /// plane coincident with the World ZX plane.
        /// </summary>
        public static Plane WorldZX
        {
            get
            {
                return new Plane { XAxis = new Vector(0, 0, 1), YAxis = new Vector(1, 0, 0), ZAxis = new Vector(0, 1, 0) };
            }
        }

        /// <summary>
        /// Gets a plane that contains Unset origin and axis vectors.
        /// </summary>
        public static Plane Unset
        {
            get
            {
                return new Plane { Origin = Vector.Unset, XAxis = Vector.Unset, YAxis = Vector.Unset, ZAxis = Vector.Unset };
            }
        }
        #endregion


        #region constructors
        /// <summary>
        /// Create a new Plane. Vectors must be unit and orthogonal, otherwise, 
        /// lots of badness may happen down the road...
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="xAxis"></param>
        /// <param name="yAxis"></param>
        internal static Plane CreateUnsafe(Vector origin, Vector xAxis, Vector yAxis)
        {
            // This is a lightweight constructor in case data is coming clean.
            Plane p = new Plane
            {
                m_origin = new Vector(origin),
                m_xaxis = new Vector(xAxis),
                m_yaxis = new Vector(yAxis)
            };
            p.m_zaxis = Vector.CrossProduct(p.m_xaxis, p.m_yaxis);

            return p;
        }

        /// <summary>Copy constructor.
        /// <para>This is nothing special and performs the same as assigning to another variable.</para>
        /// </summary>
        /// <param name="other">The source plane value.</param>
        public Plane(Plane other)
        {
            this = other;
        }

        /// <summary>
        /// Constructs a plane from a point and a normal vector.
        /// </summary>
        /// <param name="origin">Origin point of the plane.</param>
        /// <param name="normal">Non-zero normal to the plane.</param>
        public Plane(Vector origin, Vector normal)
        {
            // Adapted from RC/ON
            //UnsafeNativeMethods.ON_Plane_CreateFromNormal(ref this, origin, normal);

            // Shallow copies
            m_origin = new Vector(origin);
            m_zaxis = new Vector(normal);  

            if (!m_zaxis.Normalize())
            {
                this = Unset;
                return;
            }

            Vector.PerpendicularTo(m_zaxis, out m_xaxis);
            m_xaxis.Normalize();
            m_yaxis = Vector.CrossProduct(m_zaxis, m_xaxis);
            //m_yaxis.Normalize();  // unnecesary, z and x were normal already

        }

        /// <summary>
        /// Constructs a plane from a point and two vectors in the plane.
        /// </summary>
        /// <param name='origin'>Origin point of the plane.</param>
        /// <param name='xAxis'>
        /// Non-zero vector in the plane that determines the x-axis direction.
        /// </param>
        /// <param name='yAxis'>
        /// Non-zero vector not parallel to x_dir that is used to determine the
        /// yaxis direction. y_dir does not need to be perpendicular to x_dir.
        /// </param>
        public Plane(Vector origin, Vector xAxis, Vector yAxis)
        {
            // Adapted from RC/ON
            //UnsafeNativeMethods.ON_Plane_CreateFromFrame(ref this, origin, xDirection, yDirection);

            // Sanity
            Direction dir = Vector.CompareDirections(xAxis, yAxis);
            if (dir == Direction.Invalid || dir == Direction.Parallel || dir == Direction.Opposite)
            {
                this = Unset;
                return;
            }

            // Shallow copies
            m_origin = new Vector(origin);
            m_xaxis = new Vector(xAxis);
            m_yaxis = new Vector(yAxis);

            m_xaxis.Normalize();
            m_yaxis -= Vector.DotProduct(m_yaxis, m_xaxis) * m_xaxis;
            m_yaxis.Normalize();

            m_zaxis = Vector.CrossProduct(m_xaxis, m_yaxis);
        }



        ///// <summary>
        ///// Initializes a plane from three non-colinear points.
        ///// </summary>
        ///// <param name='origin'>Origin point of the plane.</param>
        ///// <param name='xPoint'>
        ///// Second point in the plane. The x-axis will be parallel to x_point-origin.
        ///// </param>
        ///// <param name='yPoint'>
        ///// Third point on the plane that is not colinear with the first two points.
        ///// yaxis*(y_point-origin) will be &gt; 0.
        ///// </param>
        ///// <example>
        ///// </example>
        //public Plane(Point3d origin, Point3d xPoint, Point3d yPoint)
        //  : this()
        //{
        //    UnsafeNativeMethods.ON_Plane_CreateFromPoints(ref this, origin, xPoint, yPoint);
        //}



        //        /// <summary>
        //        /// Constructs a plane from an equation
        //        /// ax+by+cz=d.
        //        /// </summary>
        //        public Plane(double a, double b, double c, double d)
        //          : this()
        //        {
        //            UnsafeNativeMethods.ON_Plane_CreateFromEquation(ref this, a, b, c, d);

        //            // David 16/05/2012
        //            // This constructor resulted in an invalid plane unless the equation 
        //            // already defined a unitized zaxis vector. Adding unitize now to fix this.
        //            this.m_zaxis.Unitize();
        //        }

        //#if RHINO_SDK
        //    /// <summary>Fit a plane through a collection of points.</summary>
        //    /// <param name="points">Points to fit to.</param>
        //    /// <param name="plane">Resulting plane.</param>
        //    /// <returns>A value indicating the result of the operation.</returns>
        //    public static PlaneFitResult FitPlaneToPoints(System.Collections.Generic.IEnumerable<Point3d> points, out Plane plane)
        //    {
        //      double max_dev;
        //      return FitPlaneToPoints(points, out plane, out max_dev);
        //    }

        //    /// <summary>Fit a plane through a collection of points.</summary>
        //    /// <param name="points">Points to fit to.</param>
        //    /// <param name="plane">Resulting plane.</param>
        //    /// <param name="maximumDeviation">The distance from the furthest point to the plane.</param>
        //    /// <returns>A value indicating the result of the operation.</returns>
        //    public static PlaneFitResult FitPlaneToPoints(System.Collections.Generic.IEnumerable<Point3d> points, out Plane plane, out double maximumDeviation)
        //    {
        //      plane = new Plane();
        //      maximumDeviation = 0.0;

        //      int count;
        //      Point3d[] ptArray = Rhino.Collections.Point3dList.GetConstPointArray(points, out count);

        //      if (null == ptArray || count < 1) { return PlaneFitResult.Failure; }

        //      int rc = UnsafeNativeMethods.RHC_FitPlaneToPoints(count, ptArray, ref plane, ref maximumDeviation);
        //      if (rc == -1) { return PlaneFitResult.Failure; }
        //      if (rc == 0) { return PlaneFitResult.Success; }

        //      return PlaneFitResult.Inconclusive;
        //    }
        //#endif

        #endregion constructors


        #region operators
        /// <summary>
        /// Determines if two planes are equal.
        /// </summary>
        /// <param name="a">A first plane.</param>
        /// <param name="b">A second plane.</param>
        /// <returns>true if the two planes have all equal components; false otherwise.</returns>
        public static bool operator ==(Plane a, Plane b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines if two planes are different.
        /// </summary>
        /// <param name="a">A first plane.</param>
        /// <param name="b">A second plane.</param>
        /// <returns>true if the two planes have any different componet components; false otherwise.</returns>
        public static bool operator !=(Plane a, Plane b)
        {
            return (a.m_origin != b.m_origin) ||
                   (a.m_xaxis != b.m_xaxis) ||
                   (a.m_yaxis != b.m_yaxis) ||
                   (a.m_zaxis != b.m_zaxis);
        }

        /// <summary>
        /// Determines if an object is a plane and has the same components as this plane.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns>true if obj is a plane and has the same components as this plane; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return ((obj is Plane) && (this == (Plane)obj));
        }

        /// <summary>
        /// Determines if another plane has the same components as this plane.
        /// </summary>
        /// <param name="plane">A plane.</param>
        /// <returns>true if plane has the same components as this plane; false otherwise.</returns>
        public bool Equals(Plane plane)
        {
            return (m_origin == plane.m_origin) &&
                   (m_xaxis == plane.m_xaxis) &&
                   (m_yaxis == plane.m_yaxis) &&
                   (m_zaxis == plane.m_zaxis);
        }

        /// <summary>
        /// Gets a non-unique hashing code for this entity.
        /// </summary>
        /// <returns>A particular number for a specific instance of plane.</returns>
        public override int GetHashCode()
        {
            // MSDN docs recommend XOR'ing the internal values to get a hash code
            return m_origin.GetHashCode() ^ m_xaxis.GetHashCode() ^ m_yaxis.GetHashCode() ^ m_zaxis.GetHashCode();
        }

        /// <summary>
        /// Constructs the string representation of this plane.
        /// </summary>
        /// <returns>Text.</returns>
        public override string ToString()
        {
            string rc = String.Format(System.Globalization.CultureInfo.InvariantCulture,
              "Origin={0} XAxis={1}, YAxis={2}, ZAxis={3}",
              Origin, XAxis, YAxis, ZAxis.ToString());
            return rc;
        }
        #endregion
    }
}