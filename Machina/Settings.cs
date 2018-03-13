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
        public int Speed;
        public int Precision;
        public MotionType MotionType;
        public ReferenceCS RefCS;
        public double ExtrusionRate;


        public Settings(int speed, int precision, MotionType mType, ReferenceCS refcs, double extrusionRate)
        {
            Speed = speed;
            Precision = precision;
            MotionType = mType;
            RefCS = refcs;
            this.ExtrusionRate = extrusionRate;
        }

        public Settings Clone() => new Settings(Speed, Precision, MotionType, RefCS, ExtrusionRate);

        public override string ToString() => $"{RefCS} {MotionType} {Speed}-{Precision} {ExtrusionRate}";
        
    }


}
