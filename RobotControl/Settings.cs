using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl
{
    class Settings
    {
        public int Velocity;
        public int Zone;
        public MotionType MotionType;

        public Settings(int vel, int zon, MotionType mType)
        {
            Velocity = vel;
            Zone = zon;
            MotionType = mType;
        }

    }
}
