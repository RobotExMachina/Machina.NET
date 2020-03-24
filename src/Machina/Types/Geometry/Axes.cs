using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Types.Geometry
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
    public class Axes : Geometry, ISerializableArray
    {
        public double A1, A2, A3, A4, A5, A6;

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
                    case 0: return A1;
                    case 1: return A2;
                    case 2: return A3;
                    case 3: return A4;
                    case 4: return A5;
                    case 5: return A6;
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
                    case 0: A1 = value; break;
                    case 1: A2 = value; break;
                    case 2: A3 = value; break;
                    case 3: A4 = value; break;
                    case 4: A5 = value; break;
                    case 5: A6 = value; break;
                }
            }
        }


        public Axes()
        {
            this.A1 = 0;
            this.A2 = 0;
            this.A3 = 0;
            this.A4 = 0;
            this.A5 = 0;
            this.A6 = 0;
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
        public Axes(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            this.A1 = j1;
            this.A2 = j2;
            this.A3 = j3;
            this.A4 = j4;
            this.A5 = j5;
            this.A6 = j6;
        }

        public Axes(Axes j)
        {
            this.A1 = j.A1;
            this.A2 = j.A2;
            this.A3 = j.A3;
            this.A4 = j.A4;
            this.A5 = j.A5;
            this.A6 = j.A6;
        }

        public void Add(Axes j)
        {
            this.A1 += j.A1;
            this.A2 += j.A2;
            this.A3 += j.A3;
            this.A4 += j.A4;
            this.A5 += j.A5;
            this.A6 += j.A6;
        }

        public void Set(Axes j)
        {
            this.A1 = j.A1;
            this.A2 = j.A2;
            this.A3 = j.A3;
            this.A4 = j.A4;
            this.A5 = j.A5;
            this.A6 = j.A6;
        }

        public void Scale(double s)
        {
            this.A1 *= s;
            this.A2 *= s;
            this.A3 *= s;
            this.A4 *= s;
            this.A5 *= s;
            this.A6 *= s;
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
            return A1 * A1 + A2 * A2 + A3 * A3 + A4 * A4 + A5 * A5 + A6 * A6;
        }


        public static Axes Add(Axes j1, Axes j2)
        {
            return new Axes(j1.A1 + j2.A1,
                              j1.A2 + j2.A2,
                              j1.A3 + j2.A3,
                              j1.A4 + j2.A4,
                              j1.A5 + j2.A5,
                              j1.A6 + j2.A6);
        }

        /// <summary>
        /// Returns a random object with values between specified double ranges. 
        /// Useful for testing and debugging.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Axes RandomFromDoubles(double min, double max)
        {
            return new Axes(
                MMath.Random(min, max), 
                MMath.Random(min, max), 
                MMath.Random(min, max),
                MMath.Random(min, max),
                MMath.Random(min, max),
                MMath.Random(min, max));
        }

        /// <summary>
        /// Returns a random object with integer values between specified double ranges. 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Axes RandomFromInts(int min, int max)
        {
            return new Axes(
                MMath.RandomInt(min, max),
                MMath.RandomInt(min, max),
                MMath.RandomInt(min, max),
                MMath.RandomInt(min, max),
                MMath.RandomInt(min, max),
                MMath.RandomInt(min, max));
        }

        public List<double> ToList()
        {
            return new List<double> { A1, A2, A3, A4, A5, A6 };
        }

        public override string ToString()
        {
            return this.ToString(false);
        }

        public string ToString(bool labels)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0}[{1}{2}, {3}{4}, {5}{6}, {7}{8}, {9}{10}, {11}{12}]",
                labels ? "Axes" : "",
                labels ? "A1:" : "",
                Math.Round(A1, MMath.STRING_ROUND_DECIMALS_DEGS),
                labels ? "A2:" : "",
                Math.Round(A2, MMath.STRING_ROUND_DECIMALS_DEGS),
                labels ? "A3:" : "",
                Math.Round(A3, MMath.STRING_ROUND_DECIMALS_DEGS),
                labels ? "A4:" : "",
                Math.Round(A4, MMath.STRING_ROUND_DECIMALS_DEGS),
                labels ? "A5:" : "",
                Math.Round(A5, MMath.STRING_ROUND_DECIMALS_DEGS),
                labels ? "A6:" : "",
                Math.Round(A6, MMath.STRING_ROUND_DECIMALS_DEGS));
        }

        public string ToArrayString(int decimals)
        {
            if (decimals < 0)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    $"[{A1},{A2},{A3},{A4},{A5},{A6}]");
            }

            return string.Format(CultureInfo.InvariantCulture, 
                "[{0},{1},{2},{3},{4},{5}]",
                Math.Round(A1, decimals),
                Math.Round(A2, decimals),
                Math.Round(A3, decimals),
                Math.Round(A4, decimals),
                Math.Round(A5, decimals),
                Math.Round(A6, decimals));
        }

        public string ToWhitespacedValues()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0} {1} {2} {3} {4} {5}",
                Math.Round(A1, MMath.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(A2, MMath.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(A3, MMath.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(A4, MMath.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(A5, MMath.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(A6, MMath.STRING_ROUND_DECIMALS_DEGS));
        }

    }
}
