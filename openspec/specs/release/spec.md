# release Specification

## Purpose
Publish a version as a GitHub release with both exes and their checksums, built by GitHub Actions.

## Requirements

### Requirement: Version source and triggers
The version SHALL come from `[assembly: AssemblyVersion("x.y.z.0")]` in `PowerModeToggle.cs`, shared by
the app and the setup. The Release workflow SHALL run when a tag `vX.Y.Z` is pushed, or from
**Actions → Release → Run workflow** with the inputs `version` (required), `notes` (Markdown, optional)
and `draft` (default true). Only one release run SHALL run at a time.

#### Scenario: Release from the Actions tab
- **WHEN** the user runs the workflow on `main` with version 1.7.0 and draft unchecked
- **THEN** the tag `v1.7.0` is created on the built commit and the release is published right away

### Requirement: Version check
The workflow SHALL fail without creating a release if the version does not look like `1.2.3`, if the
tag does not match `AssemblyVersion`, or, for a manual run, if the tag already exists.

#### Scenario: Forgot to bump the version
- **WHEN** the tag `v1.7.0` is pushed but `AssemblyVersion` is still 1.6.8
- **THEN** the workflow fails with a message to bump the version, commit and tag that commit

### Requirement: Build and name the assets
The workflow SHALL build with `build.ps1` on a Windows runner using Windows PowerShell, rename the setup
to `PowerModeToggle-Setup-<version>.exe`, and attach it, `PowerModeToggle.exe` and `SHA256SUMS.txt` to
the release. `SHA256SUMS.txt` SHALL be in `sha256sum` format (lowercase hash, two spaces, name, LF
endings) so `sha256sum -c` works and the app's updater can read it.

#### Scenario: Release assets
- **WHEN** the workflow releases 1.7.0
- **THEN** the release has `PowerModeToggle-Setup-1.7.0.exe`, `PowerModeToggle.exe` and `SHA256SUMS.txt`

### Requirement: Release notes
The release SHALL be titled "Power Mode Toggle <version>". Its notes SHALL start with the `notes`
input, otherwise the body of the annotated tag message (the subject if there is no body), otherwise a
"TODO" placeholder, and SHALL end with a Download section (setup recommended, exe for portable use), a
SHA-256 table of both exes, and a SmartScreen note about the unsigned exes. Tag messages created with
`--cleanup=whitespace` SHALL keep Markdown headings.

#### Scenario: Annotated tag with notes
- **WHEN** the tag message is "Power Mode Toggle 1.7.0" followed by "## Changed" and a list
- **THEN** the release notes start with "## Changed" and the list

### Requirement: Draft or published
A tag push SHALL create a draft release of the existing tag (`--verify-tag`) for the user to review and
publish. A manual run SHALL create a draft or a published release according to `draft`. Only published
releases SHALL be offered to the app's update check.

#### Scenario: Draft not yet published
- **WHEN** a draft release 1.7.0 exists and the app checks for updates
- **THEN** the app does not see 1.7.0

### Requirement: Re-running for an existing release
If a release for the tag already exists, the workflow SHALL only replace its files and leave its notes
alone, and SHALL print the new checksum table in the log for the user to copy.

#### Scenario: Re-run after a failed upload
- **WHEN** the workflow is re-run for an existing release v1.7.0
- **THEN** its three files are replaced and its notes are unchanged
