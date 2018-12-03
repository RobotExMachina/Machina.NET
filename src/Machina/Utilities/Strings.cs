using System;
using System.Collections.Generic;

//  ██╗   ██╗████████╗██╗██╗     ██╗████████╗██╗███████╗███████╗
//  ██║   ██║╚══██╔══╝██║██║     ██║╚══██╔══╝██║██╔════╝██╔════╝
//  ██║   ██║   ██║   ██║██║     ██║   ██║   ██║█████╗  ███████╗
//  ██║   ██║   ██║   ██║██║     ██║   ██║   ██║██╔══╝  ╚════██║
//  ╚██████╔╝   ██║   ██║███████╗██║   ██║   ██║███████╗███████║
//   ╚═════╝    ╚═╝   ╚═╝╚══════╝╚═╝   ╚═╝   ╚═╝╚══════╝╚══════╝
//                                                              
//  ███████╗████████╗██████╗ ██╗███╗   ██╗ ██████╗ ███████╗     
//  ██╔════╝╚══██╔══╝██╔══██╗██║████╗  ██║██╔════╝ ██╔════╝     
//  ███████╗   ██║   ██████╔╝██║██╔██╗ ██║██║  ███╗███████╗     
//  ╚════██║   ██║   ██╔══██╗██║██║╚██╗██║██║   ██║╚════██║     
//  ███████║   ██║   ██║  ██║██║██║ ╚████║╚██████╔╝███████║     
//  ╚══════╝   ╚═╝   ╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝     
//                                                              

namespace Machina.Utilities
{
    /// <summary>
    /// Utility functions for string operations.
    /// </summary>
    public static class Strings
    {
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
            return str?.Replace("\"", "\\\"");
        }

        /// <summary>
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
        }

        /// <summary>
        /// Checks if the argument is a valid variable name. 
        /// Basically, it returns true if the first character of the string is a letter...
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        public static bool IsValidVariableName(string varName)
        {
            return !String.IsNullOrEmpty(varName) && Char.IsLetter(varName[0]);
        }

        /// <summary>
        /// Given an array of objects, returns a string representation using their own .ToString()
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static string ObjectArrayToString(object[] objs, char separator = ',')
        {
            if (objs == null) return null;

            string str = "";
            for (int i = 0; i < objs.Length; i++)
            {
                str += objs[i]?.ToString() ?? "null";
                if (i < objs.Length - 1)
                {
                    str += ',';
                }
            }
            return str;
        }

        /// <summary>
        /// Given an array of strings, returns an enumerated quoted string with custom intermediate and closing separators.
        /// For example, for ["foo", "bar", "baz], returns '"foo", "bar" & "bar"'.
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="separator"></param>
        /// <param name="closing"></param>
        /// <returns></returns>
        public static string EnumerateArray(string[] strings, string delimiter = "\"", string separator = ", ", string closing = " & ")
        {
            switch (strings.Length)
            {
                case 0:
                    return "";

                case 1:
                    return delimiter + strings[0] + delimiter;

                default:
                    string str = "";
                    for (int i = 0; i < strings.Length; i++)
                    {
                        str += delimiter + strings[i] + delimiter;
                        if (i < strings.Length - 2)
                        {
                            str += separator;
                        }
                        else if (i == strings.Length - 2)
                        {
                            str += closing;
                        }
                    }
                    return str;
            }
        }


        /// <summary>
        /// Given a string, returns a new string with all preceding and trailing occurreces of another string removed.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="rem"></param>
        /// <returns></returns>
        public static string RemoveSideChars(string str, char rem)
        {
            if (str == "" || str == null)
                return str;

            string s = str;
            while (s[0] == rem)
            {
                s = s.Remove(0, 1);
            }

            while (s[s.Length - 1] == rem)
            {
                s = s.Remove(s.Length - 1);
            }

            return s;
        }

        /// <summary>
        /// Given a string, returns a new string with one occurrence of the param char removed. 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="rem"></param>
        /// <returns></returns>
        public static string RemoveSideChar(string str, char rem)
        {
            if (str == "" || str == null)
                return str;

            string s = str;
            if (s[0] == rem)
            {
                s = s.Remove(0, 1);
            }

            if (s[s.Length - 1] == rem)
            {
                s = s.Remove(s.Length - 1);
            }

            return s;
        }

        // This is terrible, but does the job...
        /// <summary>
        /// Given an enumerable of strings, returns a clean list with no empty lines.  
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static List<string> RemoveEmptyLines(IEnumerable<string> array)
        {
            List<string> list = new List<string>();
            foreach (var str in array)
            {
                if (str != String.Empty) list.Add(str);
            }

            return list;
        }

        /// <summary>
        /// Given a string, returns a new string with all occurrences of another string within it removed.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="rem"></param>
        /// <returns></returns>
        public static string RemoveString(string str, string rem)
        {
            return str.Replace(rem, "");
        }
    }    
}
