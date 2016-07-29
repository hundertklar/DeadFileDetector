using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeadFileDetector
{
    public static class PathHelper
    {    
        /// <summary>
        /// Stringbuilder which takes two string paths and combines them into one relative path
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="isPath1AFolder"></param>
        /// <param name="path2"></param>
        /// <param name="isPath2AFolder"></param>
        /// <returns></returns>
        public static string GetRelativePath(string path1, bool isPath1AFolder, string path2, bool isPath2AFolder)
        {
            StringBuilder sb = new StringBuilder(255);

            var successfully = PathRelativePathTo(sb, path1, isPath1AFolder ? FileAttributes.Directory : FileAttributes.Normal, path2, isPath2AFolder ? FileAttributes.Directory : FileAttributes.Normal);


            if (!successfully)
            {
                throw new ArgumentException();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Windows .Dll funktion
        /// </summary>
        /// <param name="pszPath"></param>
        /// <param name="pszFrom"></param>
        /// <param name="dwAttrFrom"></param>
        /// <param name="pszTo"></param>
        /// <param name="dwAttrTo"></param>
        /// <returns></returns>
        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern bool PathRelativePathTo([Out] StringBuilder pszPath, [In] string pszFrom, [In] FileAttributes dwAttrFrom, [In] string pszTo, [In] FileAttributes dwAttrTo);
    }
}
