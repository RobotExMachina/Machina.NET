using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
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
            this.Speed = speed;
            this.Acceleration = acc;
            this.Precision = precision;
            this.MotionType = mType;
            this.RefCS = refcs;
            this.ExtrusionRate = extrusionRate;
        }

        public Settings Clone() => new Settings(Speed, Acceleration, Precision, MotionType, RefCS, ExtrusionRate);

        public override string ToString() => $"{RefCS} {MotionType} {Speed}-{Acceleration}-{Precision} {ExtrusionRate}";
    }
}
