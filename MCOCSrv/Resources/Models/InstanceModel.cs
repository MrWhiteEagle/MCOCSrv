using MCOCSrv.Resources.Classes;
namespace MCOCSrv.Resources.Models
{
    public class InstanceModel
    {
        public ConsoleWrapper? Console;
        public string id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public InstanceType Type { get; set; }
        public string Version { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastUsed { get; set; }
        public string? CustomPath { get; set; }
        public string BasePath { get; set; }
        public string LaunchArguments { get; set; }
        public string MaxHeap { get; set; }
        public string MinHeap { get; set; }
        public List<QuickAction> Actions { get; set; }

        public InstanceModel(string Name, string? Description, InstanceType Type, string Version, string? CustomPath)
        {
            this.Name = Name;
            this.Description = Description;
            this.Type = Type;
            this.Version = Version;
            this.CustomPath = CustomPath;
            this.CreationDate = DateTime.Now;
            this.BasePath = Global.AppDataInstancesPath;
            this.LaunchArguments = "";
            this.MaxHeap = "";
            this.MinHeap = "";
            this.Actions = new();
        }

        public string GetPath()
        {
            return string.IsNullOrEmpty(CustomPath) ? BasePath : CustomPath;
        }

        public bool HasCustomPath()
        {
            if (string.IsNullOrEmpty(CustomPath))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void InitializeConsole()
        {
            this.Console = new ConsoleWrapper(this);
        }

        public void DisposeConsole()
        {
            if (this.Console != null)
            {
                this.Console.Dispose();
            }
            Console = null;
            UILogger.LogUI($"[INSTANCE {Name}] Disposed of console.");

        }

        public bool IsRunning()
        {
            if (this.Console != null && this.Console.IsRunning)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
