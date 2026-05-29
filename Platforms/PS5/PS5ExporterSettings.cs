using System.Collections.Generic;

namespace AkpEngine.Platforms.PS5
{
    /// <summary>
    /// Configurações de exportação para PS5.
    /// </summary>
    public class PS5ExportSettings : ExportSettings
    {
        public string TitleId { get; set; }
        public string ContentId { get; set; }
        public string MasterVersion { get; set; }
        public string SdkVersion { get; set; }
        public List<string> SupportedLanguages { get; set; }
        public bool EnableHDR10 { get; set; }
        public bool EnableTempest3DAudio { get; set; }
        public bool EnableAdaptiveTriggers { get; set; }
        public bool IncludeTrophies { get; set; }
        public PS5Resolution TargetResolution { get; set; }
        public DualSenseMapping ControllerMapping { get; set; }
    }

    public enum PS5Resolution { HD_1080p, UHD_4K, Dynamic }

    public class ExportResult
    {
        public bool Success { get; set; }
        public string OutputPath { get; set; }
        public string AssetManifestPath { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> SdkStubsRequired { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ExportProgress
    {
        public int Percent { get; set; }
        public string Step { get; set; }
    }
}
