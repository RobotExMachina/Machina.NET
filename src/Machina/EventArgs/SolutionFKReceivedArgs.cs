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

        // Shuld probably replace pos + ori with a matrix...
        public Vector Position { get; }

        public Orientation Orientation { get; }

        public ConfigurationABB Configuration { get; }

        public SolutionFKReceivedArgs(int id, Axes axes, Vector pos, Orientation ori, ConfigurationABB conf)
        {
            ActionId = id;
            Axes = axes;
            Position = pos;
            Orientation = ori;
            Configuration = conf;
        }

        public override string ToJSONString()
        {
            return string.Format("{{\"event\":\"solution-fk\",\"id\":{0},\"pos\":{1},\"ori\":{2},\"quat\":{3},\"axes\":{4},\"conf\":{5}}}",
                ActionId,
                Position.ToArrayString(MMath.STRING_ROUND_DECIMALS_MM),
                Orientation.ToArrayString(MMath.STRING_ROUND_DECIMALS_MM),
                Orientation.ToQuaternion().ToArrayString(MMath.STRING_ROUND_DECIMALS_QUAT),
                Axes.ToArrayString(MMath.STRING_ROUND_DECIMALS_DEGS),
                Configuration.ToArrayString(0));
        }
    }
}
