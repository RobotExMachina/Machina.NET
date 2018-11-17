using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Machina
{
    //  ███████╗██╗  ██╗████████╗███████╗██████╗ ███╗   ██╗ █████╗ ██╗      █████╗ ██╗  ██╗███████╗███████╗
    //  ██╔════╝╚██╗██╔╝╚══██╔══╝██╔════╝██╔══██╗████╗  ██║██╔══██╗██║     ██╔══██╗╚██╗██╔╝██╔════╝██╔════╝
    //  █████╗   ╚███╔╝    ██║   █████╗  ██████╔╝██╔██╗ ██║███████║██║     ███████║ ╚███╔╝ █████╗  ███████╗
    //  ██╔══╝   ██╔██╗    ██║   ██╔══╝  ██╔══██╗██║╚██╗██║██╔══██║██║     ██╔══██║ ██╔██╗ ██╔══╝  ╚════██║
    //  ███████╗██╔╝ ██╗   ██║   ███████╗██║  ██║██║ ╚████║██║  ██║███████╗██║  ██║██╔╝ ██╗███████╗███████║
    //  ╚══════╝╚═╝  ╚═╝   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝╚══════╝
    //                                                                                                     
    public class ExternalAxes : Geometry
    {
        private double?[] _externalAxes = new double?[6];
                
        // Some quick aliases (is the zero-based array index confusing with the one-index typical joint notation?)
        public double? EA1 { get { return this._externalAxes[0]; } set { this._externalAxes[0] = FilterValue(value); } }
        public double? EA2 { get { return this._externalAxes[1]; } set { this._externalAxes[1] = FilterValue(value); } }
        public double? EA3 { get { return this._externalAxes[2]; } set { this._externalAxes[2] = FilterValue(value); } }
        public double? EA4 { get { return this._externalAxes[3]; } set { this._externalAxes[3] = FilterValue(value); } }
        public double? EA5 { get { return this._externalAxes[4]; } set { this._externalAxes[4] = FilterValue(value); } }
        public double? EA6 { get { return this._externalAxes[5]; } set { this._externalAxes[5] = FilterValue(value); } }

        public double? this[int i]
        {
            get
            {
                return this._externalAxes[i];
            }
            set
            {
                this._externalAxes[i] = value;
            }
        }

        public int Length { get { return this._externalAxes.Length; } }

        /// <summary>
        /// Create an empty object with all axes null.
        /// </summary>
        public ExternalAxes()
        {
            for (int i = 0; i < _externalAxes.Length; i++)
            {
                this._externalAxes[i] = null;
            }
        }

        /// <summary>
        /// Create a new instance as a copy.
        /// </summary>
        /// <param name="extAx"></param>
        public ExternalAxes(ExternalAxes extAx)
        {
            for (int i = 0; i < extAx.Length; i++)
            {
                this._externalAxes[i] = extAx[i];
            }
        }

        public ExternalAxes(double? exa1 = null, double? exa2 = null, double? exa3 = null, double? exa4 = null, double? exa5 = null, double? exa6 = null)
        {
            this._externalAxes[0] = FilterValue(exa1);
            this._externalAxes[1] = FilterValue(exa2);
            this._externalAxes[2] = FilterValue(exa3);
            this._externalAxes[3] = FilterValue(exa4);
            this._externalAxes[4] = FilterValue(exa5);
            this._externalAxes[5] = FilterValue(exa6);
        }

        public double?[] GetDoubleArray()
        {
            return this._externalAxes;
        }

        // For ABB robots, a value of 9E9 means no axis
        private double? FilterValue(double? val)
        {
            return (val == null || val >= 9000000000) ? null : val;
        }

        
        public override string ToString()
        {
            return this.ToString(false);
        }

        public string ToString(bool labels)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0}[{1}{2}, {3}{4}, {5}{6}, {7}{8}, {9}{10}, {11}{12}]",
                labels ? "ExtAxes" : "",
                labels ? "EA1:" : "",
                this._externalAxes[0] == null
                    ? "null"
                    : Math.Round((double) this._externalAxes[0], STRING_ROUND_DECIMALS_MM).ToString(),
                labels ? "EA2:" : "",
                this._externalAxes[1] == null
                    ? "null"
                    : Math.Round((double) this._externalAxes[1], STRING_ROUND_DECIMALS_MM).ToString(),
                labels ? "EA3:" : "",
                this._externalAxes[2] == null
                    ? "null"
                    : Math.Round((double) this._externalAxes[2], STRING_ROUND_DECIMALS_MM).ToString(),
                labels ? "EA4:" : "",
                this._externalAxes[3] == null
                    ? "null"
                    : Math.Round((double) this._externalAxes[3], STRING_ROUND_DECIMALS_MM).ToString(),
                labels ? "EA5:" : "",
                this._externalAxes[4] == null
                    ? "null"
                    : Math.Round((double) this._externalAxes[4], STRING_ROUND_DECIMALS_MM).ToString(),
                labels ? "EA6:" : "",
                this._externalAxes[5] == null
                    ? "null"
                    : Math.Round((double) this._externalAxes[5], STRING_ROUND_DECIMALS_MM).ToString());
        }

        public string ToArrayString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "[{0},{1},{2},{3},{4},{5}]",
                this._externalAxes[0] == null
                    ? "null"
                    : Math.Round((double) this._externalAxes[0], STRING_ROUND_DECIMALS_MM).ToString(),
                this._externalAxes[1] == null
                    ? "null"
                    : Math.Round((double) this._externalAxes[1], STRING_ROUND_DECIMALS_MM).ToString(),
                this._externalAxes[2] == null
                    ? "null"
                    : Math.Round((double) this._externalAxes[2], STRING_ROUND_DECIMALS_MM).ToString(),
                this._externalAxes[3] == null
                    ? "null"
                    : Math.Round((double) this._externalAxes[3], STRING_ROUND_DECIMALS_MM).ToString(),
                this._externalAxes[4] == null
                    ? "null"
                    : Math.Round((double) this._externalAxes[4], STRING_ROUND_DECIMALS_MM).ToString(),
                this._externalAxes[5] == null
                    ? "null"
                    : Math.Round((double) this._externalAxes[5], STRING_ROUND_DECIMALS_MM).ToString());
        }
    }
}
