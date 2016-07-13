//██╗   ██╗████████╗██╗██╗     
//██║   ██║╚══██╔══╝██║██║     
//██║   ██║   ██║   ██║██║     
//██║   ██║   ██║   ██║██║     
//╚██████╔╝   ██║   ██║███████╗
// ╚═════╝    ╚═╝   ╚═╝╚══════╝
/// <summary>
/// A bunch of static utility functions (probably many of them could be moved to certain classes...
/// </summary>


namespace RobotControl
{
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

        /// <summary>
        /// Returns the sqaured distance between two points.
        /// </summary>
        /// <ref>https://github.com/imshz/simplify-net</ref>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double GetSquareDistance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X,
                dy = p1.Y - p2.Y,
                dz = p1.Z - p2.Z;

            return (dx * dx) + (dy * dy) + (dz * dz);
        }

        /// <summary>
        /// Returns the distance from 'p' to the segment 'p1-p2'.
        /// </summary>
        /// <ref>https://github.com/imshz/simplify-net</ref>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double GetSquareSegmentDistance(Point p, Point p1, Point p2)
        {
            var x = p1.X;
            var y = p1.Y;
            var z = p1.Z;
            var dx = p2.X - x;
            var dy = p2.Y - y;
            var dz = p2.Z - z;

            if (!dx.Equals(0.0) || !dy.Equals(0.0) || !dz.Equals(0.0))
            {
                var t = ((p.X - x) * dx + (p.Y - y) * dy + (p.Z - z) * dz) / (dx * dx + dy * dy + dz * dz);

                if (t > 1)
                {
                    x = p2.X;
                    y = p2.Y;
                    z = p2.Z;
                }
                else if (t > 0)
                {
                    x += dx * t;
                    y += dy * t;
                    z += dz * t;
                }
            }

            dx = p.X - x;
            dy = p.Y - y;
            dz = p.Z - z;

            return (dx * dx) + (dy * dy) + (dz * dz);
        }

    }
    
}
