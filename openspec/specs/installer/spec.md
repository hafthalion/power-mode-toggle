# installer Specification

## Purpose
Install, update and uninstall the app for the current user without administrator rights.

## Requirements

### Requirement: Per-user setup without admin rights
`PowerModeToggle-Setup.exe` SHALL embed `PowerModeToggle.exe`, run as invoker (no elevation), share the
app's version and dialog style (see dialogs), and install into
`%LOCALAPPDATA%\Programs\Power Mode Toggle\`. Its window SHALL show the app name, version, "Installs"
or "Updates" with the install folder, a "Start with Windows" checkbox, and "Install" or "Update" plus
"Cancel" buttons.

#### Scenario: Fresh install
- **WHEN** the app is not installed and the user runs the setup
- **THEN** it shows "Installs Power Mode Toggle for your user account in:" the folder, "Start with Windows" checked, and an "Install" button

#### Scenario: Update
- **WHEN** the app is installed and the user runs a newer setup
- **THEN** it shows "Updates", an "Update" button, and "Start with Windows" checked only if autostart is currently on

### Requirement: Install or update
Installing SHALL stop any running `PowerModeToggle` process, write `PowerModeToggle.exe` and a copy of
the setup as `Uninstall.exe` to the install folder, create the uninstall entry
`HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\PowerModeToggle` (name, version, icon,
location, uninstall command `"Uninstall.exe" --uninstall`, repository URL, estimated size, no modify or
repair), set or remove the Run value to match the checkbox, and start the installed app. Settings in
`%APPDATA%` SHALL be kept. The window SHALL then say the app is installed and running, and offer only
"Close". A failure SHALL be shown in the window.

#### Scenario: Update while the app runs
- **WHEN** the app is running and the user clicks "Update"
- **THEN** the old process is stopped, the new exe replaces it and is started, and the settings are unchanged

### Requirement: Uninstall
Running `Uninstall.exe --uninstall` (or `/uninstall`), as Settings → Apps does, SHALL show an uninstall
window with an "Also delete my settings" checkbox (unchecked). Uninstalling SHALL stop the app, remove
the Run value, the uninstall entry, the Start menu shortcut, `%TEMP%\PowerModeToggle\`, the app exe,
the settings folder if chosen, and the install folder. Because `Uninstall.exe` cannot delete itself
while running, it SHALL remove the install folder shortly after it exits.

#### Scenario: Keep settings
- **WHEN** the user uninstalls without checking "Also delete my settings"
- **THEN** everything is removed except `%APPDATA%\PowerModeToggle\`

### Requirement: Portable use stays possible
`PowerModeToggle.exe` SHALL also work on its own from any folder, without the setup.

#### Scenario: Run from Downloads
- **WHEN** the user runs `PowerModeToggle.exe` straight from the Downloads folder
- **THEN** the app works fully, and its settings go to `%APPDATA%\PowerModeToggle\`
