# tray-menu Specification

## Purpose
Give access to every mode and option from a right-click menu on the tray icon.

## Requirements

### Requirement: Menu items
Right-clicking the icon SHALL open a menu with:
- the three modes, with the current one marked,
- "Toggle efficiency / performance", shown bold as the click action,
- "Start with Windows" and "Show notifications", checked when on,
- "Edit settings file...",
- "Check for updates" (or "Install version x.y.z..." when an update is waiting),
- "About Power Mode Toggle",
- "Exit".

#### Scenario: Picking a mode
- **WHEN** the user picks "Balanced" in the menu
- **THEN** the mode becomes Balanced and the menu marks it next time

#### Scenario: Exit
- **WHEN** the user picks "Exit"
- **THEN** the icon disappears and the shortcuts stop working

### Requirement: Shortcuts shown
The mode items and the toggle item SHALL show their keyboard shortcut, or "(unavailable)" when the
shortcut could not be set up.

#### Scenario: Shortcut taken by another app
- **WHEN** another program already uses the Best efficiency shortcut
- **THEN** the menu shows "(unavailable)" next to Best efficiency

### Requirement: Option items
"Start with Windows" and "Show notifications" SHALL turn their option on or off and remember it.
"Edit settings file..." SHALL open the settings file in Notepad. "About Power Mode Toggle" SHALL open
the About window.

#### Scenario: Turning notifications off
- **WHEN** the user clicks "Show notifications" while it is checked
- **THEN** no notification is shown after the next shortcut switch, also after a restart

### Requirement: Windows look and dark mode
The menu SHALL look like a native Windows 11 menu and follow the light or dark Windows mode, including
when the mode changes while the app runs. On Windows versions that don't support dark menus it SHALL stay
light.

#### Scenario: Dark Windows mode
- **WHEN** the Windows mode is dark and the user right-clicks the icon
- **THEN** the menu is dark
