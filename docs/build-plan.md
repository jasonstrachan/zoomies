# Zoomies Build Plan - Step by Step Implementation

## Phase 1: Project Initialization

### Step 1.1: Create .NET Project Structure
**Tasks:**
- Initialize .NET 6.0 WPF application
- Set up directory structure
- Configure project files

**Commands:**
```bash
dotnet new wpf -n Zoomies -f net6.0
mkdir -p src/{Core,UI,Native,Utils} tests build tools
```

**Verification:**
- ✓ Project builds with `dotnet build`
- ✓ No compilation errors
- ✓ Directory structure matches specification

### Step 1.2: Configure Build System
**Tasks:**
- Create Makefile with standard targets
- Set up code formatting and linting
- Configure test runner

**Makefile targets:**
```makefile
build:    # Compile application
test:     # Run unit tests  
lint:     # Run code analysis
fmt:      # Format code
clean:    # Clean build artifacts
package:  # Create release package
```

**Verification:**
- ✓ `make build` produces executable
- ✓ `make test` runs successfully (even with no tests)
- ✓ `make lint` executes code analysis
- ✓ All commands exit with code 0

## Phase 2: Core Modules Development

### Step 2.1: Screen Capture Module
**Location:** `src/Core/ScreenCapture/`

**Implementation:**
1. Create `IScreenCapture` interface
2. Implement `DesktopDuplicationCapture` class
3. Add frame buffer management
4. Implement error handling

**Files to create:**
- `IScreenCapture.cs`
- `DesktopDuplicationCapture.cs`
- `FrameBuffer.cs`
- `CaptureException.cs`

**Verification:**
- ✓ Unit tests pass for frame capture
- ✓ Performance test: < 16ms per frame
- ✓ Memory usage stable over 1000 captures
- ✓ Handles multi-monitor setups

### Step 2.2: Input Handler Module
**Location:** `src/Core/Input/`

**Implementation:**
1. Create `GlobalHookManager` class
2. Implement keyboard hook (WH_KEYBOARD_LL)
3. Implement mouse hook (WH_MOUSE_LL)
4. Add hotkey combination detection

**Files to create:**
- `GlobalHookManager.cs`
- `KeyboardHook.cs`
- `MouseHook.cs`
- `HotkeyDetector.cs`

**Verification:**
- ✓ Detects CTRL+ALT+Scroll combination
- ✓ No interference with other applications
- ✓ Hooks properly released on exit
- ✓ Works from any active window

### Step 2.3: Magnification Engine
**Location:** `src/Core/Magnification/`

**Implementation:**
1. Create magnification window controller
2. Implement zoom level calculations
3. Add smooth interpolation
4. Create rendering pipeline

**Files to create:**
- `MagnificationWindow.cs`
- `ZoomController.cs`
- `RenderPipeline.cs`
- `MagnificationSettings.cs`

**Verification:**
- ✓ Smooth zoom transitions
- ✓ No visual artifacts
- ✓ < 5% CPU usage during magnification
- ✓ Maintains 60 FPS

### Step 2.4: UI Components
**Location:** `src/UI/`

**Implementation:**
1. Create overlay window (WPF)
2. Implement system tray icon
3. Add context menu
4. Create settings dialog

**Files to create:**
- `OverlayWindow.xaml/cs`
- `SystemTrayManager.cs`
- `SettingsWindow.xaml/cs`
- `Resources/Icons.resx`

**Verification:**
- ✓ Overlay always on top
- ✓ Transparent background works
- ✓ System tray icon visible
- ✓ Settings persist between runs

## Phase 3: Integration & Testing

### Step 3.1: Component Integration
**Tasks:**
1. Wire up all modules in App.xaml.cs
2. Implement application lifecycle
3. Add error handling and logging
4. Create configuration system

**Verification:**
- ✓ All components initialize correctly
- ✓ Graceful shutdown on exit
- ✓ No memory leaks (24-hour test)
- ✓ Error recovery works

### Step 3.2: Comprehensive Testing
**Test Categories:**
1. **Unit Tests** (`tests/Unit/`)
   - Screen capture logic
   - Zoom calculations
   - Settings persistence

2. **Integration Tests** (`tests/Integration/`)
   - Windows API interactions
   - Multi-component workflows
   - Startup/shutdown sequences

3. **Performance Tests** (`tests/Performance/`)
   - Frame capture benchmarks
   - Memory usage profiling
   - CPU utilization tests

**Verification:**
- ✓ > 80% code coverage
- ✓ All tests pass
- ✓ No flaky tests
- ✓ Performance within requirements

## Phase 4: Build & Deployment

### Step 4.1: Release Configuration
**Tasks:**
1. Configure self-contained publishing
2. Set up code signing (optional)
3. Create installer project
4. Configure auto-update mechanism

**Publishing profiles:**
- `Portable`: Single .exe, no installation
- `Installer`: MSI with proper registry entries
- `Store`: Package for Microsoft Store (future)

**Verification:**
- ✓ Portable exe < 30MB
- ✓ Runs on clean Windows 10/11
- ✓ No missing dependencies
- ✓ Installer adds to Programs list

### Step 4.2: CI/CD Pipeline
**GitHub Actions workflow:**
```yaml
name: Build and Test
on: [push, pull_request]
jobs:
  build:
    - Restore dependencies
    - Build solution
    - Run tests
    - Create artifacts
    - Upload releases
```

**Verification:**
- ✓ Builds on every commit
- ✓ Tests run automatically
- ✓ Release artifacts generated
- ✓ Notifications on failure

## Phase 5: Final Validation

### Step 5.1: Acceptance Testing
**Checklist:**
- [ ] Magnification activates < 100ms
- [ ] Zero crashes in 24-hour test
- [ ] Memory stable (< 50MB idle)
- [ ] CPU usage < 5% active
- [ ] All hotkeys work globally
- [ ] System tray functions correctly
- [ ] Settings persist
- [ ] Startup with Windows works

### Step 5.2: Performance Validation
**Metrics to verify:**
- Frame latency: < 16ms (60 FPS)
- Startup time: < 2 seconds
- Memory usage: < 50MB idle
- CPU usage: < 5% magnifying

### Step 5.3: Security Audit
**Security checklist:**
- [ ] No unnecessary permissions
- [ ] No network access
- [ ] Input validation on all settings
- [ ] Secure registry access
- [ ] No sensitive data logging

## Milestone Checkpoints

### Checkpoint 1: Foundation (Week 1)
- ✓ Project structure created
- ✓ Build system operational
- ✓ Basic window displays

### Checkpoint 2: Core Features (Week 2-3)
- ✓ Screen capture working
- ✓ Input hooks functional
- ✓ Basic magnification operational

### Checkpoint 3: Integration (Week 4)
- ✓ All components integrated
- ✓ System tray working
- ✓ Settings persistence

### Checkpoint 4: Polish (Week 5)
- ✓ Performance optimized
- ✓ All tests passing
- ✓ Documentation complete

### Checkpoint 5: Release (Week 6)
- ✓ Installer created
- ✓ CI/CD operational
- ✓ First release published

## Success Criteria

**Must Have:**
- Magnification with CTRL+ALT+Scroll
- System tray integration
- < 50MB memory usage
- Zero crashes in normal use

**Should Have:**
- Smooth 60 FPS rendering
- Multiple zoom levels
- Customizable hotkeys
- Auto-start option

**Nice to Have:**
- Multiple magnification modes
- Color filters
- Screenshot capability
- Portable mode

---

*This plan follows the Research → Plan → Implement workflow with verification at each step.*