using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Abstractions;
using NUnit.Framework;
using DeadFileDetector;
using System.IO.Abstractions.TestingHelpers;
using System.Diagnostics;
using FluentAssertions;
using FluentAssertions.Collections;
using UnitTests.Mocks;
using Moq;

namespace UnitTests
{
    class UnreferencedFileDetectorTests
    {
        private IFileSystem fileSystemMock;
        private ProjectFileReaderMock projectFileReader;
        private Mock<DirectoryBase> directoryBaseMock;
        string solutionDir = @"C:\PalisTfs\Palis2250";

        [SetUp]
        public void Setup()
        {
            this.directoryBaseMock = new Mock<DirectoryBase>();
            this.directoryBaseMock
                .Setup(x => x.EnumerateFileSystemEntries(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(Enumerable.Empty<string>());

            var fileMock = new Mock<FileBase>().Object;

            var mock = new Mock<IFileSystem>();
            mock.SetupGet(x => x.Directory).Returns(directoryBaseMock.Object);
            mock.SetupGet(x => x.File).Returns(fileMock);

            this.fileSystemMock = mock.Object;
            this.projectFileReader = new ProjectFileReaderMock();
        }

        [Test]
        public void UnreferencedFileDetector_PassNullAsArgument_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new UnreferencedFileDetector(fileSystemMock, null));

            Assert.Throws<ArgumentNullException>(() => new UnreferencedFileDetector(null, projectFileReader));
        }

        [Test]
        public void UnreferencedFileDetector_PassNoProjectFiles_ReturnNoReferencedFiles()
        {

            var detector = new UnreferencedFileDetector(this.fileSystemMock, this.projectFileReader);

            IDictionary<string, IEnumerable<string>> unreferencedFilesAndFolders = detector.DeterminateUnreferenceFilesAndFolders(solutionDir);

            unreferencedFilesAndFolders.Should().BeEmpty();
        }

        [Test]
        public void UnreferencedFileDetector_PassNullAsProjectFiles_ReturnNoReferencedFiles()
        {
            var detector = new UnreferencedFileDetector(this.fileSystemMock, this.projectFileReader);

            IDictionary<string, IEnumerable<string>> unreferencedFilesAndFolders = detector.DeterminateUnreferenceFilesAndFolders(this.solutionDir, null);

            unreferencedFilesAndFolders.Should().BeEmpty();
        }

        /// <summary>
        /// C:\PalisTfs\Palis2250 + Palis2250.csproj = C:\PalisTfs\Palis2250\Palis2250.csproj 
        /// C:\PalisTfs\Palis2250\
        /// 
        /// C:\PalisTfs\Palis2250 + Printer\Palis2250.csproj = C:\PalisTfs\Palis2250\Printer\Palis2250.csproj 
        /// C:\PalisTfs\Palis2250\Printer
        /// </summary>
        [Test]
        public void UnreferencedFileDetector_DeterminateUnreferencedFilesAndFolders_IsCombinedPathEqualTheExpectedPath()
        {
            // Arrange
            string relativeProjectPath = @"Printer\Palis2250.csproj";
            string expectedPath = @"C:\PalisTfs\Palis2250\Printer";
            var detector = new UnreferencedFileDetector(this.fileSystemMock, this.projectFileReader);

            // Act
            IDictionary<string, IEnumerable<string>> actualResult = detector.DeterminateUnreferenceFilesAndFolders(this.solutionDir, relativeProjectPath);

            // Assertion
            this.directoryBaseMock
                .Verify(mock => mock.EnumerateFileSystemEntries(expectedPath, It.IsAny<string>(), SearchOption.AllDirectories),
                Times.Once);
        }

        [Test]
        public void UnreferencedFileDetector_DeterminateUnreferencedFilesAndFolders_BinAndObjFolderAreFiltered()
        {
            // Arrange

            this.projectFileReader.ExpectedFilePaths = Enumerable.Empty<string>();

            string absoluteProjectPath = solutionDir + @"\Printer";
            string relativeProjectFilePath = @"Printer\projectFile.csproj";
            string mockFileData = "Hallo Tobi";
            string binFolder = @"\bin";
            string objFolder = @"\obj";
            string folderFolder = @"\folder";
            string projectFilePath = absoluteProjectPath + @"\projectFile.csproj";
            string shouldBeFound1 = absoluteProjectPath + @"\shouldBeFound1.txt";
            string shouldBeFound2 = absoluteProjectPath + @"\shouldBeFound2.txt";    
            string shouldBeFound3 = absoluteProjectPath + folderFolder + @"\shouldBeFound3.txt"; 
            string shouldBeFound4 = absoluteProjectPath + folderFolder + @"\shouldBeFound4.txt";
            string shouldNotBeFound1 = absoluteProjectPath + binFolder + @"\shouldNotBeFound1.txt";
            string shouldNotBeFound2 = absoluteProjectPath + binFolder + @"\shouldNotBeFound2.txt";
            string shouldNotBeFound3 = absoluteProjectPath + objFolder + @"\shouldNotBeFound3.txt";
            string shouldNotBeFound4 = absoluteProjectPath + objFolder + @"\shouldNotBeFound4.txt";
            

            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddDirectory(absoluteProjectPath);
            mockFileSystem.AddDirectory(absoluteProjectPath + binFolder);
            mockFileSystem.AddDirectory(absoluteProjectPath + objFolder);
            mockFileSystem.AddDirectory(absoluteProjectPath + folderFolder);
            mockFileSystem.AddFile(projectFilePath, mockFileData);
            mockFileSystem.AddFile(shouldBeFound1, mockFileData);
            mockFileSystem.AddFile(shouldBeFound2, mockFileData);
            mockFileSystem.AddFile(shouldBeFound3, mockFileData);
            mockFileSystem.AddFile(shouldBeFound4, mockFileData);
            mockFileSystem.AddFile(shouldNotBeFound1, mockFileData);
            mockFileSystem.AddFile(shouldNotBeFound2, mockFileData);
            mockFileSystem.AddFile(shouldNotBeFound3, mockFileData);
            mockFileSystem.AddFile(shouldNotBeFound4, mockFileData);

            var detector = new UnreferencedFileDetector(mockFileSystem, this.projectFileReader);

            // Act
            IDictionary<string, IEnumerable<string>> actualResult = detector.DeterminateUnreferenceFilesAndFolders(this.solutionDir, relativeProjectFilePath);



            // Assertion
            actualResult[relativeProjectFilePath].Should().Contain(shouldBeFound1);
            actualResult[relativeProjectFilePath].Should().Contain(shouldBeFound2);
            actualResult[relativeProjectFilePath].Should().Contain(shouldBeFound3);
            actualResult[relativeProjectFilePath].Should().Contain(shouldBeFound4);

        }


