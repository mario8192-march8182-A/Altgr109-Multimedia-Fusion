# AkpEngine - Mobile-First Architecture

## Overview

This document describes the architecture for adding a mobile-first editor to the AkpEngine project. The focus is on creating a MAUI-based mobile editor that shares core logic with a future desktop application.

## Project Structure

```
Altgr109-Multimedia-Fusion/
├── AkpEngine/                    # Core 2D Engine
│   ├── Core/                     # Game loop, scene management
│   ├── Renderer/                 # 2D rendering, sprites, tilesets
│   ├── Physics/                  # Physics simulation
│   ├── Audio/                    # Audio playback
│   ├── Input/                    # Input handling
│   └── Utils/                    # Utilities
├── AkpEditor.Core/               # Shared editor logic (NEW)
│   ├── EditorService.cs
│   └── ProjectManagement/
├── AkpEditor.Mobile/             # Mobile editor (MAUI) (NEW)
│   ├── Views/                    # XAML pages
│   ├── ViewModels/               # MVVM ViewModels
│   ├── Services/                 # Platform services
│   └── Models/                   # Data models
├── AkpProjectFormat/             # .akp file format parser
├── Platforms/                    # Export targets
├── Examples/                     # Example projects
└── Docs/                         # Documentation
```

## Technology Stack

### Mobile Editor (Primary Focus)
- **Framework**: .NET MAUI (Multi-platform App UI)
- **UI Pattern**: MVVM (Model-View-ViewModel)
- **Platforms**: Android, iOS
- **Libraries**:
  - `CommunityToolkit.Mvvm` - MVVM pattern implementation
  - `sqlite-net-pcl` - Local data persistence
  - `AkpEngine` - Core game engine (as dependency)
  - `AkpEditor.Core` - Shared editor logic

### Shared Core
- **Language**: C# / .NET 8.0
- **Architecture**: Service-based with dependency injection

## Data Flow

```
┌─────────────────────────────────────────────────┐
│         Mobile Editor (MAUI)                    │
│  ┌──────────────────────────────────────────┐  │
│  │     Views (XAML Pages)                   │  │
│  │  - EditorPage                            │  │
│  │  - AssetManagerPage                      │  │
│  │  - ProjectSettingsPage                   │  │
│  └──────────────────┬───────────────────────┘  │
│                     │                          │
│  ┌──────────────────▼───────────────────────┐  │
│  │     ViewModels                           │  │
│  │  - EditorViewModel                       │  │
│  │  - AssetManagerViewModel                 │  │
│  │  - ProjectSettingsViewModel              │  │
│  └──────────────────┬───────────────────────┘  │
│                     │                          │
│  ┌──────────────────▼───────────────────────┐  │
│  │     Services                             │  │
│  │  - EditorService                         │  │
│  │  - ProjectService                        │  │
│  │  - AssetService                          │  │
│  └──────────────────┬───────────────────────┘  │
└─────────────────────┼──────────────────────────┘
                      │
┌─────────────────────▼──────────────────────────┐
│     AkpEditor.Core (Shared Logic)              │
│  - EditorService                               │
│  - ProjectManagement                           │
│  - Undo/Redo System                            │
└─────────────────────┬──────────��───────────────┘
                      │
┌─────────────────────▼──────────────────────────┐
│     AkpEngine (Core Game Engine)               │
│  - Renderer                                    │
│  - Physics                                     │
│  - Audio                                       │
│  - Scene Management                            │
└────────────────────────────────────────────────┘
```

## Key Components

### Views (AkpEditor.Mobile/Views/)
- **EditorPage.xaml** - Main editing canvas with toolbar
- **AssetManagerPage.xaml** - Asset library and management
- **ProjectSettingsPage.xaml** - Project configuration
- **MainPage.xaml** - Landing page with new/open/recent projects

### ViewModels (AkpEditor.Mobile/ViewModels/)
Each ViewModel handles:
- State management
- Command execution (MVVM Toolkit RelayCommands)
- Communication with Services
- Binding to Views

### Services (AkpEditor.Mobile/Services/)
- **EditorService** - Sprite editing, undo/redo, canvas operations
- **ProjectService** - Project creation, loading, saving, export
- **AssetService** - Asset import, deletion, organization

### Models (AkpEditor.Mobile/Models/)
- **AssetItem** - Represents an asset (image, audio, font)
- **ProjectSettings** - Project configuration data

## Development Phases

### Phase 1: Foundation ✅ (Current)
- Project structure setup
- MAUI project creation
- Basic XAML pages
- Service architecture
- ViewModel scaffolding

### Phase 2: Canvas Rendering
- Implement 2D canvas in MAUI GraphicsView
- Sprite placement and manipulation
- Grid/snap-to-grid support
- Pan and zoom controls

### Phase 3: Asset Management
- Asset import from device storage
- Asset preview
- Asset categorization
- Local asset database

### Phase 4: Project Management
- New project creation
- Project saving/loading (.akp format)
- Recent projects list
- Project export (APK, IPA, HTML5)

### Phase 5: Polish & Testing
- Touch gesture support
- Performance optimization
- iOS and Android specific adjustments
- Unit and integration tests

## Dependency Injection

Services are registered in `MauiProgram.cs`:

```csharp
private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
{
    builder.Services.AddSingleton<EditorService>();
    builder.Services.AddSingleton<ProjectService>();
    builder.Services.AddSingleton<AssetService>();
    return builder;
}
```

## Future: Desktop Editor

When a desktop editor is added (WPF/WinUI):
1. Move all UI-agnostic logic to `AkpEditor.Core`
2. Create `AkpEditor.Desktop` project
3. Share ViewModels between mobile and desktop
4. Platform-specific UI implementations

## Build & Run

```bash
# Restore dependencies
dotnet restore

# Build mobile editor
cd AkpEditor.Mobile
dotnet build -f net8.0-android  # Android
dotnet build -f net8.0-ios      # iOS

# Run on emulator/device
dotnet maui run -f net8.0-android
dotnet maui run -f net8.0-ios
```

## Notes

- All user interactions flow through ViewModels
- Services provide the business logic layer
- MVVM Toolkit's `ObservableObject` and `RelayCommand` reduce boilerplate
- Dependency injection ensures loose coupling and testability
- The architecture is extensible for desktop and web variants
