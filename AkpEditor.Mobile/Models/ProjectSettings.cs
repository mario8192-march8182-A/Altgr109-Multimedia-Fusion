namespace AkpEditor.Mobile.Models;

public class ProjectSettings
{
    public string ProjectName { get; set; } = "New Project";
    public int ScreenWidth { get; set; } = 800;
    public int ScreenHeight { get; set; } = 600;
    public int TargetFps { get; set; } = 60;
    public string Version { get; set; } = "1.0.0";
}