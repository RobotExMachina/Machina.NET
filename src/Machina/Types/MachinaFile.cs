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

        internal List<string> Content { get; private set; }

        internal MachinaFile(string name, string extension, Encoding encoding)
        {
            this.Name = name;
            this.Extension = extension;
            this.Encoding = encoding;
        }

        internal void SetContent(List<string> lines)
        {
            this.Content = lines;
        }

    }
}
