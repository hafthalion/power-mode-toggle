# dialogs Specification

## Purpose
Show the About window, and the installer's windows, as small Windows 11 style dialogs that follow the
light/dark app mode and stay correctly sized on every monitor, whatever its display scale.

## Requirements

### Requirement: About window contents
"About Power Mode Toggle" SHALL open a dialog titled "About Power Mode Toggle" showing the app name, the
version (major.minor.build), a one-line description, the three hotkeys ("Toggle", "Best efficiency",
"Best performance", each with its key, "(unavailable)" or "off"), the settings file path, a link to the
GitHub repository that opens in the browser, and an OK button that also responds to Enter and Esc.

#### Scenario: About already open
- **WHEN** the About window is open and the user picks "About Power Mode Toggle" again
- **THEN** the existing window is brought to the front instead of opening a second one

### Requirement: Shared themed dialog layout
The About window and the installer SHALL share one dialog style: the exe icon at 48 px on the left,
the text column on the right (title in Segoe UI Semibold 14 pt, body in Segoe UI 9 pt, lines wrapped at
440 px), and buttons in a row at the bottom right with a minimum size of 96×32 px. All sizes are at
100% scale. The dialog SHALL size itself to its content, have a fixed border, no minimize or maximize
button and no title bar icon.

#### Scenario: Long settings path
- **WHEN** the settings path is longer than the text column
- **THEN** the line wraps at 440 px (scaled) and the dialog grows in height

### Requirement: Light and dark mode
The dialog SHALL follow the "app mode" (`AppsUseLightTheme`): white background with black text in light
mode; background (32,32,32), white text and a dark title bar in dark mode (DWM attribute 20, or 19 on
Windows 10 before 20H1). In dark mode buttons SHALL be flat dark gray, and checkboxes SHALL be drawn in
the Windows 11 dark style (rounded gray outline, accent fill with a dark check mark when checked).

#### Scenario: Dark app mode
- **WHEN** the app mode is dark and the user opens About
- **THEN** the dialog, its title bar and its button are dark

### Requirement: No white flash
The dialog SHALL stay cloaked (DWM cloak) from window creation until it has painted, so it never shows
a white frame first.

#### Scenario: Opening in dark mode
- **WHEN** the dialog opens in dark mode
- **THEN** it appears already dark, without a white frame first

### Requirement: Open centered on the monitor of the click
The dialog SHALL open centered in the working area of the monitor under the mouse pointer, which is the
monitor whose tray icon was clicked.

#### Scenario: Tray on the second monitor
- **WHEN** the user opens About from the taskbar on the second monitor
- **THEN** the dialog opens centered on that monitor

### Requirement: Sized for the monitor it opens on
The app SHALL be per-monitor DPI aware (PerMonitorV2, falling back to PerMonitor). The dialog SHALL
lay out its fonts, spacing, button sizes, checkbox and icon for the effective DPI of the monitor it
opens on (`GetDpiForMonitor`, falling back to the system DPI before Windows 8.1). Fonts SHALL be sized
in pixels for that monitor, not in points, because point sizes would use the primary monitor's DPI.
The icon SHALL be extracted from the exe at the exact pixel size so it stays sharp.

#### Scenario: Opening on a 150% monitor
- **WHEN** the primary monitor is at 100% and the user opens About on a second monitor at 150%
- **THEN** the text, spacing, button and icon are 1.5 times their 100% size, and the text is crisp

### Requirement: Rescale when dragged to another monitor
When the dialog moves to a monitor with another scale (`WM_DPICHANGED`), it SHALL rescale its fonts,
margins, paddings, minimum and maximum sizes, checkbox drawing and icon to the new scale, and move to
the position Windows suggests; its size SHALL come from its content. Rescaling SHALL always start from
the sizes at the scale the dialog was first laid out with, so dragging back and forth never accumulates
rounding errors. The text column and the buttons SHALL both grow and shrink, so the dialog gets smaller
on a smaller-scale monitor and returns to its exact original size when dragged back.

#### Scenario: Dragging to a larger scale
- **WHEN** the dialog opened at 100% is dragged to a monitor at 150%
- **THEN** it rescales to 150%

#### Scenario: Dragging to a smaller scale
- **WHEN** the dialog opened at 150% is dragged to a monitor at 100%
- **THEN** the text, spacing and buttons all shrink to their 100% size, with no oversized buttons left

#### Scenario: Dragging back and forth
- **WHEN** the dialog is dragged between a 100% and a 175% monitor several times
- **THEN** it has exactly its original size each time it is back on the first monitor
