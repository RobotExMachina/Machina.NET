using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    /// <summary>
    /// Makes an object serializable into the Machina API instruction that would generate an equal instance of it.
    /// </summary>
    interface IInstructable
    {
        string ToInstruction();
    }
}
