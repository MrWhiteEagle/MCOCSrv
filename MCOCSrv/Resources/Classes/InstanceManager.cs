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
            this.fetcher = fetcher;
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
                if (instance.CustomPath != null)
                {
                    Directory.CreateDirectory(instance.CustomPath);
                    await File.WriteAllTextAsync(Path.Combine(instance.CustomPath, $"{instance.id}.json"), json);
                }
                else
                {
                    Directory.CreateDirectory(instance.BasePath);
                    await File.WriteAllTextAsync(Path.Combine(instance.BasePath, $"{instance.id}.json"), json);
                }
                await fetcher.DownloadInstance(instance);
                await AppendInstanceListFile(instance);
                instance.InitializeConsole();
                instances.Add(instance);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[INSTANCE MANAGER] Error writing instance File: {ex.Message}");
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

