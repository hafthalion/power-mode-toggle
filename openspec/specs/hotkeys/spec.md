# hotkeys Specification

## Purpose
Let the user switch the power mode from anywhere with the keyboard.

## Requirements

### Requirement: Three shortcuts
The app SHALL provide three keyboard shortcuts that work in every program:

| Default | Action |
|---|---|
| Ctrl+Alt+, | Toggle between Best efficiency and Best performance |
| Ctrl+Alt+/ | Switch to Best efficiency |
| Ctrl+Alt+' | Switch to Best performance |

Holding a shortcut down SHALL switch only once.

#### Scenario: Toggle while in another app
- **WHEN** the user is typing in a browser and presses Ctrl+Alt+,
- **THEN** the mode toggles

### Requirement: Custom shortcuts
Each shortcut SHALL be changeable in the settings file to any combination of Ctrl, Alt, Shift and Win
plus a letter, digit, function key, other named key or punctuation key. Punctuation SHALL mean the key
at that position on a US keyboard. An empty shortcut SHALL turn it off.

#### Scenario: New shortcut
- **WHEN** the user sets the performance shortcut to Win+Shift+F9 and restarts the app
- **THEN** Win+Shift+F9 switches to Best performance

#### Scenario: Shortcut turned off
- **WHEN** the user leaves the efficiency shortcut empty
- **THEN** there is no efficiency shortcut and the About window shows it as "off"

### Requirement: Problems are reported
If a shortcut is already used by another program or is not valid, the app SHALL keep running without
it, show one warning at startup that names the problem and suggests editing the settings file, and
show it as "(unavailable)" in the menu.

#### Scenario: Shortcut in use
- **WHEN** another program already uses Ctrl+Alt+/
- **THEN** a warning says the shortcut is in use by another program

### Requirement: Feedback after a shortcut switch
A switch made with a shortcut SHALL show a notification with the new mode, unless notifications are
turned off. Switches made with the mouse SHALL NOT show one.

#### Scenario: Notifications off
- **WHEN** notifications are off and the user presses the performance shortcut
- **THEN** the mode changes without a notification
