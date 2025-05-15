using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Raw;
namespace MCOCSrv.Resources.Models
{
    public class InstanceModel
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public InstanceType Type { get; set; }
        public string TypeVersion { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastUsed { get; set; }
        public string? CustomPath { get; set; }

        public string BasePath { get; set; }

        public InstanceModel(string Name, string? Description, InstanceType Type, string TypeVersion, string? CustomPath)
        {
            this.Name = Name;
            this.Description = Description;
            this.Type = Type;
            this.TypeVersion = TypeVersion;
            this.CustomPath = CustomPath;
            this.CreationDate = DateTime.Now;
            this.BasePath = Global.AppDataInstancesPath;
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

    }
}
