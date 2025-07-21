# Zoomies Build Script for Windows PowerShell
# Run this script from Windows PowerShell or PowerShell Core

param(
    [string]$Target = "build",
    [switch]$Help
)

$ErrorActionPreference = "Stop"

function Show-Help {
    Write-Host "Zoomies Build Script" -ForegroundColor Cyan
    Write-Host "===================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usage: .\build.ps1 [-Target <target>] [-Help]"
    Write-Host ""
    Write-Host "Targets:"
    Write-Host "  build    - Build the application (default)"
    Write-Host "  test     - Run unit tests"
    Write-Host "  clean    - Clean build artifacts"
    Write-Host "  package  - Create release package"
    Write-Host "  deps     - Check/install dependencies"
    Write-Host "  run      - Run the application"
    Write-Host ""
}

function Test-DotNet {
    Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
    try {
        $dotnetVersion = dotnet --version
        Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
        
        # Check if it's .NET 6.0 or higher
        $major = [int]($dotnetVersion.Split('.')[0])
        if ($major -lt 6) {
            Write-Host "✗ .NET 6.0 or higher required. Found: $dotnetVersion" -ForegroundColor Red
            Write-Host "  Download from: https://dotnet.microsoft.com/download/dotnet/6.0" -ForegroundColor Yellow
            return $false
        }
        return $true
    }
    catch {
        Write-Host "✗ .NET SDK not found!" -ForegroundColor Red
        Write-Host "  Please install .NET 6.0 SDK from:" -ForegroundColor Yellow
        Write-Host "  https://dotnet.microsoft.com/download/dotnet/6.0" -ForegroundColor Cyan
        return $false
    }
}

function Build-Project {
    Write-Host "Building Zoomies..." -ForegroundColor Yellow
    dotnet build -c Release
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Build successful!" -ForegroundColor Green
        Write-Host "  Output: bin\Release\net6.0-windows\" -ForegroundColor Gray
    } else {
        Write-Host "✗ Build failed!" -ForegroundColor Red
        exit 1
    }
}

function Test-Project {
    Write-Host "Running tests..." -ForegroundColor Yellow
    dotnet test tests\Zoomies.Tests.csproj
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ All tests passed!" -ForegroundColor Green
    } else {
        Write-Host "✗ Tests failed!" -ForegroundColor Red
        exit 1
    }
}

function Clean-Project {
    Write-Host "Cleaning build artifacts..." -ForegroundColor Yellow
    dotnet clean
    Remove-Item -Path "bin", "obj", "build" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "✓ Clean complete!" -ForegroundColor Green
}

function Package-Project {
    Write-Host "Creating release package..." -ForegroundColor Yellow
    $publishDir = "build\publish"
    
    dotnet publish -c Release -o $publishDir `
        /p:PublishSingleFile=true `
        /p:SelfContained=true `
        /p:RuntimeIdentifier=win-x64 `
        /p:PublishReadyToRun=true `
        /p:PublishTrimmed=true
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Package created successfully!" -ForegroundColor Green
        Write-Host "  Output: $publishDir\Zoomies.exe" -ForegroundColor Gray
        
        # Show file size
        $exePath = Join-Path $publishDir "Zoomies.exe"
        if (Test-Path $exePath) {
            $size = (Get-Item $exePath).Length / 1MB
            Write-Host "  Size: $([math]::Round($size, 2)) MB" -ForegroundColor Gray
        }
    } else {
        Write-Host "✗ Package creation failed!" -ForegroundColor Red
        exit 1
    }
}

function Install-Dependencies {
    Write-Host "Installing dependencies..." -ForegroundColor Yellow
    dotnet restore
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Dependencies installed!" -ForegroundColor Green
    } else {
        Write-Host "✗ Dependency installation failed!" -ForegroundColor Red
        exit 1
    }
}

function Run-Project {
    Write-Host "Running Zoomies..." -ForegroundColor Yellow
    dotnet run
}

# Main execution
if ($Help) {
    Show-Help
    exit 0
}

# Check for .NET SDK first
if (-not (Test-DotNet)) {
    exit 1
}

# Execute target
switch ($Target.ToLower()) {
    "build" { Build-Project }
    "test" { Test-Project }
    "clean" { Clean-Project }
    "package" { Package-Project }
    "deps" { Install-Dependencies }
    "run" { Run-Project }
    default {
        Write-Host "Unknown target: $Target" -ForegroundColor Red
        Show-Help
        exit 1
    }
}