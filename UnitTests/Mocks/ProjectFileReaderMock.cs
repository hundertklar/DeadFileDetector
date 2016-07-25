using DeadFileDetector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Mocks
{
    internal class ProjectFileReaderMock : IProjectFileReader
    {

        public IEnumerable<string> ReadReferencedFiles(Stream stream)
        {
            return this.ExpectedFilePaths;
        }

        public IEnumerable<string> ExpectedFilePaths { get; set; }
    }
}
