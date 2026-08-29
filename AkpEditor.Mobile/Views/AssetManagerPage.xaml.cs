using AkpEditor.Mobile.ViewModels;

namespace AkpEditor.Mobile.Views;

public partial class AssetManagerPage : ContentPage
{
    public AssetManagerPage(AssetManagerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}