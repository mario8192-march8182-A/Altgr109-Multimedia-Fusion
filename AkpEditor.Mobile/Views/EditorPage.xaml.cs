using AkpEditor.Mobile.ViewModels;

namespace AkpEditor.Mobile.Views;

public partial class EditorPage : ContentPage
{
    public EditorPage(EditorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}