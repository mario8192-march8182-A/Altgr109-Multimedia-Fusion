using AkpEditor.Mobile.Models;

namespace AkpEditor.Mobile.Services;

public class ProjectService
{
    private ProjectSettings? _currentSettings;

    public async Task<ProjectSettings> GetSettingsAsync()
    {
        _currentSettings ??= new ProjectSettings
        {
            ProjectName = "New Project",
            ScreenWidth = 800,
            ScreenHeight = 600,
            TargetFps = 60
        };
        return await Task.FromResult(_currentSettings);
    }

    public async Task SaveSettingsAsync(ProjectSettings settings)
    {
        _currentSettings = settings;
        // TODO: Persist to file or cloud storage
        await Task.CompletedTask;
    }

    public async Task<dynamic?> GetCurrentProjectAsync()
    {
        // TODO: Implement project loading
        return await Task.FromResult<dynamic?>(null);
    }

    public async Task SaveProjectAsync(dynamic project)
    {
        // TODO: Implement project saving
        await Task.CompletedTask;
    }

    public async Task ExportProjectAsync()
    {
        // TODO: Implement project export (APK, IPA, etc.)
        await Task.CompletedTask;
    }
}