using System;
using System.Diagnostics;
using System.IO;

namespace AkpEngine.Platforms.macOS
{
    /// <summary>
    /// Converte ícones PNG para formato .icns.
    /// </summary>
    public class IconConverter
    {
        public void ConvertToIcns(string pngPath, string icnsPath)
        {
            if (!File.Exists(pngPath))
                throw new FileNotFoundException("Ícone PNG não encontrado.", pngPath);

            string tempIconset = Path.Combine(Path.GetTempPath(), "icon.iconset");
            Directory.CreateDirectory(tempIconset);

            File.Copy(pngPath, Path.Combine(tempIconset, "icon_512x512.png"), true);

            RunProcess("iconutil", $"-c icns \"{tempIconset}\" -o \"{icnsPath}\"");
        }

        private void RunProcess(string fileName, string args)
        {
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
            process.Start();
            process.WaitForExit();
        }
    }
}
