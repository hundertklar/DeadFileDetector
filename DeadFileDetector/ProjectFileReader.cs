﻿using System;
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

                //The streamreaders position is set to the beginning of the stream.
                stream.Position = 0;

                StreamReader streamReader = new StreamReader(stream);

                string streamContent = streamReader.ReadToEnd();

                //Reads the projectfile and scans for referenced file paths. You can extend the Regex.
                Regex fileDetection = new Regex("(?<=EmbeddedResource Include=\"|WCFMetadata Include=\"|Resource Include=\"|Compile Include=\"|ClInclude Include=\"|Resource Include=\"|None Include=\"|Content Include=\"|CodeAnalysisDictionary Include=\"|ApplicationDefinition Include=\"|Page Include=\"|Folder Include=\")(.+?)(?=\"(\\s/>|>|))|(?<=<ApplicationIcon>).+(?=</)");
                MatchCollection matches = fileDetection.Matches(streamContent);

                foreach (Match match in matches)
                {
                    yield return match.Value;
                }
            }
        }
    }
}

