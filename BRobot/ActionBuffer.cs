using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRobot
{
    /// <summary>
    /// A class that manages a FIFO list of Actions.
    /// </summary>
    internal class ActionBuffer
    {
        /// <summary>
        /// Actions pending to be released.
        /// </summary>
        private List<Action> pending;

        /// <summary>
        /// Keep track of past released actions.
        /// </summary>
        private List<Action> released;

        /// <summary>
        /// Main constructor.
        /// </summary>
        public ActionBuffer()
        {
            released = new List<Action>();
            pending = new List<Action>();
        }

        /// <summary>
        /// Add an Action to the pending buffer.
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public bool Add(Action act)
        {
            pending.Add(act); 
            return true;
        }

        /// <summary>
        /// Release the next pending Action with the highest priority.
        /// </summary>
        /// <returns></returns>
        public Action GetNext()
        {
            if (pending.Count == 0) return null;

            released.Add(pending[0]);
            pending.RemoveAt(0);

            return released.Last();
        }

        /// <summary>
        /// Stores especified action in the released buffer.
        /// </summary>
        /// <param name="action"></param>
        public void Save(Action action)
        {
            released.Add(action);
        }

        /// <summary>
        /// Release all pending Actions in the order they were issued.
        /// </summary>
        /// <param name="flush">If true, pending actions will be flushed from the buffer and flagged as released</param>
        /// <returns></returns>
        public List<Action> GetAllPending(bool flush)
        {
            List<Action> pending = new List<Action>();
            foreach (Action a in this.pending) pending.Add(a);  // shallow copy
            if (flush)
            {
                released.AddRange(this.pending);
                this.pending.Clear();
            }
            return pending;
        }

        /// <summary>
        /// Release all pending Actions in the order they were issued.
        /// </summary>
        /// <returns></returns>
        public List<Action> GetAllPending()
        {
            return GetAllPending(true);
        }
        
        /// <summary>
        /// Is there any Action pending in the buffer?
        /// </summary>
        /// <returns></returns>
        public bool AreActionsPending()
        {
            return pending.Count > 0;
        }

        /// <summary>
        /// How many Actions are pending in the buffer?
        /// </summary>
        /// <returns></returns>
        public int ActionsPending()
        {
            return pending.Count;
        }

        /// <summary>
        /// Has any Action ever been issued to this buffer?
        /// </summary>
        /// <returns></returns>
        public bool IsVirgin()
        {
            return released.Count == 0 && pending.Count == 0;
        }

        /// <summary>
        /// Clear all buffered and past released Actions.
        /// </summary>
        public void Flush()
        {
            released.Clear();
            pending.Clear();
        }

        /// <summary>
        /// Writes a description of each pending Action to the Console.
        /// </summary>
        public void LogBufferedActions()
        {
            foreach (Action a in pending) Console.WriteLine(a);
        }

        public override string ToString()
        {
            return string.Format("ACTION BUFFER: {0} issued, {1} remaining", released.Count, pending.Count);
        }
    }
}
