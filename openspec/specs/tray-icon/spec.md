# tray-icon Specification

## Purpose
Show the current power mode at a glance and let the user toggle it with one click.

## Requirements

### Requirement: Icon shows the mode
The app SHALL show a lightning bolt icon in the notification area whose color shows the mode: green for
Best efficiency, orange for Best performance, blue for Balanced, and gray when a power plan other than
Balanced is active.

#### Scenario: Another plan is active
- **WHEN** the High performance power plan is active
- **THEN** the icon is gray

### Requirement: Tooltip
Hovering the icon SHALL show the current mode and that clicking toggles it, with the toggle shortcut.
When another power plan is active, the tooltip SHALL name that plan, explain that power modes only work
with the Balanced plan, and say that clicking switches to it.

#### Scenario: Hover in Best efficiency
- **WHEN** the mode is Best efficiency and the user hovers the icon
- **THEN** the tooltip reads "Power mode: Best efficiency", "Click to toggle (Ctrl+Alt+,)"

### Requirement: Click to toggle
A left-click on the icon SHALL toggle between Best efficiency and Best performance.

#### Scenario: Click from Best efficiency
- **WHEN** the mode is Best efficiency and the user clicks the icon
- **THEN** the mode becomes Best performance and the icon turns orange

### Requirement: Stays in sync
When the mode is changed elsewhere, for example in Windows Settings, the icon and tooltip SHALL update
within about 2 seconds.

#### Scenario: Mode changed in Settings
- **WHEN** the user picks Balanced in Windows Settings
- **THEN** the icon turns blue within about 2 seconds

### Requirement: One instance
Only one copy of the app SHALL run at a time. Starting it again SHALL do nothing.

#### Scenario: Started twice
- **WHEN** the app is running and the user starts it again
- **THEN** no second icon appears
