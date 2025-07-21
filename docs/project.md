# Zoomies - Windows Magnifier Application

## Project Overview

**Zoomies** is a lightweight, native Windows magnifier application designed to provide screen magnification functionality without the bloat of traditional accessibility tools. The application offers real-time screen magnification controlled via keyboard shortcuts and mouse scroll.

**User fow**

1. User input **CTRL + ALT + Scroll** detected
2. Screenshot full screen image
3. Zoom to cursor postion on screenshot while keys are pressed, in/out depending on mouse wheel directon
4. User releases ***CTRL + ALT** and screenshot dissapears

## Project ID

**Project ID:** ZOOMIES-WIN-MAG-2025

## Core Features

### 1. Screen Magnification
- Magnification of screen area around mouse cursor
- Smooth rendering with minimal latency
- Adjustable magnification levels via scroll wheel

### 2. Input Controls
- **CTRL + ALT + Scroll**: Adjust magnification level
- Global hotkey support (works from any application)
- System-wide input hooks for responsive control

### 3. System Integration
- Minimize to system tray
- Start with Windows option
- Lightweight resource usage

## Technical Stack

### Primary Technology: C# with .NET

**Framework:** Windows Presentation Foundation (WPF)
- Native Windows performance
- Direct Windows API integration
- Mature ecosystem and tooling

### Key Components

1. **Screen Capture**: Desktop Duplication API
   - Efficient frame capture
   - Minimal CPU usage
   - Real-time performance

2. **Input Handling**: Low-level Windows hooks
   - `SetWindowsHookEx` with `WH_KEYBOARD_LL` and `WH_MOUSE_LL`
   - System-wide hotkey detection

3. **Rendering**: WPF overlay window
   - Transparent, borderless window
   - Always-on-top behavior
   - Hardware-accelerated graphics

4. **System Tray**: NotifyIcon component
   - Minimal UI footprint
   - Quick access controls

## Non-Functional Requirements

### Performance
- < 50MB memory usage at idle
- < 5% CPU usage during active magnification
- < 16ms frame latency (60 FPS)

### Compatibility
- Windows 10 version 1903 and later
- Windows 11 compatible
- .NET 6.0 or later runtime

### Security
- No network access required
- No data collection
- Minimal system permissions

## Development Guidelines

This project follows the comprehensive development standards outlined in the parent CLAUDE.md, with specific emphasis on:

1. **Clean Code**: All linters must pass with zero warnings
2. **Testing**: Comprehensive unit tests for core functionality
3. **Documentation**: Clear inline documentation for Windows API calls

## Deployment

- Self-contained executable (.exe)
- Optional installer for system-wide installation
- Portable mode support

## Success Criteria

1. Magnification activates within 100ms of hotkey press
2. Zero crashes in 24-hour continuous operation
3. Memory usage remains stable over extended use
4. All accessibility standards met for magnifier applications

## Maintenance

- Quarterly security updates
- Annual feature reviews
- Community feedback integration

---

*Last Updated: 2025-07-19*