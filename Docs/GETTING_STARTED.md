# Getting Started with AkpEngine

## Instalação

### Requisitos
- .NET 8.0 ou superior
- Visual Studio 2022 ou VS Code
- Git

### Setup do Projeto

```bash
# Clonar o repositório
git clone https://github.com/mario8192-march8182-A/Altgr109-Multimedia-Fusion.git
cd Altgr109-Multimedia-Fusion

# Restaurar dependências
dotnet restore

# Build
dotnet build
```

## Criar seu Primeiro Jogo

### 1. Criar uma classe que herda de `AkpGame`

```csharp
using AkpEngine.Core;

public class MeuJogo : AkpGame
{
    public MeuJogo()
    {
        Title = "Meu Primeiro Jogo";
        Width = 800;
        Height = 600;
    }

    public override void Initialize()
    {
        base.Initialize();
        // Inicialização
    }

    public override void LoadContent()
    {
        base.LoadContent();
        // Carregar assets
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        // Lógica de atualização
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        // Renderização
    }
}
```

### 2. Executar o Jogo

```csharp
class Program
{
    static void Main()
    {
        using (var game = new MeuJogo())
        {
            game.Run();
        }
    }
}
```

## Exportar para Diferentes Plataformas

### Android
```csharp
var project = AkpProject.Load("meu_projeto.akp");
var exporter = new AndroidExporter("./projetos", "./exports/android");
exporter.Export(project);
```

### PC (Windows/Linux)
```csharp
var exporter = new PCExporter("./projetos", "./exports/pc");
exporter.Export(project);
```

### iOS
```csharp
var exporter = new iOSExporter("./projetos", "./exports/ios");
exporter.Export(project);
```

### HTML5
```csharp
var exporter = new HTML5Exporter("./projetos", "./exports/html5");
exporter.Export(project);
```

## Criar um Projeto .akp

```csharp
var project = new AkpProject
{
    Name = "Meu Jogo Incrível",
    Version = "1.0.0",
    Author = "Seu Nome",
    Description = "Descrição do jogo",
    Width = 1024,
    Height = 768,
    StartScene = "MainScene"
};

project.Assets.Add("player", "assets/player.png");
project.Assets.Add("background", "assets/background.png");

project.Save("meu_projeto.akp");
```

## Próximos Passos

- Leia a [Documentação da API](./API.md)
- Explore os [Exemplos](../Examples/)
- Junte-se à comunidade no GitHub
