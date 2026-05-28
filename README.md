# AkpEngine - 2D Multimedia Game Engine

**AkpEngine** é uma engine 2D completa para desenvolvimento de jogos, com suporte a múltiplas plataformas e formato de projeto proprietário `.akp`.

## ✨ Características

- 🎮 **Engine 2D Completa** com sistema de física e colisão
- 📱 **Multi-plataforma**: Android, PC (Windows/Linux), iOS, HTML5
- 💾 **Formato de Projeto**: `.akp` (AkpProject)
- 🔧 **Linguagem**: C# / .NET
- 🎨 **Editor Visual** integrado
- ⚡ **Renderização Otimizada** com suporte a sprites e tilesets
- 🎵 **Sistema de Áudio** 3D
- 🧩 **Sistema de Componentes** modular

## 📁 Estrutura do Projeto

```
Altgr109-Multimedia-Fusion/
├── AkpEngine/                  # Core da Engine
│   ├── Core/                   # Sistema central
│   ├── Renderer/               # Renderização 2D
│   ├── Physics/                # Sistema de física
│   ├── Audio/                  # Sistema de áudio
│   ├── Input/                  # Controles e entrada
│   └── Utils/                  # Utilitários
├── AkpEditor/                  # Editor visual
├── Platforms/                  # Exportadores
│   ├── Android/
│   ├── PC/
│   ├── iOS/
│   └── HTML5/
├── AkpProjectFormat/           # Parser .akp
├── Examples/                   # Projetos exemplo
└── Docs/                       # Documentação
```

## 🚀 Quick Start

```csharp
// Criar novo projeto
var game = new AkpGame();
game.Initialize();
game.Run();
```

## 📦 Formatos Suportados

- **Imagens**: PNG, JPG, BMP
- **Áudio**: MP3, WAV, OGG
- **Fontes**: TTF, OTF
- **Projeto**: .akp (XML compactado)

## 🛠️ Build

```bash
dotnet build
dotnet publish -c Release
```

## 📄 Licença

MIT License
