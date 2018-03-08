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
        public int Zone;
        public MotionType MotionType;
        public ReferenceCS RefCS;
        public double ExtrusionRate;


        public Settings(int speed, int zone, MotionType mType, ReferenceCS refcs, double extrusionRate)
        {
            Speed = speed;
            Zone = zone;
            MotionType = mType;
            RefCS = refcs;
            this.ExtrusionRate = extrusionRate;
        }

        public Settings Clone()
        {
            return new Settings(Speed, Zone, MotionType, RefCS, ExtrusionRate);
        }

        public override string ToString()
        {
            return $"{RefCS} {MotionType} {Speed}-{Zone} {ExtrusionRate}";
        }
    }


}
