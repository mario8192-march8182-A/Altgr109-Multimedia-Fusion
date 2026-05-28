using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AkpEngine.Platforms.macOS
{
    /// <summary>
    /// Exporta projetos .akp para bundle .app nativo do macOS.
    /// </summary>
    public class MacOSExporter : IExporter
    {
        private readonly ILogger<MacOSExporter> _logger;

        public MacOSExporter(ILogger<MacOSExporter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Exporta o projeto para macOS como bundle .app.
        /// </summary>
        public async Task<ExportResult> ExportAsync(AkpProject project, ExportSettings settings)
        {
            var result = new ExportResult { Warnings = new List<string>() };

            try
            {
                if (!File.Exists(project.SourcePath))
                {
                    result.Success = false;
                    result.ErrorMessage = "Arquivo .akp não encontrado.";
                    return result;
                }

                string appPath = Path.Combine(settings.OutputPath, $"{project.Name}.app");
                string contentsPath = Path.Combine(appPath, "Contents");
                string macosPath = Path.Combine(contentsPath, "MacOS");
                string resourcesPath = Path.Combine(contentsPath, "Resources");

                Directory.CreateDirectory(macosPath);
                Directory.CreateDirectory(resourcesPath);

                _logger.LogInformation("Gerando Info.plist...");
                var plistWriter = new PlistWriter();
                plistWriter.WriteInfoPlist(contentsPath, project, settings);

                _logger.LogInformation("Exportando assets...");
                AssetExporter.CopyAssets(project, resourcesPath);

                _logger.LogInformation("Publicando executável...");
                string arch = settings.Architecture == MacOSArchitecture.Arm64 ? "osx-arm64" : "osx-x64";
                var publishResult = await RunProcessAsync("dotnet", $"publish \"{project.SourcePath}\" -c Release -r {arch} --self-contained true -p:PublishSingleFile=true");

                if (publishResult.ExitCode != 0)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Falha no publish: {publishResult.Output}";
                    return result;
                }

                string exePath = Path.Combine(project.SourcePath, "bin", "Release", arch, "publish", project.Name);
                string targetExe = Path.Combine(macosPath, project.Name);
                File.Copy(exePath, targetExe, true);

                await RunProcessAsync("chmod", $"+x \"{targetExe}\"");

                _logger.LogInformation("Convertendo ícone...");
                var iconConverter = new IconConverter();
                iconConverter.ConvertToIcns(settings.IconPath, Path.Combine(resourcesPath, $"{project.Name}.icns"));

                result.Success = true;
                result.OutputPath = appPath;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Erro inesperado: {ex.Message}";
                return result;
            }
        }

        private async Task<(int ExitCode, string Output)> RunProcessAsync(string fileName, string args)
        {
            var tcs = new TaskCompletionSource<(int, string)>();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.EnableRaisingEvents = true;
            process.Exited += (s, e) =>
            {
                string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                tcs.SetResult((process.ExitCode, output));
                process.Dispose();
            };

            process.Start();
            return await tcs.Task;
        }
    }
}
