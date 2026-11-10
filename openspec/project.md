# Project Context

## Purpose
Power Mode Toggle is a small Windows tray app that switches the Windows power mode between
**Best efficiency** and **Best performance**, the same modes as in Settings → System → Power →
Power mode, with one click or a keyboard shortcut. It runs without administrator rights, can be
used as a single portable file or installed with a setup, and keeps itself up to date.

These specs describe what the app does for its users as of version 1.6.9. How it is built is
described in the README.

## Capabilities
| Capability | What the user gets |
|---|---|
| [power-modes](specs/power-modes/spec.md) | Switching between the three power modes |
| [tray-icon](specs/tray-icon/spec.md) | An icon that shows the current mode and toggles it on click |
| [tray-menu](specs/tray-menu/spec.md) | A right-click menu with every mode and option |
| [hotkeys](specs/hotkeys/spec.md) | Keyboard shortcuts that work from anywhere |
| [notifications](specs/notifications/spec.md) | Feedback after a shortcut switch, warnings and errors |
| [settings](specs/settings/spec.md) | A settings file the user can edit |
| [autostart](specs/autostart/spec.md) | Starting with Windows |
| [dialogs](specs/dialogs/spec.md) | The About window and the setup windows: look, light/dark mode, size on every monitor |
| [updates](specs/updates/spec.md) | Finding, downloading and installing new versions |
| [installer](specs/installer/spec.md) | Installing, updating and uninstalling |
| [release](specs/release/spec.md) | Publishing a new version |

## Users
People on Windows laptops and desktops who change between a quiet, battery-friendly mode and a
fast mode several times a day and don't want to open Settings each time.

## Constraints
- Windows 11, or Windows 10 version 1709 or later.
- No administrator rights are needed to run, install, update or uninstall.
- The app works the same whether it is installed or used as a portable file.
- The only network access is looking for and downloading new versions from the project's GitHub releases.
- Power modes only work with the Windows **Balanced** power plan.
- Manufacturer tools (Lenovo Vantage, Armoury Crate, Dell Power Manager and similar) can override the power mode.

## Conventions
- Wording follows Windows: "Best efficiency", "Balanced", "Best performance".
- When behavior changes, the matching spec is updated in the same pull request.
