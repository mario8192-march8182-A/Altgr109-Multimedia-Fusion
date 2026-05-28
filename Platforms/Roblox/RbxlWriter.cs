using System.IO;
using System.Xml.Linq;

namespace AkpEngine.Platforms.Roblox
{
    /// <summary>
    /// Gera o XML .rbxl com hierarquia Roblox.
    /// </summary>
    public class RbxlWriter
    {
        public void WriteRbxl(AkpProject project, RobloxExportSettings settings, string outputPath)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("roblox",
                    new XAttribute("version", "4"),
                    new XElement("Item",
                        new XAttribute("class", "DataModel"),
                        new XElement("Properties",
                            new XElement("string", new XAttribute("name", "Name"), settings.GameTitle)
                        ),
                        // StarterGui com ScreenGui e camadas
                        RobloxXmlFactory.CreateStarterGui(project),
                        // StarterPlayerScripts com LocalScript
                        RobloxXmlFactory.CreateStarterPlayerScripts(),
                        // ReplicatedStorage com GameData
                        RobloxXmlFactory.CreateReplicatedStorage(project),
                        // Lighting neutro
                        RobloxXmlFactory.CreateLighting()
                    )
                )
            );

            doc.Save(outputPath);
        }
    }
}
