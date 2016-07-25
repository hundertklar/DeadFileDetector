using System;
using System.Collections.Generic;
using System.IO;

namespace DeadFileDetector
{
    public interface IProjectFileReader
    {
        IEnumerable<string> ReadReferencedFiles(Stream stream);
    }
}
