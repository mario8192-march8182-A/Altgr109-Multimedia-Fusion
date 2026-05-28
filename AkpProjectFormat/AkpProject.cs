using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using System.IO.Compression;

namespace AkpProjectFormat
{
    /// <summary>
    /// Representa um projeto AkpEngine (.akp)
    /// </summary>
    public class AkpProject
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string StartScene { get; set; }
        public List<string> Scenes { get; set; }
        public Dictionary<string, string> Assets { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public AkpProject()
        {
            Scenes = new List<string>();
            Assets = new Dictionary<string, string>();
            Settings = new Dictionary<string, string>();
            Version = "1.0.0";
        }

        /// <summary>
        /// Carrega um projeto .akp
        /// </summary>
        public static AkpProject Load(string filePath)
        {
            Console.WriteLine($"[AkpProject] Carregando projeto: {filePath}");

            var project = new AkpProject();

            using (var archive = ZipFile.OpenRead(filePath))
            {
                var projectXml = archive.GetEntry("project.xml");
                if (projectXml != null)
                {
                    using (var stream = projectXml.Open())
                    {
                        var doc = XDocument.Load(stream);
                        var root = doc.Root;

                        project.Name = root?.Element("Name")?.Value ?? "Unnamed";
                        project.Version = root?.Element("Version")?.Value ?? "1.0.0";
                        project.Author = root?.Element("Author")?.Value ?? "Unknown";
                        project.Description = root?.Element("Description")?.Value ?? "";
                        project.Width = int.Parse(root?.Element("Width")?.Value ?? "1024");
                        project.Height = int.Parse(root?.Element("Height")?.Value ?? "768");
                        project.StartScene = root?.Element("StartScene")?.Value ?? "MainScene";
                    }
                }
            }

            Console.WriteLine($"[AkpProject] Projeto '{project.Name}' carregado com sucesso");
            return project;
        }

        /// <summary>
        /// Salva um projeto como .akp
        /// </summary>
        public void Save(string outputPath)
        {
            Console.WriteLine($"[AkpProject] Salvando projeto: {outputPath}");

            if (File.Exists(outputPath))
                File.Delete(outputPath);

            using (var archive = ZipFile.Open(outputPath, ZipArchiveMode.Create))
            {
                // Criar arquivo project.xml
                var projectXml = new XDocument(
                    new XElement("AkpProject",
                        new XElement("Name", Name),
                        new XElement("Version", Version),
                        new XElement("Author", Author),
                        new XElement("Description", Description),
                        new XElement("Width", Width),
                        new XElement("Height", Height),
                        new XElement("StartScene", StartScene)
                    )
                );

                var entry = archive.CreateEntry("project.xml");
                using (var stream = entry.Open())
                {
                    projectXml.Save(stream);
                }
            }

            Console.WriteLine($"[AkpProject] Projeto '{Name}' salvo com sucesso");
        }
    }
}
