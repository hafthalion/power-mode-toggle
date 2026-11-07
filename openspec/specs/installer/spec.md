# installer Specification

## Purpose
Install, update and uninstall the app for the current user without administrator rights, while still
allowing portable use.

## Requirements

### Requirement: Setup file
Each release SHALL offer a setup named `PowerModeToggle-Setup-x.y.z.exe`, with its version in the name.

#### Scenario: Downloading 1.7.0
- **WHEN** the user opens the 1.7.0 release
- **THEN** it offers `PowerModeToggle-Setup-1.7.0.exe`

### Requirement: Install for the current user
The setup SHALL install the app for the current user only, without administrator rights, into
`%LOCALAPPDATA%\Programs\Power Mode Toggle`. It SHALL show the version and the install folder, offer
"Start with Windows" (checked for a new install), and start the app when done.

#### Scenario: Fresh install
- **WHEN** the user runs the setup and clicks Install
- **THEN** the app is installed, appears in Settings → Apps and its icon appears in the notification area

### Requirement: Update an installed app
When the app is already installed, the setup SHALL say it updates it, show an **Update** button, keep
the current "Start with Windows" choice, close the running app, replace it, start the new version and
keep the settings.

#### Scenario: Update while running
- **WHEN** the app is running and the user runs a newer setup and clicks Update
- **THEN** the new version runs with the same settings

### Requirement: Uninstall
The app SHALL be removable from Settings → Apps → Installed apps. Uninstalling SHALL close the app and
remove it, its Start menu entry and its startup entry, and SHALL let the user choose to also delete the
settings (kept by default).

#### Scenario: Keep settings
- **WHEN** the user uninstalls without choosing to delete the settings
- **THEN** a later install uses the same settings again

### Requirement: Portable use
The app SHALL also be offered as `PowerModeToggle.exe`, which works fully from any folder without
installing.

#### Scenario: Run from Downloads
- **WHEN** the user runs `PowerModeToggle.exe` straight from the Downloads folder
- **THEN** the app works fully
