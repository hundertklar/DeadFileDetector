using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DeadFileDetector
{
    /// <summary>
    /// Reads the different properties of a given project file.
    /// </summary>
    public class ProjectFileReader : IProjectFileReader
    {

        public IEnumerable<string> ReadReferencedFiles(Stream stream)
        {
            if (stream.CanRead)
            {
                stream.Position = 0;

                StreamReader streamReader = new StreamReader(stream);

                string streamContent = streamReader.ReadToEnd();

                Regex fileDetection = new Regex("(?<=EmbeddedResource Include=\"|Compile Include=\"|None Include=\"|Page Include=\"|Folder Include=\")(.+?)(?=\"(\\s/>|>|))");
                MatchCollection matches = fileDetection.Matches(streamContent);

                foreach (Match match in matches)
                {
                    yield return match.Value;
                }
            }
        }
    }
}

