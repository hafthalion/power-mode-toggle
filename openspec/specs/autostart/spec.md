# autostart Specification

## Purpose
Start the app automatically when the user signs in to Windows.

## Requirements

### Requirement: Start with Windows entry
"Start with Windows" SHALL be on when the value `PowerModeToggle` exists under
`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, wherever it points. Turning it on SHALL write the
quoted path of the running exe; turning it off SHALL delete the value. No administrator rights SHALL be
needed.

#### Scenario: Turning it on
- **WHEN** the user checks "Start with Windows"
- **THEN** the Run value holds `"<path to PowerModeToggle.exe>"`

### Requirement: Follow the exe
At every start, if the Run value exists but points to another path, the system SHALL update it to the
running exe.

#### Scenario: Portable exe moved
- **WHEN** autostart is on and the user moves the exe and starts it from the new folder
- **THEN** the Run value points at the new folder

#### Scenario: Restart keeps the state
- **WHEN** autostart is on and the app is restarted
- **THEN** the menu still shows "Start with Windows" as checked
