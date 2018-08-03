using System;
using System.Collections.Generic;
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
        public double? EA1 { get { return this._externalAxes[0]; } set { this._externalAxes[0] = value; } }
        public double? EA2 { get { return this._externalAxes[1]; } set { this._externalAxes[1] = value; } }
        public double? EA3 { get { return this._externalAxes[2]; } set { this._externalAxes[2] = value; } }
        public double? EA4 { get { return this._externalAxes[3]; } set { this._externalAxes[3] = value; } }
        public double? EA5 { get { return this._externalAxes[4]; } set { this._externalAxes[4] = value; } }
        public double? EA6 { get { return this._externalAxes[5]; } set { this._externalAxes[5] = value; } }

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


        public ExternalAxes()
        {
            for (int i = 0; i < _externalAxes.Length; i++)
            {
                this._externalAxes[i] = null;
            }
        }       

        public ExternalAxes(double? exa1, double? exa2, double? exa3, double? exa4, double? exa5, double? exa6)
        {
            this._externalAxes[0] = exa1;
            this._externalAxes[1] = exa2;
            this._externalAxes[2] = exa3;
            this._externalAxes[3] = exa4;
            this._externalAxes[4] = exa5;
            this._externalAxes[5] = exa6;
        }

        public double?[] GetDoubleArray()
        {
            return this._externalAxes;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1},{2},{3},{4},{5}]",

                this._externalAxes[0] == null ? "null" : Math.Round((double)this._externalAxes[0], STRING_ROUND_DECIMALS_MM).ToString(),
                this._externalAxes[1] == null ? "null" : Math.Round((double)this._externalAxes[1], STRING_ROUND_DECIMALS_MM).ToString(),
                this._externalAxes[2] == null ? "null" : Math.Round((double)this._externalAxes[2], STRING_ROUND_DECIMALS_MM).ToString(),
                this._externalAxes[3] == null ? "null" : Math.Round((double)this._externalAxes[3], STRING_ROUND_DECIMALS_MM).ToString(),
                this._externalAxes[4] == null ? "null" : Math.Round((double)this._externalAxes[4], STRING_ROUND_DECIMALS_MM).ToString(),
                this._externalAxes[5] == null ? "null" : Math.Round((double)this._externalAxes[5], STRING_ROUND_DECIMALS_MM).ToString()
            );
        }

        // For the sake of simmetry
        public string ToArrayString() => this.ToString();

    }
}
