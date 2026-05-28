using System.Collections.Generic;

namespace AkpEngine.Platforms.Roblox
{
    /// <summary>
    /// Configurações de exportação para Roblox.
    /// </summary>
    public class RobloxExportSettings : ExportSettings
    {
        public string GameTitle { get; set; }
        public string PlaceId { get; set; }
        public bool GenerateAssetManifest { get; set; } = true;
        public bool IncludeVirtualControls { get; set; } = true;
        public RobloxTargetPlatforms Platforms { get; set; }
    }

    [System.Flags]
    public enum RobloxTargetPlatforms
    {
        PC      = 1,
        Console = 2,
        Mobile  = 4,
        Tablet  = 8,
        All     = PC | Console | Mobile | Tablet
    }

    public class ExportResult
    {
        public bool Success { get; set; }
        public string OutputPath { get; set; }
        public string AssetManifestPath { get; set; }
        public List<string> Warnings { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ExportProgress
    {
        public int Percent { get; set; }
        public string Step { get; set; }
    }
}
