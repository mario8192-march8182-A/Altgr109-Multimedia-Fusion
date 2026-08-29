using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AkpEditor.Mobile.Services;
using AkpEditor.Mobile.Models;

namespace AkpEditor.Mobile.ViewModels;

public partial class AssetManagerViewModel : ObservableObject
{
    private readonly AssetService _assetService;

    [ObservableProperty]
    private ObservableCollection<AssetItem> assets = new();

    [ObservableProperty]
    private AssetItem? selectedAsset;

    public AssetManagerViewModel(AssetService assetService)
    {
        _assetService = assetService;
        LoadAssets();
    }

    [RelayCommand]
    public async Task LoadAssets()
    {
        try
        {
            var items = await _assetService.GetAssetsAsync();
            Assets = new ObservableCollection<AssetItem>(items);
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task AddAsset()
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select Asset"
            });

            if (result != null)
            {
                await _assetService.ImportAssetAsync(result.FullPath);
                await LoadAssetsCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}

public class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T> { }