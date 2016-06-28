using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl
{
    internal class StreamQueue
    {
        private List<Frame> sentTargets;
        private List<Frame> queuedTargets;

        public StreamQueue()
        {
            this.sentTargets = new List<Frame>();
            this.queuedTargets = new List<Frame>();
        }

        public void Add(Frame f)
        {
            this.queuedTargets.Add(f);
        }

        public void Add(double x, double y, double z, double vel, double zon)
        {
            Add( new Frame(x, y, z, vel, zon) );
        }

        public Frame GetNext()
        {
            if (queuedTargets.Count == 0) return null;

            sentTargets.Add(queuedTargets[0]);
            queuedTargets.RemoveAt(0);

            return sentTargets.Last();
        }

        public bool AreFramesPending()
        {
            return queuedTargets.Count > 0;
        }

        public bool EmptyQueue()
        {
            bool rem = AreFramesPending();
            queuedTargets.Clear();
            return rem;
        }
        

    }

}
