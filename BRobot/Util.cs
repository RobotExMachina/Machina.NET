//██╗   ██╗████████╗██╗██╗     
//██║   ██║╚══██╔══╝██║██║     
//██║   ██║   ██║   ██║██║     
//██║   ██║   ██║   ██║██║     
//╚██████╔╝   ██║   ██║███████╗
// ╚═════╝    ╚═╝   ╚═╝╚══════╝
/// <summary>
/// A bunch of static utility functions (probably many of them could be moved to certain related classes...)
/// </summary>


namespace Machina
{
    /// <summary>
    /// Utility static methods
    /// </summary>
    public class Util
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
    }
    
}
