using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{

    //   ██████╗ ███████╗ ██████╗ ███╗   ███╗███████╗████████╗██████╗ ██╗   ██╗
    //  ██╔════╝ ██╔════╝██╔═══██╗████╗ ████║██╔════╝╚══██╔══╝██╔══██╗╚██╗ ██╔╝
    //  ██║  ███╗█████╗  ██║   ██║██╔████╔██║█████╗     ██║   ██████╔╝ ╚████╔╝ 
    //  ██║   ██║██╔══╝  ██║   ██║██║╚██╔╝██║██╔══╝     ██║   ██╔══██╗  ╚██╔╝  
    //  ╚██████╔╝███████╗╚██████╔╝██║ ╚═╝ ██║███████╗   ██║   ██║  ██║   ██║   
    //   ╚═════╝ ╚══════╝ ╚═════╝ ╚═╝     ╚═╝╚══════╝   ╚═╝   ╚═╝  ╚═╝   ╚═╝   
    //                                                                         
    /// <summary>
    /// Base abstract class that all Geometry objects inherit from.
    /// </summary>
    public abstract class Geometry
    {
        /// <summary>
        /// Precision for floating-point comparisons.
        /// </summary>
        public static readonly double EPSILON = 0.000001;

        /// <summary>
        /// A more permissive precision factor.
        /// </summary>
        public static readonly double EPSILON2 = 0.001;

        /// <summary>
        /// A more restrictive precision factor.
        /// </summary>
        public static readonly double EPSILON3 = 0.000000001;

        /// <summary>
        /// Amount of digits for floating-point comparisons precision.
        /// </summary>
        public static readonly int EPSILON_DECIMALS = 10;

        /// <summary>
        /// Amount of decimals for rounding on ToString() operations.
        /// </summary>
        public static readonly int STRING_ROUND_DECIMALS_M = 6;
        public static readonly int STRING_ROUND_DECIMALS_MM = 3;
        public static readonly int STRING_ROUND_DECIMALS_DEGS = 3;
        public static readonly int STRING_ROUND_DECIMALS_QUAT = 4;
        public static readonly int STRING_ROUND_DECIMALS_RADS = 6;
        public static readonly int STRING_ROUND_DECIMALS_VOLTAGE = 3;
        public static readonly int STRING_ROUND_DECIMALS_TEMPERATURE = 0;
        public static readonly int STRING_ROUND_DECIMALS_KG = 3;
        public static readonly int STRING_ROUND_DECIMALS_VECTOR = 5;              // Hopefully for unit vectors and stuff

        // Angle conversion
        public static readonly double TO_DEGS = 180.0 / Math.PI;
        public static readonly double TO_RADS = Math.PI / 180.0;

        // TAU ;)
        public static readonly double TAU = 2 * Math.PI;



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

        public static double SqLength(double x, double y)
        {
            return x * x + y * y;
        }

        public static double SqLength(double x, double y, double z)
        {
            return x * x + y * y + z * z;
        }

        public static double SqLength(double w, double x, double y, double z)
        {
            return w * w + x * x + y * y + z * z;
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
            return targetStart + (targetEnd - targetStart) * (value - sourceStart) / (sourceEnd - sourceStart);
        }
    }
}
