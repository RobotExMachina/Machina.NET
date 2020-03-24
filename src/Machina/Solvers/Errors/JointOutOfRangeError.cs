using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Solvers.Errors
{
    public class JointOutOfRangeError : SolverError
    {
        public JointOutOfRangeError(string msg) : base(msg) {
            ErrorType = SolverErrorType.JointOutOfRange;
        }
    }
}
