using Machina.Types.Data;
using Machina.Types.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.EventArgs
{
    /// <summary>
    /// Argument for SolutionFKReceived events.
    /// </summary>
    public class SolutionFKReceivedArgs : MachinaEventArgs
    {
        /// <summary>
        /// Id of the action that requested the calculation.
        /// </summary>
        public int ActionId { get; }

        /// <summary>
        /// Axes used for the request.
        /// </summary>
        public Axes Axes { get; }

        /// <summary>
        /// TCP computed by the robot's FK solver.
        /// </summary>
        public Matrix TCP { get; }

        /// <summary>
        /// Robot configuration returned by the robot.
        /// </summary>
        public ConfigurationABB Configuration { get; }

        public SolutionFKReceivedArgs(int id, Axes axes, Matrix tcp, ConfigurationABB conf)
        {
            ActionId = id;
            Axes = axes;
            TCP = tcp;
            Configuration = conf;
        }

        public override string ToJSONString()
        {
            return string.Format("{{\"event\":\"solution-fk\",\"id\":{0},\"tcp\":{1},\"axes\":{2},\"conf\":{3}}}",
                ActionId,
                TCP.ToArrayString(MMath.STRING_ROUND_DECIMALS_MM),
                Axes.ToArrayString(MMath.STRING_ROUND_DECIMALS_DEGS),
                Configuration.ToArrayString(0));
        }
    }
}
