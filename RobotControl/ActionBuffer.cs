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

        ActionBuffer()
        {
            pastActions = new List<Action>();
            bufferedActions = new List<Action>();
        }

        void Add(Action act)
        {
            bufferedActions.Add(act);
        }

        Action GetNext()
        {
            if (bufferedActions.Count == 0) return null;

            pastActions.Add(bufferedActions[0]);
            bufferedActions.RemoveAt(0);

            return pastActions.Last();
        }

        bool AreActionsPending()
        {
            return bufferedActions.Count > 0;
        }

        void Flush()
        {
            pastActions.Clear();
            bufferedActions.Clear();
        }

        public override string ToString()
        {
            return string.Format("ACTION BUFFER: {0} issued, {1} remaining", pastActions.Count, bufferedActions.Count);
        }
    }
}
