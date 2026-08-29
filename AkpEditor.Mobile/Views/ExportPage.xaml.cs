using AkpEditor.Mobile.ViewModels;

namespace AkpEditor.Mobile.Views;

public partial class ExportPage : ContentPage
{
    public ExportPage(ExportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}