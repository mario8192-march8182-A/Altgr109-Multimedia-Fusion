using System.Diagnostics;
using AkpEditor.Mobile.Services.Export;

namespace AkpEditor.Mobile.Services.Export.Platforms;

public class AndroidExporter : IExportPlatform
{
    public string PlatformName => "Android";
    public string FileExtension => ".apk";
    public bool IsAvailable => true;

    private readonly string _androidSdkPath;

    public AndroidExporter()
    {
        _androidSdkPath = Environment.GetEnvironmentVariable("ANDROID_HOME") ?? "/opt/android-sdk";
    }

    public async Task<ExportResult> ExportAsync(ExportOptions options, IProgress<ExportProgress> progress)
    {
        var result = new ExportResult();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Step 1: Validate
            progress?.Report(new ExportProgress { PercentComplete = 10, CurrentStep = "Validating project" });
            if (!await ValidateProjectAsync(new ProjectData { Name = options.ProjectName })
            {
                result.Success = false;
                result.Errors.Add("Android project validation failed");
                return result;
            }

            // Step 2: Prepare build files
            progress?.Report(new ExportProgress { PercentComplete = 20, CurrentStep = "Preparing build files" });
            var buildPath = await PrepareBuildStructureAsync(options);

            // Step 3: Copy assets
            progress?.Report(new ExportProgress { PercentComplete = 40, CurrentStep = "Copying assets" });
            await CopyAssetsAsync(options, buildPath);

            // Step 4: Generate AndroidManifest.xml
            progress?.Report(new ExportProgress { PercentComplete = 50, CurrentStep = "Generating manifest" });
            GenerateAndroidManifest(options, buildPath);

            // Step 5: Compile
            progress?.Report(new ExportProgress { PercentComplete = 70, CurrentStep = "Compiling" });
            await CompileAsync(buildPath);

            // Step 6: Package
            progress?.Report(new ExportProgress { PercentComplete = 85, CurrentStep = "Packaging APK" });
            var outputFile = await PackageApkAsync(buildPath, options);

            // Step 7: Sign
            progress?.Report(new ExportProgress { PercentComplete = 95, CurrentStep = "Signing APK" });
            await SignApkAsync(outputFile);

            stopwatch.Stop();

            var fileInfo = new FileInfo(outputFile);
            result.Success = true;
            result.OutputFile = outputFile;
            result.FileSizeBytes = fileInfo.Length;
            result.ExportDuration = stopwatch.Elapsed;
            result.Message = $"Successfully exported to Android APK: {Path.GetFileName(outputFile)}";
            progress?.Report(new ExportProgress { PercentComplete = 100, CurrentStep = "Export complete" });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"Android export failed: {ex.Message}");
        }

        return result;
    }

    public async Task<bool> ValidateProjectAsync(ProjectData project)
    {
        if (string.IsNullOrWhiteSpace(project.Name))
            return false;

        // Check if Android SDK is available
        var sdkPath = Path.Combine(_androidSdkPath, "build-tools");
        return await Task.FromResult(Directory.Exists(sdkPath));
    }

    private async Task<string> PrepareBuildStructureAsync(ExportOptions options)
    {
        var buildPath = Path.Combine(options.OutputPath, "android_build");
        var dirs = new[]
        {
            Path.Combine(buildPath, "src", "main", "java"),
            Path.Combine(buildPath, "src", "main", "res", "drawable"),
            Path.Combine(buildPath, "src", "main", "res", "values"),
            Path.Combine(buildPath, "src", "main", "assets")
        };

        foreach (var dir in dirs)
        {
            Directory.CreateDirectory(dir);
        }

        return await Task.FromResult(buildPath);
    }

    private async Task CopyAssetsAsync(ExportOptions options, string buildPath)
    {
        var assetsDest = Path.Combine(buildPath, "src", "main", "assets");
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

    private void GenerateAndroidManifest(ExportOptions options, string buildPath)
    {
        var manifestPath = Path.Combine(buildPath, "src", "main", "AndroidManifest.xml");
        var manifest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<manifest xmlns:android=""http://schemas.android.com/apk/res/android""
    package=""com.akpengine.{options.ProjectName.ToLower()}"">

    <uses-permission android:name=""android.permission.INTERNET"" />
    <uses-permission android:name=""android.permission.ACCESS_NETWORK_STATE"" />
    <uses-permission android:name=""android.permission.CAMERA"" />
    <uses-permission android:name=""android.permission.RECORD_AUDIO"" />

    <application
        android:allowBackup=""true""
        android:icon=""@drawable/ic_launcher""
        android:label=""@string/app_name"">

        <activity
            android:name="".MainActivity""
            android:exported=""true"">
            <intent-filter>
                <action android:name=""android.intent.action.MAIN"" />
                <category android:name=""android.intent.category.LAUNCHER"" />
            </intent-filter>
        </activity>
    </application>
</manifest>";

        File.WriteAllText(manifestPath, manifest);
    }

    private async Task CompileAsync(string buildPath)
    {
        // Invoke Gradle build
        var processInfo = new ProcessStartInfo
        {
            FileName = "gradle",
            Arguments = $"-p {buildPath} assembleDebug",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(processInfo);
        await (process?.WaitForExitAsync() ?? Task.CompletedTask);
    }

    private async Task<string> PackageApkAsync(string buildPath, ExportOptions options)
    {
        var apkPath = Path.Combine(buildPath, "build", "outputs", "apk", "debug",
            $"{options.ProjectName}-debug.apk");
        return await Task.FromResult(apkPath);
    }

    private async Task SignApkAsync(string apkPath)
    {
        // Sign with debug key (for development)
        // TODO: Implement proper signing with release key
        await Task.CompletedTask;
    }
}