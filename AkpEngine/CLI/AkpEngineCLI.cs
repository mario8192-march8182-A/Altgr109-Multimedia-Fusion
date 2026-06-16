#!/usr/bin/env dotnet
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using AkpProjectFormat;
using AkpEngine.Platforms.Android;
using AkpEngine.Platforms.PC;
using AkpEngine.Platforms.HTML5;

namespace AkpEngine.CLI
{
    /// <summary>
    /// Interface de Linha de Comando para AkpEngine
    /// Uso: akpengine [comando] [argumentos]
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            string command = args[0].ToLower();
            string[] commandArgs = args.Skip(1).ToArray();

            try
            {
                switch (command)
                {
                    case "export":
                        HandleExport(commandArgs);
                        break;
                    case "build":
                        HandleBuild(commandArgs);
                        break;
                    case "run":
                        HandleRun(commandArgs);
                        break;
                    case "create":
                        HandleCreate(commandArgs);
                        break;
                    case "help":
                    case "-h":
                    case "--help":
                        ShowHelp();
                        break;
                    case "version":
                    case "-v":
                    case "--version":
                        ShowVersion();
                        break;
                    default:
                        Console.WriteLine($"❌ Comando desconhecido: {command}");
                        Console.WriteLine("Use 'akpengine help' para ver os comandos disponíveis");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Comando: akpengine export [plataforma] [projeto.akp] [saída]
        /// </summary>
        static void HandleExport(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Uso: akpengine export [plataforma] [projeto.akp] [saída?]");
                Console.WriteLine("\nPlataformas suportadas:");
                Console.WriteLine("  apk        - Android Package");
                Console.WriteLine("  pc         - PC (Windows/Linux)");
                Console.WriteLine("  ios        - iOS (requer Xcode)");
                Console.WriteLine("  html5      - HTML5/WebGL");
                Console.WriteLine("\nExemplo:");
                Console.WriteLine("  akpengine export apk meu_jogo.akp ./exports/");
                return;
            }

            string platform = args[0].ToLower();
            string projectPath = args[1];
            string outputPath = args.Length > 2 ? args[2] : "./exports";

            // Validar arquivo de projeto
            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException($"Projeto não encontrado: {projectPath}");
            }

            Console.WriteLine($"📦 Exportando para {platform.ToUpper()}...");
            Console.WriteLine($"📂 Projeto: {Path.GetFileName(projectPath)}");
            Console.WriteLine($"📁 Saída: {outputPath}");

            // Carregar projeto
            AkpProject project = AkpProject.Load(projectPath);
            Directory.CreateDirectory(outputPath);

            switch (platform)
            {
                case "apk":
                    ExportAPK(project, projectPath, outputPath);
                    break;
                case "pc":
                    ExportPC(project, projectPath, outputPath);
                    break;
                case "html5":
                    ExportHTML5(project, projectPath, outputPath);
                    break;
                case "ios":
                    ExportIOS(project, projectPath, outputPath);
                    break;
                default:
                    throw new ArgumentException($"Plataforma desconhecida: {platform}");
            }

            Console.WriteLine("✅ Exportação concluída com sucesso!");
        }

        /// <summary>
        /// Exportar para APK (Android)
        /// </summary>
        static void ExportAPK(AkpProject project, string projectPath, string outputPath)
        {
            Console.WriteLine("\n🤖 Gerando arquivo APK...");

            string projectDir = Path.GetDirectoryName(projectPath);
            string apkOutputPath = Path.Combine(outputPath, "android");

            var exporter = new AndroidExporter(projectDir, apkOutputPath);
            exporter.Export(project);

            // Tentar compilar APK com Android SDK se disponível
            if (CheckAndroidSDK())
            {
                Console.WriteLine("📦 Compilando APK...");
                BuildAPK(apkOutputPath, project.Name);
                
                string apkFile = Path.Combine(apkOutputPath, "dist", $"{project.Name}.apk");
                if (File.Exists(apkFile))
                {
                    Console.WriteLine($"✅ APK gerado: {apkFile}");
                }
            }
            else
            {
                Console.WriteLine("⚠️  Android SDK não encontrado. Arquivos de origem gerados em: " + apkOutputPath);
                Console.WriteLine("💡 Para compilar: instale Android SDK e execute:");
                Console.WriteLine($"   cd {apkOutputPath} && ./gradlew assembleRelease");
            }
        }

