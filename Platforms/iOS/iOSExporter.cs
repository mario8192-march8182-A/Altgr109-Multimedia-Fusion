using System;
using System.IO;
using AkpProjectFormat;

namespace AkpEngine.Platforms.iOS
{
    /// <summary>
    /// Exportador para plataforma iOS
    /// </summary>
    public class iOSExporter
    {
        private string _projectPath;
        private string _outputPath;

        public iOSExporter(string projectPath, string outputPath)
        {
            _projectPath = projectPath;
            _outputPath = outputPath;
        }

        /// <summary>
        /// Exporta o projeto para iOS
        /// </summary>
        public void Export(AkpProject project)
        {
            Console.WriteLine($"[iOSExporter] Exportando para iOS: {project.Name}");
            Console.WriteLine($"[iOSExporter] Output: {_outputPath}");

            try
            {
                Directory.CreateDirectory(_outputPath);
                Directory.CreateDirectory(Path.Combine(_outputPath, "Assets.xcassets"));
                Directory.CreateDirectory(Path.Combine(_outputPath, "Sources"));

                // Copiar assets
                CopyAssets(project);

                // Gerar Info.plist
                GenerateInfoPlist(project);

                // Gerar stubs Swift
                GenerateSwiftStubs(project);

                Console.WriteLine($"[iOSExporter] Exportação completa!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[iOSExporter] Erro na exportação: {ex.Message}");
            }
        }

        private void CopyAssets(AkpProject project)
        {
            Console.WriteLine($"[iOSExporter] Copiando assets...");
            foreach (var asset in project.Assets)
            {
                string sourcePath = Path.Combine(_projectPath, asset.Value);
                string destPath = Path.Combine(_outputPath, "Assets.xcassets", Path.GetFileName(asset.Value));

                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destPath, true);
                }
            }
        }

        private void GenerateInfoPlist(AkpProject project)
        {
            string plist = $@"<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>{project.Name}</string>
    <key>CFBundleVersion</key>
    <string>{project.Version}</string>
    <key>CFBundleExecutable</key>
    <string>{project.Name}</string>
    <key>UIMainStoryboardFile</key>
    <string>Main</string>
    <key>UISupportedInterfaceOrientations</key>
    <array>
        <string>UIInterfaceOrientationPortrait</string>
        <string>UIInterfaceOrientationLandscapeLeft</string>
        <string>UIInterfaceOrientationLandscapeRight</string>
    </array>
    <key>MinimumOSVersion</key>
    <string>13.0</string>
</dict>
</plist>";

            File.WriteAllText(Path.Combine(_outputPath, "Info.plist"), plist);
        }

        private void GenerateSwiftStubs(AkpProject project)
        {
            string swiftCode = $@"import UIKit
import GameplayKit

class GameViewController: UIViewController {{
    var gameView: AkpGameView?
    
    override func viewDidLoad() {{
        super.viewDidLoad()
        gameView = AkpGameView(frame: view.bounds)
        if let gameView = gameView {{
            view.addSubview(gameView)
            gameView.load(project: \"{project.Name}\")
        }}
    }}
}}

class AkpGameView: UIView {{
    func load(project: String) {{
        print(\"Loading project: \\(project)\")
    }}
}}";

            File.WriteAllText(Path.Combine(_outputPath, "Sources", "GameViewController.swift"), swiftCode);
        }
    }
}
