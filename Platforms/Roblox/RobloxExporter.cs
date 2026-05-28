using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AkpEngine.Platforms.Roblox
{
    /// <summary>
    /// Exporta projetos .akp para Roblox Studio (.rbxl).
    /// </summary>
    public class RobloxExporter : IExporter
    {
        private readonly ILogger<RobloxExporter> _logger;

        public RobloxExporter(ILogger<RobloxExporter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Exporta o projeto para Roblox Studio.
        /// </summary>
        public async Task<ExportResult> ExportAsync(AkpProject project, ExportSettings settings)
        {
            var robloxSettings = settings as RobloxExportSettings;
            var result = new ExportResult { Warnings = new List<string>() };

            try
            {
                if (!File.Exists(project.SourcePath))
                {
                    result.Success = false;
                    result.ErrorMessage = "Arquivo .akp não encontrado.";
                    return result;
                }

                string outputPath = Path.Combine(robloxSettings.OutputPath, $"{robloxSettings.GameTitle}.rbxl");
                string manifestPath = Path.Combine(robloxSettings.OutputPath, "AssetManifest.json");

                _logger.LogInformation("Gerando XML .rbxl...");
                var rbxlWriter = new RbxlWriter();
                rbxlWriter.WriteRbxl(project, robloxSettings, outputPath);

                _logger.LogInformation("Gerando scripts Luau...");
                var scriptGen = new LuauScriptGenerator();
                scriptGen.GenerateScripts(project, robloxSettings, outputPath);

                if (robloxSettings.GenerateAssetManifest)
                {
                    _logger.LogInformation("Gerando AssetManifest.json...");
                    var manifestWriter = new AssetManifestWriter();
                    manifestWriter.WriteManifest(project, manifestPath, result.Warnings);
                    result.AssetManifestPath = manifestPath;
                }

                result.Success = true;
                result.OutputPath = outputPath;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Erro inesperado: {ex.Message}";
                return result;
            }
        }
    }
}
