using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Machina.Types
{
    /// <summary>
    /// Represents a file with filename, extension and content as a string List.
    /// </summary>
    internal class MachinaFile
    {

        internal string Name { get; }
        internal string Extension { get; }
        internal Encoding Encoding { get; }
        internal char CommentChar { get; }

        internal List<string> Content { get; private set; }

        internal MachinaFile(string name, string extension, Encoding encoding, char commentChar)
        {
            this.Name = name;
            this.Extension = extension;
            this.Encoding = encoding;
            this.CommentChar = commentChar;
        }

        internal void SetContent(List<string> lines)
        {
            this.Content = lines;
        }
         
        public override string ToString()
        {
            return String.Join(Environment.NewLine, ToStringList());
        }

        internal List<string> ToStringList()
        {
            List<string> lines = new List<string>();
            lines.AddRange(GetHeader());
            lines.AddRange(Content);
            lines.Add("");
            return lines;
        }

        private List<string> GetHeader()
        {
            List<string> header = new List<string>();
            string ccline = new String(CommentChar, 80);
            header.Add(ccline);
            header.Add($"{CommentChar}{CommentChar} FILENAME: \"{Name}.{Extension}\"");
            header.Add(ccline);
            header.Add("");
            return header;
        }
    }
}
