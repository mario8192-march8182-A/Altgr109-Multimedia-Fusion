using AkpEditor.Mobile.Models;

namespace AkpEditor.Mobile.Services;

public class AssetPreviewService
{
    private readonly Dictionary<string, ImageSource> _previewCache = new();

    public async Task<ImageSource?> GetPreviewAsync(AssetItem asset)
    {
        if (_previewCache.TryGetValue(asset.Id, out var cached))
            return cached;

        try
        {
            ImageSource? preview = asset.Type switch
            {
                "images" => new FileImageSource { File = asset.Path },
                "sprites" => await LoadSpritePreviewAsync(asset.Path),
                "tilesets" => await LoadTilesetPreviewAsync(asset.Path),
                "audio" => new FontImageSource { FontFamily = "FontAwesome", Glyph = "🔊", Color = Colors.Gray },
                "fonts" => new FontImageSource { FontFamily = "FontAwesome", Glyph = "🔤", Color = Colors.Gray },
                _ => null
            };

            if (preview != null)
                _previewCache[asset.Id] = preview;

            return preview;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading preview: {ex.Message}");
            return null;
        }
    }

    private async Task<ImageSource?> LoadSpritePreviewAsync(string spritePath)
    {
        // TODO: Parse sprite file and generate preview
        return await Task.FromResult<ImageSource?>(null);
    }

    private async Task<ImageSource?> LoadTilesetPreviewAsync(string tilesetPath)
    {
        // TODO: Parse tileset file and generate preview
        return await Task.FromResult<ImageSource?>(null);
    }

    public void ClearCache()
    {
        _previewCache.Clear();
    }
}
