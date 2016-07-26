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
    /// Reads the different properties of a given solution file.
    /// </summary>
    public class SolutionFileReader
    {
        private Stream stream;
        
        /// <summary>
        /// Initalize a new instance of the <see cref="SolutionFileReader"/> class.
        /// </summary>
        /// <param name="stream">The stream that contains the content of the solution file.</param>
        public SolutionFileReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("Null Exception");
            }

            this.stream = stream;
        }

        /// <summary>
        /// Reads referenced project files from the solution file stream.
        /// </summary>
        /// <returns>Relative project file paths.</returns>
        public IEnumerable<string> ReadReferencedFiles()
        {
            List<string> result = new List<string>();
            if (stream.CanRead)
            {
                this.stream.Position = 0;

                StreamReader streamReader = new StreamReader(this.stream);

                string streamContent = streamReader.ReadToEnd();

                //Extracts relative project file paths from the solution file.
                Regex fileDetection = new Regex("(?<=\",\\s\").+(?=\"\\,\\s\"\\{.{8}\\-.{4}\\-.{4}\\-.{4}\\-.{12}\\})");
                MatchCollection relativeProjectFilePaths = fileDetection.Matches(streamContent);
                
                foreach (Match relativeProjectFilePath in relativeProjectFilePaths)
                {
                    result.Add(relativeProjectFilePath.Value);
                }



                //foreach (Match relativeProjectFilePath in relativeProjectFilePaths)
                //{
                //    yield return relativeProjectFilePath.Value;
                //}

            }

            return result;

        }
    }
}
    


