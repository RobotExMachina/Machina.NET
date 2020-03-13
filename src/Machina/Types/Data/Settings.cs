using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Types.Data
{
    /// <summary>
    /// A class representing a Setting state, to be un/buffered.
    /// </summary>
    class Settings
    {
        public double Speed;
        public double Acceleration;
        public double Precision;
        public MotionType MotionType;
        public ReferenceCS RefCS;
        public double ExtrusionRate;

        public Settings(double speed, double acc, double precision, MotionType mType, ReferenceCS refcs, double extrusionRate)
        {
            Speed = speed;
            Acceleration = acc;
            Precision = precision;
            MotionType = mType;
            RefCS = refcs;
            ExtrusionRate = extrusionRate;
        }

        public Settings Clone() => new Settings(Speed, Acceleration, Precision, MotionType, RefCS, ExtrusionRate);

        public override string ToString() => $"{RefCS} {MotionType} {Speed}-{Acceleration}-{Precision} {ExtrusionRate}";
    }
}
