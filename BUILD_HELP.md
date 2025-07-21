# Build Help for Zoomies

## Quick Start (Windows)

1. **Open PowerShell or Command Prompt**
2. **Navigate to project directory:**
   ```powershell
   cd C:\path\to\zoomies
   ```
3. **Build the project:**
   ```powershell
   .\build.ps1
   ```
   Or using Make:
   ```cmd
   make build
   ```

## Prerequisites Check

Run this first to check your environment:
```powershell
.\build.ps1 -Target deps
```

## Common Build Issues & Solutions

### Issue: ".NET SDK not found"
**Solution:** Install .NET 6.0 SDK
1. Download from: https://dotnet.microsoft.com/download/dotnet/6.0
2. Choose "SDK x64" for Windows
3. Run installer and restart PowerShell

### Issue: "Cannot run scripts" (PowerShell)
**Solution:** Enable script execution
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Issue: "make: command not found"
**Solution:** Use PowerShell script instead
```powershell
.\build.ps1 build
```
Or install Make for Windows:
- Option 1: Install via Chocolatey: `choco install make`
- Option 2: Use Git Bash (comes with Git for Windows)

### Issue: "The framework 'Microsoft.NETCore.App', version '6.0.0' was not found"
**Solution:** Install .NET 6.0 Runtime and SDK
```powershell
winget install Microsoft.DotNet.SDK.6
```

### Issue: Build errors about missing types
**Solution:** Restore NuGet packages first
```powershell
dotnet restore
dotnet build
```

## Build from Different Environments

### From Visual Studio 2022
1. Open `Zoomies.sln`
2. Build → Rebuild Solution
3. Check Output window for errors

### From Visual Studio Code
1. Open folder in VS Code
2. Install C# extension if prompted
3. Terminal → Run Build Task (Ctrl+Shift+B)

### From Git Bash (Windows)
```bash
make build
# or
dotnet build -c Release
```

### From WSL (if .NET is installed)
```bash
# First, install .NET in WSL:
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 6.0.0

# Then build:
make build
```

## Verify Build Success

After building, check for:
1. **No build errors** in console output
2. **Output files** in `bin\Release\net6.0-windows\`
3. **Zoomies.exe** should exist

Test the build:
```powershell
.\build.ps1 -Target run
```

## Creating a Release Build

For a single-file executable:
```powershell
.\build.ps1 -Target package
```

This creates a self-contained exe in `build\publish\`

## Troubleshooting Commands

Check .NET version:
```powershell
dotnet --version
dotnet --list-sdks
```

Clean and rebuild:
```powershell
.\build.ps1 -Target clean
.\build.ps1 -Target build
```

Verbose build for debugging:
```powershell
dotnet build -v detailed > build.log 2>&1
```

## Still Having Issues?

1. Check the `build.log` file if you ran verbose build
2. Ensure you're in the correct directory (should see `Zoomies.csproj`)
3. Try building a simple "Hello World" .NET app to verify your setup
4. Check Windows Event Viewer for any system issues

## Build Output Structure

Successful build creates:
```
zoomies/
├── bin/
│   └── Release/
│       └── net6.0-windows/
│           ├── Zoomies.exe
│           ├── Zoomies.dll
│           └── [other dependencies]
├── obj/
│   └── [build intermediates]
└── build/
    └── publish/
        └── Zoomies.exe (single-file release)
```