using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AkpEditor.Mobile.Services;

namespace AkpEditor.Mobile.ViewModels;

public partial class EditorViewModel : ObservableObject
{
    private readonly EditorService _editorService;

    [ObservableProperty]
    private string projectName = "Untitled Project";

    [ObservableProperty]
    private List<object> selectedObjects = new();

    [ObservableProperty]
    private bool canUndo = false;

    [ObservableProperty]
    private bool canRedo = false;

    public EditorViewModel(EditorService editorService)
    {
        _editorService = editorService;
    }

    [RelayCommand]
    public async Task SaveProject()
    {
        try
        {
            await _editorService.SaveProjectAsync();
            await Application.Current?.MainPage?.DisplayAlert("Success", "Project saved successfully", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task RunProject()
    {
        try
        {
            await _editorService.RunProjectAsync();
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public void Undo()
    {
        _editorService.Undo();
        UpdateUndoRedoState();
    }

    [RelayCommand]
    public void Redo()
    {
        _editorService.Redo();
        UpdateUndoRedoState();
    }

    private void UpdateUndoRedoState()
    {
        CanUndo = _editorService.CanUndo;
        CanRedo = _editorService.CanRedo;
    }
}