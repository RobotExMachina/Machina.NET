using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Types.Data
{
    /// <summary>
    /// A simple struct that defines the configuration of a 6-axis robot arm, 
    /// i.e. four numbers that represent the quadrants of their major axes.
    /// </summary>
    public struct ConfigurationABB : ISerializableArray
    {
        // Is this struct too ABB-specific? Should it be kept there?

        public int C1 { get; internal set; }
        public int C4 { get; internal set; }
        public int C6 { get; internal set; }
        public int CX { get; internal set; }

        public ConfigurationABB(int c1, int c4, int c6, int cx)
        {
            C1 = c1;
            C4 = c4;
            C6 = c6;
            CX = cx;
        }

        public string ToWhitespacedValues()
        {
            return $"{C1} {C4} {C6} {CX}";
        }

        public string ToArrayString(int decimals)
        {
            return $"[{C1},{C4},{C6},{CX}]";
        }
    }
}
