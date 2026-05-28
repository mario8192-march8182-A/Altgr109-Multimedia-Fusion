using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AkpEngine.Platforms.Roblox
{
    /// <summary>
    /// Gera o AssetManifest.json mapeando assets → rbxassetid.
    /// </summary>
    public class AssetManifestWriter
    {
        public void WriteManifest(AkpProject project, string manifestPath, List<string> warnings)
        {
            var manifest = new Dictionary<string, string>();

            foreach (var asset in project.Assets)
            {
                if (string.IsNullOrEmpty(asset.RobloxId))
                {
                    manifest[asset.Name] = "rbxassetid://0";
                    warnings.Add($"Asset {asset.Name} sem ID Roblox.");
                }
                else
                {
                    manifest[asset.Name] = $"rbxassetid://{asset.RobloxId}";
                }
            }

            File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
