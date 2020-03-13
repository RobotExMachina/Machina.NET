using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Types.Geometry
{

    //   ██████╗ ██████╗ ██╗███████╗███╗   ██╗████████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔═══██╗██╔══██╗██║██╔════╝████╗  ██║╚══██╔══╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //  ██║   ██║██████╔╝██║█████╗  ██╔██╗ ██║   ██║   ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //  ██║   ██║██╔══██╗██║██╔══╝  ██║╚██╗██║   ██║   ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //  ╚██████╔╝██║  ██║██║███████╗██║ ╚████║   ██║   ██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //   ╚═════╝ ╚═╝  ╚═╝╚═╝╚══════╝╚═╝  ╚═══╝   ╚═╝   ╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                                        

    /// <summary>
    /// Defines an Orientation in three-dimensional space represented by a triplet of orthogonal XYZ unit vectors
    /// following right-hand rule orientations. Useful for spatial and rotational orientation operations. 
    /// </summary>
    public class Orientation : Geometry
    {
        /**
         * NOTE: just as rotation, this class is just a wrapper around the underlying 
         * rotational elements that represent Orientation in space.
         * The main purpose of this class it to be an intuitive way of representing Orientation 
         * in three-dimensional space. AxisAngle is therefore not used here, since conceptually
         * there is no need to represent rotations or store overturns in an object that represents
         * pure orientation. 
         * Typical inputs will be vectors in space or conversions from other rotation representations, 
         * and typical visual outputs will be main Vectors, Rotation Matrices or Euler Angles 
         * (even though all internal computation is based on Quaternion algebra). 
         **/


        /// <summary>
        /// Get an Orientation matching the World XY plane.
        /// </summary>
        public static Orientation WorldXY => new Orientation(1, 0, 0, 0, 1, 0);

        /// <summary>
        /// Get an Orientation matching the World XZ plane.
        /// </summary>
        public static Orientation WorldXZ => new Orientation(1, 0, 0, 0, 0, 1);

        /// <summary>
        /// Get an Orientation matching the World YZ plane.
        /// </summary>
        public static Orientation WorldYZ => new Orientation(0, 1, 0, 0, 0, 1);

        /// <summary>
        /// Implicit conversion to Quaternion object.
        /// </summary>
        /// <param name="ori"></param>
        public static implicit operator Quaternion(Orientation ori) => ori.Q;

        /// <summary>
        /// Implicit conversion to RotationMatrix object.
        /// </summary>
        /// <param name="ori"></param>
        public static implicit operator RotationMatrix(Orientation ori) => ori.RM;
        

        internal Quaternion Q = null;
        internal RotationMatrix RM = null;  // useful for vector-to-quaternion conversions and as storage of orientation vectors
        
        /// <summary>
        /// The main X direction of this Orientation.
        /// </summary>
        public Vector XAxis => this.RM == null ? 
                    new Vector(1, 0, 0) : 
                    new Vector(this.RM.M11, this.RM.M21, this.RM.M31);

        /// <summary>
        /// The main Y direction of this Orientation.
        /// </summary>
        public Vector YAxis => this.RM == null ? 
                    new Vector(0, 1, 0) : 
                    new Vector(this.RM.M12, this.RM.M22, this.RM.M32);

        /// <summary>
        /// The main Z direction of this Orientation.
        /// </summary>
        public Vector ZAxis => this.RM == null ? 
                    new Vector(0, 0, 1) : 
                    new Vector(this.RM.M13, this.RM.M23, this.RM.M33);

        /// <summary>
        /// Implicit conversion from Rotation to Orientation via its Quaternion.
        /// </summary>
        /// <param name="or"></param>
        public static implicit operator Orientation(Rotation r) => r == null ? null : new Orientation(r.Q);

        /// <summary>
        /// Create a  Rotation object representing no rotation.
        /// </summary>
        public Orientation()
        {
            this.Q = new Quaternion();
            this.RM = new RotationMatrix();
        }

        /// <summary>
        /// Create a new Orientation object from the main X and Y axes.
        /// This constructor will create the best-fit orthogonal coordinate system
        /// respecting the direction of the X vector and the plane formed with the Y vector. 
        /// The Z vector will be normal to this planes, and all vectors will be unitized. 
        /// </summary>
        /// <param name="vectorX"></param>
        /// <param name="vectorY"></param>
        public Orientation(Vector vectorX, Vector vectorY) 
            : this(vectorX.X, vectorX.Y, vectorX.Z, vectorY.X, vectorY.Y, vectorY.Z) { }

        /// <summary>
        /// Create a new Orientation object from the main X and Y axes.
        /// This constructor will create the best-fit orthogonal coordinate system
        /// respecting the direction of the X vector and the plane formed with the Y vector. 
        /// The Z vector will be normal to this planes, and all vectors will be unitized. 
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y0"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        public Orientation(double x0, double x1, double x2, double y0, double y1, double y2)
        {
            this.RM = new RotationMatrix(x0, x1, x2, y0, y1, y2);
            this.Q = this.RM.ToQuaternion();
        }

        /// <summary>
        /// Create an Orientation object from a Quaternion representation.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        internal Orientation(Quaternion q)
        {
            this.Q = new Quaternion(q);
            this.RM = this.Q.ToRotationMatrix();
        }

        /// <summary>
        /// Creates an Orientation object from a Rotation representation.
        /// </summary>
        /// <param name="r"></param>
        internal Orientation(Rotation r)
            : this(r.Q) { }


        public Quaternion ToQuaternion() => this.Q;
        public RotationMatrix ToRotationMatrix() => this.RM;
        public RotationVector ToRotationVector() => this.Q.ToAxisAngle().ToRotationVector();  // @TODO: this is grose... 




        public override string ToString() => this.ToString(false);
        
        public string ToString(bool labels)
        {
            if (labels)
            {
                return string.Format(CultureInfo.InvariantCulture, 
                    "Orientation[X:[{0}, {1}, {2}], Y:[{3}, {4}, {5}], Z:[{6}, {7}, {8}]]",
                    Math.Round(this.RM.M11, MMath.STRING_ROUND_DECIMALS_MM), Math.Round(this.RM.M21, MMath.STRING_ROUND_DECIMALS_MM), Math.Round(this.RM.M31, MMath.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.RM.M12, MMath.STRING_ROUND_DECIMALS_MM), Math.Round(this.RM.M22, MMath.STRING_ROUND_DECIMALS_MM), Math.Round(this.RM.M32, MMath.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.RM.M13, MMath.STRING_ROUND_DECIMALS_MM), Math.Round(this.RM.M23, MMath.STRING_ROUND_DECIMALS_MM), Math.Round(this.RM.M33, MMath.STRING_ROUND_DECIMALS_MM));
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "[[{0}, {1}, {2}], [{3}, {4}, {5}], [{6}, {7}, {8}]]",
                    Math.Round(this.RM.M11, MMath.STRING_ROUND_DECIMALS_MM), Math.Round(this.RM.M21, MMath.STRING_ROUND_DECIMALS_MM), Math.Round(this.RM.M31, MMath.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.RM.M12, MMath.STRING_ROUND_DECIMALS_MM), Math.Round(this.RM.M22, MMath.STRING_ROUND_DECIMALS_MM), Math.Round(this.RM.M32, MMath.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.RM.M13, MMath.STRING_ROUND_DECIMALS_MM), Math.Round(this.RM.M23, MMath.STRING_ROUND_DECIMALS_MM), Math.Round(this.RM.M33, MMath.STRING_ROUND_DECIMALS_MM));
            }
        }

        public string ToArrayString()
        {
            return string.Format(CultureInfo.InvariantCulture, 
                "[{0},{1},{2},{3},{4},{5}]",
                Math.Round(this.RM.M11, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(this.RM.M21, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(this.RM.M31, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(this.RM.M12, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(this.RM.M22, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(this.RM.M32, MMath.STRING_ROUND_DECIMALS_MM));
        }

    }
}
