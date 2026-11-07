# update-install Specification

## Purpose
Install a newer release with as little effort as possible: download its setup, verify it and start it.

## Requirements

### Requirement: Start the update when a new version is found
When a check finds a newer version, the system SHALL show "Version x.y.z is available" / "Downloading
the setup..." and download and start its setup. An automatic check SHALL do this only once per version
per run, so cancelling the setup does not bring it back every day; a manual check SHALL always do it.
From then on the update menu item SHALL read "Install version x.y.z..." and start the download again
when clicked. Clicking a tray balloon while an update is known and nothing is running SHALL also start it.

#### Scenario: Setup cancelled
- **WHEN** the setup for 1.7.0 was started automatically and the user cancelled it
- **THEN** the next daily check does not start it again, and the menu offers "Install version 1.7.0..."

#### Scenario: Newer version appears later
- **WHEN** 1.7.0 was cancelled and the next daily check finds 1.7.1
- **THEN** the 1.7.1 setup is downloaded and started

### Requirement: Download the setup named in the checksum file
The system SHALL download `SHA256SUMS.txt` from
`https://github.com/hafthalion/power-mode-toggle/releases/download/v<version>/` (15-second timeout) and
find the line for the setup, matching both `PowerModeToggle-Setup-x.y.z.exe` (from 1.6.5) and
`PowerModeToggle-Setup.exe` (up to 1.6.4), in `sha256sum` format (hash, then a space and a space or
`*`, then the name). It SHALL then download that file from the same release (120-second timeout).
A checksum file without a setup line SHALL be an error.

#### Scenario: Versioned setup name
- **WHEN** `SHA256SUMS.txt` lists `PowerModeToggle-Setup-1.7.0.exe`
- **THEN** that file is downloaded

### Requirement: Verify the SHA-256 checksum
The system SHALL compute the SHA-256 of the downloaded setup and compare it, ignoring case, with the
hash in `SHA256SUMS.txt`. A mismatch SHALL be an error and the setup SHALL NOT be saved or run.

#### Scenario: Corrupted download
- **WHEN** the downloaded setup's hash differs from the listed one
- **THEN** the setup is not started and the error path is taken

### Requirement: Save and start the setup
A verified setup SHALL be saved as `%TEMP%\PowerModeToggle\PowerModeToggle-Setup-<version>.exe` and
started. The setup then offers its Update button, stops the running app, installs the new version and
starts it, keeping the user's settings (see installer).

#### Scenario: Successful update
- **WHEN** the setup for 1.7.0 is verified
- **THEN** it opens showing "Update", and after clicking it the tray app runs version 1.7.0 with the same settings

### Requirement: Fall back to the release page
If the download, the checksum check or starting the setup fails, the system SHALL show "Could not
install version x.y.z" with the reason and open the release page in the browser so the user can
download it by hand.

#### Scenario: GitHub unreachable during download
- **WHEN** downloading `SHA256SUMS.txt` times out
- **THEN** a warning is shown and the release page opens
