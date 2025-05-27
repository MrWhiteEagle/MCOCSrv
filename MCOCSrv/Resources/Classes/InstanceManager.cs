using MCOCSrv.Resources.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

namespace MCOCSrv.Resources.Classes
{
    public class InstanceManager
    {
        public ObservableCollection<InstanceModel> instances { get; private set; } = new();
        public ObservableCollection<InstanceModel> running { get; private set; } = new();
        private ServerVersionFetcher fetcher;
        public InstanceManager(ServerVersionFetcher fetcher)
        {
            this.fetcher = fetcher;
            instances = new();
            running = new();
        }

        public async Task FetchInstances()
        {
            UILogger.LogUI("[INSTANCE MANAGER] FETCH - START");
            string json;
            if (File.Exists(Global.AppDataInstancesFilePath))
            {
                json = await File.ReadAllTextAsync(Global.AppDataInstancesFilePath);
            }
            else
            {
                json = "";
            }
            if (string.IsNullOrEmpty(json))
            {
                instances.Clear();
                UILogger.LogUI("[INSTANCE MANAGER] Instances are empty/corrupted!");
            }
            else
            {
                var list = JsonSerializer.Deserialize<List<InstanceModel>>(json);
                if (list.Count > 0 || list != null)
                {
                    instances.Clear();
                    foreach (InstanceModel instance in list)
                    {
                        instance.InitializeConsole();
                        instances.Add(instance);
                        UILogger.LogUI($"[INSTANCE MANAGER] {instance.Name} Found and Initiated");
                    }
                    instances.OrderBy(i => i.id).ToList();
                    UILogger.LogUI($"[INSTANCE MANAGER] Found {instances.Count} total.");
                }
            }
            UILogger.LogUI("[INSTANCE MANAGER] FETCH - DONE");
        }

        public InstanceModel? GetInstanceById(string id)
        {
            foreach (var instance in instances)
            {
                if (instance.id == id)
                {
                    return instance;
                }
            }
            return null;
        }

        public async Task DeleteInstance(InstanceModel instance)
        {
            try
            {
                if (instance.CustomPath != null)
                {
                    Directory.Delete(instance.CustomPath, true);
                }
                else
                {
                    Directory.Delete(instance.BasePath, true);
                }
                instances.Remove(instance);
                running.Remove(instance);
                string json = JsonSerializer.Serialize(instances, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(Global.AppDataInstancesFilePath, json);
                UILogger.LogUI($"[INSTANCE MANAGER] DELETE INSTANCE - DONE");

            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[INSTANCE MANAGER] Error deleting instance: {ex.Message}");
            }
        }

        public static async Task<List<InstanceModel>> readInstances()
        {
            if (File.Exists(Path.Combine(Global.AppDataPath, "instances.json")))
            {
                string instanceData = await File.ReadAllTextAsync(Path.Combine(Global.AppDataPath, "instances.json"));
                List<InstanceModel> list = JsonSerializer.Deserialize<List<InstanceModel>>(instanceData);
                return list;
            }
            else
            {
                return new List<InstanceModel>();
            }
        }
        public async Task CreateInstance(InstanceModel instance)
        {
            instance.id = ConvertNameToFileName(instance.Name);
            instance.BasePath = Path.Combine(Global.AppDataInstancesPath, instance.id);
            if (instance.CustomPath != null)
            {
                instance.CustomPath = Path.Combine(instance.CustomPath, instance.id);
            }
            string json = JsonSerializer.Serialize(instance, new JsonSerializerOptions { WriteIndented = true });
            try
            {
                await SaveInstance(instance);
                await fetcher.DownloadInstance(instance);
                instance.InitializeConsole();
                instances.Add(instance);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[INSTANCE MANAGER] Error writing instance File: {ex.Message}");
            }
        }
        public async Task SaveInstance(InstanceModel instance)
        {
            string path = Path.Combine(instance.GetPath(), $"{instance.id}.json");
            string json = JsonSerializer.Serialize(instance, new JsonSerializerOptions { WriteIndented = true });
            try
            {
                await File.WriteAllTextAsync(path, json);
                await AppendInstanceListFile(instance);
            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[INSTANCE MANAGER] Error saving instance {instance.Name}: {ex.Message}");
            }

        }

        private async Task AppendInstanceListFile(InstanceModel instance)
        {

            if (File.Exists(Global.AppDataInstancesFilePath))
            {
                string content = await File.ReadAllTextAsync(Global.AppDataInstancesFilePath);
                List<InstanceModel> InstanceList = JsonSerializer.Deserialize<List<InstanceModel>>(content) ?? new List<InstanceModel>();
                try
                {
                    InstanceList.Add(instance);
                    string json = JsonSerializer.Serialize(InstanceList, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(Global.AppDataInstancesFilePath, json);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[INSTANCE MANAGER] Error reading or pushing instances file: {ex}");
                }
            }
            else
            {

                try
                {
                    List<InstanceModel> InstanceList = new List<InstanceModel>();
                    InstanceList.Add(instance);
                    string json = JsonSerializer.Serialize(InstanceList, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(Global.AppDataInstancesFilePath, json);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[INSTANCE MANAGER] Failed to create and write instances file: {ex}");
                }
            }

        }

        public List<Setting> GetInstanceSettings(InstanceModel instance)
        {
            string path = Path.Combine(instance.GetPath(), "server.properties");
            List<Setting> settings = new();
            if (File.Exists(path))
            {
                try
                {
                    var fileRead = File.ReadLines(path);
                    Debug.WriteLine(fileRead);
                    foreach (var line in fileRead)
                    {
                        if (!line.StartsWith('#') && !string.IsNullOrEmpty(line))
                        {
                            var setting = line.Split('=');
                            settings.Add(new Setting(name: setting[0], value: setting[1]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    UILogger.LogUI($"[INSTANCE MANAGER] Error getting {instance.Name} settings: {ex.Message}");
                }

            }
            return settings;
        }

        public async Task SaveInstanceSettings(List<Setting> settings, InstanceModel instance, string Arguments)
        {
            string path = Path.Combine(instance.GetPath(), "server.properties");
            //SAVE PROPERTIES
            try
            {
                await File.WriteAllTextAsync(path, "#Minecraft Server Properties\n#Generated By MCOCSrv\n#Last Edited On: " + DateTime.Now.ToShortDateString() + "\n");
                foreach (var setting in settings)
                {
                    await File.AppendAllTextAsync(path, $"{setting.Name}={setting.Value}");
                }
            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[INSTANCE MANAGER] Error saving {instance.Name} settings: {ex.Message}");
            }
            //SAVE ARGUMENTS
            instance.LaunchArguments = Arguments;
            await SaveInstance(instance);

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
    }
}

