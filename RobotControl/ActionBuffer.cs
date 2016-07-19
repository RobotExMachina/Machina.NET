using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl
{
    /// <summary>
    /// 
    /// </summary>
    internal class ActionBuffer
    {
        private List<Action> pastActions;
        private List<Action> bufferedActions;

        public ActionBuffer()
        {
            pastActions = new List<Action>();
            bufferedActions = new List<Action>();
        }

        public bool Add(Action act)
        {
            bufferedActions.Add(act); 
            return true;
        }

        public Action GetNext()
        {
            if (bufferedActions.Count == 0) return null;

            pastActions.Add(bufferedActions[0]);
            bufferedActions.RemoveAt(0);

            return pastActions.Last();
        }

        public bool AreActionsPending()
        {
            return bufferedActions.Count > 0;
        }

        public int ActionsPending()
        {
            return bufferedActions.Count;
        }

        /// <summary>
        /// Has any action ever been issued to this buffer?
        /// </summary>
        /// <returns></returns>
        public bool IsVirgin()
        {
            return pastActions.Count == 0 && bufferedActions.Count == 0;
        }

        public void Flush()
        {
            pastActions.Clear();
            bufferedActions.Clear();
        }

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
