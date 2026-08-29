using System.Diagnostics;
using AkpEditor.Mobile.Services.Export;

namespace AkpEditor.Mobile.Services.Export.Platforms;

public class HTML5Exporter : IExportPlatform
{
    public string PlatformName => "HTML5 (Web)";
    public string FileExtension => ".zip";
    public bool IsAvailable => true;

    public async Task<ExportResult> ExportAsync(ExportOptions options, IProgress<ExportProgress> progress)
    {
        var result = new ExportResult();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Step 1: Validate
            progress?.Report(new ExportProgress { PercentComplete = 10, CurrentStep = "Validating HTML5 project" });
            if (!await ValidateProjectAsync(new ProjectData { Name = options.ProjectName }))
            {
                result.Success = false;
                result.Errors.Add("HTML5 project validation failed");
                return result;
            }

            // Step 2: Create web structure
            progress?.Report(new ExportProgress { PercentComplete = 20, CurrentStep = "Creating web structure" });
            var webPath = await CreateWebStructureAsync(options);

            // Step 3: Generate HTML
            progress?.Report(new ExportProgress { PercentComplete = 35, CurrentStep = "Generating HTML" });
            GenerateHtmlFile(options, webPath);

            // Step 4: Compile to WebAssembly
            progress?.Report(new ExportProgress { PercentComplete = 50, CurrentStep = "Compiling to WebAssembly" });
            await CompileToWebAssemblyAsync(options, webPath);

            // Step 5: Copy assets
            progress?.Report(new ExportProgress { PercentComplete = 70, CurrentStep = "Copying assets" });
            await CopyAssetsAsync(options, webPath);

            // Step 6: Create package
            progress?.Report(new ExportProgress { PercentComplete = 85, CurrentStep = "Packaging" });
            var outputFile = await CreatePackageAsync(webPath, options);

            stopwatch.Stop();

            var fileInfo = new FileInfo(outputFile);
            result.Success = true;
            result.OutputFile = outputFile;
            result.FileSizeBytes = fileInfo.Length;
            result.ExportDuration = stopwatch.Elapsed;
            result.Message = $"Successfully exported to HTML5: {Path.GetFileName(outputFile)}";
            progress?.Report(new ExportProgress { PercentComplete = 100, CurrentStep = "Export complete" });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"HTML5 export failed: {ex.Message}");
        }

        return result;
    }

    public async Task<bool> ValidateProjectAsync(ProjectData project)
    {
        if (string.IsNullOrWhiteSpace(project.Name))
            return false;

        // Check if Emscripten is available
        var processInfo = new ProcessStartInfo
        {
            FileName = "emcc",
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
            return await Task.FromResult(true); // Allow even if Emscripten not found
        }
    }

    private async Task<string> CreateWebStructureAsync(ExportOptions options)
    {
        var webPath = Path.Combine(options.OutputPath, "web");
        var dirs = new[] { "assets", "js", "css" };

        Directory.CreateDirectory(webPath);
        foreach (var dir in dirs)
        {
            Directory.CreateDirectory(Path.Combine(webPath, dir));
        }

        return await Task.FromResult(webPath);
    }

    private void GenerateHtmlFile(ExportOptions options, string webPath)
    {
        var htmlPath = Path.Combine(webPath, "index.html");
        var html = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{options.ProjectName}</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            overflow: hidden;
            background-color: #000;
            font-family: Arial, sans-serif;
        }}
        #canvas {{
            display: block;
            width: 100%;
            height: 100vh;
        }}
        #loading {{
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            color: white;
            font-size: 18px;
        }}
    </style>
</head>
<body>
    <div id=""loading"">
        <p>Loading {options.ProjectName}...</p>
        <progress id=""progress"" value=""0"" max=""100""></progress>
    </div>
    <canvas id=""canvas"" width=""{options.ScreenWidth}"" height=""{options.ScreenHeight}""></canvas>
    <script src=""js/engine.js""></script>
    <script src=""js/game.js""></script>
    <script>
        window.addEventListener('load', () => {{
            const engine = new AkpEngine('{options.ProjectName}');
            engine.initialize(document.getElementById('canvas'));
            engine.run();
        }});
    </script>
</body>
</html>";

        File.WriteAllText(htmlPath, html);
    }

    private async Task CompileToWebAssemblyAsync(ExportOptions options, string webPath)
    {
        // Compile C# to WebAssembly using Blazor or similar
        var processInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"publish -c Release -o {webPath}/dist",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(processInfo);
        await (process?.WaitForExitAsync() ?? Task.CompletedTask);
    }

    private async Task CopyAssetsAsync(ExportOptions options, string webPath)
    {
        var assetsDest = Path.Combine(webPath, "assets");
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

    private async Task<string> CreatePackageAsync(string webPath, ExportOptions options)
    {
        var zipPath = Path.Combine(options.OutputPath, $"{options.ProjectName}-web.zip");

        if (File.Exists(zipPath))
            File.Delete(zipPath);

        // Create ZIP archive
        var processInfo = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "powershell" : "zip",
            Arguments = OperatingSystem.IsWindows() 
                ? $"-NoProfile -Command \"Compress-Archive -Path '{webPath}\\*' -DestinationPath '{zipPath}'\""
                : $"-r {zipPath} {webPath}",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(processInfo);
        await (process?.WaitForExitAsync() ?? Task.CompletedTask);

        return zipPath;
    }
}