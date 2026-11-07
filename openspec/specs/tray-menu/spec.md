# tray-menu Specification

## Purpose
Give access to every mode and option from a right-click menu on the tray icon.

## Requirements

### Requirement: Menu contents and order
Right-clicking the tray icon SHALL open a menu with, in order: a disabled "Power mode" title; radio
items "Best efficiency", "Balanced", "Best performance"; a separator; "Toggle efficiency /
performance" shown bold as the left-click action; a separator; "Start with Windows", "Show
notifications", "Edit settings file..."; a separator; the update item ("Check for updates"),
"About Power Mode Toggle" and "Exit".

#### Scenario: Picking Balanced
- **WHEN** the user picks "Balanced" in the menu
- **THEN** the mode becomes Balanced, with no notification on success

### Requirement: Current state shown when the menu opens
When the menu opens, the system SHALL refresh the mode and check the radio item of the current mode
(none when Unknown), check "Start with Windows" when the autostart entry exists, and check "Show
notifications" according to the setting.

#### Scenario: Autostart turned off elsewhere
- **WHEN** the Run entry was removed outside the app and the user opens the menu
- **THEN** "Start with Windows" is unchecked

### Requirement: Hotkeys shown as shortcuts
The mode items and the toggle item SHALL show their registered hotkey right-aligned like a shortcut,
or "(unavailable)" when the hotkey could not be registered, and nothing when the hotkey is turned off.

#### Scenario: Hotkey in use
- **WHEN** the efficiency hotkey is already used by another program
- **THEN** the "Best efficiency" item shows "(unavailable)"

### Requirement: Option items
"Start with Windows" SHALL toggle the autostart entry and show an error notification if that fails.
"Show notifications" SHALL toggle the `ShowNotifications` setting and save the settings file.
"Edit settings file..." SHALL open the settings file in Notepad. "About Power Mode Toggle" SHALL open
the About window. "Exit" SHALL remove the tray icon, unregister the hotkeys and end the app.

#### Scenario: Turning notifications off
- **WHEN** the user clicks "Show notifications" while it is checked
- **THEN** `ShowNotifications=false` is written to the settings file

### Requirement: Native menu that follows the Windows mode
The menu SHALL be a native Win32 menu (not a WinForms `ContextMenuStrip`) so it has the Windows 11 look.
Right before it opens, the system SHALL opt the process into dark or light menus to match the
"Windows mode" (`SystemUsesLightTheme`) using the undocumented `uxtheme.dll` ordinals #135
(`SetPreferredAppMode`) and #136 (`FlushMenuThemes`). On Windows versions without them the menu SHALL
stay light without errors.

#### Scenario: Dark Windows mode
- **WHEN** the Windows mode is dark and the user right-clicks the icon
- **THEN** the menu is drawn dark

#### Scenario: Theme changed while running
- **WHEN** the user switches the Windows mode from dark to light and opens the menu again
- **THEN** the menu is drawn light
