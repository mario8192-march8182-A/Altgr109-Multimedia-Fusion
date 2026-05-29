using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AkpEngine.Platforms.PS5
{
    /// <summary>
    /// Gera o param.json do pacote PS5.
    /// </summary>
    public class ParamJsonWriter
    {
        public void WriteParamJson(string outputDir, AkpProject project, PS5ExportSettings settings)
        {
            var param = new Dictionary<string, object>
            {
                ["titleId"] = settings.TitleId,
                ["contentId"] = settings.ContentId,
                ["titleName"] = project.Name,
                ["masterVersion"] = settings.MasterVersion,
                ["appVersion"] = "01.00",
                ["category"] = "gd",
                ["targetContentVersion"] = 1,
                ["requiredSystemSoftwareVersion"] = "0x8000000",
                ["storageSize"] = 0,
                ["defaultLanguage"] = "en-US",
                ["supportedLanguages"] = settings.SupportedLanguages,
                ["attribute"] = 0,
                ["attribute2"] = settings.EnableHDR10 ? 1 : 0,
                ["downloadDataSize"] = 0,
                ["sdkVersion"] = settings.SdkVersion
            };

            string sceSys = Path.Combine(outputDir, "sce_sys");
            Directory.CreateDirectory(sceSys);
            File.WriteAllText(Path.Combine(sceSys, "param.json"), JsonSerializer.Serialize(param, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
