using AkpEditor.Mobile.ViewModels;

namespace AkpEditor.Mobile.Views;

public partial class AssetManagerPage : ContentPage
{
    public AssetManagerPage(AssetManagerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        if (BindingContext is AssetManagerViewModel viewModel)
            await viewModel.SearchAssetsCommand.ExecuteAsync(null);
    }
}