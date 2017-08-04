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
        
        public Settings(int speed, int zone, MotionType mType, ReferenceCS refcs)
        {
            Speed = speed;
            Zone = zone;
            MotionType = mType;
            RefCS = refcs;
        }

        public Settings Clone()
        {
            return new Settings(Speed, Zone, MotionType, RefCS);
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}-{3}", RefCS, MotionType, Speed, Zone);
        }
    }


}
