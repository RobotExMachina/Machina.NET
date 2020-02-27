using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Machina.Types;


namespace Machina.Utilities
{
    //  ██╗   ██╗████████╗██╗██╗     ██╗████████╗██╗███████╗███████╗
    //  ██║   ██║╚══██╔══╝██║██║     ██║╚══██╔══╝██║██╔════╝██╔════╝
    //  ██║   ██║   ██║   ██║██║     ██║   ██║   ██║█████╗  ███████╗
    //  ██║   ██║   ██║   ██║██║     ██║   ██║   ██║██╔══╝  ╚════██║
    //  ╚██████╔╝   ██║   ██║███████╗██║   ██║   ██║███████╗███████║
    //   ╚═════╝    ╚═╝   ╚═╝╚══════╝╚═╝   ╚═╝   ╚═╝╚══════╝╚══════╝
    //                                                              
    //  ███████╗██╗██╗     ███████╗    ██╗ ██████╗                  
    //  ██╔════╝██║██║     ██╔════╝    ██║██╔═══██╗                 
    //  █████╗  ██║██║     █████╗      ██║██║   ██║                 
    //  ██╔══╝  ██║██║     ██╔══╝      ██║██║   ██║                 
    //  ██║     ██║███████╗███████╗    ██║╚██████╔╝                 
    //  ╚═╝     ╚═╝╚══════╝╚══════╝    ╚═╝ ╚═════╝                  
    //                                                              
    /// <summary>
    /// A static class with utility methods for file handling
    /// </summary>
    internal static class FileIO
    {
        
        /// <summary>
        /// Saves a List of strings to a file.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="filepath"></param>
        /// <param name="encoding"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        internal static bool SaveStringListToFile(List<string> lines, string filepath, Encoding encoding, RobotLogger logger)        
        {
            try
            {
                System.IO.File.WriteAllLines(filepath, lines, encoding);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Could not save program to file...");
                logger.Debug(ex);
            }
            return false;
        }

        /// <summary>
        /// Saves a List of program Files to a folder (creates the folder if needed).
        /// </summary>
        /// <param name="files"></param>
        /// <param name="folderPath"></param>
        /// <param name="encoding"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        internal static bool SaveProgramToFolder(List<MachinaFile> files, string folderPath, RobotLogger logger)
        {
            // Check if directory exists, and create it otherwise
            try
            {
                if (Directory.Exists(folderPath))
                {
                    logger.Debug("Found existing folder on " + folderPath);
                }
                else
                {
                    DirectoryInfo di = Directory.CreateDirectory(folderPath);
                    logger.Debug("Created folder " + folderPath);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Could not create folder " + folderPath);
                logger.Debug(ex);
                return false;
            }
            
            // More sanity
            if (!IsDirectoryWritable(folderPath))
            {
                logger.Error("Cannot write to folder " + folderPath);
                return false;
            }

            // Write each file
            bool success = true;
            foreach (var file in files)
            {
                string fullPath = Path.Combine(folderPath, file.Name + "." + file.Extension);
                success = success && SaveStringListToFile(file.Content, fullPath, file.Encoding, logger);
            }

            return success;
        }


        /// <summary>
        /// Checks if a directory can be written to. 
        /// From https://stackoverflow.com/a/6371533
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="throwIfFails"></param>
        /// <returns></returns>
        internal static bool IsDirectoryWritable(string dirPath, bool throwIfFails = false)
        {
            try
            {
                using (FileStream fs = File.Create(
                    Path.Combine(
                        dirPath,
                        Path.GetRandomFileName()
                    ),
                    1,
                    FileOptions.DeleteOnClose)
                )
                { }
                return true;
            }
            catch
            {
                if (throwIfFails)
                    throw;
                else
                    return false;
            }
        }


    }
}
