using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using FileMoverService;

public class StorageHub : Hub
{
    private readonly List<ApplicationFolder> _applicationFolders;
    private static readonly object _lock = new();
    private static long _lastTicks = DateTime.MinValue.Ticks; 
    private static int _lastPercentage = -1;
    private static string _Command = "";
    private static bool _Busy = false;
    private static string _targetFolder = "";

    private async Task _SendMessage(string message, string? ConnectionId = null)
    {
        if (ConnectionId is null)
        {
            await Clients.All.SendAsync("Message", message);
        }
        else
        {
            await Clients.Client(ConnectionId).SendAsync("Message", message);
        }
    }

    private async Task _SendLogMessage(string message, string? ConnectionId = null)
    {
        await _SendMessage(new LogMessage(message).Json, ConnectionId);
    }
    private async Task _ClearLogMessages()
    {
        await _SendLogMessage("");
    }

    private async Task _SendPercentage(int percentage)
    {
        if (percentage == _lastPercentage) return;

        // Send percentage only if the change is atleast 1% AND Last percentage was sent atleast 1 second ago
        // OR percentage is 0 or 100
        if (((percentage >= _lastPercentage+1) && (new TimeSpan(DateTime.Now.Ticks - _lastTicks).TotalSeconds > 1)) || 
            (percentage == 0) || 
            (percentage == 100))
        {
            await _SendMessage(new PercentageMessage(percentage, _targetFolder).Json);
            _lastPercentage = percentage;
            _lastTicks = DateTime.Now.Ticks;
        }

    }

    public override async Task OnConnectedAsync()
    {
        await _SendMessage(new BusyMessage(_Busy).Json, Context.ConnectionId);
        await _SendMessage(new CommandMessage(_Command).Json, Context.ConnectionId);
        await _SendMessage(new PercentageMessage(_lastPercentage, _targetFolder).Json, Context.ConnectionId);
    }

    public StorageHub(List<ApplicationFolder> applicationFolders)
    {
        _applicationFolders = applicationFolders;
    }

    public string GetApplicationFolder()
    {
        return JsonSerializer.Serialize(_applicationFolders);
    }
    public async Task<string> GetFolderContents(string folderPath)
    {
        var folderContents = await Task.Run(() => FolderController.GetFolderContents(folderPath, _SendLogMessage));
        return JsonSerializer.Serialize(folderContents);
    }
    public async Task<bool> DeletePath(string folderPath, string name)
    {
        if (Monitor.TryEnter(_lock))
        {
            try
            {
                _targetFolder = folderPath;

                await _ClearLogMessages();
                await _SendMessage(new BusyMessage(_Busy = true).Json);
                await _SendMessage(new CommandMessage(_Command = $"Delete '{name}' from '{folderPath}'").Json);

                if (FolderController.DeletePath(folderPath, name, _SendPercentage, _SendLogMessage))
                {
                    return true;
                }
                return false;
            }
            finally
            {
                Monitor.Exit(_lock);
                await _SendMessage(new RefreshFolderMessage(_targetFolder).Json);
                await _SendMessage(new BusyMessage(_Busy = false).Json);
            }
        }
        else
        {
            throw new InvalidOperationException("Server is busy.");
        }
    }
    public async Task<bool> CopyPathToFolder(string sourcePath, string jsonSourceItem, string destinationPath)
    {
        if (Monitor.TryEnter(_lock))
        {
            try
            {
                FolderInfo? SourceItem = JsonSerializer.Deserialize<FolderInfo>(jsonSourceItem);
                string name = SourceItem?.Name?? "";
                double size = SourceItem?.Size?? 0;
                _targetFolder = destinationPath;
                
                await _ClearLogMessages();
                await _SendMessage(new BusyMessage(_Busy = true).Json);
                await _SendMessage(new CommandMessage(_Command = $"Copy '{name}' from '{sourcePath}' to '{destinationPath}'").Json);

                if (FolderController.CopyPathToFolder(sourcePath, name, destinationPath, size, _SendPercentage, _SendLogMessage))
                {
                    return true;
                }
                return false;
            }
            finally
            {
                await _SendMessage(new RefreshFolderMessage(_targetFolder).Json);
                await _SendMessage(new BusyMessage(_Busy = false).Json);
                Monitor.Exit(_lock);
            }
        }
        else
        {
            throw new InvalidOperationException("Server is busy.");
        }
    }

    public async Task<bool> VerifyChecksum(string folderPath, string checksumFile)
    {
        if (Monitor.TryEnter(_lock))
        {
            try
            {
                string checksumFilePath = Path.Combine(folderPath, checksumFile);

                await _ClearLogMessages();
                await _SendMessage(new BusyMessage(_Busy = true).Json);
                await _SendMessage(new CommandMessage(_Command = $"Verify checksums in '{checksumFilePath}'").Json);

                return FolderController.VerifyChecksum(checksumFilePath, _SendPercentage, _SendLogMessage);
            }
            finally
            {
                await _SendMessage(new BusyMessage(_Busy = false).Json);
                Monitor.Exit(_lock);
            }
        }
        else
        {
            throw new InvalidOperationException("Server is busy.");
        }
    }

    public async Task<bool> CreateChecksum(string folderPath, string checksumItem)
    {
        if (Monitor.TryEnter(_lock))
        {
            try
            {
                _targetFolder = folderPath;
                string checksumPath = Path.Combine(folderPath, checksumItem);

                await _ClearLogMessages();
                await _SendMessage(new BusyMessage(_Busy = true).Json);
                await _SendMessage(new CommandMessage(_Command = $"Create checksums file for '{checksumPath}'").Json);

                return FolderController.CreateChecksum(folderPath, checksumItem, _SendPercentage, _SendLogMessage);
            }
            finally
            {
                await _SendMessage(new RefreshFolderMessage(_targetFolder).Json);
                await _SendMessage(new BusyMessage(_Busy = false).Json);
                Monitor.Exit(_lock);
            }
        }
        else
        {
            throw new InvalidOperationException("Server is busy.");
        }
    }
}
