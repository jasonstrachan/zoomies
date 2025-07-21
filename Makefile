# Zoomies Makefile for Windows development
# Run these commands from Windows side or WSL with .NET installed

.PHONY: all build test lint fmt clean package help

# Default target
all: build

# Build the application
build:
	@echo "Building Zoomies..."
	dotnet build -c Release

# Run unit tests
test:
	@echo "Running tests..."
	dotnet test tests/Zoomies.Tests.csproj

# Run code analysis
lint:
	@echo "Running code analysis..."
	dotnet format --verify-no-changes
	dotnet build -c Release /p:TreatWarningsAsErrors=true

# Format code
fmt:
	@echo "Formatting code..."
	dotnet format

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	rm -rf bin/ obj/ build/

# Create release package
package: build
	@echo "Creating release package..."
	dotnet publish -c Release -o build/publish \
		/p:PublishSingleFile=true \
		/p:SelfContained=true \
		/p:RuntimeIdentifier=win-x64 \
		/p:PublishReadyToRun=true \
		/p:PublishTrimmed=true

# Run the application (development)
run:
	@echo "Running Zoomies..."
	dotnet run

# Install development dependencies
deps:
	@echo "Installing dependencies..."
	dotnet restore

# Create installer (requires WiX or Inno Setup)
installer: package
	@echo "Creating installer..."
	@echo "TODO: Implement installer creation"

# Display help
help:
	@echo "Zoomies Build System"
	@echo "==================="
	@echo ""
	@echo "Available targets:"
	@echo "  make build    - Build the application"
	@echo "  make test     - Run unit tests"
	@echo "  make lint     - Run code analysis"
	@echo "  make fmt      - Format code"
	@echo "  make clean    - Clean build artifacts"
	@echo "  make package  - Create release package"
	@echo "  make run      - Run application (dev)"
	@echo "  make deps     - Install dependencies"
	@echo "  make help     - Show this help message"