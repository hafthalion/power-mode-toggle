# Power Mode Toggle

A small Windows tray utility that switches the Windows power mode between **Best efficiency** and **Best performance** with one click or a global hotkey. These are the same modes as in Settings → System → Power → Power mode.

## Features

- **Tray icon** that shows the current mode: the app's lightning bolt in **green** for Best efficiency, **orange** for Best performance and **blue** for Balanced. A **gray** bolt means a power plan other than Balanced is active.
- **Left-click** the icon to toggle between Best efficiency and Best performance. From Balanced, it switches to Best performance.
- **Global hotkeys** that work from anywhere:

  | Hotkey | Action |
  |---|---|
  | **Ctrl+Alt+,** | Toggle between Best efficiency and Best performance |
  | **Ctrl+Alt+/** | Switch to Best efficiency |
  | **Ctrl+Alt+'** | Switch to Best performance |

  The hotkeys are shown in the tray menu and can be changed in the settings file.
- **Right-click menu:**
  - Pick any of the three modes.
  - Toggle.
  - Start with Windows.
  - Show notifications.
  - Edit the settings file.
  - Check for updates.
  - About (version, hotkeys, settings location).
  - Exit.

  The menu is the native Windows menu and follows your light or dark Windows mode.
- **Notifications** when you switch with a hotkey, with the icon of the new mode.
- **Updates:** at startup and once a day, the app looks for a newer release on GitHub. When there is one, it downloads that release's setup, checks it against the release's SHA-256 checksum, and starts it. Click **Update** in the setup to install the new version; it replaces the running app and keeps your settings. If you cancel, the tray menu offers **Install version x.y.z...** to start it again. **Check for updates** in the menu checks right away. If the download or the checksum check fails, the release page opens instead.
- Stays in sync if you change the mode in Windows Settings; the tray icon updates within about 2 seconds.
- Portable: a single small exe with no installer and no admin rights.

## Requirements

- Windows 11, or Windows 10 version 1709 or later.
- .NET Framework 4.x, which is included with Windows 10 and 11.
- The **Balanced** power plan. Windows only applies power modes with that plan. If another plan (for example High performance or Power saver) is active, the tray icon turns gray and its tooltip names the plan and explains this; switching to a mode switches to the Balanced plan first.

## Install

Download **`PowerModeToggle-Setup-x.y.z.exe`** from the latest release and run it. It installs Power Mode Toggle for your user account in `%LOCALAPPDATA%\Programs\Power Mode Toggle`, without administrator rights, and starts it. Leave **Start with Windows** checked to launch it automatically. Running a newer setup later updates the app and keeps your settings.

To uninstall, use **Settings → Apps → Installed apps → Power Mode Toggle → Uninstall**. You can choose to keep or delete your settings.

### Portable use

You can also skip the installer: download `PowerModeToggle.exe`, put it in any folder and run it. Right-click the tray icon → **Start with Windows** to launch it automatically.

## Usage

The icon appears in the notification area. If you don't see it, look under the **^** overflow arrow and drag it onto the taskbar.

Only one instance runs at a time; starting it again does nothing.

## Settings

`PowerModeToggle.ini` is created in `%APPDATA%\PowerModeToggle\` on first start. Open it from the tray menu with **Edit settings file...**; its full path is also shown in the About window. Restart the app after editing it.

Before version 1.5.3 the settings file was next to the exe, and before 1.3.1 it was called `settings.ini`. An old file next to the exe is moved to the new location automatically.

```ini
ToggleHotkey=Ctrl+Alt+,
EfficiencyHotkey=Ctrl+Alt+/
PerformanceHotkey=Ctrl+Alt+'
ShowNotifications=true
CheckForUpdates=true
```

- **ToggleHotkey, EfficiencyHotkey, PerformanceHotkey:** any combination of `Ctrl`, `Alt`, `Shift` and `Win` plus a key name or a punctuation character, for example `Ctrl+Alt+P`, `Win+Shift+F9`, `Ctrl+Alt+1` or `Ctrl+Alt+/`. Key names follow the .NET `Keys` enumeration. Leave a hotkey empty to turn it off. If another program already uses a hotkey, the app shows a warning at startup and the menu marks it "(unavailable)".

  **Punctuation** (`,` `.` `/` `;` `'` `[` `]` `\` `-` `=` and `` ` ``) means the key at that position on a US keyboard. Windows binds hotkeys to keys, not characters, so on other layouts the key can type something else. On a German layout, for example, `/` is the **#** key and `'` is the **Ä** key.

  Windows treats **Ctrl+Alt** like **AltGr**, so a Ctrl+Alt hotkey stops AltGr on the same key from typing its character while the app runs. For example, Ctrl+Alt+E would block € on German and Czech layouts, and the default Ctrl+Alt+' blocks § on the Czech (QWERTY) layout. Pick another key if that character matters to you.
- **CheckForUpdates:** `true` or `false`. Turns off the automatic check (and setup download) at startup and once a day. **Check for updates** in the tray menu still works.
- **ShowNotifications:** `true` or `false`. Turns off the notification shown after a hotkey switch. Errors are always shown. You can also change this from the tray menu.

Settings files from version 1.3.x and earlier had a single `Hotkey` for toggling. They're updated automatically to the three hotkeys; a customized `Hotkey` is kept as the toggle hotkey.

## What it changes on your system

| What | Where | Why |
|---|---|---|
| `PowerModeToggle.ini` | `%APPDATA%\PowerModeToggle\` | Your settings |
| Start menu shortcut **Power Mode Toggle** | `%APPDATA%\Microsoft\Windows\Start Menu\Programs\` | Windows only shows notification pop-ups from desktop apps that have one. It's re-created on every start, so it follows the exe if you move it. |
| Notification images | `%TEMP%\PowerModeToggle\` | Mode icons for notifications, re-created when missing |
| Downloaded setup | `%TEMP%\PowerModeToggle\` | `PowerModeToggle-Setup-x.y.z.exe`, only when an update is found |
| `PowerModeToggle` value | `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` | Only when **Start with Windows** is on. Updated on every start, so it follows the exe if you move it. |
| Installed app and `Uninstall.exe` | `%LOCALAPPDATA%\Programs\Power Mode Toggle\` | Only when installed with the setup |
| **Power Mode Toggle** uninstall entry | `HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\PowerModeToggle` | Only when installed with the setup; shows the app in Settings → Apps |

### Uninstall (portable use)

1. In the tray menu, turn off **Start with Windows**, then click **Exit**.
2. Delete the exe, the settings folder `%APPDATA%\PowerModeToggle\`, the Start menu shortcut **Power Mode Toggle** and `%TEMP%\PowerModeToggle\`.

## Building

The app builds with the C# compiler that ships with Windows, so no SDK is needed:

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

This produces `PowerModeToggle.exe` and the installer `PowerModeToggle-Setup.exe` from:

| File | Purpose |
|---|---|
| `PowerModeToggle.cs` | The app |
| `Setup.cs` | The installer and uninstaller. It is built together with `PowerModeToggle.cs` (shared version, icons and dialog style) and embeds the app. |
| `app.manifest` | Per-monitor DPI awareness, Windows 10/11 compatibility |
| `app.ico` | Exe icon, also used for the Start menu shortcut and the notification header. `build.ps1` regenerates it from the app's Balanced tray icon on every build, so don't edit it by hand. |
| `build.ps1` | Build script |

The app's requirements are written down as [OpenSpec](https://github.com/Fission-AI/OpenSpec) specs in [`openspec/`](openspec/project.md), one spec per capability. Keep them in step with the code when behavior changes.

That compiler only supports **C# 5**, so newer syntax such as `$"..."` strings, `?.` and `=>` members won't compile.

## Releasing

Releases are built by GitHub Actions ([`.github/workflows/release.yml`](.github/workflows/release.yml)) on a Windows runner with the same `build.ps1`:

1. Bump the version in `[assembly: AssemblyVersion("x.y.z.0")]` at the top of `PowerModeToggle.cs` (the installer shares it) and commit.
2. Tag that commit with the same version and push the tag:

   ```powershell
   git tag -a v1.7.0 --cleanup=whitespace -m "Power Mode Toggle 1.7.0" -m "## Changed
   - What changed, in Markdown"
   git push origin v1.7.0
   ```

   The first line of the tag message is ignored; the rest becomes the top of the release notes. Keep `--cleanup=whitespace`: without it, git drops lines that start with `#`, such as Markdown headings. A tag without a message (`git tag v1.7.0`) also works and leaves a placeholder in the notes.
3. The workflow checks that the tag matches `AssemblyVersion`, builds `PowerModeToggle.exe` and `PowerModeToggle-Setup.exe`, renames the setup to `PowerModeToggle-Setup-1.7.0.exe`, and creates a **draft** release **Power Mode Toggle 1.7.0** with both exes, `SHA256SUMS.txt`, and notes that end with the download section and a SHA-256 table.
4. Open the draft under **Releases**, review or edit the notes, and click **Publish release**.

**Without pushing a tag:** after step 1, open **Actions → Release → Run workflow** on `main` and enter the version, the notes (Markdown), and whether to create a draft. The workflow creates the tag on the commit it built. If you leave **draft** unchecked, the release is published right away. It fails if the tag already exists.

If the tag doesn't match the version, the workflow fails without creating a release: delete the tag (`git push origin :refs/tags/v1.7.0` and `git tag -d v1.7.0`), fix the version and tag again. If a release for a pushed tag already exists, re-running the workflow replaces its files but leaves its notes alone, so update the checksum table from the workflow log. A run from the Actions tab can't be repeated for a version that already exists, because it stops when the tag is there; push the tag instead to rebuild the files of an existing release.

## Notes

- **Undocumented power functions:** the app reads and sets the power mode with `PowerGetEffectiveOverlayScheme` and `PowerSetActiveOverlayScheme` from `powrprof.dll`. They change the same setting as the Power mode option in Settings, but Microsoft doesn't document them, so a future Windows update could change how they behave.
- **Dark menu:** the tray menu is the native Windows menu. Windows still draws a program's native menus light unless the program opts into dark mode, and that opt-in uses two undocumented `uxtheme.dll` functions. They have been stable since Windows 10 1903; on older versions the menu stays light.
- **Per power source:** Windows remembers the power mode separately for plugged-in and on-battery. The app changes the one for the current power source, just like Settings does.
- **Manufacturer tools:** apps like Lenovo Vantage, ASUS Armoury Crate or Dell Power Manager can override the power mode.
- **Network access:** the update check is the app's only network access. It sends one request to `api.github.com` for the latest release of this repository and, when there's a newer one, downloads its setup and `SHA256SUMS.txt` from GitHub. Set `CheckForUpdates=false` to stop the automatic checks.
- **Unsigned exe:** the exe isn't digitally signed. If you copy it to another PC through a download or email, Windows SmartScreen may warn you. Right-click the exe → **Properties** → tick **Unblock**.
