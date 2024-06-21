using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography;
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
public delegate Task SendLogMessage(string message, string? ConnectionId = null);

public class FolderController
{
    public static IEnumerable<FolderInfo> GetFolderContents(string folderPath, SendLogMessage sendLogMessage)
    {
        var folderContents = new List<FolderInfo>();

        try
        {
            foreach (var directory in Directory.GetDirectories(folderPath).OrderBy(dir => dir).ToList())
            {
                var directoryInfo = new DirectoryInfo(directory);
                folderContents.Add(new FolderInfo
                {
                    Name = directoryInfo.Name,
                    Size = GetDirectorySize(directoryInfo, sendLogMessage),
                    IsDirectory = true
                });
            }

            foreach (var file in Directory.GetFiles(folderPath).OrderBy(file => file).ToList())
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
            sendLogMessage($"Access denied to {folderPath}");
        }
        catch (Exception e)
        {
            sendLogMessage($"Error getting folder contents '{folderPath}': {e.Message}");
        }

        return folderContents;
    }

    private static long GetDirectorySize(DirectoryInfo directory, SendLogMessage sendLogMessage)
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
                size += GetDirectorySize(subDirectory, sendLogMessage);
            }
        }
        catch (UnauthorizedAccessException)
        {
            sendLogMessage($"Access denied to {directory.FullName}");
        }
        catch (Exception e)
        {
            sendLogMessage($"Error getting directory size '{directory.FullName}': {e.Message}");
            throw;
        }

        return size;
    }

    public static bool CopyPathToFolder(string path, string name, string destinationFolder, double totalsize, UpdatePercentageDelegate updatePercentage, SendLogMessage sendLogMessage)
    {
        string sourcePath = Path.Combine(path, name);
        double copied = 0;

        // Check if the source path exists
        if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
        {
            sendLogMessage($"Path '{sourcePath}' not found");
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
                sendLogMessage($"Copying '{destinationPath}'");
                File.Copy(sourcePath, destinationPath, true);
            }
            else
            {
                // Copy the directory and its contents to the destination folder
                string directoryName = Path.GetFileName(sourcePath);
                string destinationPath = Path.Combine(destinationFolder, directoryName);
                sendLogMessage($"Copying directory '{destinationPath}'");
                CopyDirectory(sourcePath, destinationPath, ref copied, totalsize, updatePercentage, sendLogMessage);
            }
            return true;
        }
        catch (Exception ex)
        {
            sendLogMessage($"Error copying '{sourcePath}': {ex.Message}");
        }
        finally
        {
            updatePercentage(100);
        }
        return false;
    }

    private static void CopyDirectory(string sourceDir, string destinationDir, ref double copied, double totalsize, UpdatePercentageDelegate updatePercentage, SendLogMessage sendLogMessage)
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
            sendLogMessage($"Copying '{targetFilePath}'");

            file.CopyTo(targetFilePath);
            copied += file.Length;
            updatePercentage((int)(100 * copied / totalsize));
        }

        // Copy each subdirectory using recursion
        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            sendLogMessage($"Copying directory '{newDestinationDir}'");
            CopyDirectory(subDir.FullName, newDestinationDir, ref copied, totalsize, updatePercentage, sendLogMessage);
        }
    }

    public static bool DeletePath(string folderPath, string name, UpdatePercentageDelegate updatePercentage, SendLogMessage sendLogMessage)
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
                sendLogMessage($"Deleting directory '{fullPath}'");
                Directory.Delete(fullPath, true);
            }
            else
            {
                // Delete the file
                sendLogMessage($"Deleting file '{fullPath}'");
                File.Delete(fullPath);
            }
            return true;
        }
        catch (DirectoryNotFoundException ex)
        {
            sendLogMessage($"Directory not found: {ex.Message}");
        }
        catch (FileNotFoundException ex)
        {
            sendLogMessage($"File not found: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            sendLogMessage($"Access denied: {ex.Message}");
        }
        catch (IOException ex)
        {
            sendLogMessage($"An I/O error occurred: {ex.Message}");
        }
        catch (Exception ex)
        {
            sendLogMessage($"Error deleting: {ex.Message}");
        }
        finally
        {
            updatePercentage(100);
        }
        return false;
    }
    public static bool VerifyChecksum(string checksumFilePath, UpdatePercentageDelegate updatePercentage, SendLogMessage sendLogMessage)
    {
        // Check if the source path exists
        if (!File.Exists(checksumFilePath))
        {
            sendLogMessage($"Checksum file '{checksumFilePath}' does not exist");
            return false;
        }

        try
        {
            updatePercentage(0);

            // Read the .sha256 file
            var lines = File.ReadAllLines(checksumFilePath);

            string[] separator = ["  "];
            
            foreach (var line in lines)
            {
                // Split the line into checksum and filename
                var parts = line.Split(separator, StringSplitOptions.None);
                if (parts.Length != 2)
                {
                    sendLogMessage($"Invalid line format: {line}");
                    return false;
                }

                string expectedChecksum = parts[0];
                string filename = parts[1];

                string filePath = Path.Combine(Path.GetDirectoryName(checksumFilePath)?? "", filename);

                if (!File.Exists(filePath))
                {
                    sendLogMessage($"File not found: {filePath}");
                    return false;
                }

                sendLogMessage($"Verifying checksum for file: {filePath}");

                // Compute the SHA256 checksum of the file
                string computedChecksum;
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    using SHA256 sha256 = SHA256.Create();
                    byte[] hashBytes = sha256.ComputeHash(fileStream);
                    computedChecksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }

                // Compare the expected and computed checksums
                if (expectedChecksum != computedChecksum)
                {
                    sendLogMessage($"Checksum mismatch for file: {filePath}");
                    sendLogMessage($"Expected: {expectedChecksum}");
                    sendLogMessage($"Computed: {computedChecksum}");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            sendLogMessage($"Error verifying '{checksumFilePath}': {ex.Message}");
        }
        finally
        {
            updatePercentage(100);
        }
        return false;
    }

    public static bool CreateChecksum(string folderPath, string checksumItem, UpdatePercentageDelegate updatePercentage, SendLogMessage sendLogMessage)
    {
        // Check if the source path exists
        string checksumPath = Path.Combine(folderPath, checksumItem);
        if (!File.Exists(checksumPath) && !Directory.Exists(checksumPath))
        {
            sendLogMessage($"Source path '{checksumPath}' does not exist");
            return false;
        }

        try
        {
            updatePercentage(0);
            string checksumFilePath = checksumPath + ".sha256";

            if (File.Exists(checksumPath))
            {
                // Process a single file
                sendLogMessage($"Calculating checksum for '{checksumPath}'");
                CreateChecksumForFile(checksumPath, checksumFilePath);
            }
            else
            {
                // Process all files in the directory
                using (StreamWriter writer = new StreamWriter(checksumFilePath))
                {
                    foreach (string filePath in Directory.GetFiles(checksumPath, "*", SearchOption.AllDirectories))
                    {
                        sendLogMessage($"Calculating checksum for '{filePath}'");
                        string hash = ComputeSHA256Checksum(filePath);
                        string relativePath = Path.GetRelativePath(folderPath, filePath).Replace('\\', '/');
                        writer.WriteLine($"{hash}  {relativePath}");
                    }
                }
            }
            sendLogMessage($"Checksum file '{checksumFilePath}' created");

            return true;
        }
        catch (Exception ex)
        {
            sendLogMessage($"Error creating checksum for '{checksumPath}': {ex.Message}");
        }
        finally
        {
            updatePercentage(100);
        }
        return false;
    }


    private static void CreateChecksumForFile(string filePath, string checksumFilePath)
    {
        using (StreamWriter writer = new StreamWriter(checksumFilePath))
        {
            string hash = ComputeSHA256Checksum(filePath);
            writer.WriteLine($"{hash}  {Path.GetFileName(filePath)}");
        }
    }

    private static string ComputeSHA256Checksum(string filePath)
    {
        using (FileStream fileStream = File.OpenRead(filePath))
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(fileStream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }

}
