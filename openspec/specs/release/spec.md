# release Specification

## Purpose
Let the maintainer publish a new version in a few clicks, so users get it through the app's updates.

## Requirements

### Requirement: Start a release
The maintainer SHALL be able to release a version either from GitHub's Actions tab, entering the
version, the release notes and whether to make a draft, or by pushing a version tag. Both SHALL build
the app on GitHub, not on the maintainer's PC.

#### Scenario: Release from the Actions tab
- **WHEN** the maintainer runs the release for 1.7.0 with notes and "draft" unchecked
- **THEN** release "Power Mode Toggle 1.7.0" is published and users' apps can find it

### Requirement: Safe versioning
A release SHALL only be made when the entered version matches the app's version, and SHALL NOT replace
an existing version when started from the Actions tab.

#### Scenario: Version not bumped
- **WHEN** the maintainer releases 1.7.0 but the app's version is still 1.6.8
- **THEN** no release is created and the reason is shown

### Requirement: Release contents
Each release SHALL contain the setup `PowerModeToggle-Setup-x.y.z.exe`, the portable
`PowerModeToggle.exe`, and a checksum file. Its notes SHALL start with the maintainer's notes and end
with a download section recommending the setup, the checksums of both files, and a hint about the
Windows SmartScreen warning for unsigned apps.

#### Scenario: Release page
- **WHEN** a user opens the 1.7.0 release page
- **THEN** they see what changed, which file to download, and the checksums

### Requirement: Draft or published
A release started from a tag SHALL be created as a draft for the maintainer to review and publish.
A release from the Actions tab SHALL be a draft or published right away, as chosen. Users' apps SHALL
only see published releases.

#### Scenario: Draft
- **WHEN** a draft for 1.7.0 exists
- **THEN** users' apps don't offer 1.7.0 until it is published

### Requirement: Re-running a release
Running a release again for a version that already exists SHALL only replace its files and keep its notes.

#### Scenario: Re-run
- **WHEN** the maintainer re-runs the release for 1.7.0
- **THEN** its files are rebuilt and its notes stay as they were
