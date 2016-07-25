using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadFileDetector
{
    public class UnreferencedFileDetector
    {
        private readonly static string[] IgnoredFileExtensions = new string[] { ".suo", ".vspscc" };
        private readonly static string[] IgnoredFolders = new string[] { "bin", "obj" };

        IFileSystem fileSystem;
        private IProjectFileReader projectFileReader;
        private IFileSystem fileSystemMock;
        private FileSystem fileSystem1;

        public UnreferencedFileDetector(IFileSystem fileSystem, IProjectFileReader projectFileReader)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }

            if (projectFileReader == null)
            {
                throw new ArgumentNullException("projectFileReader");
            }

            this.fileSystem = fileSystem;
            this.projectFileReader = projectFileReader;
        }

        public IEnumerable<string> DeterminateUnreferenceFilesAndFolders(string slnDir, params string[] projectFiles)
        {
            HashSet<string> unreferencedFilesAndDirectories = new HashSet<string>();

            if (projectFiles != null
                && projectFiles.Length != 0)
            {
                string combinedSlnDir = @"C:\Users\dkussberger\Source\Repos\DeadFileDetector";

                foreach (string projectFile in projectFiles)
                {
                    string absolutProjectFilePath = Path.Combine(combinedSlnDir, projectFile);

                    string projectDir = Path.GetDirectoryName(absolutProjectFilePath);

                    string searchPattern = "*";

                    // Collect all files from project directory
                    IEnumerable<string> fileSystemEntries = fileSystem.Directory
                                                            .EnumerateFileSystemEntries(projectDir, searchPattern, SearchOption.AllDirectories)
                                                            .Where(s => this.Is(s));

                    foreach (string fileSystemEntry in fileSystemEntries)
                    {
                        unreferencedFilesAndDirectories.Add(fileSystemEntry);
                    }

                    if (unreferencedFilesAndDirectories.Count > 0)
                    {
                        // Find all unreferenced files from project
                        using (Stream projectFileStream = this.fileSystem.File.OpenRead(absolutProjectFilePath))
                        {
                            IEnumerable<string> referencedFiles = projectFileReader.ReadReferencedFiles(projectFileStream);

                            foreach (var item in referencedFiles)
                            {
                                unreferencedFilesAndDirectories.Remove(item);
                            }
                        }
                    }
                }
            }

            return unreferencedFilesAndDirectories.OrderBy(x => Path.GetExtension(x));
        }

        private bool Is(string fileSystemEntry)
        {
            return !IgnoredFileExtensions.Contains(Path.GetExtension(fileSystemEntry))
               && !(IgnoredFolders.Any(f => fileSystemEntry.EndsWith(Path.DirectorySeparatorChar + f, StringComparison.OrdinalIgnoreCase)
                           || fileSystemEntry.Contains(Path.DirectorySeparatorChar + f + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
