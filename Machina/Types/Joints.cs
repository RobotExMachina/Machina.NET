using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{

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

        public static Joints Add(Joints j1, Joints j2)
        {
            return new Joints(j1.J1 + j2.J1,
                              j1.J2 + j2.J2,
                              j1.J3 + j2.J3,
                              j1.J4 + j2.J4,
                              j1.J5 + j2.J5,
                              j1.J6 + j2.J6);
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
}
