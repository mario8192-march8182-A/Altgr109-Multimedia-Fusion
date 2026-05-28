using System;
using System.IO;
using AkpProjectFormat;

namespace AkpEngine.Platforms.PC
{
    /// <summary>
    /// Exportador para plataforma PC (Windows/Linux)
    /// </summary>
    public class PCExporter
    {
        private string _projectPath;
        private string _outputPath;

        public PCExporter(string projectPath, string outputPath)
        {
            _projectPath = projectPath;
            _outputPath = outputPath;
        }

        /// <summary>
        /// Exporta o projeto para PC
        /// </summary>
        public void Export(AkpProject project)
        {
            Console.WriteLine($"[PCExporter] Exportando para PC (Windows/Linux): {project.Name}");
            Console.WriteLine($"[PCExporter] Output: {_outputPath}");

            try
            {
                Directory.CreateDirectory(_outputPath);
                Directory.CreateDirectory(Path.Combine(_outputPath, "Content"));
                Directory.CreateDirectory(Path.Combine(_outputPath, "bin"));

                // Copiar conteúdo
                CopyContent(project);

                // Gerar executável stub
                GenerateExecutableStub(project);

                // Gerar arquivo de configuração
                GenerateConfigFile(project);

                Console.WriteLine($"[PCExporter] Exportação completa! Executável gerado em: {_outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PCExporter] Erro na exportação: {ex.Message}");
            }
        }

        private void CopyContent(AkpProject project)
        {
            Console.WriteLine($"[PCExporter] Copiando conteúdo...");
            string contentPath = Path.Combine(_outputPath, "Content");

            foreach (var asset in project.Assets)
            {
                string sourcePath = Path.Combine(_projectPath, asset.Value);
                string destPath = Path.Combine(contentPath, Path.GetFileName(asset.Value));

                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destPath, true);
                }
            }
        }

        private void GenerateExecutableStub(AkpProject project)
        {
            string stubCode = $@"using AkpEngine.Core;

namespace {project.Name}Game
{{
    class Program
    {{
        static void Main(string[] args)
        {{
            using (var game = new {project.Name}Game())
            {{
                game.Run();
            }}
        }}
    }}

    class {project.Name}Game : AkpGame
    {{
        public override void Initialize()
        {{
            base.Initialize();
            Title = \"{project.Name}\";
            Width = {project.Width};
            Height = {project.Height};
        }}

        public override void LoadContent()
        {{
            base.LoadContent();
            // Carregar assets aqui
        }}

        public override void Update(GameTime gameTime)
        {{
            base.Update(gameTime);
            // Lógica de atualização
        }}

        public override void Draw(GameTime gameTime)
        {{
            base.Draw(gameTime);
            // Renderização
        }}
    }}
}}";

            string binPath = Path.Combine(_outputPath, "Program.cs");
            File.WriteAllText(binPath, stubCode);
        }

        private void GenerateConfigFile(AkpProject project)
        {
            string config = $@"[Project]
Name={project.Name}
Version={project.Version}
Author={project.Author}
Width={project.Width}
Height={project.Height}
StartScene={project.StartScene}

[Display]
Fullscreen=false
VSync=true
TargetFPS=60
";

            File.WriteAllText(Path.Combine(_outputPath, "config.ini"), config);
        }
    }
}
