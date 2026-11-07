# settings Specification

## Purpose
Keep the user's options in a simple text file that survives updates and moving the app.

## Requirements

### Requirement: Settings file
The app SHALL keep its settings in `PowerModeToggle.ini` in the user's profile
(`%APPDATA%\PowerModeToggle\`), create it with the defaults on first start, and keep it when the app is
moved, updated or reinstalled. The About window SHALL show where it is, and the menu SHALL open it.
Edits made by hand SHALL apply after restarting the app.

#### Scenario: First start
- **WHEN** the app starts for the first time
- **THEN** a settings file with the default values is created

### Requirement: Options
The settings file SHALL hold the three shortcuts, whether notifications are shown after a shortcut
switch (on by default), and whether the app looks for updates automatically (on by default). The file
SHALL explain its options in comments.

#### Scenario: Turning off automatic updates
- **WHEN** the user sets `CheckForUpdates=false` and restarts the app
- **THEN** the app no longer looks for updates by itself

### Requirement: Settings from older versions are kept
Settings from older versions SHALL be carried over: a file stored next to the app or under its old
name `settings.ini` SHALL be moved to the new place, and a customized single shortcut from versions
before 1.4 SHALL become the toggle shortcut. Missing options SHALL be added with their defaults.

#### Scenario: Upgrade from 1.3
- **WHEN** a user with a customized shortcut in a 1.3 settings file starts the new version
- **THEN** that shortcut still toggles the mode, and the file lists all current options
