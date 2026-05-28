using System.Collections.Generic;

namespace AkpEngine.Platforms.macOS
{
    /// <summary>
    /// Configurações para exportação macOS.
    /// </summary>
    public class ExportSettings
    {
        public string OutputPath { get; set; }
        public string BundleIdentifier { get; set; }
        public string Version { get; set; }
        public MacOSArchitecture Architecture { get; set; }
        public string IconPath { get; set; }
    }

    public enum MacOSArchitecture { x64, Arm64, Universal }

    public class ExportResult
    {
        public bool Success { get; set; }
        public string OutputPath { get; set; }
        public List<string> Warnings { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ExportProgress
    {
        public int Percent { get; set; }
        public string Step { get; set; }
    }
}
