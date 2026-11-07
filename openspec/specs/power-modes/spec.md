# power-modes Specification

## Purpose
Let the user switch the Windows power mode quickly between Best efficiency, Balanced and Best performance.

## Requirements

### Requirement: Three power modes
The app SHALL let the user switch to Best efficiency, Balanced or Best performance. These SHALL be the
same modes as in Windows Settings, so a change in the app shows in Settings and the other way round.

#### Scenario: Switching in the app
- **WHEN** the user switches to Best performance in the app
- **THEN** Windows Settings also shows Best performance

### Requirement: Toggle
Toggling SHALL switch to Best efficiency when the mode is Best performance, and to Best performance
otherwise, including from Balanced.

#### Scenario: Toggle from Balanced
- **WHEN** the mode is Balanced and the user toggles
- **THEN** the mode becomes Best performance

#### Scenario: Toggle from Best performance
- **WHEN** the mode is Best performance and the user toggles
- **THEN** the mode becomes Best efficiency

### Requirement: Works with any power plan
Power modes only take effect with the Balanced power plan. When another plan is active (for example
High performance or Power saver), the app SHALL show that power modes are not in effect, and switching
to any mode SHALL first change to the Balanced plan.

#### Scenario: Power saver plan active
- **WHEN** the Power saver plan is active and the user picks Best efficiency
- **THEN** the Balanced plan becomes active and the mode is Best efficiency

### Requirement: Current power source only
A switch SHALL change the mode for the current power source only (plugged in or on battery), as Windows
Settings does.

#### Scenario: Unplugging after a switch
- **WHEN** the user picks Best performance while plugged in and then unplugs
- **THEN** Windows uses the mode it remembers for battery

### Requirement: Failed switches are reported
If a switch does not take effect, the app SHALL tell the user, mention that power modes need the
Balanced power plan, and say that it may be missing on this PC.

#### Scenario: Balanced plan missing
- **WHEN** the Balanced plan has been removed and the user picks Best performance
- **THEN** an error notification says the switch failed and why
