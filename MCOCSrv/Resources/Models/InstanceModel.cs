using MCOCSrv.Resources.Raw;
using System.Text.Json;
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

        public string BasePath;

        public InstanceModel(string Name, string? Description, InstanceType Type, string TypeVersion, string? CustomPath)
        {
            this.Name = ConvertNameToFileName(Name);
            this.Description = Description;
            this.Type = Type;
            this.TypeVersion = TypeVersion;
            this.CustomPath = CustomPath != null ? Path.Combine(CustomPath, ConvertNameToFileName(Name)) : null;
            this.CreationDate = DateTime.Now;
            this.BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MCOCSrv", "Instances", ConvertNameToFileName(Name));
        }

        public async Task CreateInstance()
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            try
            {
                if (CustomPath != null)
                {
                    Directory.CreateDirectory(CustomPath);
                    await File.WriteAllTextAsync(Path.Combine(CustomPath, $"{Name}.json"), json);
                }
                else
                {
                    Directory.CreateDirectory(BasePath);
                    await File.WriteAllTextAsync(Path.Combine(BasePath, $"{Name}.json"), json);
                }
                await AppendInstanceListFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing instance File: {ex.Message}");
            }
        }

        private async Task AppendInstanceListFile()
        {
            string InstancesFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MCOCSrv", "instances.json");

            if (File.Exists(InstancesFileLocation))
            {
                string content = await File.ReadAllTextAsync(InstancesFileLocation);
                List<InstanceModel> InstanceList = JsonSerializer.Deserialize<List<InstanceModel>>(content) ?? new List<InstanceModel>();
                try
                {
                    InstanceList.Add(this);
                    string json = JsonSerializer.Serialize(InstanceList, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(InstancesFileLocation, json);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading or pushing instances file: {ex}");
                }
            }
            else
            {

                try
                {
                    List<InstanceModel> InstanceList = new List<InstanceModel>();
                    InstanceList.Add(this);
                    string json = JsonSerializer.Serialize(InstanceList, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(InstancesFileLocation, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create and write instances file: {ex}");
                }
            }

        }

        private static string ConvertNameToFileName(string rawName)
        {
            if (rawName.Contains(' '))
            {
                string[] words = rawName.Split(' ');
                string newName = string.Join("_", words);
                return newName;
            }
            else
            {
                return rawName;
            }
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
