using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    static class IO
    {
        /// <summary>
        /// Saves a string List to a file.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="filepath"></param>
        /// <returns></returns>
        internal static bool SaveStringListToFile(List<string> lines, string filepath, Encoding encoding)
        {
            try
            {
                System.IO.File.WriteAllLines(filepath, lines, encoding);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not save program to file...");
                Console.WriteLine(ex);
            }
            return false;
        }

        /// <summary>
        /// Saves a resource text file to a path.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="filepath"></param>
        /// <returns></returns>
        internal static bool SaveTextResourceToFile(string resourceName, string filepath, Encoding encoding)
        {
            try
            {
                System.IO.File.WriteAllLines(filepath,
                    ReadLines(() => Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)),
                    encoding);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not save resource to file...");
                Console.WriteLine(ex);
            }
            return false;
        }

        /// <summary>
        /// Returns an IEnumerable of strings from a streamReader provider. https://stackoverflow.com/a/13312954/1934487
        /// </summary>
        /// <param name="streamProvider"></param>
        /// <returns></returns>
        private static IEnumerable<string> ReadLines(Func<Stream> streamProvider)
        {
            using (var stream = streamProvider())
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
