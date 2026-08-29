using AkpEditor.Mobile.Models;
using System.Collections.ObjectModel;

namespace AkpEditor.Mobile.Services;

public class AssetDatabaseService
{
    private readonly string _assetDatabasePath;
    private readonly Dictionary<string, AssetItem> _assetsCache = new();
    private readonly List<AssetCategory> _categories = new();

    public event EventHandler<AssetChangedEventArgs>? AssetAdded;
    public event EventHandler<AssetChangedEventArgs>? AssetRemoved;
    public event EventHandler<AssetChangedEventArgs>? AssetUpdated;

    public AssetDatabaseService()
    {
        _assetDatabasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AkpEngine",
            "Assets"
        );
        InitializeDatabase();
        LoadCategories();
    }

    private void InitializeDatabase()
    {
        if (!Directory.Exists(_assetDatabasePath))
        {
            Directory.CreateDirectory(_assetDatabasePath);
        }

        // Create subdirectories for each asset type
        var assetTypes = new[] { "Images", "Audio", "Fonts", "Sprites", "Tilesets" };
        foreach (var type in assetTypes)
        {
            var path = Path.Combine(_assetDatabasePath, type);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }

    private void LoadCategories()
    {
        _categories.Clear();
        _categories.AddRange(new[]
        {
            new AssetCategory { Id = "images", Name = "Images", Icon = "🖼️", Extensions = new[] { ".png", ".jpg", ".bmp" } },
            new AssetCategory { Id = "audio", Name = "Audio", Icon = "🔊", Extensions = new[] { ".mp3", ".wav", ".ogg" } },
            new AssetCategory { Id = "fonts", Name = "Fonts", Icon = "🔤", Extensions = new[] { ".ttf", ".otf" } },
            new AssetCategory { Id = "sprites", Name = "Sprites", Icon = "🎭", Extensions = new[] { ".sprite" } },
            new AssetCategory { Id = "tilesets", Name = "Tilesets", Icon = "🧩", Extensions = new[] { ".tileset" } }
        });
    }

    public async Task<List<AssetItem>> GetAssetsByTypeAsync(string categoryId)
    {
        var category = _categories.FirstOrDefault(c => c.Id == categoryId);
        if (category == null)
            return new List<AssetItem>();

        var categoryPath = Path.Combine(_assetDatabasePath, category.Name);
        var assets = new List<AssetItem>();

        try
        {
            foreach (var filePath in Directory.GetFiles(categoryPath))
            {
                var fileInfo = new FileInfo(filePath);
                if (category.Extensions.Contains(fileInfo.Extension.ToLower()))
                {
                    var asset = new AssetItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = fileInfo.Name,
                        Type = categoryId,
                        Path = filePath,
                        Size = fileInfo.Length,
                        CreatedAt = fileInfo.CreationTime,
                        LastModified = fileInfo.LastWriteTime
                    };
                    assets.Add(asset);
                    _assetsCache[asset.Id] = asset;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading assets: {ex.Message}");
        }

        return await Task.FromResult(assets);
    }

    public async Task<List<AssetItem>> GetAllAssetsAsync()
    {
        var allAssets = new List<AssetItem>();
        foreach (var category in _categories)
        {
            var categoryAssets = await GetAssetsByTypeAsync(category.Id);
            allAssets.AddRange(categoryAssets);
        }
        return allAssets;
    }

    public async Task<AssetItem?> ImportAssetAsync(string sourceFilePath, string categoryId)
    {
        try
        {
            var category = _categories.FirstOrDefault(c => c.Id == categoryId);
            if (category == null)
                throw new ArgumentException($"Category {categoryId} not found");

            var fileInfo = new FileInfo(sourceFilePath);
            var extension = fileInfo.Extension.ToLower();

            if (!category.Extensions.Contains(extension))
                throw new InvalidOperationException($"File type {extension} not allowed in category {categoryId}");

            var destPath = Path.Combine(_assetDatabasePath, category.Name, fileInfo.Name);
            File.Copy(sourceFilePath, destPath, overwrite: true);

            var asset = new AssetItem
            {
                Id = Guid.NewGuid().ToString(),
                Name = fileInfo.Name,
                Type = categoryId,
                Path = destPath,
                Size = fileInfo.Length,
                CreatedAt = DateTime.Now,
                LastModified = DateTime.Now
            };

            _assetsCache[asset.Id] = asset;
            AssetAdded?.Invoke(this, new AssetChangedEventArgs { Asset = asset });

            return await Task.FromResult(asset);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error importing asset: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteAssetAsync(string assetId)
    {
        try
        {
            if (!_assetsCache.TryGetValue(assetId, out var asset))
                return false;

            if (File.Exists(asset.Path))
                File.Delete(asset.Path);

            _assetsCache.Remove(assetId);
            AssetRemoved?.Invoke(this, new AssetChangedEventArgs { Asset = asset });

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting asset: {ex.Message}");
            return false;
        }
    }

    public async Task<AssetItem?> GetAssetAsync(string assetId)
    {
        if (_assetsCache.TryGetValue(assetId, out var asset))
            return await Task.FromResult(asset);
        return null;
    }

    public List<AssetCategory> GetCategories() => _categories;

    public string GetAssetDatabasePath() => _assetDatabasePath;
}

public class AssetCategory
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string[] Extensions { get; set; } = Array.Empty<string>();
}

public class AssetChangedEventArgs : EventArgs
{
    public AssetItem? Asset { get; set; }
}