using AkpEditor.Core;

namespace AkpEditor.Mobile.Services;

public class EditorService
{
    private readonly ProjectService _projectService;
    private Stack<object> _undoStack = new();
    private Stack<object> _redoStack = new();

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public EditorService(ProjectService projectService)
    {
        _projectService = projectService;
    }

    public async Task SaveProjectAsync()
    {
        var project = await _projectService.GetCurrentProjectAsync();
        if (project != null)
        {
            await _projectService.SaveProjectAsync(project);
        }
    }

    public async Task RunProjectAsync()
    {
        // TODO: Implement project execution logic
        await Task.CompletedTask;
    }

    public void Undo()
    {
        if (CanUndo)
        {
            var action = _undoStack.Pop();
            _redoStack.Push(action);
        }
    }

    public void Redo()
    {
        if (CanRedo)
        {
            var action = _redoStack.Pop();
            _undoStack.Push(action);
        }
    }
}