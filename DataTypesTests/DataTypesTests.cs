using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypesTests
{
    public class DataTypesTests
    {
        internal static Random rnd = new System.Random();

        public double Random(double min, double max)
        {
            return Lerp(min, max, rnd.NextDouble());
        }

        public double Random()
        {
            return Random(0, 1);
        }

        public double Random(double max)
        {
            return Random(0, max);
        }

        public double Lerp(double start, double end, double norm)
        {
            return start + (end - start) * norm;
        }

        public double Normalize(double value, double start, double end)
        {
            return (value - start) / (end - start);
        }

        public double Map(double value, double sourceStart, double sourceEnd, double targetStart, double targetEnd)
        {
            //double n = Normalize(value, sourceStart, sourceEnd);
            //return targetStart + n * (targetEnd - targetStart);
            return targetStart + (targetEnd - targetStart) * (value - sourceStart) / (sourceEnd - sourceStart);
        }
    }
}
