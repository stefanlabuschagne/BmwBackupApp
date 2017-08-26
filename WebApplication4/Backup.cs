using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;
using WebApplication4.Models;
using System.Threading.Tasks;

namespace BackupUtilites

{
    static class BackupSync

    {

        private static String SourcePath;
        private static String TargetPath;
        private static String BackupID;


        public static void Main(String TheSourcePath, String TheTargetPath, string TheBackupID)
        {

            //String SourcePath = @"C:\Users\vul2214\Desktop\SourceFiles";  // No \ after the last directory!
            //String TargetPath = @"C:\Users\vul2214\Desktop\TargetFiles"; // No \ after the last directory!

            // Using .NET 4

            SourcePath = TheSourcePath;
            TargetPath = TheTargetPath;
            BackupID = TheBackupID;

            var dir1 = new DirectoryInfo(SourcePath);
            var dir2 = new DirectoryInfo(TargetPath);

            // Files in both Structures that differ (To Be copied)
            var changedFiles = from fi1 in dir1.EnumerateFiles("*", SearchOption.AllDirectories)
                               from fi2 in dir2.EnumerateFiles("*", SearchOption.AllDirectories)
                               where fi1.FullName == fi2.FullName.Replace(TargetPath, SourcePath).ToString()
                               & fi1.LastWriteTime != fi2.LastWriteTime & fi1.Length != fi2.Length
                               select fi1;


            bool Retval = CopyChangedFiles(changedFiles);


            // #2
            var FilesInSource = from fi1 in dir1.EnumerateFiles("*", SearchOption.AllDirectories)
                                select fi1.FullName;

            var FilesInDestination = from fi1 in dir2.EnumerateFiles("*", SearchOption.AllDirectories)
                                     select fi1.FullName.Replace(TargetPath, SourcePath).ToString();

            // This is the NEW files we need to copy.
            var filesWeNeedtoCopy = from fi1 in FilesInSource.Except(FilesInDestination)
                                    select fi1;

            bool Retval1 = CopyFiles(filesWeNeedtoCopy);


            // #3
            // This is files in the Destination we need to Delete
            FilesInSource = from fi1 in dir1.EnumerateFiles("*", SearchOption.AllDirectories)
                            select fi1.FullName.Replace(SourcePath, TargetPath).ToString();

            FilesInDestination = from fi1 in dir2.EnumerateFiles("*", SearchOption.AllDirectories)
                                 select fi1.FullName;

            // This is the NEW files we need to copy.
            var FilesToDelete = from fi1 in FilesInDestination.Except(FilesInSource)
                                select fi1;

            bool Retval2 = DeleteFiles(FilesToDelete);

            HttpRuntime.Cache[BackupID.ToString()] = DateTime.Now.ToString() + ": Backup Complete!  " + HttpRuntime.Cache[BackupID.ToString()].ToString();

            //Console.Write("Press any key...");
            //Console.ReadKey();

        }

        static bool CopyChangedFiles(IEnumerable<FileInfo> changedFiles)
        {

            // TEST - Copies over everything from the source to the destination
            // Source and Tagrget Files and Directories obviously exist already
            foreach (var fi in changedFiles)
            {
                Console.WriteLine(fi.FullName);

                // Path  that needs to exist for the file 
                String DaTargetPath = fi.DirectoryName.ToString().Replace(SourcePath, TargetPath).ToString();

                string destFile = System.IO.Path.Combine(DaTargetPath, fi.Name.ToString());

                // To copy a file to another location and 
                // overwrite the destination file if it already exists.
                try
                {
                    System.IO.File.Copy(fi.FullName, destFile, true);
                    HttpRuntime.Cache[BackupID.ToString()] = DateTime.Now.ToString() + ": Updating  " + destFile.ToString() + HttpRuntime.Cache[BackupID.ToString()].ToString();
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.Message.ToString());
                }
            }

            return true;
        }



        static bool DeleteFiles(System.Collections.Generic.IEnumerable<string> FilesToDelete)
        {

            // Delets all files in the Passed Array 

            // TEST - Copies over everything from the source to the destination

            foreach (var DaFilename in FilesToDelete)
            {

                Console.WriteLine(DaFilename);
                try
                {

                    // Delete the Filename!
                    System.IO.File.Delete(DaFilename.ToString());
                    HttpRuntime.Cache[BackupID.ToString()] = DateTime.Now.ToString() + ": Deleting  " + DaFilename.ToString() +  HttpRuntime.Cache[BackupID.ToString()].ToString();

                    // If the directory is empty, delete it as well
                    // This way we clean up the Lost Directories.
                    try
                    {
                        Directory.Delete(DaFilename.Substring(0, DaFilename.LastIndexOf(@"\")));
                    }
                    catch (Exception Ex)
                    {
                        // Dir not Empty! :-)
                    }
                }
                catch (Exception Ex)
                {
                    // Report the exception that the file did not delete.....
                    // As part of Feedback
                    Console.WriteLine(Ex.InnerException.Message);
                }

            }

            return true;

        }


        static bool CopyFiles(System.Collections.Generic.IEnumerable<string> FilesToCopy)

        {

            // Copies files form Source to Destinatrion

            // TEST - Copies over everything from the source to the destination

            foreach (var DaFile in FilesToCopy)
            {
                Console.WriteLine(DaFile);

                // Path  that needs to exist for the file 
                String DaTargetPath = DaFile.ToString().Replace(SourcePath, TargetPath).ToString();
                DaTargetPath = DaTargetPath.Substring(0, DaTargetPath.LastIndexOf(@"\")).ToString();

                // Get the Directory Here

                // string destFile = System.IO.Path.Combine(DaTargetPath, fi.Name.ToString());

                // Create the Target Directory if it does noit exist
                if (!System.IO.Directory.Exists(DaTargetPath))
                {
                    System.IO.Directory.CreateDirectory(DaTargetPath);
                }

                // To copy a file to another location and 
                // overwrite the destination file if it already exists.
                System.IO.File.Copy(DaFile, DaFile.Replace(SourcePath, TargetPath), true);
                HttpRuntime.Cache[BackupID.ToString()] = DateTime.Now.ToString() + ": Copying  " + TargetPath.ToString() + HttpRuntime.Cache[BackupID.ToString()].ToString();

            }

            return true;

        }

    }
}