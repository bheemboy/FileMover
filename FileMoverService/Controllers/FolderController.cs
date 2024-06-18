using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
public class FolderInfo
{
    [JsonRequired]
    public required string Name { get; set; }
    public double Size { get; set; }
    public bool IsDirectory { get; set; }
}

public delegate Task UpdatePercentageDelegate(int percentageDone);

public class FolderController
{
    public static IEnumerable<FolderInfo> GetFolderContents(string folderPath)
    {
        var folderContents = new List<FolderInfo>();

        try
        {
            foreach (var directory in Directory.GetDirectories(folderPath))
            {
                var directoryInfo = new DirectoryInfo(directory);
                folderContents.Add(new FolderInfo
                {
                    Name = directoryInfo.Name,
                    Size = GetDirectorySize(directoryInfo),
                    IsDirectory = true
                });
            }

            foreach (var file in Directory.GetFiles(folderPath))
            {
                var fileInfo = new FileInfo(file);
                folderContents.Add(new FolderInfo
                {
                    Name = fileInfo.Name,
                    Size = fileInfo.Length,
                    IsDirectory = false
                });
            }
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Access denied to {folderPath}");
        }

        return folderContents;
    }

    private static long GetDirectorySize(DirectoryInfo directory)
    {
        long size = 0;
        try
        {
            foreach (var file in directory.GetFiles())
            {
                size += file.Length;
            }

            foreach (var subDirectory in directory.GetDirectories())
            {
                size += GetDirectorySize(subDirectory);
            }
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Access denied to {directory.FullName}");
        }

        return size;
    }

    public static bool CopyPathToFolder(string path, string name, string destinationFolder, double totalsize, UpdatePercentageDelegate updatePercentage)
    {
        string sourcePath = Path.Combine(path, name);
        double copied = 0;

        // Check if the source path exists
        if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
        {
            Console.WriteLine($"Source path '{sourcePath}' does not exist.");
            return false;
        }

        // Determine if the source path is a file or a directory
        bool isFile = File.Exists(sourcePath);

        try
        {
            updatePercentage(0);

            // Create the destination directory if it doesn't exist
            Directory.CreateDirectory(destinationFolder);

            if (isFile)
            {
                // Copy the file to the destination folder
                string fileName = Path.GetFileName(sourcePath);
                string destinationPath = Path.Combine(destinationFolder, fileName);
                File.Copy(sourcePath, destinationPath, true);
                Console.WriteLine($"File '{sourcePath}' copied to '{destinationPath}'");
            }
            else
            {
                // Copy the directory and its contents to the destination folder
                string directoryName = Path.GetFileName(sourcePath);
                string destinationPath = Path.Combine(destinationFolder, directoryName);
                CopyDirectory(sourcePath, destinationPath, ref copied, totalsize, updatePercentage);
                Console.WriteLine($"Directory '{sourcePath}' copied to '{destinationPath}'");
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying '{sourcePath}': {ex.Message}");
        }
        finally
        {
            updatePercentage(100);
        }
        return false;
    }

    private static void CopyDirectory(string sourceDir, string destinationDir, ref double copied, double totalsize, UpdatePercentageDelegate updatePercentage)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the destination directory exists, if not, create it
        if (!Directory.Exists(destinationDir))
            Directory.CreateDirectory(destinationDir);

        // Copy each file into the new directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
            copied += file.Length;
            updatePercentage((int)(100*copied/totalsize));
        }

        // Copy each subdirectory using recursion
        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir, ref copied, totalsize, updatePercentage);
        }
    }

    public static bool DeletePath(string folderPath, string name, UpdatePercentageDelegate updatePercentage)
    {
        try
        {
            updatePercentage(0);

            string fullPath = Path.Combine(folderPath, name);

            FileAttributes attr = File.GetAttributes(fullPath);

            // Check if the path is a directory
            if (attr.HasFlag(FileAttributes.Directory))
            {
                // Recursively delete the directory and its contents
                Directory.Delete(fullPath, true);
                Console.WriteLine($"Directory '{fullPath}' deleted successfully.");
            }
            else
            {
                // Delete the file
                File.Delete(fullPath);
                Console.WriteLine($"File '{fullPath}' deleted successfully.");
            }
            return true;
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"Directory not found: {ex.Message}");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File not found: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"An I/O error occurred: {ex.Message}");
        }
        finally
        {
            updatePercentage(100);
        }
        return false;
    }
}
