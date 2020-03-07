using System;
using System.Collections.Generic;
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
        /// Tests a value for inclusion in the interval domain.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool IncludesParameter(double val)
        {
            return val >= Min && val <= Max;
        }
    }
}
