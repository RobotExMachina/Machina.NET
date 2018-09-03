using System;
using System.Collections.Generic;

//██╗   ██╗████████╗██╗██╗     
//██║   ██║╚══██╔══╝██║██║     
//██║   ██║   ██║   ██║██║     
//██║   ██║   ██║   ██║██║     
//╚██████╔╝   ██║   ██║███████╗
// ╚═════╝    ╚═╝   ╚═╝╚══════╝

/// <summary>
/// A bunch of static utility functions (probably many of them could be moved to certain related classes...)
/// </summary>
namespace Machina
{
    /// <summary>
    /// Utility static methods
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Remaps a value from source to target numerical domains.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="newMin"></param>
        /// <param name="newMax"></param>
        /// <returns></returns>
        public static double Remap(double val, double min, double max, double newMin, double newMax)
        {
            return newMin + (val - min) * (newMax - newMin) / (max - min);
        }

        /// <summary>
        /// Converts an array of signed int32 to a byte array. Useful for buffering. 
        /// </summary>
        /// <param name="intArray"></param>
        /// <param name="littleEndian">Set endianness. Windows systems are little endian, while most network communication is bigendian.</param>
        /// <returns></returns>
        public static byte[] Int32ArrayToByteArray(int[] intArray, bool littleEndian = false)
        {
            byte[] buffer = new byte[4 * intArray.Length];

            // Windows systems are little endian, but the UR takes bigendian... :(
            if (BitConverter.IsLittleEndian == littleEndian)
            {
                Buffer.BlockCopy(intArray, 0, buffer, 0, buffer.Length);
            }

            // If the system stores data differently than requested, must manually reverse each byte!
            else
            {
                byte[] bint;
                for (var i = 0; i < intArray.Length; i++)
                {
                    bint = BitConverter.GetBytes(intArray[i]);
                    Array.Reverse(bint);
                    for (var j = 0; j < 4; j++)
                    {
                        buffer[4 * i + j] = bint[j];
                    }
                }
            }

            return buffer;
        }

        /// <summary>
        /// Converts an array of bytes to an array of signed int32. 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="byteCount">If 0, the whole byte array will be used.</param>
        /// <param name="bytesAreLittleEndian">Is the byte array little endian? This will be used to define how to translate the buffer to this system's endianness.</param>
        /// <returns></returns>
        public static int[] ByteArrayToInt32Array(byte[] bytes, int byteCount = 0, bool bytesAreLittleEndian = false)
        {
            if (byteCount == 0)
            {
                byteCount = bytes.Length;
            }

            //// Sanity -> for the sake of performance, let's trust the user knows what s/he is doing... 
            //if (byteCount % 4 != 0) throw new Exception("byteCount must be multiple of 4");
            //if (byteCount > bytes.Length) throw new Exception("byteCount is larger than array size");
            int[] ints = new int[byteCount / 4];

            // Windows systems are little endian... 
            if (BitConverter.IsLittleEndian == bytesAreLittleEndian)
            {
                Buffer.BlockCopy(bytes, 0, ints, 0, byteCount);
            }

            // ...but network communication is usually bigendian.
            // If the system stores data differently than the array to process, must manually reverse each 4 bytes!
            else
            {
                byte[] clone = new byte[ints.Length * 4];  // don't reverse the order of the passed array
                byte first, second;
                for (int i = 0; i < ints.Length; i++)
                {
                    first = bytes[4 * i];
                    second = bytes[4 * i + 1];

                    clone[4 * i] = bytes[4 * i + 3];
                    clone[4 * i + 1] = bytes[4 * i + 2];
                    clone[4 * i + 2] = second;
                    clone[4 * i + 3] = first;
                }

                Buffer.BlockCopy(clone, 0, ints, 0, byteCount);
            }

            return ints;
        }

        /// <summary>
        /// Returns a string with safe ASCII characters to be used as a robot program name. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string SafeProgramName(string name)
        {
            string safe = "";
            if (name.Length == 0) safe = "Machina";

            // Replace whitespaces with underscores
            safe = name.Replace(' ', '_');

            // Check if the name starts with a digit
            if (char.IsDigit(safe[0])) safe = "_" + safe;

            return safe;
        }

        /// <summary>
        /// Finds double quotes on a string and scapes them adding a backlash in front.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EscapeDoubleQuotes(string str)
        {
            return str.Replace("\"", "\\\"");
        }

        /// <summary>
<<<<<<< HEAD
        /// Compares strings representing semantic versioning versions, like "1.3.2".
        /// Returns 1 if A is newer than B, -1 if B is newer than A, and 0 if same version. 
        /// </summary>
        /// <param name="versionA"></param>
        /// <param name="versionB"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static int CompareVersions(string versionA, string versionB, char delimiter = '.')
        {
            string[] A = versionA.Split(delimiter);
            string[] B = versionB.Split(delimiter);

            if (A.Length != B.Length)
            {
                throw new Exception("Incorrectly formatted version numbers, lengths must be equal");
            }

            int a, b;
            for (int i = 0; i < A.Length; i++)
            {
                a = Convert.ToInt32(A[i]);
                b = Convert.ToInt32(B[i]);
                if (a > b) return 1;
                if (a < b) return -1;
            }

            return 0;
=======
        /// Returns a new copy instance of a generic Dictionary. 
        /// Note that this method only works for primitive elements; objects will be copied by reference. 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Dictionary<T1, T2> CopyGenericDictionary<T1, T2>(Dictionary<T1, T2> source)
        {
            Dictionary<T1, T2> copy = new Dictionary<T1, T2>();
            foreach (KeyValuePair<T1, T2> item in source)
            {
                copy[item.Key] = item.Value;
            }
            return copy;
>>>>>>> 2ce80f32d646ca2ed599525ba68a2bd47278da4e
        }

    }



}
