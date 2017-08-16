using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{

    //  ███╗   ███╗ █████╗ ████████╗██████╗ ██╗██╗  ██╗██████╗ ██████╗ 
    //  ████╗ ████║██╔══██╗╚══██╔══╝██╔══██╗██║╚██╗██╔╝╚════██╗╚════██╗
    //  ██╔████╔██║███████║   ██║   ██████╔╝██║ ╚███╔╝  █████╔╝ █████╔╝
    //  ██║╚██╔╝██║██╔══██║   ██║   ██╔══██╗██║ ██╔██╗  ╚═══██╗ ╚═══██╗
    //  ██║ ╚═╝ ██║██║  ██║   ██║   ██║  ██║██║██╔╝ ██╗██████╔╝██████╔╝
    //  ╚═╝     ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝╚═╝╚═╝  ╚═╝╚═════╝ ╚═════╝ 
    //                                                                

    /// <summary>
    /// A class representing a 3x3 rotation matrix
    /// </summary>
    public class RotationMatrix : Geometry
    {
        /// <summary>
        /// Elements of the rotation matrix, ordered in row to column way, 
        /// i.e. r[2] is r13 (row 1 col 3), r[6] is r31, etc. 
        /// </summary>
        public double[] R { get; internal set; }

        /// <summary>
        /// Alias
        /// </summary>
        public double m00 { get { return this.R[0]; } internal set { R[0] = value; } }

        /// <summary>
        /// Alias
        /// </summary>
        public double m01 { get { return this.R[1]; } internal set { R[1] = value; } }

        /// <summary>
        /// Alias
        /// </summary>
        public double m02 { get { return this.R[2]; } internal set { R[2] = value; } }

        /// <summary>
        /// Alias
        /// </summary>
        public double m10 { get { return this.R[3]; } internal set { R[3] = value; } }

        /// <summary>
        /// Alias
        /// </summary>
        public double m11 { get { return this.R[4]; } internal set { R[4] = value; } }

        /// <summary>
        /// Alias
        /// </summary>
        public double m12 { get { return this.R[5]; } internal set { R[5] = value; } }

        /// <summary>
        /// Alias
        /// </summary>
        public double m20 { get { return this.R[6]; } internal set { R[6] = value; } }

        /// <summary>
        /// Alias
        /// </summary>
        public double m21 { get { return this.R[7]; } internal set { R[7] = value; } }

        /// <summary>
        /// Alias
        /// </summary>
        public double m22 { get { return this.R[8]; } internal set { R[8] = value; } }

        /// <summary>
        /// Test if this RotationMatrix is approximately equal to another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSimilar(RotationMatrix other)
        {
            return Math.Abs(this.R[0] - other.R[0]) < EPSILON
                && Math.Abs(this.R[1] - other.R[1]) < EPSILON
                && Math.Abs(this.R[2] - other.R[2]) < EPSILON
                && Math.Abs(this.R[3] - other.R[3]) < EPSILON
                && Math.Abs(this.R[4] - other.R[4]) < EPSILON
                && Math.Abs(this.R[5] - other.R[5]) < EPSILON
                && Math.Abs(this.R[6] - other.R[6]) < EPSILON
                && Math.Abs(this.R[7] - other.R[7]) < EPSILON
                && Math.Abs(this.R[8] - other.R[8]) < EPSILON;
        }

        //public static bool operator ==(RotationMatrix m1, RotationMatrix m2)
        //{
        //    return Math.Abs(m1.R[0] - m2.R[0]) < EPSILON
        //        && Math.Abs(m1.R[1] - m2.R[1]) < EPSILON
        //        && Math.Abs(m1.R[2] - m2.R[2]) < EPSILON
        //        && Math.Abs(m1.R[3] - m2.R[3]) < EPSILON
        //        && Math.Abs(m1.R[4] - m2.R[4]) < EPSILON
        //        && Math.Abs(m1.R[5] - m2.R[5]) < EPSILON
        //        && Math.Abs(m1.R[6] - m2.R[6]) < EPSILON
        //        && Math.Abs(m1.R[7] - m2.R[7]) < EPSILON
        //        && Math.Abs(m1.R[8] - m2.R[8]) < EPSILON;
        //}

        //public static bool operator !=(RotationMatrix m1, RotationMatrix m2)
        //{
        //    return Math.Abs(m1.R[0] - m2.R[0]) > EPSILON
        //        || Math.Abs(m1.R[1] - m2.R[1]) > EPSILON
        //        || Math.Abs(m1.R[2] - m2.R[2]) > EPSILON
        //        || Math.Abs(m1.R[3] - m2.R[3]) > EPSILON
        //        || Math.Abs(m1.R[4] - m2.R[4]) > EPSILON
        //        || Math.Abs(m1.R[5] - m2.R[5]) > EPSILON
        //        || Math.Abs(m1.R[6] - m2.R[6]) > EPSILON
        //        || Math.Abs(m1.R[7] - m2.R[7]) > EPSILON
        //        || Math.Abs(m1.R[8] - m2.R[8]) > EPSILON;
        //}

        /// <summary>
        /// Create a 3x3 identity matrix representing no rotation.
        /// </summary>
        public RotationMatrix()
        {
            R = new double[9];
            this.R[0] = 1;
            this.R[4] = 1;
            this.R[8] = 1;
        }

        /// <summary>
        /// Create a 3x3 Rotation Matrix from it's constituent components. 
        /// Elements of the rotation matrix, ordered in row to column way, 
        /// i.e. r[2] is r13 (row 1 col 3), r[6] is r31, etc. 
        /// This Matrix will be reorthogonalized if necessary.
        /// </summary>
        /// <param name="r00"></param>
        /// <param name="r01"></param>
        /// <param name="r02"></param>
        /// <param name="r10"></param>
        /// <param name="r11"></param>
        /// <param name="r12"></param>
        /// <param name="r20"></param>
        /// <param name="r21"></param>
        /// <param name="r22"></param>
        public RotationMatrix(double r00, double r01, double r02,
                              double r10, double r11, double r12,
                              double r20, double r21, double r22)
        {
            this.Initialize(r00, r01, r02,
                            r10, r11, r12,
                            r20, r21, r22, true);
        }

        /// <summary>
        /// A private constructor to bypass orthogonalization when input parameters are
        /// guaranteed to be orthogonal.
        /// </summary>
        /// <param name="r00"></param>
        /// <param name="r01"></param>
        /// <param name="r02"></param>
        /// <param name="r10"></param>
        /// <param name="r11"></param>
        /// <param name="r12"></param>
        /// <param name="r20"></param>
        /// <param name="r21"></param>
        /// <param name="r22"></param>
        /// <param name="orthogonalize"></param>
        internal RotationMatrix(double r00, double r01, double r02,
                        double r10, double r11, double r12,
                        double r20, double r21, double r22, bool orthogonalize)
        {
            this.Initialize(r00, r01, r02,
                            r10, r11, r12,
                            r20, r21, r22, orthogonalize);
        }

        /// <summary>
        /// Create a 3x3 Rotation Matrix from it's constituent components. 
        /// Elements of the rotation matrix, ordered in row to column way, 
        /// i.e. r[2] is r13 (row 1 col 3), r[6] is r31, etc. 
        /// </summary>
        /// <param name="rotationValues"></param>
        public RotationMatrix(double[] rotationValues)
        {
            if (rotationValues.Length < 9)
            {
                throw new Exception("Need an array of at least 9 values to create a RotationMatrix");
            }

            this.Initialize(rotationValues[0], rotationValues[1], rotationValues[2],
                            rotationValues[3], rotationValues[4], rotationValues[5],
                            rotationValues[6], rotationValues[7], rotationValues[8], true);  // we don't know where these values came from, orthogonalize them
        }

        /// <summary>
        /// Create a 3x3 Rotation Matrix as a shallow copy of another.
        /// </summary>
        /// <param name="rotationMatrix"></param>
        public RotationMatrix(RotationMatrix rotationMatrix)
        {
            this.Initialize(rotationMatrix.R[0], rotationMatrix.R[1], rotationMatrix.R[2],
                            rotationMatrix.R[3], rotationMatrix.R[4], rotationMatrix.R[5],
                            rotationMatrix.R[6], rotationMatrix.R[7], rotationMatrix.R[8], false);  // let's assume the RotationMatrix was already orthogonal... 
        }

        /// <summary>
        /// Create a RotationMatrix from two Vectors. 
        /// This constructor will create the best-fit orthogonal 3x3 matrix 
        /// respecting the direction of the X vector and the plane formed with the Y vector. 
        /// The Z vector will be normal to this planes, and all vectors will be unitized. 
        /// </summary>
        /// <param name="vecX"></param>
        /// <param name="vecY"></param>
        public RotationMatrix(Vector vecX, Vector vecY)
        {
            // Rely on internal orthoginalization to correctly form this matrix
            this.Initialize(vecX.X, vecY.X, 0,
                            vecX.Y, vecY.Y, 0,
                            vecX.Z, vecY.Z, 1, true);
        }

        /// <summary>
        /// Create a RotationMatrix from the components of two Vectors. 
        /// This constructor will create the best-fit orthogonal 3x3 matrix 
        /// respecting the direction of the X vector and the plane formed with the Y vector. 
        /// The Z vector will be normal to this planes, and all vectors will be unitized. 
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y0"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        public RotationMatrix(double x0, double x1, double x2, double y0, double y1, double y2)
        {
            // Rely on internal orthoginalization to correctly form this matrix
            this.Initialize(x0, y0, 0,
                            x1, y1, 0,
                            x2, y2, 1, true);
        }


        /// <summary>
        /// An internal initializator to start this matrix from its components. 
        /// The method allows for optional re-orthogonalization of this Matrix.
        /// </summary>
        /// <param name="r00"></param>
        /// <param name="r01"></param>
        /// <param name="r02"></param>
        /// <param name="r10"></param>
        /// <param name="r11"></param>
        /// <param name="r12"></param>
        /// <param name="r20"></param>
        /// <param name="r21"></param>
        /// <param name="r22"></param>
        internal void Initialize(double r00, double r01, double r02,
                                    double r10, double r11, double r12,
                                    double r20, double r21, double r22, bool orthonogonalize)
        {
            R = new double[9];

            this.R[0] = r00;
            this.R[1] = r01;
            this.R[2] = r02;
            this.R[3] = r10;
            this.R[4] = r11;
            this.R[5] = r12;
            this.R[6] = r20;
            this.R[7] = r21;
            this.R[8] = r22;

            if (orthonogonalize)
            {
                this.Orthogonalize();
            }
        }

        /// <summary>
        /// Is this an identity matrix?
        /// </summary>
        /// <returns></returns>
        public bool IsIdentity()
        {
            return Math.Abs(R[0] - 1) < EPSILON && Math.Abs(R[1]) < EPSILON && Math.Abs(R[2]) < EPSILON
                && Math.Abs(R[3]) < EPSILON && Math.Abs(R[4] - 1) < EPSILON && Math.Abs(R[5]) < EPSILON
                && Math.Abs(R[6]) < EPSILON && Math.Abs(R[7]) < EPSILON && Math.Abs(R[8] - 1) < EPSILON;
        }

        /// <summary>
        /// Is this 
        /// </summary>
        /// <returns></returns>
        public bool IsOrthogonal()
        {
            // Orthogonal matrices satisfy that:
            // Q * Qt = Qt * Q = I;  
            // (the matrix multiplied by its transpose yields the identity matrix)
            // As a consequence, it also holds that the transpose of an orthogonal matrix equals its inverse:
            // Qt = Q^-1
            RotationMatrix t = new RotationMatrix(this);
            t.Transpose();
            RotationMatrix ident = RotationMatrix.Multiply(this, t);
            return ident.IsIdentity() && Math.Abs(this.Determinant() - 1) < EPSILON;
        }

        /// <summary>
        /// Force the orthogonalization of this matrix.
        /// </summary>
        /// <returns></returns>
        public bool Orthogonalize()
        {
            // This algorithm will orthogonalize this matrix by
            // maintaining the main X direction and XY plane, 
            // and recomputing the Y and Z axes to comply with this condition.
            Vector vecX = new Vector(R[0], R[3], R[6]),
                vecY = new Vector(R[1], R[4], R[7]),
                vecZ;

            // Some sanity
            int dir = Vector.CompareDirections(vecX, vecY);
            if (dir == 1 || dir == 3)
            {
                Console.WriteLine("Cannot orthogonalize a Matrix with X & Y parallel vectors");
                this.Identity();
                return false;
            }

            // Create unit X axis
            vecX.Normalize();

            // Find normal vector to plane
            vecZ = Vector.CrossProduct(vecX, vecY);
            vecZ.Normalize();

            // Y axis is the cross product of both
            vecY = Vector.CrossProduct(vecZ, vecX);

            // Initialize the Matrix
            this.Initialize(vecX.X, vecY.X, vecZ.X,
                            vecX.Y, vecY.Y, vecZ.Y,
                            vecX.Z, vecY.Z, vecZ.Z, false);

            return false;
        }

        /// <summary>
        /// Returns the determinant of this Matrix.
        /// </summary>
        /// <returns></returns>
        public double Determinant()
        {
            return R[0] * R[4] * R[8]
                + R[1] * R[5] * R[6]
                + R[2] * R[3] * R[7]
                - R[0] * R[5] * R[7]
                - R[1] * R[3] * R[8]
                - R[2] * R[4] * R[6];
        }

        /// <summary>
        /// Transposes this Matrix.
        /// </summary>
        public void Transpose()
        {
            double old01 = R[1],
                old02 = R[2],
                old12 = R[5];

            this.R[1] = R[3];
            this.R[2] = R[6];
            this.R[5] = R[7];

            this.R[3] = old01;
            this.R[6] = old02;
            this.R[7] = old12;
        }

        /// <summary>
        /// Inverts this Matrix.
        /// Returns false if the Matrix could not be inverted (singular matrix). 
        /// </summary>
        /// <returns></returns>
        public bool Invert()
        {
            // If this matrix is orthogonal (which it should be if it represents a rotation),
            // its transpose is equal to its inverse (saving a lot of calculation?)
            if (this.IsOrthogonal())
            {
                this.Transpose();
                return true;
            }

            // From glmatrix.js: https://github.com/toji/gl-matrix/blob/master/src/gl-matrix/mat3.js
            double a00 = R[0], a01 = R[1], a02 = R[2];
            double a10 = R[3], a11 = R[4], a12 = R[5];
            double a20 = R[6], a21 = R[7], a22 = R[8];

            double b01 = a22 * a11 - a12 * a21;
            double b11 = -a22 * a10 + a12 * a20;
            double b21 = a21 * a10 - a11 * a20;

            // Calculate the determinant
            double det = a00 * b01 + a01 * b11 + a02 * b21;

            if (det < EPSILON)
            {
                return false;
            }
            det = 1.0 / det;

            R[0] = b01 * det;
            R[1] = (-a22 * a01 + a02 * a21) * det;
            R[2] = (a12 * a01 - a02 * a11) * det;
            R[3] = b11 * det;
            R[4] = (a22 * a00 - a02 * a20) * det;
            R[5] = (-a12 * a00 + a02 * a10) * det;
            R[6] = b21 * det;
            R[7] = (-a21 * a00 + a01 * a20) * det;
            R[8] = (a11 * a00 - a01 * a10) * det;

            return true;
        }

        /// <summary>
        /// Turn this into an indentity matrix, representing no transformation. 
        /// </summary>
        public void Identity()
        {
            R[0] = 1; R[1] = 0; R[2] = 0;
            R[3] = 0; R[4] = 1; R[5] = 0;
            R[6] = 0; R[7] = 0; R[8] = 1;
        }

        /// <summary>
        /// Multiplies two rotation matrices.
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        public static RotationMatrix Multiply(RotationMatrix m1, RotationMatrix m2)
        {
            RotationMatrix m = new RotationMatrix();

            m.R[0] = m1.R[0] * m2.R[0] + m1.R[1] * m2.R[3] + m1.R[2] * m2.R[6];
            m.R[1] = m1.R[0] * m2.R[1] + m1.R[1] * m2.R[4] + m1.R[2] * m2.R[7];
            m.R[2] = m1.R[0] * m2.R[2] + m1.R[1] * m2.R[5] + m1.R[2] * m2.R[8];
            m.R[3] = m1.R[3] * m2.R[0] + m1.R[4] * m2.R[3] + m1.R[5] * m2.R[6];
            m.R[4] = m1.R[3] * m2.R[1] + m1.R[4] * m2.R[4] + m1.R[5] * m2.R[7];
            m.R[5] = m1.R[3] * m2.R[2] + m1.R[4] * m2.R[5] + m1.R[5] * m2.R[8];
            m.R[6] = m1.R[6] * m2.R[0] + m1.R[7] * m2.R[3] + m1.R[8] * m2.R[6];
            m.R[7] = m1.R[6] * m2.R[1] + m1.R[7] * m2.R[4] + m1.R[8] * m2.R[7];
            m.R[8] = m1.R[6] * m2.R[2] + m1.R[7] * m2.R[5] + m1.R[8] * m2.R[8];

            return m;
        }



        /// <summary>
        /// Returns a Quaternion representing the same rotation as this Matrix.
        /// </summary>
        /// <returns></returns>
        public Quaternion ToQuaternion()
        {
            // This conversion assumes the rotation matrix is special orthogonal.
            // As a result, the returned Quaternion will be a versor.
            // Based on http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
            double trace = m00 + m11 + m22;
            Quaternion q = new Quaternion();
            double s;

            // Compute a regular conversion
            if (trace > 0)
            {
                s = 2 * Math.Sqrt(trace + 1);
                q.W = 0.25 * s;
                q.X = (m21 - m12) / s;
                q.Y = (m02 - m20) / s;
                q.Z = (m10 - m01) / s;
            }

            // If trace is zero or negative, avoid division by zero, square root of negative and floating-point degeneracy
            // by searching which major diagonal element has the greatest value:
            else
            {
                if (m00 > m11 && m00 > m22)
                {
                    s = 2 * Math.Sqrt(1 + m00 - m11 - m22);
                    q.W = (m21 - m12) / s;
                    q.X = 0.25 * s;
                    q.Y = (m01 + m10) / s;
                    q.Z = (m02 + m20) / s;
                }
                else if (m11 > m22)
                {
                    s = 2 * Math.Sqrt(1 + m11 - m00 - m22);
                    q.W = (m02 - m20) / s;
                    q.X = (m01 + m10) / s;
                    q.Y = 0.25 * s;
                    q.Z = (m12 + m21) / s;
                }
                else
                {
                    s = 2 * Math.Sqrt(1 + m22 - m00 - m11);
                    q.W = (m10 - m01) / s;
                    q.X = (m02 + m20) / s;
                    q.Y = (m12 + m21) / s;
                    q.Z = 0.25 * s;
                }
            }


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

            return q;
        }


        /// <summary>
        /// Returns an Axis Angle representation of this Rotation Matrix. 
        /// Note that the returned Axis Angle will always represent a positive rotation between [0, 180]
        /// </summary>
        /// <returns></returns>
        public AxisAngle ToAxisAngle()
        {
            // Taken from http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToAngle/index.htm
            double x, y, z, angle;
            double sinv = 1 / Math.Sqrt(2);  // 0.7071...

            // Check for singularities
            if (Math.Abs(m01 - m10) < EPSILON
                && Math.Abs(m02 - m20) < EPSILON
                && Math.Abs(m12 - m21) < EPSILON)
            {
                // If identity matrix (angle = 0), return a non rotation AxisAngle
                if (this.IsIdentity())
                {
                    return new AxisAngle();
                }
                // Otherwise, angle is 180
                angle = 180;
                double xx = 0.5 * m00 + 0.5;
                double yy = 0.5 * m11 + 0.5;
                double zz = 0.5 * m22 + 0.5;
                double xy = 0.25 * (m01 + m10);
                double xz = 0.25 * (m02 + m20);
                double yz = 0.25 * (m12 + m21);

                // If m00 is the largest diagonal
                if (xx > yy && xx > zz)
                {
                    if (xx < EPSILON)
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
                    if (yy < EPSILON)
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
                    if (zz < EPSILON)
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
            double s = Math.Sqrt((m21 - m12) * (m21 - m12)
                    + (m02 - m20) * (m02 - m20)
                    + (m10 - m01) * (m10 - m01));  // for normalization (is this necessary here?)
            if (Math.Abs(s) < EPSILON) s = 1;  // "prevent divide by zero, should not happen if matrix is orthogonal and should be caught by singularity test above, but I've left it in just in case"
            angle = TO_DEGS * Math.Acos(0.5 * (m00 + m11 + m22 - 1));
            x = (m21 - m12) / s;
            y = (m02 - m20) / s;
            z = (m10 - m01) / s;

            return new AxisAngle(x, y, z, angle, false);
        }

        /// <summary>
        /// Returns a Rotation Vector representation of this Matrix.
        /// </summary>
        /// <returns></returns>
        public RotationVector ToRotationVector()
        {
            return this.ToAxisAngle().ToRotationVector();
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
            if (this.m20 < -1 + EPSILON3)
            {
                xAng = 0;
                yAng = 0.5 * Math.PI;
                zAng = Math.Atan2(this.m12, this.m02);
                if (zAng < -Math.PI) zAng += TAU;  // remap to [-180, 180]
                else if (zAng > Math.PI) zAng -= TAU;
            }

            // South pole singularity (yAng ~ -90degs)? Note m02 is -sin(y) = -sin(-90) = 1
            else if (this.m20 > 1 - EPSILON3) 
            {
                xAng = 0;
                yAng = -0.5 * Math.PI;
                zAng = Math.Atan2(-this.m12, -this.m02);
                if (zAng < -Math.PI) zAng += TAU;  // remap to [-180, 180]
                else if (zAng > Math.PI) zAng -= TAU;
            }
            
            // Regular derivation
            else
            {
                xAng = Math.Atan2(this.m21, this.m22);
                yAng = -Math.Asin(this.m20);
                zAng = Math.Atan2(this.m10, this.m00);
            }

            return new YawPitchRoll(TO_DEGS * xAng, TO_DEGS * yAng, TO_DEGS * zAng);
        }




        public override string ToString()
        {
            return string.Format("RotationMatrix[[{0}, {1}, {2}], [{3}, {4}, {5}], [{6}, {7}, {8}]]",
                Math.Round(m00, STRING_ROUND_DECIMALS_MM),
                Math.Round(m01, STRING_ROUND_DECIMALS_MM),
                Math.Round(m02, STRING_ROUND_DECIMALS_MM),
                Math.Round(m10, STRING_ROUND_DECIMALS_MM),
                Math.Round(m11, STRING_ROUND_DECIMALS_MM),
                Math.Round(m12, STRING_ROUND_DECIMALS_MM),
                Math.Round(m20, STRING_ROUND_DECIMALS_MM),
                Math.Round(m21, STRING_ROUND_DECIMALS_MM),
                Math.Round(m22, STRING_ROUND_DECIMALS_MM));
        }

    }
}
