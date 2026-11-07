# power-modes Specification

## Purpose
Read and change the Windows power mode (Best efficiency, Balanced, Best performance), the same setting
as Settings → System → Power → Power mode, including when a power plan other than Balanced is active.

## Requirements

### Requirement: Read the current power mode
The system SHALL read the effective power mode with `PowerGetEffectiveOverlayScheme` and report it as
Best efficiency, Balanced or Best performance from the overlay GUID. It SHALL report the mode as
Unknown when the active power plan is not Balanced, when the call fails, or when the GUID is not one
of the three known modes.

#### Scenario: Balanced plan with Best performance
- **WHEN** the Balanced plan is active and the effective overlay is `ded574b5-45a0-4f42-8737-46345c09c238`
- **THEN** the mode reads as Best performance

#### Scenario: Another plan is active
- **WHEN** the active power plan is, for example, High performance
- **THEN** the mode reads as Unknown, because Windows ignores power modes with that plan

### Requirement: Set a power mode
The system SHALL set a mode with `PowerSetActiveOverlayScheme`. If the active plan is not Balanced, or
cannot be read, it SHALL first switch to the Balanced plan with `PowerSetActiveScheme`, and SHALL stop
with that call's error if it fails. The change SHALL apply to the current power source only (plugged in
or on battery), as Settings does.

#### Scenario: Switching from another plan
- **WHEN** the Power saver plan is active and the user picks Best efficiency
- **THEN** the system activates the Balanced plan and then sets Best efficiency

### Requirement: Toggle between efficiency and performance
Toggling SHALL switch to Best efficiency when the current mode is Best performance, and to Best
performance in every other case (Best efficiency, Balanced or Unknown).

#### Scenario: Toggle from Balanced
- **WHEN** the mode is Balanced and the user toggles
- **THEN** the mode becomes Best performance

#### Scenario: Toggle from Best performance
- **WHEN** the mode is Best performance and the user toggles
- **THEN** the mode becomes Best efficiency

### Requirement: Report failed switches
After setting a mode, the system SHALL read the mode again. If the call returned an error or the mode
read back differs from the requested one, it SHALL show an error notification "Could not switch to
<mode>" with the error code and a hint that power modes need the Balanced power plan, which may be
missing. Errors SHALL be shown even when notifications are turned off.

#### Scenario: Balanced plan missing
- **WHEN** the Balanced plan does not exist and the user picks Best performance
- **THEN** an error notification says the switch failed and that the Balanced plan may be missing

### Requirement: Name the other active plan
When the active plan is not Balanced, the system SHALL provide its friendly name (from
`PowerReadFriendlyName`), or "another" if the name cannot be read, for display in the tray tooltip.

#### Scenario: Plan name unavailable
- **WHEN** a non-Balanced plan is active and its friendly name cannot be read
- **THEN** the plan name used is "another"
