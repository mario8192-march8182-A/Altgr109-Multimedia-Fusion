using System;
using System.IO;
using AkpProjectFormat;

namespace AkpEngine.Platforms.HTML5
{
    /// <summary>
    /// Exportador para plataforma HTML5 (WebGL)
    /// </summary>
    public class HTML5Exporter
    {
        private string _projectPath;
        private string _outputPath;

        public HTML5Exporter(string projectPath, string outputPath)
        {
            _projectPath = projectPath;
            _outputPath = outputPath;
        }

        /// <summary>
        /// Exporta o projeto para HTML5
        /// </summary>
        public void Export(AkpProject project)
        {
            Console.WriteLine($"[HTML5Exporter] Exportando para HTML5 (WebGL): {project.Name}");
            Console.WriteLine($"[HTML5Exporter] Output: {_outputPath}");

            try
            {
                Directory.CreateDirectory(_outputPath);
                Directory.CreateDirectory(Path.Combine(_outputPath, "assets"));
                Directory.CreateDirectory(Path.Combine(_outputPath, "js"));
                Directory.CreateDirectory(Path.Combine(_outputPath, "css"));

                // Copiar assets
                CopyAssets(project);

                // Gerar HTML
                GenerateHTML(project);

                // Gerar JavaScript
                GenerateJavaScript(project);

                // Gerar CSS
                GenerateCSS(project);

                Console.WriteLine($"[HTML5Exporter] Exportação completa! Abra index.html no navegador");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HTML5Exporter] Erro na exportação: {ex.Message}");
            }
        }

        private void CopyAssets(AkpProject project)
        {
            Console.WriteLine($"[HTML5Exporter] Copiando assets...");
            foreach (var asset in project.Assets)
            {
                string sourcePath = Path.Combine(_projectPath, asset.Value);
                string destPath = Path.Combine(_outputPath, "assets", Path.GetFileName(asset.Value));

                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destPath, true);
                }
            }
        }

        private void GenerateHTML(AkpProject project)
        {
            string html = $@"<!DOCTYPE html>
<html lang=\"pt-BR\">
<head>
    <meta charset=\"UTF-8\">
    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">
    <title>{project.Name}</title>
    <link rel=\"stylesheet\" href=\"css/style.css\">
</head>
<body>
    <div id=\"game-container\">
        <canvas id=\"gameCanvas\" width=\"{project.Width}\" height=\"{project.Height}\"></canvas>
    </div>
    <script src=\"js/akpengine.js\"></script>
    <script src=\"js/game.js\"></script>
</body>
</html>";

            File.WriteAllText(Path.Combine(_outputPath, "index.html"), html);
        }

        private void GenerateJavaScript(AkpProject project)
        {
            string js = $@"// AkpEngine HTML5 Runtime

class AkpGameHTML5 {{
    constructor(canvasId) {{
        this.canvas = document.getElementById(canvasId);
        this.ctx = this.canvas.getContext('2d');
        this.width = this.canvas.width;
        this.height = this.canvas.height;
        this.isRunning = false;
        this.lastFrameTime = Date.now();
        this.deltaTime = 0;
    }}

    initialize() {{
        console.log('[AkpEngine HTML5] Inicializando: {project.Name}');
        console.log(`[AkpEngine HTML5] Resolução: ${{this.width}}x${{this.height}}`);
    }}

    loadContent() {{
        // Carregar assets aqui
    }}

    update() {{
        const now = Date.now();
        this.deltaTime = (now - this.lastFrameTime) / 1000.0;
        this.lastFrameTime = now;
    }}

    draw() {{
        // Limpar canvas
        this.ctx.fillStyle = '#000000';
        this.ctx.fillRect(0, 0, this.width, this.height);
    }}

    run() {{
        this.initialize();
        this.loadContent();
        this.isRunning = true;
        this.loop();
    }}

    loop() {{
        this.update();
        this.draw();
        if (this.isRunning) {{
            requestAnimationFrame(() => this.loop());
        }}
    }}

    exit() {{
        this.isRunning = false;
    }}
}}

// Inicializar jogo
const game = new AkpGameHTML5('gameCanvas');
game.run();";

            File.WriteAllText(Path.Combine(_outputPath, "js", "akpengine.js"), js);

            string gameJs = @"// Código do jogo
// Adicionar lógica aqui";

            File.WriteAllText(Path.Combine(_outputPath, "js", "game.js"), gameJs);
        }

        private void GenerateCSS(AkpProject project)
        {
            string css = $@"* {{
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}}

body {{
    font-family: Arial, sans-serif;
    background-color: #1a1a1a;
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
}}

#game-container {{
    display: flex;
    justify-content: center;
    align-items: center;
}}

canvas {{
    border: 2px solid #333;
    background-color: #000;
    image-rendering: pixelated;
    image-rendering: -moz-crisp-edges;
    image-rendering: crisp-edges;
}}";

            File.WriteAllText(Path.Combine(_outputPath, "css", "style.css"), css);
        }
    }
}
