using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileMoverService
{
    public enum MessageType{busy=1, command=2, percentage=3, refreshfolder=4}
    public class Message 
    {
        public MessageType Type {get; set;}
        public Message(MessageType type)
        {
            Type = type;
        }
        [JsonIgnore]
        public string Json 
        { 
            get 
            {
                return JsonSerializer.Serialize(this, this.GetType()); 
            }
        }
    }
    public class BusyMessage : Message 
    {
        public bool Busy {get; set;}
        public BusyMessage(bool busy) : base(MessageType.busy)
        {
            Busy = busy;
        }
    }

    public class CommandMessage : Message 
    {
        public string Command {get; set;}
        public CommandMessage(string command) : base(MessageType.command)
        {
            Command = command;
        }
    }
    public class RefreshFolderMessage : Message  
    {
        public string Folder {get; set;}
        public RefreshFolderMessage(string folder) : base(MessageType.refreshfolder)
        {
            Folder = folder;
        }
    }
    public class PercentageMessage : RefreshFolderMessage  
    {
        public int Percentage {get; set;}
        public PercentageMessage(int percentage, string folder) : base(folder)
        {
            Type = MessageType.percentage;
            Percentage = percentage;
        }
    }
}
