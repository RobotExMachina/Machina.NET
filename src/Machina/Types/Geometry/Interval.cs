using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Types.Geometry
{
    //  ██╗███╗   ██╗████████╗███████╗██████╗ ██╗   ██╗ █████╗ ██╗     
    //  ██║████╗  ██║╚══██╔══╝██╔════╝██╔══██╗██║   ██║██╔══██╗██║     
    //  ██║██╔██╗ ██║   ██║   █████╗  ██████╔╝██║   ██║███████║██║     
    //  ██║██║╚██╗██║   ██║   ██╔══╝  ██╔══██╗╚██╗ ██╔╝██╔══██║██║     
    //  ██║██║ ╚████║   ██║   ███████╗██║  ██║ ╚████╔╝ ██║  ██║███████╗
    //  ╚═╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝╚═╝  ╚═╝  ╚═══╝  ╚═╝  ╚═╝╚══════╝
    //                                                                 
    /// <summary>
    /// Represents a numerical interval between two extremes. 
    /// </summary>
    public struct Interval
    {
        #region Public Fields
        /// <summary>
        /// Initial value of this interval.
        /// </summary>
        public double Start { get; set; }

        /// <summary>
        /// End value of this interval.
        /// </summary>
        public double End { get; set; }

        /// <summary>
        /// Get signed length.
        /// </summary>
        public double Length => End - Start;

        /// <summary>
        /// Gets max of two extremes.
        /// </summary>
        public double Max => End > Start ? End : Start;

        /// <summary>
        /// Gets min of two extremes. 
        /// </summary>
        public double Min => End > Start ? Start : End;
        #endregion Public Fields

        private static readonly Interval _zero = new Interval(0, 0);

        /// <summary>
        /// Returns a [0, 0] interval.
        /// </summary>
        /// <returns></returns>
        public static Interval Zero => _zero;

        /// <summary>
        /// Create new Interval.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Interval(double start, double end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Multiplies the extremes of this interval by a factor.
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static Interval operator *(double factor, Interval interval)
        {
            return new Interval(interval.Start * factor, interval.End * factor);
        }

        /// <summary>
        /// Multiplies the extremes of this interval by a factor.
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Interval operator *(Interval interval, double factor)
        {
            return new Interval(interval.Start * factor, interval.End * factor);
        }

        /// <summary>
        /// Tests a value for inclusion in the interval domain.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool IncludesParameter(double val)
        {
            return val >= Min && val <= Max;
        }


        public override string ToString()
        {
            CultureInfo ci = CultureInfo.InvariantCulture;

            return String.Format(ci, "[{0}, {1}]", Start.ToString(ci), End.ToString(ci));
        }
    }
}
