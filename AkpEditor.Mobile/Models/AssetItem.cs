namespace AkpEditor.Mobile.Models;

public class AssetItem
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Image, Audio, Font, etc
    public string Path { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}