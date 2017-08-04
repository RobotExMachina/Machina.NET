using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    /// <summary>
    /// A buffer manager for Settings objects.
    /// </summary>
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
            int it = 0;
            foreach (Settings s in buffer) Console.WriteLine(it++ + ": " + s);
        }

    }
}
