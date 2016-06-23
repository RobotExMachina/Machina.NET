using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl
{
    public class Util
    {
        public static double Remap(double val, double min, double max, double newMin, double newMax)
        {
            return newMin + (val - min) * (newMax - newMin) / (max - min);
        }
    }

    
}
