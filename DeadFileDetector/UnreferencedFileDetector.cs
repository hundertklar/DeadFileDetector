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

        //Ignored file extensions and ignored Folders
        private readonly static string[] IgnoredFileExtensions = new string[] { ".suo", ".vspscc", ".sln", ".csproj", ".user", ".vcxproj", ".wixproj", ".filters" };
        private readonly static string[] IgnoredFolders = new string[] { "bin", "obj" };

        IFileSystem fileSystem;
        private IProjectFileReader projectFileReader;

        /// <summary>
        /// Throws ArgumentNullException when either "fileSystem" or "projectFileReader" is null.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="projectFileReader"></param>
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

        public IDictionary<string, IEnumerable<string>> DeterminateUnreferenceFilesAndFolders(string solutionDirectory, params string[] projectFiles)
        {
            Dictionary<string, IEnumerable<CaseIgnoredString>> unreferencedFilesAndDirectoriesPerProject = new Dictionary<string, IEnumerable<CaseIgnoredString>>();

            if (projectFiles != null
                && projectFiles.Length != 0)
            {
                foreach (string projectFile in projectFiles)
                {
                    HashSet<CaseIgnoredString> unreferencedFilesAndDirectories = new HashSet<CaseIgnoredString>();

                    unreferencedFilesAndDirectoriesPerProject.Add(projectFile, unreferencedFilesAndDirectories);

                    string absolutProjectFilePath = Path.Combine(solutionDirectory, projectFile);

                    if (this.fileSystem.File.Exists(absolutProjectFilePath))
                    {
                        string projectFileDirectory = Path.GetDirectoryName(absolutProjectFilePath);

                        string searchPattern = "*";

                        // Collect all files from project directory
                        IEnumerable<string> fileSystemEntries = fileSystem.Directory
                                                                .EnumerateFileSystemEntries(projectFileDirectory, searchPattern, SearchOption.AllDirectories)
                                                                .Where(s => this.Is(s));

                        foreach (string fileSystemEntry in fileSystemEntries)
                        {
                            string relativePath = PathHelper.GetRelativePath(solutionDirectory, true, fileSystemEntry, true);

                            unreferencedFilesAndDirectories.Add(new CaseIgnoredString(relativePath));
                        }
                    }
                }

                foreach (string projectFile in projectFiles)
                {
                    string absolutProjectFilePath = Path.Combine(solutionDirectory, projectFile);

                    if (this.fileSystem.File.Exists(absolutProjectFilePath))
                    {
                        string projectFileDirectory = Path.GetDirectoryName(absolutProjectFilePath);

                        // Find all unreferenced files from project
                        using (Stream projectFileStream = this.fileSystem.File.OpenRead(absolutProjectFilePath))
                        {
                            IEnumerable<string> referencedFiles = projectFileReader.ReadReferencedFiles(projectFileStream);

                            foreach (var referencedFile in referencedFiles)
                            {
                                string relativePath = string.Empty;

                                if (!Path.IsPathRooted(referencedFile))
                                {
                                    if (Path.IsPathRooted(projectFile))
                                    {
                                        relativePath = Path.Combine(projectFile, referencedFile);
                                    }
                                    else
                                    {
                                        if (!projectFile.StartsWith(".\\"))
                                        {
                                            relativePath = ".\\";
                                        }

                                        relativePath = Path.Combine(relativePath + Path.GetDirectoryName(projectFile), referencedFile);
                                    }
                                }

                                foreach (HashSet<CaseIgnoredString> curUnreferencedFilesAndDirectories in unreferencedFilesAndDirectoriesPerProject.Values)
                                {
                                    curUnreferencedFilesAndDirectories.Remove(new CaseIgnoredString(relativePath));
                                }

                                foreach (string subDir in GetAllSubdirectoriesExceptProjectDir(relativePath, projectFileDirectory))
                                {
                                    foreach (HashSet<CaseIgnoredString> curUnreferencedFilesAndDirectories in unreferencedFilesAndDirectoriesPerProject.Values)
                                    {
                                        curUnreferencedFilesAndDirectories.Remove(new CaseIgnoredString(subDir));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Dictionary<string, IEnumerable<string>> returnValue = new Dictionary<string, IEnumerable<string>>();

            foreach (var item in unreferencedFilesAndDirectoriesPerProject)
            {
                returnValue.Add(item.Key, item.Value.Select(x => x.ToString()));
            }

            return returnValue;
        }

        private static int GetFolderCount(string path)
        {
            int i = 0;

            while (path != string.Empty)
            {
                path = Path.GetDirectoryName(path);
                i++;
            }

            return i;
        }

        private static IEnumerable<string> GetAllSubdirectoriesExceptProjectDir(string path, string projectDirectory)
        {
            var folderPath = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(folderPath) && !string.Equals(folderPath, projectDirectory, StringComparison.OrdinalIgnoreCase))
            {
                yield return folderPath;

                foreach (var item in GetAllSubdirectoriesExceptProjectDir(folderPath, projectDirectory))
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

        private class CaseIgnoredString
        {
            private readonly string value;

            private readonly int hashCode;

            public CaseIgnoredString(string value)
            {
                this.value = value;
                this.hashCode = value.ToLower().GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return (obj is string && string.Equals((string)obj, this.value, StringComparison.OrdinalIgnoreCase))
                    || (obj is CaseIgnoredString && string.Equals(obj.ToString(), this.value, StringComparison.OrdinalIgnoreCase));
            }

            public override int GetHashCode()
            {
                return this.hashCode;
            }

            public override string ToString()
            {
                return this.value.ToString();
            }
        }

    }
}
