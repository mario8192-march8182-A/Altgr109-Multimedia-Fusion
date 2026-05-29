using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AkpEngine.Platforms.PS5
{
    /// <summary>
    /// Exporta projetos .akp para pacote PS5.
    /// </summary>
    public class PS5Exporter : IExporter
    {
        private readonly ILogger<PS5Exporter> _logger;

        public PS5Exporter(ILogger<PS5Exporter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Exporta o projeto para PS5.
        /// </summary>
        public async Task<ExportResult> ExportAsync(AkpProject project, ExportSettings settings)
        {
            var ps5Settings = settings as PS5ExportSettings;
            var result = new ExportResult { Warnings = new List<string>(), SdkStubsRequired = new List<string>() };

            try
            {
                if (!File.Exists(project.SourcePath))
                {
                    result.Success = false;
                    result.ErrorMessage = "Arquivo .akp não encontrado.";
                    return result;
                }

                string outputDir = Path.Combine(ps5Settings.OutputPath, $"{project.Name}_PS5");
                Directory.CreateDirectory(outputDir);

                _logger.LogInformation("Gerando param.json...");
                var paramWriter = new ParamJsonWriter();
                paramWriter.WriteParamJson(outputDir, project, ps5Settings);

                _logger.LogInformation("Convertendo assets...");
                AssetConverter.ConvertAssets(project, outputDir, result.Warnings, result.SdkStubsRequired);

                _logger.LogInformation("Compilando eboot.bin...");
                await BuildEboot(project, outputDir, result.SdkStubsRequired);

                result.Success = true;
                result.OutputPath = outputDir;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Erro inesperado: {ex.Message}";
                return result;
            }
        }

        private async Task BuildEboot(AkpProject project, string outputDir, List<string> stubs)
        {
            // Compila via dotnet publish
            var publishResult = await ProcessRunner.RunAsync("dotnet", $"publish \"{project.SourcePath}\" -c Release -r prospero-x64 --self-contained true");

            if (publishResult.ExitCode != 0)
                throw new Exception($"Falha no publish: {publishResult.Output}");

            string binPath = Path.Combine(project.SourcePath, "bin", "Release", "prospero-x64", "publish", "AkpGame.bin");
            string ebootPath = Path.Combine(outputDir, "eboot.bin");

            File.Copy(binPath, ebootPath, true);

            // Stub de assinatura
            stubs.Add("[SonySDKCall] SignExecutable");
            stubs.Add("[SonySDKCall] PackagePkg");
        }
    }
}
