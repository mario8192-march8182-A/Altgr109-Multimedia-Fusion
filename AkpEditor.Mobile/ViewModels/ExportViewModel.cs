using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AkpEditor.Mobile.Services;
using AkpEditor.Mobile.Services.Export;

namespace AkpEditor.Mobile.ViewModels;

public partial class ExportViewModel : ObservableObject
{
    private readonly ExportManagerService _exportManager;

    [ObservableProperty]
    private ObservableCollection<ExportPlatformViewModel> availablePlatforms = new();

    [ObservableProperty]
    private ObservableCollection<ExportPlatformViewModel> selectedPlatforms = new();

    [ObservableProperty]
    private bool isExporting = false;

    [ObservableProperty]
    private int exportProgress = 0;

    [ObservableProperty]
    private string currentExportStep = string.Empty;

    [ObservableProperty]
    private string exportMessage = string.Empty;

    public ExportViewModel(ExportManagerService exportManager)
    {
        _exportManager = exportManager;
        LoadAvailablePlatforms();
    }

    private void LoadAvailablePlatforms()
    {
        var platforms = _exportManager.GetAvailablePlatforms();
        AvailablePlatforms = new ObservableCollection<ExportPlatformViewModel>(
            platforms.Select(p => new ExportPlatformViewModel
            {
                PlatformName = p.PlatformName,
                FileExtension = p.FileExtension,
                IsAvailable = p.IsAvailable
            })
        );
    }

    [RelayCommand]
    public void SelectPlatforms(IList<object>? selectedItems)
    {
        SelectedPlatforms.Clear();
        if (selectedItems != null)
        {
            foreach (var item in selectedItems.Cast<ExportPlatformViewModel>())
            {
                SelectedPlatforms.Add(item);
            }
        }
    }

    [RelayCommand]
    public async Task Export()
    {
        if (SelectedPlatforms.Count == 0)
        {
            ExportMessage = "Please select at least one platform";
            return;
        }

        IsExporting = true;
        ExportMessage = "Starting export...";
        ExportProgress = 0;

        try
        {
            var outputPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Documents),
                "AkpEngine",
                "Exports",
                DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
            );
            Directory.CreateDirectory(outputPath);

            var platformNames = SelectedPlatforms.Select(p => p.PlatformName).ToList();
            var progress = new Progress<ExportProgress>(p =>
            {
                ExportProgress = p.PercentComplete;
                CurrentExportStep = p.CurrentStep;
                ExportMessage = p.DetailMessage;
            });

            var results = await _exportManager.ExportMultiplePlatformsAsync(platformNames, outputPath, progress);

            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);

            ExportMessage = successCount > 0
                ? $"✅ {successCount} platform(s) exported successfully!\n📁 Output: {outputPath}"
                : $"❌ Export failed for {failureCount} platform(s)";

            if (results.Any(r => r.Errors.Count > 0))
            {
                var errors = string.Join("\n", results.SelectMany(r => r.Errors));
                await Application.Current?.MainPage?.DisplayAlert("Export Warnings", errors, "OK");
            }
        }
        catch (Exception ex)
        {
            ExportMessage = $"❌ Export error: {ex.Message}";
            await Application.Current?.MainPage?.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsExporting = false;
        }
    }
}

public class ExportPlatformViewModel : ObservableObject
{
    public string PlatformName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}