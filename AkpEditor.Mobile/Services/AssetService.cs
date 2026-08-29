using AkpEditor.Mobile.Models;

namespace AkpEditor.Mobile.Services;

public class AssetService
{
    private readonly List<AssetItem> _assets = new();

    public async Task<List<AssetItem>> GetAssetsAsync()
    {
        // TODO: Load from project file or database
        return await Task.FromResult(_assets);
    }

    public async Task ImportAssetAsync(string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            var extension = Path.GetExtension(filePath).ToLower();

            var asset = new AssetItem
            {
                Name = fileName,
                Type = extension switch
                {
                    ".png" or ".jpg" or ".bmp" => "Image",
                    ".mp3" or ".wav" or ".ogg" => "Audio",
                    ".ttf" or ".otf" => "Font",
                    _ => "Unknown"
                },
                Path = filePath
            };

            _assets.Add(asset);
            // TODO: Copy asset to project folder
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to import asset: {ex.Message}");
        }
    }

    public async Task DeleteAssetAsync(string assetName)
    {
        var asset = _assets.FirstOrDefault(a => a.Name == assetName);
        if (asset != null)
        {
            _assets.Remove(asset);
            // TODO: Delete from project folder
            await Task.CompletedTask;
        }
    }
}