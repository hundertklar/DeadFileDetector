using DeadFileDetector;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Collections;

namespace UnitTests
{
    public class ProjectFileReaderTests
    {
        //[Test]
        //public void ConstructorThrowsAnArgumentNullExceptionIfStreamParameterIsNull();
        //public void ProjectFileReader_PassNullAsArgument_ThrowsArgumentNullException()
        //{
        //    ProjectFileReader reader = new ProjectFileReader();
       
        //}
        
        [Test]
        public void ProjectFileReader_ReadReferencedFiles_ReturnedFilesAreExpected()
        {
            // Arrange 
            IEnumerable<string> expectedFiles = new[] { "File1.cs", "File2.cs", "File3.cs" };
            using (Stream projectFileStream = GetStreamToProjectFile("3Files.csproj"))
            {
                ProjectFileReader reader = new ProjectFileReader();

                // Action 
                IEnumerable<string> result = reader.ReadReferencedFiles(projectFileStream);

                // Assertion
                result.Should().HaveCount(3);
            }
        }

        [Test]
        public void ProjectFileReader_ReadReferencedFilesWithClosedStream_ReturnEmptyList()
        {
            // Arrange 
            IEnumerable<string> expectedFiles = Enumerable.Empty<string>();
            using (Stream projectFileStream = GetStreamToProjectFile("3Files.csproj"))
            {
                ProjectFileReader reader = new ProjectFileReader();
                projectFileStream.Dispose();
                // Action 
                IEnumerable<string> result = reader.ReadReferencedFiles(projectFileStream);

                // Assertion

                result.Should().HaveCount(0);
                result.Should().BeEquivalentTo(expectedFiles);
            }
        }

        public static Stream GetStreamToProjectFile(string fileName)
        {
            return typeof(ProjectFileReaderTests).Assembly.GetManifestResourceStream("UnitTests.ProjectFiles." + fileName);
        }
    }
}
