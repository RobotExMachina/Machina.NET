using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Machina.Types.Data
{
    /// <summary>
    /// Represents a file inside a RobotProgram. Includes filename, extension and content as a string List.
    /// </summary>
    public class RobotProgramFile
    {

        public string Name { get; }
        public string Extension { get; }
        internal Encoding Encoding { get; }
        internal char CommentChar { get; }

        internal List<string> Lines { get; private set; }

        internal RobotProgramFile(string name, string extension, Encoding encoding, char commentChar)
        {
            this.Name = name;
            this.Extension = extension;
            this.Encoding = encoding;
            this.CommentChar = commentChar;
        }

        internal void SetContent(List<string> lines)
        {
            this.Lines = lines;
        }
         
        public override string ToString()
        {
            return $"Robot Program File \"{Name}.{Extension}\" with {Lines.Count} lines of code.";
        }

        internal List<string> ToStringList()
        {
            List<string> lines = new List<string>();
            lines.AddRange(GetHeader());
            lines.AddRange(Lines);
            lines.AddRange(GetFooter());
            lines.Add("");
            return lines;
        }

        private List<string> GetHeader()
        {
            List<string> header = new List<string>();
            string ccline = new String(CommentChar, 65);
            header.Add(ccline);
            header.Add($"{CommentChar}{CommentChar} START OF FILE \"{Name}.{Extension}\"");
            header.Add(ccline);
            header.Add("");
            return header;
        }

        private List<string> GetFooter()
        {
            List<string> footer = new List<string>();
            string ccline = new String(CommentChar, 65);
            footer.Add(ccline);
            footer.Add($"{CommentChar}{CommentChar} END OF FILE \"{Name}.{Extension}\"");
            footer.Add(ccline);
            footer.Add("");
            return footer;
        }
    }
}
