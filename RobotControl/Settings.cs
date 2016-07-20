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

        public Settings Clone()
        {
            return new Settings(Velocity, Zone, MotionType);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}-{2}", MotionType, Velocity, Zone);
        }
    }

    class SettingsBuffer
    {
        private static int limit = 32;  // @todo: implement some kind of limit for too many pushes without pops?
        private List<Settings> buffer = new List<Settings>();

        public SettingsBuffer() { }

        public bool Push(Settings set)
        {
            if (buffer.Count >= limit) throw new Exception("TOO MANY PUSHES WITHOUT POPS?");
            buffer.Add(set);
            return true;
        }

        public Settings Pop()
        {
            if (buffer.Count > 0)
            {
                Settings s = buffer.Last();
                buffer.RemoveAt(buffer.Count - 1);
                return s;
            }
            return null;
        }

        public void LogBuffer()
        {
            foreach (Settings s in buffer) Console.WriteLine(s);
        }

    }
}
