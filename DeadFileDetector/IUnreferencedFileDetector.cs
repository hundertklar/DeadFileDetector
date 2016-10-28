using System;
using System.Collections.Generic;
namespace DeadFileDetector
{
    interface IUnreferencedFileDetector
    {
        IDictionary<string, IEnumerable<string>> DeterminateUnreferenceFilesAndFolders(string slnDir, params string[] projectFiles);
    }
}
