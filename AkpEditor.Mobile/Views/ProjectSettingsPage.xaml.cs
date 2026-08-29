using AkpEditor.Mobile.ViewModels;

namespace AkpEditor.Mobile.Views;

public partial class ProjectSettingsPage : ContentPage
{
    public ProjectSettingsPage(ProjectSettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}