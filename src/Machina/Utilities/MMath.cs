using System;

namespace Machina
{
    //  ███╗   ███╗ █████╗  ██████╗██╗  ██╗██╗███╗   ██╗ █████╗ 
    //  ████╗ ████║██╔══██╗██╔════╝██║  ██║██║████╗  ██║██╔══██╗
    //  ██╔████╔██║███████║██║     ███████║██║██╔██╗ ██║███████║
    //  ██║╚██╔╝██║██╔══██║██║     ██╔══██║██║██║╚██╗██║██╔══██║
    //  ██║ ╚═╝ ██║██║  ██║╚██████╗██║  ██║██║██║ ╚████║██║  ██║
    //  ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝
    //                                                          
    //  ███╗   ███╗ █████╗ ████████╗██╗  ██╗                    
    //  ████╗ ████║██╔══██╗╚══██╔══╝██║  ██║                    
    //  ██╔████╔██║███████║   ██║   ███████║                    
    //  ██║╚██╔╝██║██╔══██║   ██║   ██╔══██║                    
    //  ██║ ╚═╝ ██║██║  ██║   ██║   ██║  ██║                    
    //  ╚═╝     ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝                    
    //                                                          
    /// <summary>
    /// Utility static numerical methods and constants.
    /// </summary>
    public static class MMath
    {
        /// <summary>
        /// A more permissive precision factor for floating-point comparisons.
        /// </summary>
        public const double EPSILON = 0.001;

        /// <summary>
        /// Precision for floating-point comparisons.
        /// </summary>
        public const double EPSILON2 = 0.000001;

        /// <summary>
        /// A more restrictive precision factor for floating-point comparisons.
        /// </summary>
        public const double EPSILON3 = 0.000000001;

        /// <summary>
        /// Amount of digits for floating-point comparisons precision.
        /// </summary>
        public const int EPSILON_DECIMALS = 10;

        // Amount of decimals for rounding on ToString() operations.
        public const int STRING_ROUND_DECIMALS_M = 6;
        public const int STRING_ROUND_DECIMALS_MM = 3;
        public const int STRING_ROUND_DECIMALS_DEGS = 3;
        public const int STRING_ROUND_DECIMALS_QUAT = 4;
        public const int STRING_ROUND_DECIMALS_RADS = 6;
        public const int STRING_ROUND_DECIMALS_VOLTAGE = 3;
        public const int STRING_ROUND_DECIMALS_TEMPERATURE = 0;
        public const int STRING_ROUND_DECIMALS_KG = 3;
        public const int STRING_ROUND_DECIMALS_VECTOR = 5;              // Hopefully for unit vectors and stuff

        // Angle conversion
        public const double TO_DEGS = 180.0 / Math.PI;
        public const double TO_RADS = Math.PI / 180.0;

        // TAU ;)
        public const double TAU = 2 * Math.PI;

        /// <summary>
        /// Gets the Machina standard Unset value. Use this value rather than Double.NaN when 
        /// a bogus floating point value is required.
        /// </summary>
        public const double UNSET_VALUE = -1.23432101234321e+308;  // from rhinocommon

        /// <summary>
        /// Gets the single precision floating point number that is considered 'unset' in Machina.
        /// </summary>
        public const float UNSET_VALUE_FLOAT = -1.234321e+38f;  // from rhinocommon

        /// <summary>
        /// Gets the Zero Tolerance constant (1.0e-12).
        /// </summary>
        public const double ZERO_TOLERANCE = 1.0e-12;  // from rhinocommon

        /// <summary>
        /// Represents a default value that is used when comparing square roots.
        /// This value is several orders of magnitude larger than <see cref="MMath.ZERO_TOLERANCE"/>.
        /// </summary>
        public const double EPSILON_SQRT = 1.490116119385000000e-8;  // from rhinocommon

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

        public static float Length(float x, float y, float z)
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
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


        public static bool Normalize(double x, double y, double z,
            out double newX, out double newY, out double newZ)
        {
            double len = Length(x, y, z);
            if (len < EPSILON2)
            {
                newX = x;
                newY = y;
                newZ = z;
                return false;
            }
            newX = x / len;
            newY = y / len;
            newZ = z / len;

            return true;
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

        /// <summary>
        /// Remaps a value from source to target numerical domains.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="newMin"></param>
        /// <param name="newMax"></param>
        /// <returns></returns>
        public static double Remap(double val, double min, double max, double newMin, double newMax)
        {
            return newMin + (val - min) * (newMax - newMin) / (max - min);
        }

        /// <summary>
        /// Compares two arrays of nullable doubles for similarity.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static bool AreSimilar(double?[] a, double?[] b, double epsilon)
        {
            if (a == null && b == null)
            {
                return true;
            }

            try
            {
                if (a.Length != b.Length) return false;

                double diff;
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] == null && b[i] == null) continue;
                    diff = (double)(a[i] - b[i]);
                    if (Math.Abs(diff) > epsilon)
                    {
                        return false;
                    }
                }

                // If here, they were similar
                return true;
            }
            catch
            {
                return false;
            }

        }

    }


    
}
