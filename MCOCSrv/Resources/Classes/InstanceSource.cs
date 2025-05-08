using System.Text.Json;

namespace MCOCSrv.Resources.Raw
{
    public class ServerVersionFetcher
    {
        private Dictionary<string, string> Vanilla = new Dictionary<string, string>();
        private Dictionary<string, string> Forge = new Dictionary<string, string>();
        private Dictionary<string, string> NeoForge = new Dictionary<string, string>();
        private Dictionary<string, string> Fabric = new Dictionary<string, string>();
        private Dictionary<string, string> quilt = new Dictionary<string, string>();
        private Dictionary<string, string> paper = new Dictionary<string, string>();
        private Dictionary<string, string> purpur = new Dictionary<string, string>();
        private Dictionary<string, string> sponge = new Dictionary<string, string>();

        private async Task FetchVanilla()
        {
            var httpClient = new HttpClient();
            string ManifestUrl = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json";
            string ManifestJson = await httpClient.GetStringAsync(ManifestUrl);
            using JsonDocument document = JsonDocument.Parse(ManifestJson);
            var versions = document.RootElement.GetProperty("versions");

            foreach (var version in versions.EnumerateArray())
            {
                string id = version.GetProperty("id").GetString();
                string versionUrl = version.GetProperty("url").GetString();

                try
                {
                    var versionJson = await httpClient.GetStringAsync(versionUrl);
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
    }
}
