using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRobot
{
    /// <summary>
    /// A queue manager for 'execution' mode and Path objects
    /// </summary>
    internal class Queue
    {
        private List<Path> sentPaths;
        private List<Path> queuedPaths;

        public Queue()
        {
            sentPaths = new List<Path>();
            queuedPaths = new List<Path>();
        }

        public void Add(Path path)
        {
            queuedPaths.Add(path);
        }

        public Path GetNext()
        {
            if (queuedPaths.Count == 0) return null;

            sentPaths.Add(queuedPaths[0]);
            queuedPaths.RemoveAt(0);

            return sentPaths.Last();
        }

        /// <summary>
        /// Returns true if there are elements pending on the queue
        /// </summary>
        /// <returns></returns>
        public bool ArePathsPending()
        {
            return queuedPaths.Count > 0;
        }

        public bool EmptyQueue()
        {
            bool rem = ArePathsPending();
            queuedPaths.Clear();
            return rem;
        }
        
    }

    
}
