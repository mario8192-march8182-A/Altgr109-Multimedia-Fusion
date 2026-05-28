using System.IO;
using System.Xml.Linq;

namespace AkpEngine.Platforms.macOS
{
    /// <summary>
    /// Gera o arquivo Info.plist para o bundle macOS.
    /// </summary>
    public class PlistWriter
    {
        public void WriteInfoPlist(string contentsPath, AkpProject project, ExportSettings settings)
        {
            var plist = new XElement("plist",
                new XAttribute("version", "1.0"),
                new XElement("dict",
                    new XElement("key", "CFBundleName"), new XElement("string", project.Name),
                    new XElement("key", "CFBundleDisplayName"), new XElement("string", project.Name),
                    new XElement("key", "CFBundleIdentifier"), new XElement("string", settings.BundleIdentifier),
                    new XElement("key", "CFBundleVersion"), new XElement("string", settings.Version),
                    new XElement("key", "CFBundleShortVersionString"), new XElement("string", settings.Version),
                    new XElement("key", "CFBundleExecutable"), new XElement("string", project.Name),
                    new XElement("key", "CFBundleIconFile"), new XElement("string", $"{project.Name}.icns"),
                    new XElement("key", "NSHighResolutionCapable"), new XElement("true"),
                    new XElement("key", "LSMinimumSystemVersion"), new XElement("string", "10.15"),
                    new XElement("key", "CFBundlePackageType"), new XElement("string", "APPL")
                )
            );

            string plistPath = Path.Combine(contentsPath, "Info.plist");
            plist.Save(plistPath);
        }
    }
}
