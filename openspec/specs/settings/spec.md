# settings Specification

## Purpose
Keep the user's options in a plain text file in their profile, independent of where the exe is.

## Requirements

### Requirement: Settings file location
Settings SHALL be stored in `%APPDATA%\PowerModeToggle\PowerModeToggle.ini`. The file SHALL be created
with the defaults on first start. Its full path SHALL be shown in the About window, and the tray menu
SHALL open it in Notepad. Changes made by hand SHALL take effect after restarting the app.

#### Scenario: First start
- **WHEN** the app starts and no settings file exists anywhere
- **THEN** `%APPDATA%\PowerModeToggle\PowerModeToggle.ini` is created with the default values

### Requirement: Keys and defaults
The file SHALL support `ToggleHotkey` (default `Ctrl+Alt+,`), `EfficiencyHotkey` (default `Ctrl+Alt+/`),
`PerformanceHotkey` (default `Ctrl+Alt+'`), `ShowNotifications` (default `true`) and `CheckForUpdates`
(default `true`). Keys SHALL be case-insensitive; values SHALL be trimmed; blank lines, lines without
`=`, and comment lines starting with `;` or `#` SHALL be ignored. A boolean SHALL be false only for `0`
or `false` (any case). Unknown keys SHALL be ignored.

#### Scenario: Boolean spelled differently
- **WHEN** the file has `ShowNotifications=FALSE`
- **THEN** notifications after hotkey switches are off

### Requirement: Written format
When the system saves the file, it SHALL write comment lines explaining the hotkey syntax and the
update check, followed by every key with its current value. Saving SHALL NOT fail the app if the file
cannot be written.

#### Scenario: Toggle from the menu
- **WHEN** the user turns "Show notifications" off in the menu
- **THEN** the whole file is rewritten with `ShowNotifications=false` and all other values kept

### Requirement: Migration from older versions
If the file does not exist in `%APPDATA%\PowerModeToggle\`, the system SHALL move `PowerModeToggle.ini`
(before 1.5.3) or `settings.ini` (before 1.3.1) from the exe's folder there, copying it instead if it
cannot be moved. A file from 1.3.x or earlier with a single `Hotkey` key SHALL keep a customized value as
`ToggleHotkey`, while the old default `Ctrl+Alt+P` SHALL be replaced by the new default. A file that is
missing any current key SHALL be rewritten so it lists every key.

#### Scenario: Customized old hotkey
- **WHEN** an old file has `Hotkey=Ctrl+Shift+F12` and no `ToggleHotkey`
- **THEN** `ToggleHotkey=Ctrl+Shift+F12` is used and written back with the other keys

#### Scenario: Read-only exe folder
- **WHEN** an old settings file sits next to the exe in a folder the user cannot write to
- **THEN** it is copied to the new location
