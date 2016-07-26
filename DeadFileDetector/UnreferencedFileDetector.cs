using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadFileDetector
{
    public class UnreferencedFileDetector : DeadFileDetector.IUnreferencedFileDetector
    {
        private readonly static string[] IgnoredFileExtensions = new string[] { ".suo", ".vspscc", ".sln", ".csproj", ".user"};
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
                foreach (string projectFile in projectFiles)
                {
                    string absolutProjectFilePath = Path.Combine(slnDir, projectFile);

                    string projectFileDir = Path.GetDirectoryName(absolutProjectFilePath);

                    string searchPattern = "*";

                    // Collect all files from project directory
                    IEnumerable<string> fileSystemEntries = fileSystem.Directory
                                                            .EnumerateFileSystemEntries(projectFileDir, searchPattern, SearchOption.AllDirectories)
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
                            string absolutePathWithoutFileName = Path.GetDirectoryName(absolutProjectFilePath);

                            IEnumerable<string> referencedFiles = projectFileReader.ReadReferencedFiles(projectFileStream);

                            foreach (var referencedFile in referencedFiles)
                            {
                                string combinedFilePaths = Path.Combine(absolutePathWithoutFileName, referencedFile);
                                unreferencedFilesAndDirectories.Remove(combinedFilePaths);

                                foreach (string subDir in GetAllSubdirectoriesExceptProjectDir(combinedFilePaths, projectFileDir))
                                {
                                    unreferencedFilesAndDirectories.Remove(subDir);
                                }
                            }

                            //foreach (var item in unreferencedFilesAndDirectories)
                            //{
                            //    int relativePathStartIndex = item.IndexOf("Repos");
                            //    string substring = item.Substring(relativePathStartIndex);
                            //    string relativePath = @"..\..\" + substring;
                            //}

                        }

                    }
                }
            }

            return unreferencedFilesAndDirectories.OrderBy(x => Path.GetExtension(x));

        }

        private static IEnumerable<string> GetAllSubdirectoriesExceptProjectDir(string path, string projectDir)
        {
            var folderPath = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(folderPath) && !string.Equals(folderPath, projectDir, StringComparison.OrdinalIgnoreCase))
            {
                yield return folderPath;

                foreach (var item in GetAllSubdirectoriesExceptProjectDir(folderPath, projectDir))
                {
                    yield return item;
                }
            }
        }

        private bool Is(string fileSystemEntry)
        {
            return !IgnoredFileExtensions.Contains(Path.GetExtension(fileSystemEntry))
               && !(IgnoredFolders.Any(f => fileSystemEntry.EndsWith(Path.DirectorySeparatorChar + f, StringComparison.OrdinalIgnoreCase)
                           || fileSystemEntry.Contains(Path.DirectorySeparatorChar + f + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)));
        }

    }
}
