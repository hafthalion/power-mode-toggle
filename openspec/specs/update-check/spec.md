# update-check Specification

## Purpose
Find out when a newer version of the app has been released on GitHub.

## Requirements

### Requirement: Query the latest GitHub release
The system SHALL request `https://api.github.com/repos/hafthalion/power-mode-toggle/releases/latest`
(which excludes drafts and pre-releases) with the `application/vnd.github+json` Accept header, a
`PowerModeToggle/<version>` User-Agent, TLS 1.2 enabled and a 15-second timeout. It SHALL read the
version from `tag_name` (with or without a leading `v`) and the release page from `html_url`, falling
back to the repository's `/releases/latest` page. A reply without a version SHALL count as an error.
The check SHALL run off the UI thread.

#### Scenario: Tag with v prefix
- **WHEN** the latest release's `tag_name` is `v1.7.0`
- **THEN** the latest version is 1.7.0

### Requirement: Compare versions
The system SHALL compare major.minor.build of the latest release with the running app's
`AssemblyVersion` (whose fourth part is ignored). Only a strictly greater version SHALL count as an
update.

#### Scenario: Same version
- **WHEN** the app is 1.6.8 and the latest release is v1.6.8
- **THEN** no update is available

### Requirement: Automatic checks
When `CheckForUpdates` is true (the default), the system SHALL check about 10 seconds after start and
then once every 24 hours while running. Automatic checks SHALL be silent when there is no update or
the check fails. When `CheckForUpdates` is false, no automatic check SHALL run.

#### Scenario: Offline at startup
- **WHEN** the PC is offline during the automatic check
- **THEN** nothing is shown

#### Scenario: Automatic checks turned off
- **WHEN** `CheckForUpdates=false`
- **THEN** the app makes no network request unless the user clicks "Check for updates"

### Requirement: Manual check from the menu
"Check for updates" in the tray menu SHALL check right away regardless of `CheckForUpdates` and always
answer: "No update available" with the current version, "Could not check for updates" with the error,
or the update flow (see update-install). While a check or download runs, the menu item SHALL be
disabled and read "Checking for updates..." or "Downloading version x.y.z...", and a new check SHALL
NOT start.

#### Scenario: Up to date
- **WHEN** the user clicks "Check for updates" and the app is the latest version
- **THEN** a notification "No update available" says "Version 1.6.8 is the latest."

### Requirement: Only network access
The update check and the setup download SHALL be the app's only network access.

#### Scenario: Normal use without updates
- **WHEN** the user only switches modes and update checks are off
- **THEN** the app makes no network connections
