# Project Context

## Purpose
Power Mode Toggle is a small Windows tray utility that switches the Windows power mode between
**Best efficiency** and **Best performance** (the modes in Settings → System → Power → Power mode)
with one click on the tray icon or a global hotkey. It is a single small exe that needs no
administrator rights, with an optional per-user installer, and it updates itself from GitHub releases.

These specs describe the behavior of version 1.6.8. The README is the user-facing documentation;
the specs here are the requirements the code must keep meeting.

## Capabilities
| Capability | What it covers |
|---|---|
| [power-modes](specs/power-modes/spec.md) | Reading and setting the power mode, the Balanced plan rule, toggling |
| [tray-icon](specs/tray-icon/spec.md) | Icon colors, tooltip, left-click, staying in sync, single instance |
| [tray-menu](specs/tray-menu/spec.md) | The right-click menu, its items and its dark mode |
| [hotkeys](specs/hotkeys/spec.md) | Global hotkeys, their syntax and conflicts |
| [notifications](specs/notifications/spec.md) | Toast notifications and the balloon fallback |
| [settings](specs/settings/spec.md) | `PowerModeToggle.ini`: location, keys, migration |
| [autostart](specs/autostart/spec.md) | Start with Windows |
| [dialogs](specs/dialogs/spec.md) | The About window and the shared themed dialog, light/dark and per-monitor DPI |
| [update-check](specs/update-check/spec.md) | Looking for a newer GitHub release |
| [update-install](specs/update-install/spec.md) | Downloading, verifying and starting the new setup |
| [installer](specs/installer/spec.md) | Per-user setup and uninstaller |
| [build](specs/build/spec.md) | `build.ps1`, icon generation, manifest, CI build |
| [release](specs/release/spec.md) | The GitHub Actions release workflow and release assets |

## Tech Stack
- C# on .NET Framework 4.x with Windows Forms, compiled by the `csc.exe` that ships with Windows
  (`%WINDIR%\Microsoft.NET\Framework64\v4.0.30319`). No SDK, NuGet or project file.
- Win32 and undocumented Windows APIs via P/Invoke: `powrprof.dll` (power modes and plans),
  `user32.dll` (hotkeys, icons, redraw), `uxtheme.dll` ordinals #135/#136 (dark native menus),
  `dwmapi.dll` (dark title bar, cloaking), `shcore.dll` (per-monitor DPI), `shell32.dll` (AppUserModelID).
- WinRT toast notifications through reflection (`ContentType=WindowsRuntime`).
- PowerShell build script (`build.ps1`) and GitHub Actions on `windows-latest`.

## Project Conventions

### Code Style
- **C# 5 only.** The in-box compiler rejects `$"..."` strings, `?.`, `=>` members, `nameof` and other
  newer syntax.
- Two source files: `PowerModeToggle.cs` (the app) and `Setup.cs` (installer). The installer is
  compiled together with `PowerModeToggle.cs` using `/define:SETUP`, so they share the version,
  icons and the `ThemedForm` dialog style.
- The single source of the version is `[assembly: AssemblyVersion("x.y.z.0")]` in `PowerModeToggle.cs`.
- Short comments that explain *why* (Windows quirks, undocumented APIs), not *what*.
- UI text follows Windows wording: "Best efficiency", "Balanced", "Best performance".

### Architecture Patterns
- `ApplicationContext` (`TrayApp`) with a `NotifyIcon` and a native `ContextMenu`; no main window.
- A hidden message-only `NativeWindow` receives `WM_HOTKEY`.
- A 2-second timer polls the power mode; a separate timer runs update checks.
- Network work runs on the thread pool and posts results back to the UI thread.
- Failures in non-essential features (toasts, shortcut, settings save, dark menu) are swallowed so the
  tray app keeps running; user-relevant errors are shown as notifications.

### Testing Strategy
- No automated tests. CI (`build.yml`) compiles both exes on every pull request and push to `main`.
- Behavior is verified by hand on Windows 10/11, including multi-monitor setups with different scales
  and light/dark modes.

### Git Workflow
- Every change goes through a pull request from a new branch; a branch is never reused after its PR
  is merged, and merged branches are deleted.
- The version bump can go in the same PR as the feature.
- Releases are made after merge by running the Release workflow (see [release](specs/release/spec.md)).

## Domain Context
- Windows power *modes* are "overlay schemes" on top of a power *plan*. They only take effect with the
  **Balanced** plan (`381b4222-f694-41f0-9685-ff5bb260df2e`).
- Overlay GUIDs: Best efficiency `961cc777-2547-4f9d-8174-7d86181b8a7a`, Balanced `Guid.Empty`,
  Best performance `ded574b5-45a0-4f42-8737-46345c09c238`.
- Windows remembers the mode separately for plugged in and on battery; the app changes the one for the
  current power source, as Settings does.
- Unpackaged desktop apps only get toast pop-ups if a Start menu shortcut carries their AppUserModelID.
- Hotkeys are bound to virtual keys, not characters; Ctrl+Alt acts as AltGr.

## Important Constraints
- Windows 11, or Windows 10 version 1709 or later; .NET Framework 4.x (in-box).
- No administrator rights for running, installing, updating or uninstalling.
- The only network access is the update check against `api.github.com` and downloads from the
  repository's GitHub releases.
- The exes are not code-signed.
- Manufacturer tools (Lenovo Vantage, Armoury Crate, Dell Power Manager) may override the power mode.

## External Dependencies
- GitHub REST API `GET /repos/hafthalion/power-mode-toggle/releases/latest` and release asset downloads.
- GitHub Actions (`actions/checkout@v4`, `gh` CLI on the runner) for builds and releases.
