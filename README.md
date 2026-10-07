# Power Mode Toggle

A small Windows tray utility that switches the Windows power mode between **Best efficiency** and **Best performance** with one click or a global hotkey. These are the same modes as in Settings → System → Power → Power mode.

## Features

- **Tray icon** that shows the current mode: a green leaf for Best efficiency, an orange lightning bolt for Best performance, and a blue half-circle for Balanced.
- **Left-click** the icon to toggle between Best efficiency and Best performance. From Balanced, it switches to Best performance.
- **Global hotkey** (default **Ctrl+Alt+P**) does the same from anywhere.
- **Right-click menu:**
  - Pick any of the three modes.
  - Toggle.
  - Start with Windows.
  - Show notifications.
  - Edit the settings file.
  - Exit.

  The menu is the native Windows menu and follows your light or dark Windows mode.
- **Notifications** when you switch with the hotkey, with the icon of the new mode.
- Stays in sync if you change the mode in Windows Settings; the tray icon updates within about 2 seconds.
- Portable: a single small exe with no installer and no admin rights.

## Requirements

- Windows 11, or Windows 10 version 1709 or later.
- .NET Framework 4.x, which is included with Windows 10 and 11.
- The **Balanced** power plan must be active. Windows only offers power modes with that plan, so the app shows an error if another plan is selected.

## Usage

1. Put `PowerModeToggle.exe` in a folder you can write to (not `Program Files`), for example `C:\Users\<you>\Software\PowerModeToggle\`.
2. Run it. The icon appears in the notification area. If you don't see it, look under the **^** overflow arrow and drag it onto the taskbar.
3. Right-click the icon → **Start with Windows** to launch it automatically.

Only one instance runs at a time; starting it again does nothing.

## Settings

`settings.ini` is created next to the exe on first start. Restart the app after editing it.

```ini
Hotkey=Ctrl+Alt+P
ShowNotifications=true
```

- **Hotkey:** any combination of `Ctrl`, `Alt`, `Shift` and `Win` plus a key name, for example `Ctrl+Alt+P`, `Win+Shift+F9` or `Ctrl+Alt+1`. Key names follow the .NET `Keys` enumeration. If another program already uses the hotkey, the app shows a warning at startup.
- **ShowNotifications:** `true` or `false`. Turns off the notification shown after a hotkey switch. Errors are always shown. You can also change this from the tray menu.

## What it changes on your system

| What | Where | Why |
|---|---|---|
| `settings.ini` | Next to the exe | Your settings |
| Start menu shortcut **Power Mode Toggle** | `%APPDATA%\Microsoft\Windows\Start Menu\Programs\` | Windows only shows notification pop-ups from desktop apps that have one. It's re-created on every start, so it follows the exe if you move it. |
| Notification images | `%TEMP%\PowerModeToggle\` | Mode icons for notifications, re-created when missing |
| `PowerModeToggle` value | `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` | Only when **Start with Windows** is on |

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
| `app.ico` | Exe icon, also used for the Start menu shortcut and the notification header |
| `build.ps1` | Build script |

That compiler only supports **C# 5**, so newer syntax such as `$"..."` strings, `?.` and `=>` members won't compile.

## Notes

- **Undocumented power functions:** the app reads and sets the power mode with `PowerGetEffectiveOverlayScheme` and `PowerSetActiveOverlayScheme` from `powrprof.dll`. They change the same setting as the Power mode option in Settings, but Microsoft doesn't document them, so a future Windows update could change how they behave.
- **Dark menu:** the tray menu is the native Windows menu. Windows still draws a program's native menus light unless the program opts into dark mode, and that opt-in uses two undocumented `uxtheme.dll` functions. They have been stable since Windows 10 1903; on older versions the menu stays light.
- **Per power source:** Windows remembers the power mode separately for plugged-in and on-battery. The app changes the one for the current power source, just like Settings does.
- **Manufacturer tools:** apps like Lenovo Vantage, ASUS Armoury Crate or Dell Power Manager can override the power mode.
- **Unsigned exe:** the exe isn't digitally signed. If you copy it to another PC through a download or email, Windows SmartScreen may warn you. Right-click the exe → **Properties** → tick **Unblock**.
