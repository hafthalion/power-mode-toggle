# notifications Specification

## Purpose
Show the result of hotkey switches, warnings and errors as modern Windows notifications.

## Requirements

### Requirement: Toast notifications with the app's identity
At startup the system SHALL set the process AppUserModelID to `PowerModeToggle` and create or update the
Start menu shortcut `Power Mode Toggle.lnk` in the user's Programs folder, pointing at the running exe and
carrying that AppUserModelID, because Windows only shows toast pop-ups from unpackaged desktop apps that
have such a shortcut. The shortcut SHALL be re-created on every start so it follows the exe if it moves.
The notification header SHALL show "Power Mode Toggle" and the exe icon.

#### Scenario: Exe moved
- **WHEN** the user moves the portable exe to another folder and starts it
- **THEN** the Start menu shortcut points at the new location and notifications still appear

### Requirement: Mode notifications
A mode notification SHALL be a short toast with the mode name as text and the mode's colored bolt as
its logo, drawn as a 128 px PNG with the icon at 88 px and transparent padding. It SHALL be silent, SHALL
replace the previous mode toast instead of stacking, and SHALL expire after 15 seconds. The images SHALL
be written to `%TEMP%\PowerModeToggle\` once per run and again when missing.

#### Scenario: Two quick switches
- **WHEN** the user presses the toggle hotkey twice in a row
- **THEN** the second toast replaces the first

### Requirement: Message notifications
Warnings, errors and update messages SHALL be toasts with a title and text, the default sound, no logo,
and SHALL replace the previous message toast. Errors SHALL be shown regardless of `ShowNotifications`.

#### Scenario: Error while notifications are off
- **WHEN** `ShowNotifications=false` and a switch fails
- **THEN** the error notification is still shown

### Requirement: Balloon fallback
If toasts are unavailable or fail, the system SHALL show the same title and text as a tray balloon tip
with an info, warning or error icon.

#### Scenario: WinRT unavailable
- **WHEN** creating the toast throws
- **THEN** a tray balloon with the same text is shown
