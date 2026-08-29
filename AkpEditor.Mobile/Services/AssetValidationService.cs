using AkpEditor.Mobile.Models;

namespace AkpEditor.Mobile.Services;

public class AssetValidationService
{
    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50MB

    private readonly Dictionary<string, (int minWidth, int maxWidth, int minHeight, int maxHeight)> _imageLimits = new()
    {
        { ".png", (16, 4096, 16, 4096) },
        { ".jpg", (16, 4096, 16, 4096) },
        { ".bmp", (16, 2048, 16, 2048) }
    };

    public async Task<ValidationResult> ValidateAssetAsync(AssetItem asset)
    {
        var result = new ValidationResult { IsValid = true };

        // Check file size
        if (asset.Size > MaxFileSizeBytes)
        {
            result.IsValid = false;
            result.Errors.Add($"File size exceeds maximum of {MaxFileSizeBytes / (1024 * 1024)}MB");
        }

        // Type-specific validation
        if (asset.Type == "images")
        {
            result = await ValidateImageAsync(asset);
        }
        else if (asset.Type == "audio")
        {
            result = await ValidateAudioAsync(asset);
        }

        return await Task.FromResult(result);
    }

    private async Task<ValidationResult> ValidateImageAsync(AssetItem asset)
    {
        var result = new ValidationResult { IsValid = true };
        var ext = Path.GetExtension(asset.Path).ToLower();

        try
        {
            using var stream = File.OpenRead(asset.Path);
            var image = await PlatformImage.FromStreamAsync(stream);

            if (image == null)
            {
                result.IsValid = false;
                result.Errors.Add("Failed to load image");
                return result;
            }

            // Validate dimensions
            if (_imageLimits.TryGetValue(ext, out var limits))
            {
                if (image.Width < limits.minWidth || image.Width > limits.maxWidth)
                    result.Errors.Add($"Image width must be between {limits.minWidth} and {limits.maxWidth}px");

                if (image.Height < limits.minHeight || image.Height > limits.maxHeight)
                    result.Errors.Add($"Image height must be between {limits.minHeight} and {limits.maxHeight}px");
            }

            result.IsValid = result.Errors.Count == 0;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Image validation failed: {ex.Message}");
        }

        return result;
    }

    private async Task<ValidationResult> ValidateAudioAsync(AssetItem asset)
    {
        var result = new ValidationResult { IsValid = true };

        try
        {
            // Basic audio file validation
            var ext = Path.GetExtension(asset.Path).ToLower();
            var validAudioExtensions = new[] { ".mp3", ".wav", ".ogg" };

            if (!validAudioExtensions.Contains(ext))
            {
                result.IsValid = false;
                result.Errors.Add($"Audio format {ext} not supported");
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Audio validation failed: {ex.Message}");
        }

        return await Task.FromResult(result);
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
