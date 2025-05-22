using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Raw;
namespace MCOCSrv.Resources.Models
{
    public class InstanceModel
    {
        public ConsoleWrapper? Console;
        public string Name { get; set; }
        public string? Description { get; set; }
        public InstanceType Type { get; set; }
        public string TypeVersion { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastUsed { get; set; }
        public string? CustomPath { get; set; }
        public string BasePath { get; set; }

        public string LaunchArguments { get; set; }

        public InstanceModel(string Name, string? Description, InstanceType Type, string TypeVersion, string? CustomPath)
        {
            this.Name = Name;
            this.Description = Description;
            this.Type = Type;
            this.TypeVersion = TypeVersion;
            this.CustomPath = CustomPath;
            this.CreationDate = DateTime.Now;
            this.BasePath = Global.AppDataInstancesPath;
            this.LaunchArguments = "";
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
