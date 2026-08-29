using System.Diagnostics;
using AkpEditor.Mobile.Services.Export;

namespace AkpEditor.Mobile.Services.Export.Platforms;

public class MacOSExporter : IExportPlatform
{
    public string PlatformName => "macOS";
    public string FileExtension => ".app";
    public bool IsAvailable => OperatingSystem.IsMacOS();

    public async Task<ExportResult> ExportAsync(ExportOptions options, IProgress<ExportProgress> progress)
    {
        var result = new ExportResult();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Step 1: Validate
            progress?.Report(new ExportProgress { PercentComplete = 10, CurrentStep = "Validating macOS project" });
            if (!await ValidateProjectAsync(new ProjectData { Name = options.ProjectName }))
            {
                result.Success = false;
                result.Errors.Add("macOS project validation failed");
                return result;
            }

            // Step 2: Create app bundle
            progress?.Report(new ExportProgress { PercentComplete = 25, CurrentStep = "Creating app bundle" });
            var bundlePath = await CreateAppBundleAsync(options);

            // Step 3: Copy assets
            progress?.Report(new ExportProgress { PercentComplete = 40, CurrentStep = "Copying assets" });
            await CopyAssetsAsync(options, bundlePath);

            // Step 4: Generate project files
            progress?.Report(new ExportProgress { PercentComplete = 55, CurrentStep = "Generating project files" });
            GenerateProjectFiles(options, bundlePath);

            // Step 5: Build
            progress?.Report(new ExportProgress { PercentComplete = 75, CurrentStep = "Building app" });
            await BuildAsync(bundlePath);

            // Step 6: Sign and notarize
            progress?.Report(new ExportProgress { PercentComplete = 90, CurrentStep = "Signing and notarizing" });
            await SignAndNotarizeAsync(bundlePath);

            stopwatch.Stop();

            result.Success = true;
            result.OutputFile = bundlePath;
            result.FileSizeBytes = GetDirectorySize(bundlePath);
            result.ExportDuration = stopwatch.Elapsed;
            result.Message = $"Successfully exported to macOS APP: {Path.GetFileName(bundlePath)}";
            progress?.Report(new ExportProgress { PercentComplete = 100, CurrentStep = "Export complete" });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"macOS export failed: {ex.Message}");
        }

        return result;
    }

    public async Task<bool> ValidateProjectAsync(ProjectData project)
    {
        if (string.IsNullOrWhiteSpace(project.Name))
            return false;

        // Check if .NET SDK is available on macOS
        var processInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "--version",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        try
        {
            using var process = Process.Start(processInfo);
            process?.WaitForExit();
            return await Task.FromResult(process?.ExitCode == 0);
        }
        catch
        {
            return await Task.FromResult(false);
        }
    }

    private async Task<string> CreateAppBundleAsync(ExportOptions options)
    {
        var bundlePath = Path.Combine(options.OutputPath, $"{options.ProjectName}.app", "Contents");
        var dirs = new[] { "MacOS", "Resources", "Assets" };

        Directory.CreateDirectory(bundlePath);
        foreach (var dir in dirs)
        {
            Directory.CreateDirectory(Path.Combine(bundlePath, dir));
        }

        return await Task.FromResult(bundlePath);
    }

    private async Task CopyAssetsAsync(ExportOptions options, string bundlePath)
    {
        var assetsDest = Path.Combine(bundlePath, "Assets");
        var assetsSource = Path.Combine(options.ProjectPath, "Assets");

        if (Directory.Exists(assetsSource))
        {
            foreach (var file in Directory.GetFiles(assetsSource, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(assetsSource, file);
                var destFile = Path.Combine(assetsDest, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destFile) ?? assetsDest);
                File.Copy(file, destFile, overwrite: true);
            }
        }

        await Task.CompletedTask;
    }

    private void GenerateProjectFiles(ExportOptions options, string bundlePath)
    {
        // Create Info.plist
        var plistPath = Path.Combine(bundlePath, "Info.plist");
        var plist = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version=""1.0"">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
    <key>CFBundleExecutable</key>
    <string>{options.ProjectName}</string>
    <key>CFBundleIdentifier</key>
    <string>com.akpengine.{options.ProjectName.ToLower()}</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>{options.ProjectName}</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleVersion</key>
    <string>1.0</string>
</dict>
</plist>";
        File.WriteAllText(plistPath, plist);
    }

    private async Task BuildAsync(string bundlePath)
    {
        // Build the macOS binary
        var macosPath = Path.Combine(bundlePath, "MacOS");
        var processInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build -c Release -o {macosPath}",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(processInfo);
        await (process?.WaitForExitAsync() ?? Task.CompletedTask);
    }

    private async Task SignAndNotarizeAsync(string bundlePath)
    {
        // Sign the app bundle
        var appPath = bundlePath.Replace("/Contents", "");
        var processInfo = new ProcessStartInfo
        {
            FileName = "codesign",
            Arguments = $"-s - {appPath}",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(processInfo);
        await (process?.WaitForExitAsync() ?? Task.CompletedTask);
    }

    private long GetDirectorySize(string path)
    {
        return new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories)
            .Sum(fi => fi.Length);
    }
}