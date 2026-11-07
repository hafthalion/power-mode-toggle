# hotkeys Specification

## Purpose
Switch the power mode from anywhere with configurable global hotkeys.

## Requirements

### Requirement: Three global hotkeys
The system SHALL register three system-wide hotkeys from the settings: `ToggleHotkey` (toggle between
Best efficiency and Best performance), `EfficiencyHotkey` (Best efficiency) and `PerformanceHotkey`
(Best performance). The defaults SHALL be Ctrl+Alt+, , Ctrl+Alt+/ and Ctrl+Alt+' respectively.
Holding a hotkey down SHALL NOT repeat it (`MOD_NOREPEAT`).

#### Scenario: Default toggle hotkey
- **WHEN** the mode is Best efficiency and the user presses Ctrl+Alt+,
- **THEN** the mode becomes Best performance

### Requirement: Hotkey syntax
A hotkey SHALL be any combination of the modifiers `Ctrl`/`Control`, `Alt`, `Shift` and `Win`/`Windows`
plus one key, joined with `+`, case-insensitive. The key SHALL be a .NET `Keys` name (for example `P`,
`F9`), a single digit (`1` means `D1`), or one of the punctuation characters `, . / ; ' [ ] \ - =` and
`` ` ``, which mean the key at that position on a US keyboard. An empty value SHALL turn the hotkey off.

#### Scenario: Function key with Win
- **WHEN** `PerformanceHotkey=Win+Shift+F9`
- **THEN** Win+Shift+F9 switches to Best performance

#### Scenario: Hotkey turned off
- **WHEN** `EfficiencyHotkey=` is empty
- **THEN** no efficiency hotkey is registered and the About window lists it as "off"

### Requirement: Invalid or unavailable hotkeys
If a hotkey names an unknown key, has no key, or is already registered by another program, the system
SHALL skip it, mark it "(unavailable)" in the menu, and show one warning notification at startup that
lists every problem and tells the user to edit the settings file.

#### Scenario: Hotkey taken by another app
- **WHEN** another program already owns Ctrl+Alt+/
- **THEN** a warning "Hotkey unavailable" says it is in use and the menu shows "(unavailable)"

#### Scenario: Typo in a key name
- **WHEN** `ToggleHotkey=Ctrl+Alt+Pp`
- **THEN** the warning says the key "Pp" is unknown

### Requirement: Notify after a hotkey switch
A successful switch made with a hotkey SHALL show a notification with the new mode's name and icon,
unless `ShowNotifications` is off. Switches from the tray icon or menu SHALL NOT notify on success.

#### Scenario: Notifications off
- **WHEN** `ShowNotifications=false` and the user presses the performance hotkey
- **THEN** the mode changes and no notification is shown
