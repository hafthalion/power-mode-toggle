# build Specification

## Purpose
Build both exes on any Windows 10/11 machine without installing an SDK.

## Requirements

### Requirement: Build with the in-box C# compiler
`build.ps1` SHALL find `csc.exe` in `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319` or
`Framework\v4.0.30319`, fail with a clear message if neither exists, and build from the repository
folder regardless of the current directory. The source SHALL compile as C# 5.

#### Scenario: No .NET Framework 4
- **WHEN** neither compiler path exists
- **THEN** the build stops with "csc.exe not found. .NET Framework 4.x is required"

### Requirement: Generate the exe icon from the app's drawing code
Each build SHALL first compile `PowerModeToggle.cs` to a temporary library, draw the Balanced tray icon
with `Icons.Draw` at 16, 20, 24, 32, 40, 48, 64, 128 and 256 px, and write them as PNG entries to
`app.ico`, so the exe icon always matches the tray icons. `app.ico` SHALL NOT be edited by hand.

#### Scenario: Icon drawing changed
- **WHEN** the bolt shape in `Icons.Draw` changes and the user builds
- **THEN** `app.ico` and the exe icon show the new shape

### Requirement: Build the app and the setup
The build SHALL produce `PowerModeToggle.exe` (winexe, optimized, AnyCPU, with `app.manifest` and
`app.ico`) and then `PowerModeToggle-Setup.exe` from `PowerModeToggle.cs` and `Setup.cs` with
`/define:SETUP`, entry point `PowerModeToggle.SetupProgram`, and the freshly built app embedded as the
resource `PowerModeToggle.exe`. Any compiler error SHALL fail the build.

#### Scenario: Successful build
- **WHEN** the user runs `powershell -ExecutionPolicy Bypass -File build.ps1`
- **THEN** both exes are written next to the script

### Requirement: Manifest
`app.manifest` SHALL request `asInvoker`, declare Windows 10/11 compatibility, and declare per-monitor
DPI awareness (`PerMonitorV2, PerMonitor`, and `true/pm` for older systems).

#### Scenario: Running without admin rights
- **WHEN** a standard user starts either exe
- **THEN** no elevation prompt appears

### Requirement: Continuous build
The Build workflow SHALL run `build.ps1` on `windows-latest` for every pull request and every push to
`main`, so compile errors show up before a release.

#### Scenario: C# 6 syntax in a pull request
- **WHEN** a pull request uses `$"..."` string interpolation
- **THEN** the Build check fails
