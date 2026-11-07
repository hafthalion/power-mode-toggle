# notifications Specification

## Purpose
Tell the user what happened when there is no other visible feedback, and warn about problems.

## Requirements

### Requirement: Mode notification
After a shortcut switch, the app SHALL show a short, silent Windows notification with the new mode's
name and colored icon. A new mode notification SHALL replace the previous one instead of piling up.

#### Scenario: Two quick switches
- **WHEN** the user presses the toggle shortcut twice in a row
- **THEN** only the latest mode is shown

### Requirement: Warnings and errors
Warnings, errors and update messages SHALL be shown as notifications with a title and the normal
notification sound. Errors SHALL be shown even when notifications are turned off.

#### Scenario: Error with notifications off
- **WHEN** notifications are off and a switch fails
- **THEN** the error is still shown

### Requirement: App identity
Notifications SHALL show "Power Mode Toggle" and the app icon as their sender, also for the portable
version and after the app file was moved.

#### Scenario: Portable app moved
- **WHEN** the user moves the portable app to another folder and starts it
- **THEN** notifications still appear with the app's name and icon

### Requirement: Fallback
If Windows notifications are not available, the app SHALL show the same message as a balloon at the
tray icon.

#### Scenario: Notifications unavailable
- **WHEN** Windows can't show the notification
- **THEN** a balloon with the same text appears at the tray icon
