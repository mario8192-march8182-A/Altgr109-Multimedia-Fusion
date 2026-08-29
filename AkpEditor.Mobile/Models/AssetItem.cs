namespace AkpEditor.Mobile.Models;

public class AssetItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // images, audio, fonts, sprites, tilesets
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; } // bytes
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SpriteAsset : AssetItem
{
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    public int FrameCount { get; set; }
}

public class TilesetAsset : AssetItem
{
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public int TilesPerRow { get; set; }
}
