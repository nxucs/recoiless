# Recoiless

Recoiless is a portable Windows accessibility utility for configuring mouse movement compensation profiles. It is built as a lightweight C# Windows Forms app with local-only profile storage, game/loadout organization, staged tuning, and hotkey-based switching.

> Recoiless is intended for personal accessibility and input-assistance use. Respect the rules and terms of service for any software you use it with.

## Features

- Portable Windows executable, no installer required.
- Local profile database stored in `profiles.xml` beside the executable.
- `profiles.xml` is intentionally ignored by Git and is not included in this repository.
- Multi-game profile database with independent loadouts per game.
- Separate Weapon 1 and Weapon 2 compensation values.
- Four timed recoil stages per profile, each with its own delay and movement values.
- Per-game variance setting for less mechanical movement.
- Optional left-click-only trigger mode.
- Configurable pause key, with double-tap resume behavior.
- F1 app enable/disable toggle.
- F2 silent save shortcut.
- Number row or numpad `1` / `2` weapon switching.
- Per-profile hotkeys with Ctrl, Alt, and Shift modifier support.
- Optional topmost app pin.
- Optional topmost recoil timer overlay with adjustable size.
- INI import support for simple recoil configuration files.
- Dark, borderless, resizable UI.

## Download

If this repository has GitHub Releases, download the latest `Recoiless.exe` from the Releases page.

You can also build from source using the steps below.

## Build From Source

Requirements:

- Windows
- .NET Framework 4.x developer tools, including `csc.exe`

Build:

```bat
build.bat
```

The output executable is written to:

```text
Recoiless.exe
```

## GitHub Actions

This repository includes a Windows build workflow at `.github/workflows/build.yml`. Each push or pull request builds the app and uploads `Recoiless.exe` as a workflow artifact.

## Profile Data

User-created profile data lives in `profiles.xml`. That file can contain personal game/loadout configuration, so it is excluded from source control by `.gitignore`.

The repository includes only a generic sample INI file:

```text
examples/example-recoil.ini
```

## Repository Layout

```text
Recoiless.cs             Main Windows Forms app
Recoiless.manifest       Requests administrator privileges
build.bat                Local Windows build script
examples/                Sample import files
.github/workflows/       GitHub Actions CI
```

## License

MIT
