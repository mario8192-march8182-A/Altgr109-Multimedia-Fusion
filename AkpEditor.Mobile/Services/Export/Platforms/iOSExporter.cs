using System.Diagnostics;
using AkpEditor.Mobile.Services.Export;

namespace AkpEditor.Mobile.Services.Export.Platforms;

public class iOSExporter : IExportPlatform
{
    public string PlatformName => "iOS";
    public string FileExtension => ".ipa";
    public bool IsAvailable => DeviceInfo.Platform == DevicePlatform.iOS;

    public async Task<ExportResult> ExportAsync(ExportOptions options, IProgress<ExportProgress> progress)
    {
        var result = new ExportResult();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Step 1: Validate
            progress?.Report(new ExportProgress { PercentComplete = 10, CurrentStep = "Validating iOS project" });
            if (!await ValidateProjectAsync(new ProjectData { Name = options.ProjectName }))
            {
                result.Success = false;
                result.Errors.Add("iOS project validation failed");
                return result;
            }

            // Step 2: Prepare Xcode project
            progress?.Report(new ExportProgress { PercentComplete = 25, CurrentStep = "Preparing Xcode project" });
            var projectPath = await PrepareXcodeProjectAsync(options);

            // Step 3: Copy assets
            progress?.Report(new ExportProgress { PercentComplete = 40, CurrentStep = "Copying assets" });
            await CopyAssetsAsync(options, projectPath);

            // Step 4: Configure build settings
            progress?.Report(new ExportProgress { PercentComplete = 55, CurrentStep = "Configuring build settings" });
            ConfigureBuildSettings(options, projectPath);

            // Step 5: Build for Archive
            progress?.Report(new ExportProgress { PercentComplete = 70, CurrentStep = "Building archive" });
            var archivePath = await BuildArchiveAsync(projectPath);

            // Step 6: Export IPA
            progress?.Report(new ExportProgress { PercentComplete = 85, CurrentStep = "Exporting IPA" });
            var outputFile = await ExportIpaAsync(archivePath, options);

            stopwatch.Stop();

            var fileInfo = new FileInfo(outputFile);
            result.Success = true;
            result.OutputFile = outputFile;
            result.FileSizeBytes = fileInfo.Length;
            result.ExportDuration = stopwatch.Elapsed;
            result.Message = $"Successfully exported to iOS IPA: {Path.GetFileName(outputFile)}";
            progress?.Report(new ExportProgress { PercentComplete = 100, CurrentStep = "Export complete" });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"iOS export failed: {ex.Message}");
        }

        return result;
    }

    public async Task<bool> ValidateProjectAsync(ProjectData project)
    {
        if (string.IsNullOrWhiteSpace(project.Name))
            return false;

        // Check if Xcode is available
        var processInfo = new ProcessStartInfo
        {
            FileName = "xcode-select",
            Arguments = "-p",
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

    private async Task<string> PrepareXcodeProjectAsync(ExportOptions options)
    {
        var projectPath = Path.Combine(options.OutputPath, $"{options.ProjectName}.xcodeproj");
        Directory.CreateDirectory(projectPath);
        return await Task.FromResult(projectPath);
    }

    private async Task CopyAssetsAsync(ExportOptions options, string projectPath)
    {
        var assetsPath = Path.Combine(projectPath, "Assets");
        Directory.CreateDirectory(assetsPath);

        var sourceAssetsPath = Path.Combine(options.ProjectPath, "Assets");
        if (Directory.Exists(sourceAssetsPath))
        {
            foreach (var file in Directory.GetFiles(sourceAssetsPath, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceAssetsPath, file);
                var destFile = Path.Combine(assetsPath, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destFile) ?? assetsPath);
                File.Copy(file, destFile, overwrite: true);
            }
        }

        await Task.CompletedTask;
    }

    private void ConfigureBuildSettings(ExportOptions options, string projectPath)
    {
        // Configure Xcode build settings
        var settingsPath = Path.Combine(projectPath, "build_settings.pbxproj");
        var settings = $@"// Xcode Build Settings for {options.ProjectName}
GCC_VERSION = com.apple.compilers.llvm.clang.1_0
RESOURCES_FOLDER = Resources
TARGET_BUILD_DIR = Build
INCLUDES = -I/usr/local/include
";
        File.WriteAllText(settingsPath, settings);
    }

    private async Task<string> BuildArchiveAsync(string projectPath)
    {
        var archivePath = Path.Combine(projectPath, "build", $"App.xcarchive");
        Directory.CreateDirectory(Path.GetDirectoryName(archivePath) ?? projectPath);

        var processInfo = new ProcessStartInfo
        {
            FileName = "xcodebuild",
            Arguments = $"-scheme App -configuration Release -archivePath {archivePath} archive",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(processInfo);
        await (process?.WaitForExitAsync() ?? Task.CompletedTask);

        return archivePath;
    }

    private async Task<string> ExportIpaAsync(string archivePath, ExportOptions options)
    {
        var ipaPath = Path.Combine(options.OutputPath, $"{options.ProjectName}.ipa");
        var exportOptions = Path.Combine(options.OutputPath, "ExportOptions.plist");

        // Create export options plist
        var plist = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version=""1.0"">
<dict>
    <key>signingStyle</key>
    <string>automatic</string>
    <key>teamID</key>
    <string></string>
</dict>
</plist>";

        File.WriteAllText(exportOptions, plist);

        var processInfo = new ProcessStartInfo
        {
            FileName = "xcodebuild",
            Arguments = $"-exportArchive -archivePath {archivePath} -exportPath {options.OutputPath} -exportOptionsPlist {exportOptions}",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(processInfo);
        await (process?.WaitForExitAsync() ?? Task.CompletedTask);

        return ipaPath;
    }
}