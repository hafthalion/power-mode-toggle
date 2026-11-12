# updates Specification

## Purpose
Keep the app up to date with as little effort as possible.

## Requirements

### Requirement: Automatic check
When automatic updates are on (the default), the app SHALL look for a newer published release on
GitHub shortly after it starts and then once a day. Draft and pre-release versions SHALL be ignored.
When there is no update or the check fails, it SHALL show nothing.

#### Scenario: No update
- **WHEN** the app is the latest version
- **THEN** the automatic check shows nothing

#### Scenario: Offline
- **WHEN** the PC is offline during the check
- **THEN** nothing is shown

### Requirement: Check from the menu
"Check for updates" in the menu SHALL check right away, also when automatic updates are off, and
always tell the user the result: that the app is up to date, that the check failed, or that a new
version is being downloaded. While it is checking or downloading, the item SHALL say so and be disabled.

#### Scenario: Up to date
- **WHEN** the user clicks "Check for updates" and there is no newer version
- **THEN** a notification says "Version 1.6.8 is the latest."

### Requirement: Download, verify and start the setup
When a newer version is found, the app SHALL say that it is available, download that release's setup,
check it against the checksum published with the release, and start it. The user SHALL then only need to
click through the setup, which closes and replaces the running app and keeps the settings. A setup that does
not match its checksum SHALL never be started.

#### Scenario: Update found
- **WHEN** version 1.7.0 is published and the app finds it
- **THEN** the 1.7.0 setup opens, and after the user clicks through it the app runs version 1.7.0 with the same settings

#### Scenario: Update of the portable app
- **WHEN** the user runs the portable app without having installed it, and it finds version 1.7.0
- **THEN** the setup offers **Install**, closes the portable app, and from then on the installed 1.7.0 runs with the same settings

#### Scenario: Damaged download
- **WHEN** the downloaded setup doesn't match its checksum
- **THEN** it is not started

### Requirement: Fallback to the release page
If the download, the checksum check or starting the setup fails, the app SHALL say so and open the
release page so the user can download it by hand.

#### Scenario: Download fails
- **WHEN** GitHub can't be reached during the download
- **THEN** a warning is shown and the release page opens in the browser

### Requirement: Not pushy after cancelling
If the user cancels the setup, the app SHALL NOT start it again for the same version on its own while
it keeps running, and the menu SHALL offer "Install version x.y.z..." to start it later. A newer version
SHALL be offered again.

#### Scenario: Setup cancelled
- **WHEN** the user cancels the 1.7.0 setup
- **THEN** the next daily check doesn't open it again, and the menu offers "Install version 1.7.0..."

### Requirement: Privacy
Looking for and downloading updates SHALL be the app's only network access, and turning automatic
updates off SHALL stop all network access except when the user checks from the menu.

#### Scenario: Automatic updates off
- **WHEN** automatic updates are off and the user never clicks "Check for updates"
- **THEN** the app doesn't connect to the internet
