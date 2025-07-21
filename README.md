# Zoomies - Windows Magnifier Application

A lightweight, native Windows magnifier application with minimal bloat.

## Project Status

🚧 **Under Development** - Phase 1: Foundation

## Requirements

- Windows 10 version 1903 or later
- .NET 6.0 SDK
- Visual Studio 2022 or Visual Studio Code with C# extension

## Building

### From Windows

1. Install .NET 6.0 SDK from https://dotnet.microsoft.com/download
2. Clone the repository
3. Build using:
   ```cmd
   make build
   ```
   Or directly:
   ```cmd
   dotnet build -c Release
   ```

### From Visual Studio

1. Open `Zoomies.sln`
2. Build → Build Solution (Ctrl+Shift+B)

## Development

- `make build` - Build the application
- `make test` - Run unit tests
- `make lint` - Run code analysis
- `make fmt` - Format code
- `make clean` - Clean build artifacts
- `make package` - Create release package

## Project Structure

```
/src
  /Core           - Core business logic
    /ScreenCapture - Screen capture functionality
    /Input        - Keyboard/mouse hooks
    /Magnification - Zoom logic
  /UI            - WPF user interface
  /Native        - Windows API interop
  /Utils         - Helper utilities
/tests          - Unit and integration tests
/docs           - Project documentation
```

## Features (Planned)

- [x] Project structure
- [ ] Screen capture with Desktop Duplication API
- [ ] Global hotkeys (CTRL+ALT+Scroll)
- [ ] Magnification overlay
- [ ] System tray integration
- [ ] Auto-start with Windows

## License

TBD