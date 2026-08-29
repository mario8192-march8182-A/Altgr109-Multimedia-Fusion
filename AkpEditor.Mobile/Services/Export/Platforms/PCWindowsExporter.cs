using System.Diagnostics;
using AkpEditor.Mobile.Services.Export;

namespace AkpEditor.Mobile.Services.Export.Platforms;

public class PCWindowsExporter : IExportPlatform
{
    public string PlatformName => "PC (Windows)";
    public string FileExtension => ".exe";
    public bool IsAvailable => OperatingSystem.IsWindows();

    public async Task<ExportResult> ExportAsync(ExportOptions options, IProgress<ExportProgress> progress)
    {
        var result = new ExportResult();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Step 1: Validate
            progress?.Report(new ExportProgress { PercentComplete = 10, CurrentStep = "Validating Windows project" });
            if (!await ValidateProjectAsync(new ProjectData { Name = options.ProjectName }))
            {
                result.Success = false;
                result.Errors.Add("Windows project validation failed");
                return result;
            }

            // Step 2: Create project structure
            progress?.Report(new ExportProgress { PercentComplete = 20, CurrentStep = "Creating project structure" });
            var projectPath = await CreateProjectStructureAsync(options);

            // Step 3: Copy assets
            progress?.Report(new ExportProgress { PercentComplete = 35, CurrentStep = "Copying assets" });
            await CopyAssetsAsync(options, projectPath);

            // Step 4: Generate .csproj
            progress?.Report(new ExportProgress { PercentComplete = 50, CurrentStep = "Generating C# project" });
            GenerateCsProject(options, projectPath);

            // Step 5: Compile
            progress?.Report(new ExportProgress { PercentComplete = 70, CurrentStep = "Compiling..." });
            await CompileAsync(projectPath);

            // Step 6: Package
            progress?.Report(new ExportProgress { PercentComplete = 85, CurrentStep = "Creating installer" });
            var outputFile = await PackageInstallerAsync(projectPath, options);

            stopwatch.Stop();

            var fileInfo = new FileInfo(outputFile);
            result.Success = true;
            result.OutputFile = outputFile;
            result.FileSizeBytes = fileInfo.Length;
            result.ExportDuration = stopwatch.Elapsed;
            result.Message = $"Successfully exported to Windows EXE: {Path.GetFileName(outputFile)}";
            progress?.Report(new ExportProgress { PercentComplete = 100, CurrentStep = "Export complete" });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"Windows export failed: {ex.Message}");
        }

        return result;
    }

    public async Task<bool> ValidateProjectAsync(ProjectData project)
    {
        if (string.IsNullOrWhiteSpace(project.Name))
            return false;

        // Check if .NET SDK is available
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

    private async Task<string> CreateProjectStructureAsync(ExportOptions options)
    {
        var projectPath = Path.Combine(options.OutputPath, options.ProjectName);
        var dirs = new[] { "Assets", "bin", "obj", "src" };

        Directory.CreateDirectory(projectPath);
        foreach (var dir in dirs)
        {
            Directory.CreateDirectory(Path.Combine(projectPath, dir));
        }

        return await Task.FromResult(projectPath);
    }

    private async Task CopyAssetsAsync(ExportOptions options, string projectPath)
    {
        var assetsDest = Path.Combine(projectPath, "Assets");
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

    private void GenerateCsProject(ExportOptions options, string projectPath)
    {
        var csprojPath = Path.Combine(projectPath, $"{options.ProjectName}.csproj");
        var csproj = $@"<Project Sdk=""Microsoft.NET.Sdk.WindowsDesktop"">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <AssemblyName>{options.ProjectName}</AssemblyName>
    <RootNamespace>{options.ProjectName}</RootNamespace>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\\..\\AkpEngine\\AkpEngine.csproj" />
  </ItemGroup>
</Project>";

        File.WriteAllText(csprojPath, csproj);
    }

    private async Task CompileAsync(string projectPath)
    {
        var csprojFile = Directory.GetFiles(projectPath, "*.csproj").FirstOrDefault();
        if (csprojFile == null)
            throw new FileNotFoundException("Project file not found");

        var processInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"publish {csprojFile} -c Release",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(processInfo);
        await (process?.WaitForExitAsync() ?? Task.CompletedTask);
    }

    private async Task<string> PackageInstallerAsync(string projectPath, ExportOptions options)
    {
        var publishPath = Path.Combine(projectPath, "bin", "Release", "net8.0-windows", "publish", $"{options.ProjectName}.exe");
        return await Task.FromResult(publishPath);
    }
}