using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Types
{
    /// <summary>
    /// Represents a robot program written in the robot's native language. Includes the files that make it up.
    /// </summary>
    public class RobotProgram
    {
        public string Name { get; }
        public List<RobotProgramFile> Files { get; private set; }
        
        internal char CommentChar { get; }

        internal RobotProgram(string programName, char commentChar)
        {
            this.Name = programName;
            this.CommentChar = commentChar;
            this.Files = new List<RobotProgramFile>();
        }

        internal void Add(RobotProgramFile file)
        {
            Files.Add(file);
        }

        /// <summary>
        /// Saves the files in this program to a folder in the system.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        internal bool SaveToFolder(string folderPath, RobotLogger logger)
        {
            return Utilities.FileIO.SaveProgramToFolder(this, folderPath, logger);
        }

        public override string ToString()
        {
            return $"Robot Program \"{Name}\" with {Files.Count} files.";
        }

        public List<string> ToStringList()
        {
            List<string> lines = new List<string>();
            lines.AddRange(GetHeader());
            
            foreach(var file in Files)
            {
                lines.AddRange(file.ToStringList());
            }

            lines.AddRange(GetFooter());
            lines.Add("");
            return lines;
        }
        
        private List<string> GetHeader()
        {
            List<string> header = new List<string>();
            string ccline = new String(CommentChar, 80);
            header.Add(ccline);
            header.Add($"{CommentChar}{CommentChar} START OF ROBOT PROGRAM \"{Name}\"");
            header.Add(ccline);
            header.Add("");
            return header;
        }

        private List<string> GetFooter()
        {
            List<string> footer = new List<string>();
            string ccline = new String(CommentChar, 80);
            footer.Add(ccline);
            footer.Add($"{CommentChar}{CommentChar} END OF ROBOT PROGRAM \"{Name}\"");
            footer.Add(ccline);
            footer.Add("");
            return footer;
        }

    }
}
