namespace AkpEditor.Mobile.Services.Export;

public interface IExportPlatform
{
    string PlatformName { get; }
    string FileExtension { get; }
    bool IsAvailable { get; }
    Task<ExportResult> ExportAsync(ExportOptions options, IProgress<ExportProgress> progress);
    Task<bool> ValidateProjectAsync(ProjectData project);
}

public class ExportOptions
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public int ScreenWidth { get; set; } = 800;
    public int ScreenHeight { get; set; } = 600;
    public int TargetFps { get; set; } = 60;
    public bool IncludeDebugInfo { get; set; } = false;
    public bool OptimizeAssets { get; set; } = true;
    public Dictionary<string, string> PlatformSpecificOptions { get; set; } = new();
}

public class ExportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string OutputFile { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public TimeSpan ExportDuration { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class ExportProgress
{
    public int PercentComplete { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public string DetailMessage { get; set; } = string.Empty;
}

public class ProjectData
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public List<string> AssetPaths { get; set; } = new();
    public Dictionary<string, object> Settings { get; set; } = new();
}