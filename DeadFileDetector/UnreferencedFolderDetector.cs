using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeadFileDetector
{
    class UnreferencedFolderDetector
    {
        private readonly static string[] IgnoredFolders = new string[] { ".nuget", "packages"};

        private readonly IFileSystem filesystem;

        public UnreferencedFolderDetector(IFileSystem filesystem)
        {
            this.filesystem = filesystem;
        }

        public IEnumerable<string> DetectUnreferencedFolders(string slnDir, IEnumerable<string> referencedProjectFiles)
        {

            HashSet<string> referencedFolder = new HashSet<string>();

            foreach (string referencedProjectFile in referencedProjectFiles)
            {
                string projectRoot = GetProjectRootDirectory(slnDir, referencedProjectFile);

                if (projectRoot != null)
                {
                    referencedFolder.Add(projectRoot);
                }
            }

            foreach (string folder in this.filesystem.Directory.EnumerateDirectories(slnDir, "*", SearchOption.TopDirectoryOnly))
            {
                string relativePath = PathHelper.GetRelativePath(slnDir, true, folder, true);

                if (relativePath != null)
                {
                    relativePath = relativePath.Substring(2);

                    if (!referencedFolder.Contains(relativePath) && !IgnoredFolders.Contains(relativePath))
                    {
                        yield return PathHelper.GetRelativePath(slnDir, true, folder, true);
                    }                    
                }

            }
        }

        public static string GetProjectRootDirectory(string slnDir, string projectFilePath)
        {

            if (projectFilePath.StartsWith("..\\"))
            {
                return null;
            }

            if (projectFilePath.StartsWith(".\\"))
            {
                projectFilePath = projectFilePath.Substring(2);
            }

            string projectFileRoot = Path.GetDirectoryName(projectFilePath);

            while (projectFileRoot != string.Empty && Path.GetDirectoryName(projectFileRoot) != string.Empty)
            {
                projectFileRoot = Path.GetDirectoryName(projectFileRoot);
            }

            if (projectFileRoot == string.Empty)
            {
                return null;
            }
            else
            {
                return projectFileRoot;
            }
        }
    }
}
