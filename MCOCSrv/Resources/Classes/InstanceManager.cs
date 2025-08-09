using MCOCSrv.Resources.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MCOCSrv.Resources.Classes
{
    public class InstanceManager
    {
        //MAIN CLASS USED FOR HANDLING INSTANCE-RELATED OPERATIONS
        public ObservableCollection<InstanceModel> instances { get; private set; } = new();
        public ObservableCollection<InstanceModel> running { get; private set; } = new();
        private ServerVersionFetcher fetcher;

        public event EventHandler<InstanceModel>? InstanceSaved;
        public InstanceManager(ServerVersionFetcher fetcher)
        {
            this.fetcher = fetcher;
            instances = new();
            running = new();
        }

        //MAIN METHOD TO FETCH INSTALLED INSTANCES
        #region Fetch methods
        public async Task FetchInstances()
        {
            //CALL FILE CONTAINING KEY-VALUE PAIRS WITH INFO ABOUT INSTANCE LOCATIONS
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
                var list = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (list != null || list?.Count > 0)
                {
                    instances.Clear();
                    foreach (var instancePath in list)
                    {
                        try
                        {
                            //DESERIALIZE RECEIVED FILES CONTAINING INSTANCE DATA
                            string instancejson = await File.ReadAllTextAsync(Path.Combine(instancePath.Value, $"{instancePath.Key}.json"));
                            InstanceModel? instance = JsonSerializer.Deserialize<InstanceModel>(instancejson);
                            if (instance != null)
                            {
                                instance.InitializeConsole();
                                instance.Settings = GetInstanceSettings(instance);
                                instance.BannedPlayers = GetBannedPlayers(instance);
                                instance.OppedPlayers = GetOppedPlayers(instance);
                                instances.Add(instance);
                                UILogger.LogUI($"[INSTANCE MANAGER] {instance.Name} Found and Initiated");
                            }
                            else
                            {
                                UILogger.LogUI($"[INSTANCE MANAGER] Instance was null!");
                            }

                        }
                        catch (Exception ex)
                        {
                            UILogger.LogUI($"[INSTANCE MANAGER] Cannot read instance file for {instancePath.Key}: {ex.Message}");
                        }
                    }
                    instances.OrderBy(i => i.id).ToList();
                    UILogger.LogUI($"[INSTANCE MANAGER] Found {instances.Count} total.");
                }
            }
            UILogger.LogUI("[INSTANCE MANAGER] FETCH - DONE");
        }

        public ObservableCollection<PlayerData> GetBannedPlayers(InstanceModel instance)
        {
            string path = Path.Combine(instance.GetPath(), "banned-players.json");
            if (File.Exists(path))
            {
                try
                {
                    ObservableCollection<PlayerData> players = new();
                    string json = File.ReadAllText(path);
                    JsonDocument doc = JsonDocument.Parse(json);
                    foreach (JsonElement element in doc.RootElement.EnumerateArray())
                    {
                        players.Add(new PlayerData(element.GetProperty("uuid").ToString(), element.GetProperty("name").ToString()));
                    }
                    return players;
                }
                catch (Exception ex)
                {
                    UILogger.LogUI($"[INSTANCE MANAGER] Error getting banned players for {instance.Name}: {ex.Message}");
                }
            }
            return new ObservableCollection<PlayerData>();
        }

        public ObservableCollection<PlayerData> GetOppedPlayers(InstanceModel instance)
        {
            string path = Path.Combine(instance.GetPath(), "ops.json");
            if (File.Exists(path))
            {
                try
                {
                    ObservableCollection<PlayerData> players = new();
                    string json = File.ReadAllText(path);
                    JsonDocument doc = JsonDocument.Parse(json);
                    foreach (JsonElement element in doc.RootElement.EnumerateArray())
                    {
                        players.Add(new PlayerData(element.GetProperty("uuid").ToString(), element.GetProperty("name").ToString()));
                    }
                    return players;
                }
                catch (Exception ex)
                {
                    UILogger.LogUI($"[INSTANCE MANAGER] Error getting operators for {instance.Name}: {ex.Message}");
                }
            }
            return new ObservableCollection<PlayerData>();
        }
        #endregion
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
                //DELETE FILES>REMOVE FROM MEMORY>SERIALIZE NEW INSTANCE DATA TO MAIN FILE
                Directory.Delete(instance.GetPath(), true);
                //If instance running, stop it first
                if (instance.Console.IsRunning)
                {
                    instance.Console.StopServer();
                    instance.Console.Dispose();
                }
                instances.Remove(instance);
                running.Remove(instance);
                string json = JsonSerializer.Serialize(GetInstancesPathData(), new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(Global.AppDataInstancesFilePath, json);
                UILogger.LogUI($"[INSTANCE MANAGER] DELETE INSTANCE - DONE");

            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[INSTANCE MANAGER] Error deleting instance: {ex.Message}");
            }
        }

        //RUN DURING CREATION - CORRECT PROPERTY ASSIGNMENT
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
                //ADD TO CURRENT
                instance.InitializeConsole();
                instances.Add(instance);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[INSTANCE MANAGER] Error writing instance File: {ex.Message}");
            }
        }

        //SAVE ANY CHANGES TO INSTANCE PROPERTIES ITSELF AND CHANGE THE MAIN FILE (in case of path change)
        public async Task SaveInstance(InstanceModel instance)
        {
            string path = Path.Combine(instance.GetPath(), $"{instance.id}.json");
            string json = JsonSerializer.Serialize(instance, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            Directory.CreateDirectory(instance.GetPath());
            try
            {
                await File.WriteAllTextAsync(path, json);
                await AppendInstanceListFile(instance.id, instance);
            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[INSTANCE MANAGER] Error saving instance {instance.Name}: {ex.Message}");
            }

            InstanceSaved?.Invoke(this, instance);

        }

        //CHANGE MAIN FILE - contain info about instance paths
        private async Task AppendInstanceListFile(string id, InstanceModel toChange)
        {

            if (File.Exists(Global.AppDataInstancesFilePath))
            {
                string content = await File.ReadAllTextAsync(Global.AppDataInstancesFilePath);
                Dictionary<string, string> InstanceDataList = JsonSerializer.Deserialize<Dictionary<string, string>>(content) ?? new Dictionary<string, string>();
                try
                {
                    foreach (var data in InstanceDataList)
                    {
                        if (data.Key == toChange.id)
                        {
                            InstanceDataList.Remove(data.Key);
                        }
                    }
                    InstanceDataList.Add(toChange.id, toChange.GetPath());
                    string json = JsonSerializer.Serialize(InstanceDataList, new JsonSerializerOptions { WriteIndented = true });
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
                    Dictionary<string, string> InstanceDataList = new();
                    InstanceDataList.Add(toChange.id, toChange.GetPath());
                    string json = JsonSerializer.Serialize(InstanceDataList, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(Global.AppDataInstancesFilePath, json);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[INSTANCE MANAGER] Failed to create and write instances file: {ex}");
                }
            }

        }

        //RETURNS LIST OF SERVER.PROPERTIES FILE AS A LIST OF SETTING OBJECTS
        public List<Setting> GetInstanceSettings(InstanceModel instance)
        {
            string path = Path.Combine(instance.GetPath(), "server.properties");
            List<Setting> settings = new();
            if (File.Exists(path))
            {
                try
                {
                    var fileRead = File.ReadLines(path);
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

        //SAVES A LIST OF SETTING OBJECTS AS A SERVER.PROPERTIES FILE
        public async Task SaveInstanceSettings(List<Setting> settings, InstanceModel instance, string Arguments)
        {
            string path = Path.Combine(instance.GetPath(), "server.properties");
            //SAVE PROPERTIES
            try
            {
                List<string> finalSettings = new();
                foreach (var setting in settings)
                {
                    finalSettings.Add($"{setting.Name}={setting.Value}");
                }
                await File.WriteAllTextAsync(path, "#Minecraft Server Properties\n#Generated By MCOCSrv\n#Last Edited On: " + DateTime.Now.ToShortDateString() + "\n");
                await File.AppendAllLinesAsync(path, finalSettings);
            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[INSTANCE MANAGER] Error saving {instance.Name} settings: {ex.Message}");
            }
            //SAVE ARGUMENTS
            instance.LaunchArguments = Arguments;
            await SaveInstance(instance);

        }

        //MOVE INSTANCE FOLDER TO ANOTHER DIRECTORY
        public async Task MoveInstance(InstanceModel instance, string NewPath)
        {
            NewPath += $"\\{Path.GetFileName(instance.GetPath())}";
            try
            {
                await Task.Run(() =>
                {
                    Debug.WriteLine(NewPath);
                    Directory.Move(instance.GetPath(), NewPath);
                });
                if (NewPath == instance.BasePath)
                {
                    instance.CustomPath = null;
                }
                else
                {
                    instance.CustomPath = NewPath;
                }
                await SaveInstance(instance);

            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[INSTANCE MANAGER] Cannot move instance: {ex.Message}");
            }
        }

        //ID TO FILENAME (CHANGE SPACES TO UNDERSCORES)
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

        //SUPPORT METHOD RETURNING A DICTIONARY CONTAINING INSTANCE DATA KEY=ID VALUE=PATH FROM THE MAIN FILE
        private Dictionary<string, string> GetInstancesPathData()
        {
            Dictionary<string, string> result = new();
            foreach (var instance in instances)
            {
                result.Add(instance.Name, instance.GetPath());
            }
            return result;
        }



        public InstanceManager GetInstanceManager()
        {
            return this;
        }
    }
}

