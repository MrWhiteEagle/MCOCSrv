using MCOCSrv.Resources.Models;
using MCOCSrv.Resources.Raw;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace MCOCSrv.Resources.Classes
{

    public class ServerVersionFetcher
    {
        private Dictionary<string, string> Vanilla = new();
        private Dictionary<string, string> Forge = new();
        private Dictionary<string, string> NeoForge = new();
        private Dictionary<string, string> Fabric = new();
        private Dictionary<string, string> Paper = new();
        private Dictionary<string, string> Purpur = new();
        private Dictionary<string, string> Sponge = new();
        private HttpClient client;

        public ServerVersionFetcher()
        {
            this.client = new HttpClient();
        }

        public async Task DownloadInstance(InstanceModel instance)
        {
            Toaster.ToastifyLong($"Starting Download of: \n{instance.Type}-{instance.TypeVersion}\n Please wait...");
            UILogger.LogUI($"[SERVER FETCHER] Start Download: Type - {instance.Type}, Version - {instance.TypeVersion} to: {instance.GetPath()}...");
            try
            {
                var download = await client.GetByteArrayAsync(GetUrl(instance.Type, instance.TypeVersion));

                await File.WriteAllBytesAsync(Path.Combine(instance.GetPath(), $"{instance.Name}-{instance.TypeVersion}.jar"), download);

                UILogger.LogUI($"[SERVER FETCHER] DONWLOAD OK - PATH: {instance.GetPath()}");
                Toaster.ToastifyLong($"Download for: \n{instance.Type}-{instance.TypeVersion}\n FINISHED!");

            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[SERVER FETCHER] Failed download: {ex.Message}");
                Toaster.ToastifyLong($"Download of: \n{instance.Type}-{instance.TypeVersion} failed. \nException: {ex.Message}");
            }

        }
        private string GetUrl(InstanceType type, string version) => type switch
        {
            InstanceType.Vanilla => Vanilla[version],
            InstanceType.Forge => Forge[version],
            InstanceType.NeoForge => NeoForge[version],
            InstanceType.Fabric => Fabric[version],
            InstanceType.Paper => Paper[version],
            InstanceType.Purpur => Purpur[version],
            InstanceType.Sponge => Sponge[version]
        };
        public List<string> getVersions(InstanceType type)
        {
            switch (type)
            {
                case InstanceType.Vanilla:
                    return Vanilla.Keys.ToList();
                case InstanceType.Forge:
                    return Forge.Keys.ToList();
                case InstanceType.NeoForge:
                    return NeoForge.Keys.ToList();
                case InstanceType.Fabric:
                    return Fabric.Keys.ToList();
                case InstanceType.Paper:
                    return Paper.Keys.ToList();
                case InstanceType.Purpur:
                    return Purpur.Keys.ToList();
                case InstanceType.Sponge:
                    return Sponge.Keys.ToList();
                default:
                    throw new ArgumentException("Invalid InstanceType", nameof(type));

            }
        }

        private bool FileCheck()
        {
            if (File.Exists(Path.Combine(Global.AppDataSourcesPath, "serversource-vanilla.json")) &&
                File.Exists(Path.Combine(Global.AppDataSourcesPath, "serversource-forge.json")) &&
                File.Exists(Path.Combine(Global.AppDataSourcesPath, "serversource-neoforge.json")) &&
                File.Exists(Path.Combine(Global.AppDataSourcesPath, "serversource-fabric.json")) &&
                File.Exists(Path.Combine(Global.AppDataSourcesPath, "serversource-paper.json")) &&
                File.Exists(Path.Combine(Global.AppDataSourcesPath, "serversource-purpur.json")) &&
                File.Exists(Path.Combine(Global.AppDataSourcesPath, "serversource-sponge.json"))
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task initializeSources()
        {
            if (FileCheck())
            {
                string vanillaJson = await File.ReadAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-vanilla.json"));
                string forgeJson = await File.ReadAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-forge.json"));
                string neoforgeJson = await File.ReadAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-neoforge.json"));
                string fabricJson = await File.ReadAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-fabric.json"));
                string paperJson = await File.ReadAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-paper.json"));
                string purpurJson = await File.ReadAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-purpur.json"));
                string spongeJson = await File.ReadAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-sponge.json"));

                Vanilla = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonDocument.Parse(vanillaJson));
                Forge = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonDocument.Parse(forgeJson));
                NeoForge = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonDocument.Parse(neoforgeJson));
                Fabric = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonDocument.Parse(fabricJson));
                Paper = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonDocument.Parse(paperJson));
                Purpur = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonDocument.Parse(purpurJson));
                Sponge = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonDocument.Parse(spongeJson));

            }
            else
            {
                UILogger.LogUI("[SERVER FETCHER] Source files not generated or deleted - starting fetch");
                UILogger.LogUI("[SERVER FETCHER] FETCH - START");
                Toaster.ToastifyLong($"Source URL files are missing, redownloading....");
                try
                {
                    await FetchVanilla(client);
                    await FetchForge();
                    await FetchNeoForge(client);
                    await FetchFabric(client);
                    await FetchPaper(client);
                    await FetchPurpur(client);
                    await FetchSponge(client);
                    await SaveSources();
                    UILogger.LogUI("[SERVER FETCHER] FETCH - FINISH");
                }
                catch (Exception ex)
                {
                    UILogger.LogUI($"[SERVER FETCHER] FETCH - FAIL: {ex.Message}");
                }
            }

        }
        private async Task SaveSources()
        {
            JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };
            try
            {
                string VanillaJson = JsonSerializer.Serialize(this.Vanilla, options);
                string ForgeJson = JsonSerializer.Serialize(this.Forge, options);
                string NeoForgeJson = JsonSerializer.Serialize(this.NeoForge, options);
                string FabricJson = JsonSerializer.Serialize(this.Fabric, options);
                string PaperJson = JsonSerializer.Serialize(this.Paper, options);
                string PurpurJson = JsonSerializer.Serialize(this.Purpur, options);
                string SpongeJson = JsonSerializer.Serialize(this.Sponge, options);

                try
                {
                    await File.WriteAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-vanilla.json"), VanillaJson);
                    await File.WriteAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-forge.json"), ForgeJson);
                    await File.WriteAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-neoforge.json"), NeoForgeJson);
                    await File.WriteAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-fabric.json"), FabricJson);
                    await File.WriteAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-paper.json"), PaperJson);
                    await File.WriteAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-purpur.json"), PurpurJson);
                    await File.WriteAllTextAsync(Path.Combine(Global.AppDataSourcesPath, "serversource-sponge.json"), SpongeJson);
                }
                catch (Exception ex)
                {
                    UILogger.LogUI($"[SERVER FETCHER] Failed to write source file: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[SERVER FETCHER] Failed to serialize dictionary: {ex.Message}");
            }
        }

        private async Task FetchVanilla(HttpClient client)
        {
            string ManifestUrl = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json";
            string ManifestJson = await client.GetStringAsync(ManifestUrl);
            using JsonDocument document = JsonDocument.Parse(ManifestJson);
            var versions = document.RootElement.GetProperty("versions");

            foreach (var version in versions.EnumerateArray())
            {
                string id = version.GetProperty("id").GetString();
                string versionUrl = version.GetProperty("url").GetString();

                try
                {
                    var versionJson = await client.GetStringAsync(versionUrl);
                    using JsonDocument versionDocument = JsonDocument.Parse(versionJson);
                    if (versionDocument.RootElement.TryGetProperty("downloads", out JsonElement downloads) &&
                        downloads.TryGetProperty("server", out JsonElement server) &&
                        server.TryGetProperty("url", out JsonElement serverUrl))
                    {
                        if (versionDocument.RootElement.TryGetProperty("type", out JsonElement type) && !(type.GetString() == "snapshot"))
                        {
                            //id = $"{id}--snapshot";
                            UILogger.LogUI($"[VANILLA FETCH] Found: {id}");
                            Vanilla[id] = serverUrl.GetString();
                        }
                        //UILogger.LogUI($"[VANILLA FETCH] Found: {id}");
                        //Vanilla[id] = serverUrl.GetString();
                    }
                    else
                    {
                        UILogger.LogUI($"[VANILLA FETCH] No server for {id}.");
                    }

                }
                catch (Exception ex)
                {
                    UILogger.LogUI($"[VANILLA FETCH] Error fetching data for {id}.");
                }
            }
            UILogger.LogUI("[VANILLA FETCH] DONE");
        }

        private async Task FetchForge()
        {
            string SourcePath = Path.Combine(Global.AppDataSourcesPath, "forge-versions.json");
            if (File.Exists(SourcePath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(SourcePath);
                    Forge = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (Forge == null || Forge.Count == 0)
                    {
                        UILogger.LogUI("[FORGE FETCH] Manifest was read, but is empty/null!");
                    }
                    else
                    {
                        UILogger.LogUI($"[FORGE FETCH] Succesfully got manifest! Versions: {Forge.Count}");
                    }

                }
                catch (Exception ex)
                {
                    UILogger.LogUI($"[FORGE FETCH] Couldn't read manifest source file! {ex.Message}");
                }
            }
            else
            {
                UILogger.LogUI($"[FORGE FETCH] Manifest file doesn't exist!");
            }
            UILogger.LogUI("[FORGE FETCH] DONE");
        }

        private async Task FetchNeoForge(HttpClient client)
        {
            string mavenManifest = "https://maven.neoforged.net/releases/net/neoforged/neoforge/maven-metadata.xml";
            var versions = new List<string>();

            try
            {
                string xmlData = await client.GetStringAsync(mavenManifest);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlData);
                var nodes = xmlDoc.SelectNodes("//metadata/versioning/versions/version");
                foreach (XmlNode node in nodes)
                {
                    if (!node.InnerText.Contains("beta"))
                    {
                        UILogger.LogUI($"[NEOFORGE FETCH] Found stable: {node.InnerText}");
                        versions.Add(node.InnerText.Trim());
                    }
                }
                UILogger.LogUI($"[NEOFORGE FETCH] Found {versions.Count} total versions from manifest!");
                if (versions.Count == 0)
                {
                    UILogger.LogUI("[NEOFORGE FETCH] Found no versions - skipping fetch...");
                    return;
                }
                versions.Sort((a, b) => Version.Parse(a).CompareTo(Version.Parse(b)));
                foreach (var version in versions)
                {
                    string url = $"https://maven.neoforged.net/releases/net/neoforged/neoforge/{version}/neoforge-{version}-installer.jar";
                    try
                    {
                        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                        if (response.IsSuccessStatusCode)
                        {
                            NeoForge["1." + version] = url;
                        }
                        else
                        {
                            UILogger.LogUI($"[NEOFORGE FETCH] File for {version} doesn't exist!");
                        }
                    }
                    catch (Exception ex)
                    {
                        UILogger.LogUI($"[NEOFORGE FETCH] Error fetching file for {version}: {ex.Message}");
                    }

                }
            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[NEOFORGE FETCH] Error fetching versions from Maven! {ex.Message}");
            }
            UILogger.LogUI("[NEOFORGE FETCH] DONE");
        }

        private async Task FetchFabric(HttpClient client)
        {
            string versionsUrl = "https://meta.fabricmc.net/v2/versions/game";
            var versionList = new List<string>();
            try
            {
                string jsonVersions = await client.GetStringAsync(versionsUrl);
                using JsonDocument versionsDocument = JsonDocument.Parse(jsonVersions);
                foreach (var element in versionsDocument.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("stable", out JsonElement stable) && element.TryGetProperty("version", out JsonElement version))
                    {
                        if (stable.GetBoolean() && version.GetString() != null)
                        {
                            UILogger.LogUI($"[FABRIC FETCH] Found: {version.GetString()}");
                            versionList.Add(version.GetString());
                        }
                        else
                        {
                            UILogger.LogUI($"[FABRIC FETCH] Version {version.GetString()} is not stable! - skipping");
                        }

                    }
                    else
                    {
                        UILogger.LogUI($"[FABRIC FETCH] Error getting element from manifest: {element.ToString()}"); continue;
                    }
                }

            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[FABRIC FETCH] Could not get version manifest from fabricmc.net: {ex.Message}");
            }

            foreach (var version in versionList)
            {
                string url = $"https://meta.fabricmc.net/v2/versions/loader/{version}/0.16.14/1.0.3/server/jar";
                Fabric[version] = url;

            }
            UILogger.LogUI("[FABRIC FETCH] DONE");
            UILogger.LogUI($"[FABRIC FETCH] Got {Fabric.Count} total versions!");
        }

        private async Task FetchPaper(HttpClient client)
        {
            string versionManifestUrl = "https://api.papermc.io/v2/projects/paper";
            var versionList = new List<string>();
            try
            {
                string versionJson = await client.GetStringAsync(versionManifestUrl);
                using JsonDocument versionDocument = JsonDocument.Parse(versionJson);
                if (versionDocument.RootElement.TryGetProperty("versions", out JsonElement versionsElement))
                {
                    foreach (var version in versionsElement.EnumerateArray())
                    {
                        UILogger.LogUI($"[PAPER FETCH] Found: {version.GetString()}");
                        versionList.Add(version.GetString());
                    }
                    UILogger.LogUI($"[PAPER FETCH] Got {versionList.Count} total versions!");
                }
            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[PAPER FETCH] Error getting version manifest! {ex.Message}");
            }

            foreach (var version in versionList)
            {
                try
                {
                    string buildsManifest = await client.GetStringAsync(versionManifestUrl + $"/versions/{version}/builds");
                    using JsonDocument buildsDocument = JsonDocument.Parse(buildsManifest);
                    if (buildsDocument.RootElement.TryGetProperty("builds", out JsonElement builds))
                    {
                        var buildArray = builds.EnumerateArray().ToList();

                        for (int i = buildArray.Count - 1; i >= 0; i--)
                        {
                            var build = buildArray[i];
                            string channel = build.GetProperty("channel").GetString();
                            if (channel == "default")
                            {
                                int buildNumber = build.GetProperty("build").GetInt32();
                                Paper[version] = $"https://api.papermc.io/v2/projects/paper/versions/{version}/builds/{buildNumber}/downloads/paper-{version}-{buildNumber}.jar";
                                break;
                            }
                        }
                    }
                    else
                    {
                        UILogger.LogUI($"[PAPER FETCH] Got no builds for {version} - skipping.");
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    UILogger.LogUI($"[PAPER FETCH] Error getting data for version {version}: {ex.Message}");
                }
            }
            UILogger.LogUI("[PAPER FETCH] DONE");
            UILogger.LogUI($"[PAPER FETCH] Found {Paper.Count} total server files!");
        }

        private async Task FetchPurpur(HttpClient client)
        {
            string versionManifestUrl = "https://api.purpurmc.org/v2/purpur";
            var versionList = new List<string>();

            try
            {
                string versionJson = await client.GetStringAsync(versionManifestUrl);
                using JsonDocument versionDocument = JsonDocument.Parse(versionJson);
                if (versionDocument.RootElement.TryGetProperty("versions", out JsonElement versions))
                {
                    foreach (var version in versions.EnumerateArray())
                    {
                        UILogger.LogUI($"[PURPUR FETCH] Found: {version}.");
                        versionList.Add(version.GetString());
                    }
                }
            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[PURPUR FETCH] Error getting version manifest: {ex.Message}");
            }

            foreach (var version in versionList)
            {
                try
                {
                    string buildJson = await client.GetStringAsync($"https://api.purpurmc.org/v2/purpur/{version}");
                    using JsonDocument buildDoc = JsonDocument.Parse(buildJson);

                    string build = buildDoc.RootElement.GetProperty("builds").GetProperty("latest").GetString();
                    if (!string.IsNullOrEmpty(build))
                    {
                        string url = $"https://api.purpurmc.org/v2/purpur/{version}/{build}/download";
                        Purpur[version] = url;
                    }
                    else
                    {
                        UILogger.LogUI($"[PURPUR FETCH] latest build for {version} returned null! - skipping");
                    }
                }
                catch (Exception ex)
                {
                    UILogger.LogUI($"[PURPUR FETCH] Error getting build for {version}: {ex.Message}");
                }
            }
            UILogger.LogUI("[PURPUR FETCH] DONE");
        }

        private async Task FetchSponge(HttpClient client)
        {
            var SpongeVanillaVersionList = new List<string>();
            var SpongeForgeVersionList = new List<string>();
            var SpongeNeoVersionList = new List<string>();
            try
            {
                string SVXml = await client.GetStringAsync("https://repo.spongepowered.org/repository/maven-releases/org/spongepowered/spongevanilla/maven-metadata.xml");
                string SFXml = await client.GetStringAsync("https://repo.spongepowered.org/repository/maven-releases/org/spongepowered/spongeforge/maven-metadata.xml");
                string SNXml = await client.GetStringAsync("https://repo.spongepowered.org/repository/maven-releases/org/spongepowered/spongeneo/maven-metadata.xml");

                XDocument SVDoc = XDocument.Parse(SVXml);
                XDocument SFDoc = XDocument.Parse(SFXml);
                XDocument SNDoc = XDocument.Parse(SNXml);

                SpongeVanillaVersionList = SVDoc.Descendants("version").Select(x => x.Value).Where(IsSpongeStable).OrderBy(v => v).ToList();
                SpongeForgeVersionList = SFDoc.Descendants("version").Select(x => x.Value).Where(IsSpongeStable).OrderBy(v => v).ToList();
                SpongeNeoVersionList = SNDoc.Descendants("version").Select(x => x.Value).Where(IsSpongeStable).OrderBy(v => v).ToList();

                foreach (string version in SpongeVanillaVersionList)
                {
                    UILogger.LogUI($"[SPONGE FETCH]Found: SpongeVanilla: {version}");
                    string url = $"https://repo.spongepowered.org/repository/maven-releases/org/spongepowered/spongevanilla/{version}/spongevanilla-{version}-universal.jar";
                    Sponge[$"SpongeVanilla-{version}"] = url;
                }

                foreach (string version in SpongeForgeVersionList)
                {
                    UILogger.LogUI($"[SPONGE FETCH]Found: SpongeForge: {version}");
                    string url = $"https://repo.spongepowered.org/repository/maven-releases/org/spongepowered/spongeforge/{version}/spongeforge-{version}-universal.jar";
                    Sponge[$"SpongeForge-{version}"] = url;
                }

                foreach (string version in SpongeNeoVersionList)
                {
                    UILogger.LogUI($"[SPONGE FETCH]Found: SpongeNeo: {version}");
                    string url = $"https://repo.spongepowered.org/repository/maven-releases/org/spongepowered/spongeneo/{version}/spongeneo-{version}-universal.jar";
                    Sponge[$"SpongeNeo-{version}"] = url;
                }
            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[SPONGE FETCH] Failed to resolve sponge's metadata files! {ex.Message}");
            }

            bool IsSpongeStable(string version)
            {
                return !System.Text.RegularExpressions.Regex.IsMatch(version, @"[a-zA-Z]");
            }
            UILogger.LogUI("[SPONGE FETCH] DONE");
        }

    }
}
