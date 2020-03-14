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
        /// <summary>
        /// Gets the normal of this plane. This is essentially the ZAxis of the plane.
        /// </summary>
        public Vector Normal
        {
            get { return m_zaxis; }
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

        #region methods
        /// <summary>
        /// Evaluate a point on the plane.
        /// </summary>
        /// <param name="u">evaulation parameter.</param>
        /// <param name="v">evaulation parameter.</param>
        /// <returns>plane.origin + u*plane.xaxis + v*plane.yaxis.</returns>
        public Vector PointAt(double u, double v)
        {
            return (Origin + u * XAxis + v * YAxis);
        }

        /// <summary>
        /// Evaluate a point on the plane.
        /// </summary>
        /// <param name="u">evaulation parameter.</param>
        /// <param name="v">evaulation parameter.</param>
        /// <param name="w">evaulation parameter.</param>
        /// <returns>plane.origin + u*plane.xaxis + v*plane.yaxis + z*plane.zaxis.</returns>
        public Vector PointAt(double u, double v, double w)
        {
            return (Origin + u * XAxis + v * YAxis + w * ZAxis);
        }

        #region projections
        /// <summary>
        /// Gets the parameters of the point on the plane closest to a test point.
        /// </summary>
        /// <param name="testPoint">Point to get close to.</param>
        /// <param name="s">Parameter along plane X-direction.</param>
        /// <param name="t">Parameter along plane Y-direction.</param>
        /// <returns>
        /// true if a parameter could be found, 
        /// false if the point could not be projected successfully.
        /// </returns>
        public bool ClosestParameter(Vector testPoint, out double s, out double t)
        {
            // The projection length of vector v over unit axis is their dot product.
            Vector v = testPoint - Origin;
            s = v * XAxis;
            t = v * YAxis;

            return true;
        }

        /// <summary>
        /// Gets the point on the plane closest to a test point.
        /// </summary>
        /// <param name="testPoint">Point to get close to.</param>
        /// <returns>
        /// The point on the plane that is closest to testPoint, 
        /// or Point3d.Unset on failure.
        /// </returns>
        public Vector ClosestPoint(Vector testPoint)
        {
            ClosestParameter(testPoint, out double s, out double  t);
            return PointAt(s, t);
        }

        /// <summary>
        /// Returns the signed distance from testPoint to its projection onto this plane. 
        /// If the point is below the plane, a negative distance is returned.
        /// </summary>
        /// <param name="testPoint">Point to test.</param>
        /// <returns>Signed distance from this plane to testPoint.</returns>
        public double DistanceTo(Vector testPoint)
        {
            // The signed distance of vector u projected on unit vector v is their dot product.
            Vector relativePos = testPoint - m_origin;
            return relativePos * m_zaxis;
        }

        /// <summary>
        /// Convert a point from World space coordinates into Plane space coordinates.
        /// </summary>
        /// <param name="ptSample">World point to remap.</param>
        /// <param name="ptPlane">Point in plane (s,t,d) coordinates.</param>
        /// <returns>true on success, false on failure.</returns>
        /// <remarks>D stands for distance, not disease.</remarks>
        public bool RemapToPlaneSpace(Vector ptSample, out Vector ptPlane)
        {
            ClosestParameter(ptSample, out double x, out double y);
            double z = DistanceTo(ptSample);

            ptPlane = new Vector(x, y, z);
            return true;
        }
        #endregion

        #region transformations
        /// <summary>
        /// Flip this plane by swapping out the X and Y axes and inverting the Z axis.
        /// </summary>
        public void Flip()
        {
            Vector v = new Vector(m_xaxis);  // shalow copy
            m_xaxis = m_yaxis;
            m_yaxis = v;
            m_zaxis = -m_zaxis;
        }

        /// <summary>
        /// Transform the plane with a Transformation matrix.
        /// </summary>
        /// <param name="transform">Transformation to apply to plane.</param>
        /// <returns>true on success, false on failure.</returns>
        public bool Transform(Matrix transform)
        {
            //return UnsafeNativeMethods.ON_Plane_Transform(ref this, ref xform);
        }

        /// <summary>
        /// Translate (move) the plane along a vector.
        /// </summary>
        /// <param name="delta">Translation (motion) vector.</param>
        /// <returns>true on success, false on failure.</returns>
        public bool Translate(Vector3d delta)
        {
            if (!delta.IsValid)
                return false;

            Origin += delta;
            return true;
        }

        /// <summary>
        /// Rotate the plane about its origin point.
        /// </summary>
        /// <param name="sinAngle">Sin(angle).</param>
        /// <param name="cosAngle">Cos(angle).</param>
        /// <param name="axis">Axis of rotation.</param>
        /// <returns>true on success, false on failure.</returns>
        public bool Rotate(double sinAngle, double cosAngle, Vector3d axis)
        {
            bool rc = true;
            if (axis == ZAxis)
            {
                Vector3d x = cosAngle * XAxis + sinAngle * YAxis;
                Vector3d y = cosAngle * YAxis - sinAngle * XAxis;
                XAxis = x;
                YAxis = y;
            }
            else
            {
                Point3d origin_pt = Origin;
                rc = Rotate(sinAngle, cosAngle, axis, Origin);
                Origin = origin_pt; // to kill any fuzz
            }
            return rc;
        }

        /// <summary>
        /// Rotate the plane about its origin point.
        /// </summary>
        /// <param name="angle">Angle in radians.</param>
        /// <param name="axis">Axis of rotation.</param>
        /// <returns>true on success, false on failure.</returns>
        public bool Rotate(double angle, Vector3d axis)
        {
            return Rotate(Math.Sin(angle), Math.Cos(angle), axis);
        }

        /// <summary>
        /// Rotate the plane about a custom anchor point.
        /// </summary>
        /// <param name="angle">Angle in radians.</param>
        /// <param name="axis">Axis of rotation.</param>
        /// <param name="centerOfRotation">Center of rotation.</param>
        /// <returns>true on success, false on failure.</returns>
        public bool Rotate(double angle, Vector3d axis, Point3d centerOfRotation)
        {
            return Rotate(Math.Sin(angle), Math.Cos(angle), axis, centerOfRotation);
        }

        /// <summary>Rotate the plane about a custom anchor point.</summary>
        /// <param name="sinAngle">Sin(angle)</param>
        /// <param name="cosAngle">Cos(angle)</param>
        /// <param name="axis">Axis of rotation.</param>
        /// <param name="centerOfRotation">Center of rotation.</param>
        /// <returns>true on success, false on failure.</returns>
        public bool Rotate(double sinAngle, double cosAngle, Vector3d axis, Point3d centerOfRotation)
        {
            if (centerOfRotation == Origin)
            {
                Transform rot = Rhino.Geometry.Transform.Rotation(sinAngle, cosAngle, axis, Point3d.Origin);
                XAxis = rot * XAxis;
                YAxis = rot * YAxis;
                ZAxis = rot * ZAxis;
                return true;
            }
            Transform rot2 = Rhino.Geometry.Transform.Rotation(sinAngle, cosAngle, axis, centerOfRotation);
            return Transform(rot2);
        }
        #endregion
        #endregion
    }
}