using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Abstractions.TestingHelpers;

namespace DeadFileDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            string slnDir = @"C:\Users\dkussberger\Source\Repos\DeadFileDetector\DeadFileDetector.sln";

            string combinedSlnDir = @"C:\Users\dkussberger\Source\Repos\DeadFileDetector";

            IFileSystem fileSystem = new FileSystem();

            IProjectFileReader projectFileReader = new ProjectFileReader();

            UnreferencedFileDetector unreferencedFileDetector = new UnreferencedFileDetector(fileSystem, projectFileReader);

            using (Stream solutionFileStream = fileSystem.File.OpenRead(slnDir))
            {
                SolutionFileReader solutionFileReader = new SolutionFileReader(solutionFileStream);

                var referencedFiles = solutionFileReader.ReadReferencedFiles();


                var detector = new UnreferencedFileDetector(fileSystem, projectFileReader); //detector anlegen

                

                foreach (var projectFile in referencedFiles)
                {
                   unrefer
                    //foreach (var item in detector.DeterminateUnreferenceFilesAndFolders(combinedSlnDir, projectFile))
                    //{
                        
                    //    int relativePathStartIndex = item.IndexOf("Repos");
                    //    string substring = item.Substring(relativePathStartIndex);
                    //    string relativePath = @"..\..\" + substring;

                    //    Console.WriteLine(relativePath);

                    }

                    Console.ReadLine();



                    // ProjectFileReader anlegen +
                    // Detektor anlegen +
                    // projectFiles iterier
                    // und project File path Detektor übergeben

                    // Ergebnis vom Detektor speichern 
                    // listen verbinden
                    // liste ausgeben
                }




                // IProjectFileReader projectFileReader = new ProjectFileReader();



                //string projectFile1 = @"DeadFileDetector\DeadFileDetector.csproj";

                ////foreach (var item in detector.DeterminateUnreferenceFilesAndFolders(slnDir, projectFile1))
                //{
                //    int relativePathStartIndex = item.IndexOf("Repos");
                //    string substring = item.Substring(relativePathStartIndex);
                //    string relativePath = @"..\..\" + substring;

                //    Console.WriteLine(relativePath);
                //}
                //Console.ReadKey();
            }
        }
    }
}


