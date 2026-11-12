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
When the app is already installed, the setup SHALL install into the same folder without asking again,
keep the current "Start with Windows" choice (including a change made in the tray menu), close the
running app, replace it, start the new version and keep the settings. Updating an install made by a setup
before 1.7.0 SHALL leave a single entry in Settings → Apps.

#### Scenario: Update while running
- **WHEN** the app is running and the user runs a newer setup and clicks through it
- **THEN** the new version runs with the same settings

### Requirement: Uninstall
The app SHALL be removable from Settings → Apps → Installed apps. Uninstalling SHALL close the app and
remove it, its Start menu entry, its startup entry and its install folder, and SHALL let the user choose
to also delete the settings (kept by default). It SHALL do this without starting hidden command windows,
which antivirus software treats as suspicious.

#### Scenario: Keep settings
- **WHEN** the user uninstalls without choosing to delete the settings
- **THEN** a later install uses the same settings again

#### Scenario: Install folder removed
- **WHEN** the user uninstalls from Settings → Apps
- **THEN** the `Power Mode Toggle` folder in `%LOCALAPPDATA%\Programs` is gone

### Requirement: Well-known installer
The setup SHALL be built with a widely used installer (Inno Setup), not a custom exe that embeds the app,
so antivirus software doesn't flag it as a dropper.

#### Scenario: Antivirus scan
- **WHEN** an antivirus program scans `PowerModeToggle-Setup-x.y.z.exe`
- **THEN** it sees a standard Inno Setup installer

### Requirement: File details
The setup and the app SHALL show the company **Hafnium**, the product name, a description, a copyright
and the version in their file properties (Properties → Details).

#### Scenario: Checking the setup's details
- **WHEN** the user opens Properties → Details of `PowerModeToggle-Setup-x.y.z.exe`
- **THEN** it shows company Hafnium and version x.y.z

### Requirement: Portable use
The app SHALL also be offered as `PowerModeToggle.exe`, which works fully from any folder without
installing.

#### Scenario: Run from Downloads
- **WHEN** the user runs `PowerModeToggle.exe` straight from the Downloads folder
- **THEN** the app works fully
