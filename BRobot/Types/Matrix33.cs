using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRobot
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
    public class Matrix33 : Geometry
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
        /// Create a 3x3 Rotation Matrix from it's constituent components. 
        /// Elements of the rotation matrix, ordered in row to column way, 
        /// i.e. r[2] is r13 (row 1 col 3), r[6] is r31, etc. 
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
        public Matrix33(double r00, double r01, double r02,
                        double r10, double r11, double r12,
                        double r20, double r21, double r22)
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
        }

        /// <summary>
        /// Create a 3x3 Rotation Matrix from it's constituent components. 
        /// Elements of the rotation matrix, ordered in row to column way, 
        /// i.e. r[2] is r13 (row 1 col 3), r[6] is r31, etc. 
        /// </summary>
        /// <param name="rotationValues"></param>
        public Matrix33(double[] rotationValues)
        {
            R = new double[9];

            int i;
            for (i = 0; i < rotationValues.Length || i < 9; i++)
            {
                R[i] = rotationValues[i];
            } 

            // If rotationValues had less than 9 elements
            while (i < 9)
            {
                R[i++] = 0;
            }
        }

        /// <summary>
        /// Create a 3x3 Rotation Matrix as a shallow copy of another.
        /// </summary>
        /// <param name="rotationMatrix"></param>
        public Matrix33(Matrix33 rotationMatrix)
        {
            R = new double[9];

            // Make a shallow copy
            for (var i = 0; i < 9; i++)
            {
                R[i] = rotationMatrix.R[i];
            }
        }

        //public bool IsOrthogonal()
        //{

        //}

        /// <summary>
        /// Returns a Quaternion representing the same rotation as this Matrix.
        /// </summary>
        /// <returns></returns>
        public Quaternion ToQuaternion()
        {
            // This conversion assumes the rotation matrix is special orthogonal 
            // Based on http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
            double trace = m00 + m11 + m22;
            Quaternion q = new Quaternion();
            double s;

            // Compute a regular conversion
            if (trace > EPSILON)
            {
                s = 2 * Math.Sqrt(trace + 1);
                q.W = 0.25 * s;
                q.X = (m21 - m12) / s;
                q.Y = (m02 - m20) / s;
                q.Z = (m10 - m01) / s;
            }

            // If trace is close to zero, avoid division by zero and floating-point degeneracy
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

            return q;
        }



    }
}
