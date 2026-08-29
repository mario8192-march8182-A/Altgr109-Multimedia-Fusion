using AkpEditor.Mobile.Models;
using AkpEditor.Mobile.Services;

namespace AkpEditor.Mobile.ViewModels;

public partial class AssetManagerViewModel : ObservableObject
{
    private readonly AssetDatabaseService _databaseService;
    private readonly AssetPreviewService _previewService;
    private readonly AssetValidationService _validationService;
    private string _currentCategoryId = "images";

    [ObservableProperty]
    private ObservableCollection<AssetItem> assets = new();

    [ObservableProperty]
    private ObservableCollection<AssetCategoryViewModel> categories = new();

    [ObservableProperty]
    private AssetItem? selectedAsset;

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public AssetManagerViewModel(AssetDatabaseService databaseService, AssetPreviewService previewService, AssetValidationService validationService)
    {
        _databaseService = databaseService;
        _previewService = previewService;
        _validationService = validationService;

        // Subscribe to asset changes
        _databaseService.AssetAdded += (s, e) => LoadAssetsCommand.ExecuteAsync(null);
        _databaseService.AssetRemoved += (s, e) => LoadAssetsCommand.ExecuteAsync(null);

        InitializeCategories();
        LoadAssetsCommand.ExecuteAsync(null);
    }

    private void InitializeCategories()
    {
        var categoriesList = _databaseService.GetCategories();
        Categories = new ObservableCollection<AssetCategoryViewModel>(
            categoriesList.Select(c => new AssetCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon
            })
        );
    }

    [RelayCommand]
    public async Task LoadAssets()
    {
        IsLoading = true;
        StatusMessage = "Loading assets...";

        try
        {
            var items = await _databaseService.GetAssetsByTypeAsync(_currentCategoryId);
            
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                items = items.Where(a => a.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            Assets = new ObservableCollection<AssetItem>(items);
            StatusMessage = $"Loaded {items.Count} assets";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            await Application.Current?.MainPage?.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SelectCategory(string categoryId)
    {
        _currentCategoryId = categoryId;
        SearchQuery = string.Empty;
        await LoadAssetsCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public async Task AddAsset()
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select Asset",
                FileTypes = GetFileTypesForCategory(_currentCategoryId)
            });

            if (result != null)
            {
                StatusMessage = "Importing asset...";
                var asset = await _databaseService.ImportAssetAsync(result.FullPath, _currentCategoryId);

                if (asset != null)
                {
                    var validation = await _validationService.ValidateAssetAsync(asset);
                    if (validation.IsValid)
                    {
                        StatusMessage = $"Asset '{asset.Name}' imported successfully";
                        await LoadAssetsCommand.ExecuteAsync(null);
                    }
                    else
                    {
                        StatusMessage = $"Validation failed: {string.Join(", ", validation.Errors)}";
                        await _databaseService.DeleteAssetAsync(asset.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error importing asset: {ex.Message}";
            await Application.Current?.MainPage?.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task DeleteAsset()
    {
        if (SelectedAsset == null)
            return;

        var result = await Application.Current?.MainPage?.DisplayAlert(
            "Confirm",
            $"Delete '{SelectedAsset.Name}'?",
            "Yes", "No"
        ) ?? false;

        if (result)
        {
            await _databaseService.DeleteAssetAsync(SelectedAsset.Id);
            StatusMessage = $"Asset '{SelectedAsset.Name}' deleted";
            SelectedAsset = null;
        }
    }

    [RelayCommand]
    public async Task SearchAssets()
    {
        await LoadAssetsCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public async Task RefreshAssets()
    {
        _previewService.ClearCache();
        await LoadAssetsCommand.ExecuteAsync(null);
    }

    private FilePickerFileType GetFileTypesForCategory(string categoryId)
    {
        return categoryId switch
        {
            "images" => new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "public.image" } },
                { DevicePlatform.Android, new[] { "image/png", "image/jpeg", "image/bmp" } },
                { DevicePlatform.WinUI, new[] { ".png", ".jpg", ".bmp" } },
                { DevicePlatform.macOS, new[] { "public.image" } }
            }),
            "audio" => new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "public.audio" } },
                { DevicePlatform.Android, new[] { "audio/mpeg", "audio/wav", "audio/ogg" } },
                { DevicePlatform.WinUI, new[] { ".mp3", ".wav", ".ogg" } },
                { DevicePlatform.macOS, new[] { "public.audio" } }
            }),
            "fonts" => new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "public.font" } },
                { DevicePlatform.Android, new[] { "font/ttf", "font/otf" } },
                { DevicePlatform.WinUI, new[] { ".ttf", ".otf" } },
                { DevicePlatform.macOS, new[] { "com.apple.truetype-font", "com.adobe.opentype-font" } }
            }),
            _ => new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "public.item" } },
                { DevicePlatform.Android, new[] { "*/*" } },
                { DevicePlatform.WinUI, new[] { "*.*" } },
                { DevicePlatform.macOS, new[] { "public.item" } }
            })
        };
    }
}

public class AssetCategoryViewModel : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
