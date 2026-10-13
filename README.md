![RW_Ver](https://img.shields.io/badge/Rimworld-1.0%20~%201.7-brightgreen)
![GitHub tag (latest SemVer)](https://img.shields.io/github/v/tag/zzzz465/ModMisMatchFormatter?label=mod%20version)
![downloadCounter](https://raster.shields.io/steam/downloads/1872244972.png?color=blue&label=Workshop&logo=steam)
![SteamSub](https://img.shields.io/steam/subscriptions/1872244972)
[![build](https://github.com/zzzz465/ModMisMatchFormatter/actions/workflows/build.yml/badge.svg)](https://github.com/zzzz465/ModMisMatchFormatter/actions/workflows/build.yml)

simple mod for comparing two mismatching mod list

https://steamcommunity.com/sharedfiles/filedetails/?id=1872244972

## Building

Requires the .NET 10 SDK.

```
dotnet build Source/ModMismatchFormatter.slnx -c Release
dotnet test --project Source/ModMismatchFormatter.Tests/ModMismatchFormatter.Tests.csproj
```

| Target | RimWorld | Output | References |
| --- | --- | --- | --- |
| `net472` | 1.6 (Unity) | `1.6/Assemblies` | `Krafs.Rimworld.Ref`, `Lib.Harmony.Ref` 2.4 |
| `net10.0` | 1.7 (Waylith) | `1.7/Assemblies` | local game files, `Lib.Harmony.Ref` 3.0 |

There is no reference package for RimWorld 1.7 yet, so the 1.7 target is only built when
`RimWorld17Dir` points at a folder containing the 1.7 `Assembly-CSharp.dll` and `UnityStubs.dll`.
It is picked up automatically from the default Steam path when that install is on the unstable
branch; otherwise set it in `Source/Local.props` (git-ignored):

```xml
<Project>
  <PropertyGroup>
    <RimWorld17Dir>D:\Games\RimWorld-1.7</RimWorld17Dir>
  </PropertyGroup>
</Project>
```

## Releasing

The assemblies are built locally and committed; CI never downloads the game.

1. Bump the version in `About/Manifest.xml` and `<Version>` in `Source/ModMismatchFormatter/ModMismatchFormatter.csproj`.
2. Build both targets (`dotnet build Source/ModMismatchFormatter.slnx -c Release`, with RimWorld 1.7
   available) and commit `1.6/Assemblies` and `1.7/Assemblies`.
3. Push a tag: `git tag v1.2.0 && git push origin v1.2.0`. Tags with a suffix (`v1.2.0-beta.1`) become pre-releases.

`.github/workflows/release.yml` then checks that the tag matches both versions, that the committed
assemblies exist (and warns if they predate the last source change), validates the XML, runs the
tests, creates a GitHub release with `ModMismatchFormatter-<tag>.zip` (mod files only), and updates
the Steam Workshop item from `About/PublishedFileId.txt` with the same files. Pre-release tags skip
the Workshop.

The Workshop description lives in [`workshop/description.bbcode`](workshop/description.bbcode) (Steam
BBCode, at most 8000 characters, no `"` or `\`). Every release replaces the Workshop description with it,
and the script reads the published text back to check it arrived unchanged. To publish only a
description change, run the workflow manually with `sync-description`. Title, preview, visibility and
tags are left alone.

steamcmd cannot set Workshop tags, and the Web API method for it needs a publisher key. When
`supportedVersions` changes, update the version tags locally with the Steamworks API instead (the same
call RimWorld's in-game uploader makes, without uploading files):

```
dotnet run --project Source/Tools/WorkshopTags -c Release -- --dry-run
dotnet run --project Source/Tools/WorkshopTags -c Release
```

Run it from the repository root with the Steam client running and logged in as the item's owner. It needs
`RimWorld17Dir` (it uses the Steamworks.NET build and `steam_api64.dll` shipped with RimWorld 1.7), sets the
tags to `Mod` + `supportedVersions` from `About/About.xml`, and reads them back from Steam. Note that
a steamcmd login with the same account elsewhere (including the release workflow) can sign the local Steam
client out.

The Workshop upload uses steamcmd with a cached login, from two repository secrets:

- `STEAM_USERNAME`: the account that owns the Workshop item.
- `STEAM_CONFIG_VDF`: base64 of `config/config.vdf` from a steamcmd that has logged in to that account.
  When it expires, log in with steamcmd again and update the secret.

Running the workflow manually (Actions → release → Run workflow) with `login-check` only checks that this login still works.

The assemblies for 1.0–1.5 are prebuilt and no longer rebuilt from source.
