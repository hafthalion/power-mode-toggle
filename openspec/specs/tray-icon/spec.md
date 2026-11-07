# tray-icon Specification

## Purpose
Show the current power mode in the notification area and let the user toggle it with a left-click.

## Requirements

### Requirement: Mode-colored lightning bolt icon
The tray icon SHALL be the app's white lightning bolt on a colored circle, drawn at runtime from the
same code used for the exe icon. The circle color SHALL show the mode: green (46,160,67) for Best
efficiency, orange (232,96,28) for Best performance, blue (0,120,212) for Balanced, and gray when the
mode is Unknown (a plan other than Balanced is active).

#### Scenario: Another plan is active
- **WHEN** the High performance plan is active
- **THEN** the tray icon is a gray bolt

### Requirement: Tooltip with mode and toggle hint
With the Balanced plan, the tooltip SHALL read "Power mode: <mode>" and "Click to toggle", followed by
the toggle hotkey in parentheses when one is registered. With another plan, it SHALL read "Power plan:
<plan name>", explain that power modes only work with the Balanced plan, and say that clicking switches
to it and toggles. The tooltip SHALL allow up to 127 characters (beyond `NotifyIcon.Text`'s 63-character
limit), shortening a long plan name with "..." so the explanation stays whole, and falling back to 63
characters if the longer tooltip cannot be set.

#### Scenario: Long plan name
- **WHEN** the active plan has a name long enough to push the tooltip past 127 characters
- **THEN** the plan name is cut and ends with "...", and the explanation is shown in full

### Requirement: Left-click toggles
A left-click on the tray icon SHALL toggle between Best efficiency and Best performance (see
power-modes), without showing a notification on success.

#### Scenario: Click from Best efficiency
- **WHEN** the mode is Best efficiency and the user left-clicks the icon
- **THEN** the mode becomes Best performance and the icon turns orange

### Requirement: Stay in sync with Windows
The system SHALL poll the power mode every 2 seconds and whenever the menu opens, and update the icon,
tooltip and menu checks when the mode or the active plan changes, including changes made in Windows
Settings or by other programs.

#### Scenario: Mode changed in Settings
- **WHEN** the user changes the power mode in Windows Settings
- **THEN** the tray icon shows the new mode within about 2 seconds

### Requirement: Single instance
Only one instance of the app SHALL run per user session, enforced by the mutex
`Local\PowerModeToggle.SingleInstance`. Starting the app again SHALL exit silently.

#### Scenario: Second start
- **WHEN** the app is running and the user starts the exe again
- **THEN** the second process exits without showing anything
