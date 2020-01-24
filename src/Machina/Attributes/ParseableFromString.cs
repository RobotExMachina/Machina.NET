using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Attributes
{
    /// <summary>
    /// A `ParseableFromString` API method is one that can be invoked through reflection
    /// by parsing a string representation of a call to such method. 
    /// This attribute is used to flag those methods that can be called by typing the
    /// action with plain text, like the Bridge Console or a string from Grasshopper. 
    /// </summary>
    public class ParseableFromString : Attribute
    {
    }
}
