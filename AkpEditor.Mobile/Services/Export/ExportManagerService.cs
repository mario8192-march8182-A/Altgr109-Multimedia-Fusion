using AkpEditor.Mobile.Services.Export;
using AkpEditor.Mobile.Services.Export.Platforms;

namespace AkpEditor.Mobile.Services;

public class ExportManagerService
{
    private readonly List<IExportPlatform> _exporters;
    private readonly ProjectService _projectService;

    public ExportManagerService(ProjectService projectService)
    {
        _projectService = projectService;
        _exporters = new List<IExportPlatform>
        {
            new AndroidExporter(),
            new iOSExporter(),
            new PCWindowsExporter(),
            new MacOSExporter(),
            new HTML5Exporter()
        };
    }

    public List<IExportPlatform> GetAvailablePlatforms()
    {
        return _exporters.Where(e => e.IsAvailable).ToList();
    }

    public IExportPlatform? GetExporter(string platformName)
    {
        return _exporters.FirstOrDefault(e => e.PlatformName.Equals(platformName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<ExportResult> ExportAsync(
        string platformName,
        string outputPath,
        IProgress<ExportProgress>? progress = null)
    {
        var exporter = GetExporter(platformName);
        if (exporter == null)
            return new ExportResult
            {
                Success = false,
                Errors = { $"Platform '{platformName}' not found" }
            };

        if (!exporter.IsAvailable)
            return new ExportResult
            {
                Success = false,
                Errors = { $"Platform '{platformName}' is not available on this system" }
            };

        var settings = await _projectService.GetSettingsAsync();
        var options = new ExportOptions
        {
            ProjectName = settings.ProjectName,
            ProjectPath = settings.ProjectPath ?? Directory.GetCurrentDirectory(),
            OutputPath = outputPath,
            ScreenWidth = settings.ScreenWidth,
            ScreenHeight = settings.ScreenHeight,
            TargetFps = settings.TargetFps
        };

        return await exporter.ExportAsync(options, progress);
    }

    public async Task<List<ExportResult>> ExportMultiplePlatformsAsync(
        List<string> platformNames,
        string outputPath,
        IProgress<ExportProgress>? progress = null)
    {
        var results = new List<ExportResult>();

        foreach (var platformName in platformNames)
        {
            var result = await ExportAsync(platformName, outputPath, progress);
            results.Add(result);
        }

        return results;
    }
}