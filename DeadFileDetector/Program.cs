﻿using System;
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

                    bool deleteFiles = args.Any(x => string.Equals(x, "-delete", StringComparison.OrdinalIgnoreCase));

                    if (!File.Exists(solutionFilePath))
                    {
                        throw new ApplicationException("Specified solution file not found.", ApplicationExitCode.FileNotFound);
                    }

                    string solutionDir = Path.GetDirectoryName(solutionFilePath);


                    //New instance of IFileSystem/ProjectFileReader/IUnreferencedFileDetector/UnreferencedFolderDetector
                    IFileSystem fileSystem = new FileSystem();
                    IProjectFileReader projectFileReader = new ProjectFileReader();
                    IUnreferencedFileDetector unreferencedFileDetector = new UnreferencedFileDetector(fileSystem, projectFileReader);
                    UnreferencedFolderDetector unreferencedFolderDetector = new UnreferencedFolderDetector(fileSystem);


                    //Opens and reads a solution file to determine unreferenced files and folders
                    using (Stream solutionFileStream = fileSystem.File.OpenRead(solutionFilePath))
                    {
                        int unreferencedFolderCount = 0;
                        int unreferencedFileCount = 0;

                        //New instance of SolutionFileReader
                        SolutionFileReader solutionFileReader = new SolutionFileReader(solutionFileStream);

                        var projectFiles = solutionFileReader.ReadReferencedProjectFiles().ToList();

                        Console.WriteLine();
                        Console.WriteLine("Determining unreferenced solution folders:");
                        Console.WriteLine();

                        string[] unreferencedFolders = unreferencedFolderDetector.DetectUnreferencedFolders(solutionDir, projectFiles).ToArray();

                        if (unreferencedFolders.Any())
                        {
                            foreach (string unreferencedFolder in unreferencedFolders)
                            {
                                bool deleted = false;

                                if (deleteFiles)
                                {
                                    string absPath = Path.Combine(solutionDir, unreferencedFolder);

                                    Directory.Delete(absPath, true);

                                    deleted = !Directory.Exists(absPath);
                                }

                                unreferencedFolderCount++;
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                Console.Write(string.Format("{0}{1}", new string(IndentChar, 2), unreferencedFolder));

                                //Determined files are getting deleted
                                if (deleteFiles)
                                {
                                    Console.Write("\t");

                                    if (deleted)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.Write("deleted");
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.Write("not deleted");
                                    }
                                }

                                Console.WriteLine();
                                Console.ResetColor();
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(string.Format("{0}No unreferenced solution folders found.", new string(IndentChar, 4)));
                            Console.ResetColor();
                        }


                        var detector = new UnreferencedFileDetector(fileSystem, projectFileReader);

                        Console.WriteLine();
                        Console.WriteLine("Determining unreferenced projects:");

                        if (projectFiles.Any())
                        {
                            IDictionary<string, IEnumerable<string>> dict = detector.DeterminateUnreferenceFilesAndFolders(solutionDir, projectFiles.ToArray());

                            foreach (var projectFile in projectFiles)
                            {
                                Console.WriteLine();
                                Console.WriteLine(string.Format("{0}{1}:", new string(IndentChar, 2), projectFile));
                                Console.WriteLine();

                                var unreferencedFiles = dict[projectFile];

                                if (unreferencedFiles.Any())
                                {
                                    foreach (var item in unreferencedFiles)
                                    {
                                        unreferencedFileCount++;

                                        bool deleted = false;

                                        //Determined folders are getting deleted
                                        if (deleteFiles)
                                        {
                                            string absPath = Path.Combine(solutionDir, item);

                                            if (Directory.Exists(absPath))
                                            {
                                                Directory.Delete(absPath, true);
                                                deleted = !Directory.Exists(absPath);
                                            }
                                            else if (File.Exists(absPath))
                                            {
                                                File.Delete(absPath);
                                                deleted = !File.Exists(absPath);
                                            }
                                            else
                                            {
                                                // because its deleted by the delete of the parent folder
                                                deleted = true;
                                            }
                                        }

                                        Console.ForegroundColor = ConsoleColor.Magenta;
                                        Console.Write(string.Format("{0}{1}", new string(IndentChar, 4), item));

                                        if (deleteFiles)
                                        {
                                            Console.CursorLeft = dict.SelectMany(x => x.Value).Max(x => x.Length) + 8;

                                            if (deleted)
                                            {
                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.Write("deleted");
                                            }
                                            else
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.Write("not deleted");
                                            }
                                        }

                                        Console.WriteLine();
                                        Console.ResetColor();
                                    }


                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine(string.Format("{0}No unreferenced files found.", new string(IndentChar, 4)));
                                    Console.ResetColor();
                                }
                            }

                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("No referenced projects found.");
                        }


                        if (unreferencedFolderCount > 0 && unreferencedFileCount > 0)
                        {
                            throw new ApplicationException(string.Format("{0} unreferenced files and {1} unreferenced solution folders found.", unreferencedFileCount, unreferencedFolderCount), ApplicationExitCode.UnreferencedSolutionFoldersAndFilesFound);
                        }

                        if (unreferencedFileCount > 0)
                        {
                            throw new ApplicationException(string.Format("{0} unreferenced files found.", unreferencedFileCount), ApplicationExitCode.UnreferencedFilesFound);
                        }

                        if (unreferencedFolderCount > 0)
                        {
                            throw new ApplicationException(string.Format("{0} unreferenced solution folders found.", unreferencedFolderCount), ApplicationExitCode.UnreferencedSolutionFoldersFound);
                        }
                    }
                }
            }
            catch (ApplicationException ex)
            {

                switch (ex.ExitCode)
                {
                    case ApplicationExitCode.UnreferencedFilesFound:
                    case ApplicationExitCode.UnreferencedSolutionFoldersFound:
                    case ApplicationExitCode.UnreferencedSolutionFoldersAndFilesFound:
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
            Console.WriteLine("DeadFileDetector PathToTargetSolution [-delete]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("\t-delete: deletes the found files and folders");
        }
    }
}


