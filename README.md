![RW_Ver](https://img.shields.io/badge/Rimworld-1.0%20~%201.7-brightgreen)
![GitHub tag (latest SemVer)](https://img.shields.io/github/v/tag/zzzz465/ModMisMatchFormatter?label=mod%20version)
![downloadCounter](https://raster.shields.io/steam/downloads/1872244972.png?color=blue&label=Workshop&logo=steam)
![SteamSub](https://img.shields.io/steam/subscriptions/1872244972)
[![release](https://github.com/zzzz465/ModMisMatchFormatter/actions/workflows/release.yml/badge.svg)](https://github.com/zzzz465/ModMisMatchFormatter/actions/workflows/release.yml)

# Better ModMismatch Window

A RimWorld mod that replaces the mod mismatch dialog with a side-by-side comparison of the mods in a save
and the mods running now, aligned row by row like a diff.

- Added, removed, reordered and version-changed mods are colored and counted.
- Search, and toggles to hide unchanged mods or load order changes.
- Optional version checking: mod versions are recorded in the save and compared on load.

Supports RimWorld 1.0 to 1.7 (1.7 runs on Ludeon's new Waylith engine) and requires Harmony.

- Steam Workshop: <https://steamcommunity.com/sharedfiles/filedetails/?id=1872244972>
- Releases (zip): <https://github.com/zzzz465/ModMisMatchFormatter/releases>

## Repository layout

| Path | Contents |
| --- | --- |
| `About/`, `Languages/`, `LoadFolders.xml` | Mod metadata and translations |
| `1.0/` … `1.7/`, `v1.1/` | Assemblies per RimWorld version (1.6 and 1.7 are built from `Source/`, older ones are prebuilt) |
| `Source/ModMismatchFormatter/` | The mod (`Core/` engine-independent logic, `Patches/` Harmony patches, `UI/` the window) |
| `Source/ModMismatchFormatter.Tests/` | Unit tests for `Core/` and the transpiler helper |
| `Source/Tools/WorkshopTags/` | Local tool that sets the Workshop version tags |
| `workshop/description.bbcode` | The Steam Workshop page text, synced on release |
| `.github/` | Release workflow and the steamcmd upload script |

## Building

Requires the .NET 10 SDK.

```
dotnet build Source/ModMismatchFormatter.slnx -c Release
dotnet test --project Source/ModMismatchFormatter.Tests/ModMismatchFormatter.Tests.csproj
```

The build writes `1.6/Assemblies` (net472, from NuGet references) and, when a RimWorld 1.7 install is
available, `1.7/Assemblies` (net10.0). See [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) for setup, design
notes and how to test in game, and [docs/RELEASING.md](docs/RELEASING.md) for publishing.