        //        [Test]
        //        public void UnreferencedFileDetector_CombinePath_ReturnEmptyPath()
        //        {
        //            //Arrange
        //            var detector = new UnreferencedFileDetector(new FileSystem());
        //            string slnDir = @"C:\Users\TestUser1\Testfolder1\TestFolder2";
        //            string projectFileTest = @"TestFolder3\TestFile1.csproj";

        //            //Act
        //            string absolutProjectFilePath = Path.Combine(slnDir, projectFileTest);

        //            //Assertion
        //            Assert.AreEqual(@"C:\Users\TestUser1\Testfolder1\TestFolder2\TestFolder3\TestFile1.csproj", absolutProjectFilePath);
        //        }

        //        private const string solutionPath = @"C:\SolutionDir\" + solutionFolderName + @"\Test4.sln";
        //        private const string solutionFolderName = @"Folder";

        //        [Test]
        //        public void UnreferencedFileDetector_EnumerateFilesystem()
        //        {
        //            //Arrange
        //            IFileSystem fileSystemMock = new MockFileSystem();

        //            var detector = new UnreferencedFileDetector(this.fileSystemMock, this.projectFileReader);

        //            string solutionDir = Path.GetDirectoryName(solutionPath);

        //            MockFileData mockFileData = @"DeadFileDetector\DeadFileDetector.csproj
        //                                        UnitTests\UnitTests.csproj";

        //            const string projectName = "Project1";
        //            string projectDir = Path.Combine(solutionDir, projectName);


        //            var mockFileSystem = new MockFileSystem();
        //            mockFileSystem.AddDirectory(solutionDir);
        //            mockFileSystem.AddDirectory(projectDir);
        //            mockFileSystem.AddDirectory(Path.Combine(solutionDir, projectDir, "bin"));
        //            mockFileSystem.AddDirectory(Path.Combine(solutionDir, projectDir, "obj"));
        //            mockFileSystem.AddDirectory(Path.Combine(solutionDir, projectDir, "mock"));
        //            mockFileSystem.AddFile(Path.Combine(solutionDir, projectDir) + @"\Test3.csproj", mockFileData);
        //            mockFileSystem.AddFile(Path.Combine(solutionDir, projectDir) + @"\Test5.csproj", mockFileData);
        //            mockFileSystem.AddFile(Path.Combine(solutionDir, projectDir) + @"\Test6.csproj", mockFileData);

        //            //IEnumerable<string> expectedFiles = new[] { @"C:\Testordner1\Testordner2\Test3.csproj", @"C:\Testordner1\Testordner2\Test4.vspscc", @"C:\Testordner1\Testordner2\Test5.csproj", @"C:\Testordner1\Testordner2\Test6.csproj" };

        //            //Act
        //            IEnumerable<string> actualResult = detector.DeterminateUnreferenceFilesAndFolders(solutionDir);

        //            //Assertion
        //            actualResult.Should().HaveCount(0);
        //            //actualResult.Should().BeEquivalentTo(expectedFiles);
        //        }
        //        [Test]
        //        public void UnreferencedFileDetector_SortAndFilterEntries()
        //        {

        //        }
        /* bool Is(string fileSystemEntry)
         {
             string[] IgnoredFileExtensions = new string[] { ".suo", ".vspscc" };
             string[] IgnoredFolders = new string[] { "bin", "obj" };
             return !IgnoredFileExtensions.Contains(Path.GetExtension(fileSystemEntry))
                && !(IgnoredFolders.Any(f => fileSystemEntry.EndsWith(Path.DirectorySeparatorChar + f, StringComparison.OrdinalIgnoreCase)
                          //  || fileSystemEntry.Contains(Path.DirectorySeparatorChar + f + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase())));
   
         */
    }

}


