        /// <summary>
        /// Exportar para PC
        /// </summary>
        static void ExportPC(AkpProject project, string projectPath, string outputPath)
        {
            Console.WriteLine("\n🖥️  Gerando executável para PC...");

            string projectDir = Path.GetDirectoryName(projectPath);
            string pcOutputPath = Path.Combine(outputPath, "pc");

            var exporter = new PCExporter(projectDir, pcOutputPath);
            exporter.Export(project);

            // Compilar com dotnet
            Console.WriteLine("🔨 Compilando com .NET...");
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "publish -c Release",
                WorkingDirectory = pcOutputPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    Console.WriteLine("✅ Executável gerado com sucesso!");
                }
            }
        }

        /// <summary>
        /// Exportar para HTML5
        /// </summary>
        static void ExportHTML5(AkpProject project, string projectPath, string outputPath)
        {
            Console.WriteLine("\n🌐 Gerando projeto HTML5...");

            string projectDir = Path.GetDirectoryName(projectPath);
            string html5OutputPath = Path.Combine(outputPath, "html5");

            var exporter = new HTML5Exporter(projectDir, html5OutputPath);
            exporter.Export(project);

            string indexPath = Path.Combine(html5OutputPath, "index.html");
            Console.WriteLine($"✅ Projeto HTML5 gerado: {indexPath}");
            Console.WriteLine("💡 Para testar localmente:");
            Console.WriteLine($"   cd {html5OutputPath}");
            Console.WriteLine("   python3 -m http.server 8000");
            Console.WriteLine("   # Acesse http://localhost:8000");
        }

        /// <summary>
        /// Exportar para iOS (com limitações)
        /// </summary>
        static void ExportIOS(AkpProject project, string projectPath, string outputPath)
        {
            Console.WriteLine("\n🍎 Preparando projeto para iOS...");
            Console.WriteLine("⚠️  Exportação para iOS requer Xcode e provisioning profiles.");
            Console.WriteLine("💡 Para compilar iOS:");
            Console.WriteLine("   1. Instale Xcode (Mac)");
            Console.WriteLine("   2. Configure provisioning profiles");
            Console.WriteLine("   3. Use xcodebuild para compilar");
        }

        /// <summary>
        /// Verificar se Android SDK está instalado
        /// </summary>
        static bool CheckAndroidSDK()
        {
            // Verificar variáveis de ambiente
            string androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME");
            if (string.IsNullOrEmpty(androidHome))
            {
                androidHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Android", "Sdk");
            }

            return Directory.Exists(androidHome);
        }

        /// <summary>
        /// Compilar APK usando Gradle
        /// </summary>
        static void BuildAPK(string projectPath, string projectName)
        {
            string gradleWrapper = Path.Combine(projectPath, "gradlew");
            if (!File.Exists(gradleWrapper))
            {
                Console.WriteLine("⚠️  Gradle wrapper não encontrado. Pulando compilação.");
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"{gradleWrapper} assembleRelease",
                WorkingDirectory = projectPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                        Console.WriteLine(line);
                }
                process.WaitForExit();
            }
        }

        /// <summary>
        /// Comando: akpengine build [projeto.csproj] [configuração?]
        /// </summary>
        static void HandleBuild(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Uso: akpengine build [projeto.csproj] [Debug|Release]");
                Console.WriteLine("\nExemplo:");
                Console.WriteLine("  akpengine build MyGame.csproj Release");
                return;
            }

            string projectFile = args[0];
            string configuration = args.Length > 1 ? args[1] : "Debug";

            if (!File.Exists(projectFile))
            {
                throw new FileNotFoundException($"Projeto não encontrado: {projectFile}");
            }

            Console.WriteLine($"🔨 Compilando {Path.GetFileName(projectFile)}...");

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build {projectFile} -c {configuration}",
                UseShellExecute = false,
                CreateNoWindow = false
            };

            using (var process = Process.Start(psi))
            {
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    Console.WriteLine("✅ Compilação concluída!");
                }
                else
                {
                    throw new Exception("Erro na compilação");
                }
            }
        }

        /// <summary>
        /// Comando: akpengine run [arquivo.exe|.dll]
        /// </summary>
        static void HandleRun(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Uso: akpengine run [arquivo.exe|.dll]");
                Console.WriteLine("\nExemplo:");
                Console.WriteLine("  akpengine run ./bin/Debug/net8.0/MyGame.dll");
                return;
            }

            string executable = args[0];

            if (!File.Exists(executable))
            {
                throw new FileNotFoundException($"Arquivo não encontrado: {executable}");
            }

            Console.WriteLine($"▶️  Executando {Path.GetFileName(executable)}...\n");

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = executable,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            using (var process = Process.Start(psi))
            {
                process.WaitForExit();
            }
        }

        /// <summary>
        /// Comando: akpengine create [nome] [template?]
        /// </summary>
        static void HandleCreate(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Uso: akpengine create [nome] [template?]");
                Console.WriteLine("\nTemplates disponíveis:");
                Console.WriteLine("  2d         - Jogo 2D simples");
                Console.WriteLine("  fnaf       - Five Nights at Freddy's");
                Console.WriteLine("  snake      - Jogo Snake");
                Console.WriteLine("  platformer - Platformer 2D");
                Console.WriteLine("\nExemplo:");
                Console.WriteLine("  akpengine create MeuJogo 2d");
                return;
            }

            string projectName = args[0];
            string template = args.Length > 1 ? args[1] : "2d";

            Console.WriteLine($"🆕 Criando novo projeto: {projectName}");
            Console.WriteLine($"📋 Template: {template}");

            // Criar diretório
            Directory.CreateDirectory(projectName);
            
            // Criar arquivo de projeto .akp básico
            var project = new AkpProject
            {
                Name = projectName,
                Version = "1.0.0",
                Author = "Desenvolvedor",
                Description = $"Jogo criado com AkpEngine - Template: {template}",
                Width = 1024,
                Height = 768,
                StartScene = "MainScene"
            };

            string projectPath = Path.Combine(projectName, $"{projectName}.akp");
            project.Save(projectPath);

            Console.WriteLine($"✅ Projeto criado: {projectPath}");
            Console.WriteLine($"💡 Para exportar: akpengine export apk {projectPath} ./exports");
        }

        static void ShowHelp()
        {
            Console.WriteLine(@"
╔════════════════════════════════════════════════════════════╗
║             AkpEngine - Command Line Interface            ║
╚════════════════════════════════════════════════════════════╝

📋 COMANDOS DISPONÍVEIS:

  export [plataforma] [arquivo.akp] [saída]
    Exporta o projeto para a plataforma especificada
    Plataformas: apk, pc, html5, ios
    
    Exemplo:
      akpengine export apk meu_jogo.akp ./exports/

  build [arquivo.csproj] [configuração]
    Compila um projeto C#
    
    Exemplo:
      akpengine build MyGame.csproj Release

  run [arquivo.exe|.dll]
    Executa uma aplicação compilada
    
    Exemplo:
      akpengine run ./bin/Debug/net8.0/MyGame.dll

  create [nome] [template]
    Cria um novo projeto
    Templates: 2d, fnaf, snake, platformer
    
    Exemplo:
      akpengine create MyGame 2d

  help, -h, --help
    Mostra esta mensagem de ajuda

  version, -v, --version
    Mostra a versão do AkpEngine

═════════════════════════════════════════════════════════════

🚀 QUICK START:

  1. Criar projeto:
     akpengine create MyGame 2d

  2. Exportar para APK:
     akpengine export apk MyGame/MyGame.akp ./exports/

  3. Exportar para HTML5:
     akpengine export html5 MyGame/MyGame.akp ./exports/

═════════════════════════════════════════════════════════════
");
        }

        static void ShowVersion()
        {
            Console.WriteLine("AkpEngine CLI v1.0.0");
            Console.WriteLine("Framework: .NET 8.0");
            Console.WriteLine("Suporte a plataformas: Android, PC, iOS, HTML5");
        }
    }
}
