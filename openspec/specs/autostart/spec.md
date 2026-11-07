# autostart Specification

## Purpose
Start the app automatically when the user signs in to Windows.

## Requirements

### Requirement: Start with Windows
The user SHALL be able to turn "Start with Windows" on and off from the tray menu and in the setup.
When on, the app SHALL start when the user signs in. No administrator rights SHALL be needed.

#### Scenario: Turned on
- **WHEN** the user checks "Start with Windows" and signs out and in again
- **THEN** the tray icon appears without starting the app by hand

### Requirement: Follows the app
If the portable app is moved and started from its new place, "Start with Windows" SHALL start it from
the new place.

#### Scenario: App moved
- **WHEN** "Start with Windows" is on and the user moves the app to another folder and starts it there
- **THEN** the next sign-in starts the app from the new folder

### Requirement: Shows the real state
The menu SHALL show "Start with Windows" as checked whenever it is on, also after a restart.

#### Scenario: Restart
- **WHEN** "Start with Windows" is on and the app is restarted
- **THEN** the menu still shows it as checked
