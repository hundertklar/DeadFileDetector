﻿using System;
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
        private readonly static string[] IgnoredFileExtensions = new string[] { ".suo", ".vspscc", ".sln", ".csproj", ".user", ".vcxproj", ".filters" };
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

        public IEnumerable<string> DeterminateUnreferenceFilesAndFolders(string solutionDirectory, params string[] projectFiles)
        {
            Dictionary<string, string> unreferencedFilesAndDirectories = new Dictionary<string,string>();

            if (projectFiles != null
                && projectFiles.Length != 0)
            {
                foreach (string projectFile in projectFiles)
                {
                    string absolutProjectFilePath = Path.Combine(solutionDirectory, projectFile);

                    string projectFileDirectory = Path.GetDirectoryName(absolutProjectFilePath);

                    string searchPattern = "*";

                    // Collect all files from project directory
                    IEnumerable<string> fileSystemEntries = fileSystem.Directory
                                                            .EnumerateFileSystemEntries(projectFileDirectory, searchPattern, SearchOption.AllDirectories)
                                                            .Where(s => this.Is(s));

                    foreach (string fileSystemEntry in fileSystemEntries)
                    {
                        string relativePath = PathHelper.GetRelativePath(solutionDirectory, true, fileSystemEntry, true);

                        unreferencedFilesAndDirectories.Add(relativePath.ToUpper(), relativePath);
                    }

                    if (unreferencedFilesAndDirectories.Count > 0)
                    {
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

                                        relativePath = Path.Combine(relativePath + Path.GetDirectoryName(projectFile) , referencedFile);
                                    }

                                }

                                unreferencedFilesAndDirectories.Remove(relativePath.ToUpper());

                                foreach (string subDir in GetAllSubdirectoriesExceptProjectDir(relativePath, projectFileDirectory))
                                {
                                    unreferencedFilesAndDirectories.Remove(subDir.ToUpper());
                                }
                            }
                        }
                    }
                }
            }

            return unreferencedFilesAndDirectories.Select(x => x.Value).OrderBy(x => GetFolderCount(x) * -1 + (Path.GetFileName(x).Contains('.') ? 1 : 0)).ThenBy(x => x);

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

    }
}
