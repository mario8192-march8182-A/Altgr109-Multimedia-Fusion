using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AkpEditor.Mobile.Services;

namespace AkpEditor.Mobile.ViewModels;

public partial class ProjectSettingsViewModel : ObservableObject
{
    private readonly ProjectService _projectService;

    [ObservableProperty]
    private string projectName = "New Project";

    [ObservableProperty]
    private string screenWidth = "800";

    [ObservableProperty]
    private string screenHeight = "600";

    [ObservableProperty]
    private List<int> fpsOptions = new() { 30, 60, 120 };

    [ObservableProperty]
    private int selectedFps = 60;

    public ProjectSettingsViewModel(ProjectService projectService)
    {
        _projectService = projectService;
        LoadSettings();
    }

    [RelayCommand]
    public async Task LoadSettings()
    {
        try
        {
            var settings = await _projectService.GetSettingsAsync();
            ProjectName = settings.ProjectName;
            ScreenWidth = settings.ScreenWidth.ToString();
            ScreenHeight = settings.ScreenHeight.ToString();
            SelectedFps = settings.TargetFps;
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task SaveSettings()
    {
        try
        {
            await _projectService.SaveSettingsAsync(new()
            {
                ProjectName = ProjectName,
                ScreenWidth = int.Parse(ScreenWidth),
                ScreenHeight = int.Parse(ScreenHeight),
                TargetFps = SelectedFps
            });
            await Application.Current?.MainPage?.DisplayAlert("Success", "Settings saved", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task ExportProject()
    {
        try
        {
            await _projectService.ExportProjectAsync();
            await Application.Current?.MainPage?.DisplayAlert("Success", "Project exported", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}