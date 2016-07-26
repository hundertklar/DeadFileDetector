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
        private const char IndentChar = ' ';

        static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    throw new ApplicationException("No solution file specified.", ApplicationExitCode.InvalidArguments);
                }
                else if (string.Equals(args[0], "/help", StringComparison.OrdinalIgnoreCase))
                {
                    WriteHelpToConsole();
                }
                else
                {
                    string solutionFilePath = args[0];

                    if (!File.Exists(solutionFilePath))
                    {
                        throw new ApplicationException("Specified soluten file not found.", ApplicationExitCode.FileNotFound);
                    }

                    string solutionDir = Path.GetDirectoryName(solutionFilePath);

                    IFileSystem fileSystem = new FileSystem();
                    IProjectFileReader projectFileReader = new ProjectFileReader();
                    IUnreferencedFileDetector unreferencedFileDetector = new UnreferencedFileDetector(fileSystem, projectFileReader);

                    using (Stream solutionFileStream = fileSystem.File.OpenRead(solutionFilePath))
                    {
                        SolutionFileReader solutionFileReader = new SolutionFileReader(solutionFileStream);

                        var referencedFiles = solutionFileReader.ReadReferencedFiles().ToArray();


                        var detector = new UnreferencedFileDetector(fileSystem, projectFileReader); //detector anlegen


                        Console.WriteLine();
                        Console.WriteLine("Determining referenct projects:");

                        if (referencedFiles.Any())
                        {
                            int unreferencedFileCount = 0;

                            foreach (var projectFile in referencedFiles)
                            {
                                Console.WriteLine();
                                Console.WriteLine(string.Format("{0}{1}:", new string(IndentChar, 2), projectFile));
                                Console.WriteLine();

                                var unreferencedFiles = detector.DeterminateUnreferenceFilesAndFolders(solutionDir, projectFile).ToArray();

                                if (unreferencedFiles.Any())
                                {
                                    foreach (var item in unreferencedFiles)
                                    {
                                        unreferencedFileCount++;

                                        Console.ForegroundColor = ConsoleColor.Magenta;
                                        Console.WriteLine(string.Format("{0}{1}:", new string(IndentChar, 4), item));
                                        Console.ResetColor();

                                    }
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine(string.Format("{0}No unreferenced Files found.", new string(IndentChar, 4)));
                                    Console.ResetColor();
                                }

                            }

                            if (unreferencedFileCount > 0)
                            {
                                throw new ApplicationException(string.Format("{0} unreferenced files found.", unreferencedFileCount), ApplicationExitCode.UnreferencedFilesFound);
                            }

                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("No referenced projects found.");
                        }
                    }
                }
            }
            catch (ApplicationException ex)
            {

                switch (ex.ExitCode)
                {
                    case ApplicationExitCode.UnreferencedFilesFound:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine();
                        Console.WriteLine(ex.Message);
                        Console.ResetColor();
                        break;
                    case ApplicationExitCode.Failed:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine();
                        Console.WriteLine(ex.Message);
                        Console.ResetColor();
                        break;
                    case ApplicationExitCode.InvalidArguments:
                        WriteHelpToConsole(ex.Message);
                        break;
                    case ApplicationExitCode.FileNotFound:
                        WriteHelpToConsole(ex.Message);
                        break;
                    default:
                        break;
                }

                return (int)ex.ExitCode;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                return (int)ApplicationExitCode.Failed;
            }
            finally
            {
                if (Debugger.IsAttached)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }
            }

            return (int)ApplicationExitCode.Succeeded;
        }

        static void WriteHelpToConsole(string reason = null)
        {
            if (reason != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine();
                Console.WriteLine(reason);
                Console.ResetColor();

            }

            Console.WriteLine();
            Console.WriteLine("DeadFileDetector PathToTargetSolution");
        }

    }

}



// ProjectFileReader anlegen +
// Detektor anlegen +
// projectFiles iterier
// und project File path Detektor übergeben

// Ergebnis vom Detektor speichern 
// listen verbinden
// liste ausgeben

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