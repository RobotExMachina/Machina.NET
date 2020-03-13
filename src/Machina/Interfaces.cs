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

    /// <summary>
    /// Makes the type numerically comparable to another type under an epsilon threshold.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEpsilonComparable<in T>
    {
        /// <summary>
        /// Compare to other object under a numeric threshold.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        bool IsSimilarTo(T other, double epsilon);
    }

    /// <summary>
    /// Makes the type able to serialize itself to a JSON array-like string representation. 
    /// </summary>
    public interface ISerializableArray
    {
        /// <summary>
        /// JSON array representation of this object with rounding decimals. Use -1 for no rounding.
        /// </summary>
        /// <param name="decimals"></param>
        /// <returns></returns>
        string ToArrayString(int decimals);
    }

    /// <summary>
    /// Makes the type able to serialize itself to a JSON object-like string.
    /// </summary>
    public interface ISerializableJSON
    {
        /// <summary>
        /// JSON representation of this object, with rounding decimals. Use -1 for no rounding.
        /// </summary>
        /// <param name="decimals"></param>
        /// <returns></returns>
        string ToJSONString(int decimals);
    }
}
