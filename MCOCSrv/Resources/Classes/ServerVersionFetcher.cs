using System.Text.Json;
using System.Xml;

namespace MCOCSrv.Resources.Classes
{

    public class ServerVersionFetcher
    {
        private Dictionary<string, string> Vanilla = new Dictionary<string, string>();
        private Dictionary<string, string> Forge = new Dictionary<string, string>();
        private Dictionary<string, string> NeoForge = new Dictionary<string, string>();
        private Dictionary<string, string> Fabric = new Dictionary<string, string>();
        private Dictionary<string, string> Paper = new Dictionary<string, string>();
        private Dictionary<string, string> Purpur = new Dictionary<string, string>();
        private Dictionary<string, string> Pponge = new Dictionary<string, string>();
        private HttpClient client;

        public ServerVersionFetcher()
        {
            this.client = new HttpClient();
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
                        if (versionDocument.RootElement.TryGetProperty("type", out JsonElement type) && type.GetString() == "snapshot")
                        {
                            id = $"{id}--snapshot";
                        }
                        Console.WriteLine($"[VANILLA FETCH] Found server file for {id}!");
                        Vanilla[id] = serverUrl.GetString();
                    }
                    else
                    {
                        Console.WriteLine($"[VANILLA FETCH] No server for {id}.");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VANILLA FETCH] Error fetching data for {id}.");
                }
            }


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
                        Console.WriteLine("[FORGE FETCH] Manifest was read, but is empty/null!");
                    }
                    else
                    {
                        Console.WriteLine($"[FORGE FETCH] Succesfully got manifest! Versions: {Forge.Count}");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FORGE FETCH] Couldn't read manifest source file! {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"[FORGE FETCH] Manifest file doesn't exist!");
            }
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
                        versions.Add(node.InnerText.Trim());
                    }
                }
                Console.WriteLine($"[NEOFORGE FETCH] Found {versions.Count} total versions from manifest!");
                if (versions.Count == 0)
                {
                    Console.WriteLine("[NeoForge FETCH] Found no versions - skipping fetch...");
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
                            Console.WriteLine($"[NEOFORGE FETCH] File for {version} doesn't exist!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[NEOFORGE FETCH] Error fetching file for {version}: {ex.Message}");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NEOFORGE FETCH] Error fetching versions from Maven! {ex.Message}");
            }

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
                            versionList.Add(version.GetString());
                        }
                        else
                        {
                            Console.WriteLine($"[FABRIC FETCH] Version {version.GetString()} is not stable! - skipping");
                        }

                    }
                    else
                    {
                        Console.WriteLine($"[FABRIC FETCH] Error getting element from manifest: {element.ToString()}"); continue;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FABRIC FETCH] Could not get version manifest from fabricmc.net: {ex.Message}");
            }

            foreach (var version in versionList)
            {
                string url = $"https://meta.fabricmc.net/v2/versions/loader/{version}/0.16.14/1.0.3/server/jar";
                Fabric[version] = url;

            }

            Console.WriteLine($"[FABRIC FETCH] Got {Fabric.Count} total versions!");
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
                        versionList.Add(version.GetString());
                    }
                    Console.WriteLine($"[PAPER FETCH] Got {versionList.Count} total versions!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PAPER FETCH] Error getting version manifest! {ex.Message}");
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
                        Console.WriteLine($"[PAPER FETCH] Got no builds for {version} - skipping.");
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PAPER FETCH] Error getting data for version {version}: {ex.Message}");
                }
            }
            Console.WriteLine($"[PAPER FETCH] Found {Paper.Count} total server files!");
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
                        versionList.Add(version.GetString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PURPUR FETCH] Error getting version manifest: {ex.Message}");
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
                        Console.WriteLine($"[PURPUR FETCH] latest build for {version} returned null! - skipping");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PURPUR FETCH] Error getting build for {version}: {ex.Message}");
                }
            }
        }
    }
}
