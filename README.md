# SnapMate

A modern screenshot tool built with WPF and .NET 8.

## Features

- **Screenshot Capture** - Region, full screen, and window capture
- **Image Editing** - Arrows, rectangles, text, and blur annotations
- **History** - Browse and search all screenshots with thumbnails
- **OCR** - Extract text from images
- **Auto-save** - Automatic saving with customizable naming patterns
- **Clipboard** - Quick copy to clipboard

## Tech Stack

- .NET 8 + WPF
- WPF-UI (Modern Fluent Design)
- MVVM with CommunityToolkit.Mvvm
- Dependency Injection (Microsoft.Extensions)
- LiteDB (Local database)
- SkiaSharp (Image processing)
- Tesseract (OCR)

## Getting Started

### Prerequisites
- .NET 8 SDK
- Windows 10/11

### Run

```bash
dotnet restore
dotnet run
```

### Default Hotkeys
- `Ctrl+Shift+A` - Region capture
- `Ctrl+Shift+S` - Full screen
- `Ctrl+Shift+W` - Window capture

## Project Structure

```
SnapMate/
©À©¤©¤ Models/      # Data models
©À©¤©¤ Services/         # Business logic
©À©¤©¤ ViewModels/       # MVVM view models
©À©¤©¤ Views/       # XAML UI
©À©¤©¤ Helpers/          # Utilities
©¸©¤©¤ App.xaml.cs       # App entry point
```

## Configuration

- **Settings**: `%AppData%\SnapMate\settings.json`
- **Screenshots**: `%UserProfile%\Pictures\SnapMate\`
- **Database**: `%AppData%\SnapMate\history.db`

## License

MIT License