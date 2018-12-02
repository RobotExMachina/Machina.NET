using System;

//  ██╗   ██╗████████╗██╗██╗     ██╗████████╗██╗███████╗███████╗
//  ██║   ██║╚══██╔══╝██║██║     ██║╚══██╔══╝██║██╔════╝██╔════╝
//  ██║   ██║   ██║   ██║██║     ██║   ██║   ██║█████╗  ███████╗
//  ██║   ██║   ██║   ██║██║     ██║   ██║   ██║██╔══╝  ╚════██║
//  ╚██████╔╝   ██║   ██║███████╗██║   ██║   ██║███████╗███████║
//   ╚═════╝    ╚═╝   ╚═╝╚══════╝╚═╝   ╚═╝   ╚═╝╚══════╝╚══════╝
//                                                              
//  ███╗   ██╗██╗   ██╗███╗   ███╗███████╗██████╗ ██╗ ██████╗   
//  ████╗  ██║██║   ██║████╗ ████║██╔════╝██╔══██╗██║██╔════╝   
//  ██╔██╗ ██║██║   ██║██╔████╔██║█████╗  ██████╔╝██║██║        
//  ██║╚██╗██║██║   ██║██║╚██╔╝██║██╔══╝  ██╔══██╗██║██║        
//  ██║ ╚████║╚██████╔╝██║ ╚═╝ ██║███████╗██║  ██║██║╚██████╗   
//  ╚═╝  ╚═══╝ ╚═════╝ ╚═╝     ╚═╝╚══════╝╚═╝  ╚═╝╚═╝ ╚═════╝   
//                                                              

/// <summary>
/// A bunch of static utility functions (probably many of them could be moved to certain related classes...)
/// </summary>
namespace Machina.Utilities
{
    /// <summary>
    /// Utility static methods
    /// </summary>
    public static class Numeric
    {
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
