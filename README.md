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
  - About (version, hotkeys, settings location).
  - Exit.

  The menu is the native Windows menu and follows your light or dark Windows mode.
- **Notifications** when you switch with a hotkey, with the icon of the new mode.
- Stays in sync if you change the mode in Windows Settings; the tray icon updates within about 2 seconds.
- Portable: a single small exe with no installer and no admin rights.

## Requirements

- Windows 11, or Windows 10 version 1709 or later.
- .NET Framework 4.x, which is included with Windows 10 and 11.
- The **Balanced** power plan. Windows only applies power modes with that plan. If another plan (for example High performance or Power saver) is active, the tray icon turns gray and its tooltip names the plan and explains this; switching to a mode switches to the Balanced plan first.

## Usage

1. Put `PowerModeToggle.exe` in a folder you can write to (not `Program Files`), for example `C:\Users\<you>\Software\PowerModeToggle\`.
2. Run it. The icon appears in the notification area. If you don't see it, look under the **^** overflow arrow and drag it onto the taskbar.
3. Right-click the icon → **Start with Windows** to launch it automatically.

Only one instance runs at a time; starting it again does nothing.

## Settings

`PowerModeToggle.ini` is created next to the exe on first start (a `settings.ini` from version 1.3.0 or earlier is renamed automatically). Restart the app after editing it.

```ini
ToggleHotkey=Ctrl+Alt+,
EfficiencyHotkey=Ctrl+Alt+/
PerformanceHotkey=Ctrl+Alt+'
ShowNotifications=true
```

- **ToggleHotkey, EfficiencyHotkey, PerformanceHotkey:** any combination of `Ctrl`, `Alt`, `Shift` and `Win` plus a key name or a punctuation character, for example `Ctrl+Alt+P`, `Win+Shift+F9`, `Ctrl+Alt+1` or `Ctrl+Alt+/`. Key names follow the .NET `Keys` enumeration. Leave a hotkey empty to turn it off. If another program already uses a hotkey, the app shows a warning at startup and the menu marks it "(unavailable)".

  **Punctuation** (`,` `.` `/` `;` `'` `[` `]` `\` `-` `=` and `` ` ``) means the key at that position on a US keyboard. Windows binds hotkeys to keys, not characters, so on other layouts the key can type something else. On a German layout, for example, `/` is the **#** key and `'` is the **Ä** key.

  Windows treats **Ctrl+Alt** like **AltGr**, so a Ctrl+Alt hotkey stops AltGr on the same key from typing its character while the app runs. For example, Ctrl+Alt+E would block € on German and Czech layouts, and the default Ctrl+Alt+' blocks § on the Czech (QWERTY) layout. Pick another key if that character matters to you.
- **ShowNotifications:** `true` or `false`. Turns off the notification shown after a hotkey switch. Errors are always shown. You can also change this from the tray menu.

Settings files from version 1.3.x and earlier had a single `Hotkey` for toggling. They're updated automatically to the three hotkeys; a customized `Hotkey` is kept as the toggle hotkey.

## What it changes on your system

| What | Where | Why |
|---|---|---|
| `PowerModeToggle.ini` | Next to the exe | Your settings |
| Start menu shortcut **Power Mode Toggle** | `%APPDATA%\Microsoft\Windows\Start Menu\Programs\` | Windows only shows notification pop-ups from desktop apps that have one. It's re-created on every start, so it follows the exe if you move it. |
| Notification images | `%TEMP%\PowerModeToggle\` | Mode icons for notifications, re-created when missing |
| `PowerModeToggle` value | `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` | Only when **Start with Windows** is on. Updated on every start, so it follows the exe if you move it. |

### Uninstall

1. In the tray menu, turn off **Start with Windows**, then click **Exit**.
2. Delete the app folder, the Start menu shortcut **Power Mode Toggle** and `%TEMP%\PowerModeToggle\`.

## Building

The app builds with the C# compiler that ships with Windows, so no SDK is needed:

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

This produces `PowerModeToggle.exe` from:

| File | Purpose |
|---|---|
| `PowerModeToggle.cs` | All source code |
| `app.manifest` | Per-monitor DPI awareness, Windows 10/11 compatibility |
| `app.ico` | Exe icon, also used for the Start menu shortcut and the notification header. `build.ps1` regenerates it from the app's Balanced tray icon on every build, so don't edit it by hand. |
| `build.ps1` | Build script |

That compiler only supports **C# 5**, so newer syntax such as `$"..."` strings, `?.` and `=>` members won't compile.

## Notes

- **Undocumented power functions:** the app reads and sets the power mode with `PowerGetEffectiveOverlayScheme` and `PowerSetActiveOverlayScheme` from `powrprof.dll`. They change the same setting as the Power mode option in Settings, but Microsoft doesn't document them, so a future Windows update could change how they behave.
- **Dark menu:** the tray menu is the native Windows menu. Windows still draws a program's native menus light unless the program opts into dark mode, and that opt-in uses two undocumented `uxtheme.dll` functions. They have been stable since Windows 10 1903; on older versions the menu stays light.
- **Per power source:** Windows remembers the power mode separately for plugged-in and on-battery. The app changes the one for the current power source, just like Settings does.
- **Manufacturer tools:** apps like Lenovo Vantage, ASUS Armoury Crate or Dell Power Manager can override the power mode.
- **Unsigned exe:** the exe isn't digitally signed. If you copy it to another PC through a download or email, Windows SmartScreen may warn you. Right-click the exe → **Properties** → tick **Unblock**.
