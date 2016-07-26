using System;
namespace DeadFileDetector
{
    interface IUnreferencedFileDetector
    {
        System.Collections.Generic.IEnumerable<string> DeterminateUnreferenceFilesAndFolders(string slnDir, params string[] projectFiles);
    }
}
