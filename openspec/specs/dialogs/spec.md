# dialogs Specification

## Purpose
Show the About window as a small, clean Windows 11 style dialog that matches the light or dark mode
and looks right on every monitor. Since 1.7.0 the setup uses the standard Inno Setup wizard (see
installer), so these requirements no longer cover it.

## Requirements

### Requirement: About window
The About window SHALL show the app name and version, a one-line description, the three shortcuts
(or "off" / "(unavailable)"), where the settings file is, a link to the project page, and an OK button.
Opening About again while it is open SHALL bring the open window to the front.

#### Scenario: Opening About
- **WHEN** the user picks "About Power Mode Toggle"
- **THEN** a window shows version 1.7.0, the shortcuts and the settings file location

### Requirement: Light and dark mode
The dialogs SHALL follow the light or dark app mode of Windows, including the title bar, buttons and
checkboxes, and SHALL appear without a white flash in dark mode.

#### Scenario: Dark mode
- **WHEN** the app mode is dark and the user opens About
- **THEN** the whole window is dark from the moment it appears

### Requirement: Opens on the right monitor
A dialog SHALL open centered on the monitor with the mouse pointer. For the About window, that is the
monitor whose tray icon the user clicked.

#### Scenario: Second monitor
- **WHEN** the user opens About from the taskbar on the second monitor
- **THEN** the window opens centered on that monitor

### Requirement: Right size on every monitor
A dialog SHALL be sized for the display scale of the monitor it opens on, with sharp text and icon,
also when that monitor uses another scale than the main one.

#### Scenario: Opening on a 150% monitor
- **WHEN** the main monitor is at 100% and the user opens About on a monitor at 150%
- **THEN** the window, text and buttons are 1.5 times their 100% size and the text is sharp

### Requirement: Adjusts when moved between monitors
When a dialog is dragged to a monitor with another display scale, it SHALL resize its text, spacing,
icon and buttons for that monitor, growing on a larger scale and shrinking on a smaller one. Dragging it
back SHALL return it to exactly its original size, however often it is moved.

#### Scenario: Dragging to a smaller scale
- **WHEN** a dialog opened at 150% is dragged to a monitor at 100%
- **THEN** the text, spacing and buttons all shrink to their 100% size

#### Scenario: Dragging back and forth
- **WHEN** the dialog is dragged between a 100% and a 175% monitor several times
- **THEN** it has its original size each time it is back on the first monitor
