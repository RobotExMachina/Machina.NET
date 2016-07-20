using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl
{
    /// <summary>
    /// A class that manages a FIFO list of Actions.
    /// </summary>
    internal class ActionBuffer
    {
        /// <summary>
        /// Actions pending to be released.
        /// </summary>
        private List<Action> bufferedActions;

        /// <summary>
        /// Keep track of past released actions.
        /// </summary>
        private List<Action> pastActions;

        /// <summary>
        /// Main constructor.
        /// </summary>
        public ActionBuffer()
        {
            pastActions = new List<Action>();
            bufferedActions = new List<Action>();
        }

        /// <summary>
        /// Add an Action to the pending buffer.
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public bool Add(Action act)
        {
            bufferedActions.Add(act); 
            return true;
        }

        /// <summary>
        /// Release the next pending Action with the highest priority.
        /// </summary>
        /// <returns></returns>
        public Action GetNext()
        {
            if (bufferedActions.Count == 0) return null;

            pastActions.Add(bufferedActions[0]);
            bufferedActions.RemoveAt(0);

            return pastActions.Last();
        }

        /// <summary>
        /// Release all pending Actions in the order they were issued. 
        /// </summary>
        /// <returns></returns>
        public List<Action> GetAllPending()
        {
            List<Action> pending = new List<Action>();
            foreach (Action a in bufferedActions) pending.Add(a);  // shallow copy
            pastActions.AddRange(bufferedActions);
            bufferedActions.Clear();
            return pending;
        }

        /// <summary>
        /// Is there any Action pending in the buffer?
        /// </summary>
        /// <returns></returns>
        public bool AreActionsPending()
        {
            return bufferedActions.Count > 0;
        }

        /// <summary>
        /// How many Actions are pending in the buffer?
        /// </summary>
        /// <returns></returns>
        public int ActionsPending()
        {
            return bufferedActions.Count;
        }

        /// <summary>
        /// Has any Action ever been issued to this buffer?
        /// </summary>
        /// <returns></returns>
        public bool IsVirgin()
        {
            return pastActions.Count == 0 && bufferedActions.Count == 0;
        }

        /// <summary>
        /// Clear all buffered and past released Actions.
        /// </summary>
        public void Flush()
        {
            pastActions.Clear();
            bufferedActions.Clear();
        }

        /// <summary>
        /// Writes a description of each pending Action to the Console.
        /// </summary>
        public void LogBufferedActions()
        {
            foreach (Action a in bufferedActions) Console.WriteLine(a);
        }

        public override string ToString()
        {
            return string.Format("ACTION BUFFER: {0} issued, {1} remaining", pastActions.Count, bufferedActions.Count);
        }
    }
}
