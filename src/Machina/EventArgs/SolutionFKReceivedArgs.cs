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
    public class SolutionFKReceivedArgs : MachinaEventArgs
    {
        public int ActionId { get; }

        public Axes Axes { get; }

        public Matrix TCP { get; }

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
