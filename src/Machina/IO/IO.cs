using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    public static class IO
    {
        /// <summary>
        /// Reads a text resource file and returns it as a string.
        /// https://stackoverflow.com/a/3314213/1934487
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        internal static string ReadTextResource(string resourceName)
        {
            string resource;
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                resource = reader.ReadToEnd();
            }

            return resource;
        }

        internal static bool SaveStringToFile(string filepath, string text, Encoding encoding)
        {
            try
            {
                System.IO.File.WriteAllText(filepath, text, encoding);
                return true;
            }
            catch (Exception ex)
            {
                Machina.Logger.Error("Could not save string to file...");
                Machina.Logger.Error(ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// Saves a string List to a file.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="filepath"></param>
        /// <returns></returns>
        internal static bool SaveStringListToFile(string filepath, List<string> lines, Encoding encoding)
        {
            try
            {
                System.IO.File.WriteAllLines(filepath, lines, encoding);
                return true;
            }
            catch (Exception ex)
            {
                Machina.Logger.Error("Could not save stringList to file...");
                Machina.Logger.Error(ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// Saves a resource text file to a path.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="filepath"></param>
        /// <returns></returns>
        internal static bool SaveTextResourceToFile(string filepath, string resourceName, Encoding encoding)
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
                //Console.WriteLine("Could not save resource to file...");
                //Console.WriteLine(ex);

                Machina.Logger.Error("Could not save resource to file...");
                Machina.Logger.Error(ex.ToString());
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
