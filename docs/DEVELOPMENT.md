# Development

## Build setup

| Target | RimWorld | Output | References |
| --- | --- | --- | --- |
| `net472` | 1.6 (Unity / Mono) | `1.6/Assemblies` | `Krafs.Rimworld.Ref` 1.6.4871, `Lib.Harmony.Ref` 2.4.3 |
| `net10.0` | 1.7 (Waylith / CoreCLR) | `1.7/Assemblies` | the game's `Assembly-CSharp.dll` and `UnityStubs.dll`, `Lib.Harmony.Ref` 3.0.0-rc.2 |

There is no NuGet reference package for RimWorld 1.7 yet, so the `net10.0` target is only built when
`RimWorld17Dir` points at a folder with the 1.7 `Assembly-CSharp.dll` and `UnityStubs.dll` (the game root).
`Source/Directory.Build.props` picks up the default Steam path when that install is on the unstable branch;
otherwise set it in `Source/Local.props` (git-ignored):

```xml
<Project>
  <PropertyGroup>
    <RimWorld17Dir>D:\Games\RimWorld-1.7</RimWorld17Dir>
  </PropertyGroup>
</Project>
```

The built DLLs are committed. Builds are deterministic and do not embed the commit hash, so rebuilding
unchanged source leaves them byte-identical.

## Design notes

- **Mismatch dialog.** `MismatchDialogPatch` prefixes `ScribeMetaHeaderUtility.TryCreateDialogsForVersionMismatchWarnings`.
  It opens `ModMismatchWindow` when the mod lists differ, or when version checking is on and a mod's version
  changed; otherwise vanilla runs (and checks the game version).
- **Alignment.** `Core/ModListAligner` matches mods by package id (case-insensitive; folder name for saves made
  before 1.1) with a longest-common-subsequence table after trimming the common prefix and suffix. Mods present
  on both sides but not aligned are reported as moved. Differences too large for a 4M-cell table fall back to
  the pre-1.2 greedy algorithm, which tests keep equal to a verbatim copy of the old code (`Tests/Legacy`).
- **Mod versions in saves.** `WriteMetaHeaderPatch` transpiles `ScribeMetaHeaderUtility.WriteMetaHeader` to write
  `<meta><modMetaDatas><li><Identifier/><version/></li>…` right before the `<meta>` node closes, only while
  `GameDataSaveLoader.SaveGame` runs. `LoadGameDataHeaderPatch` reads it back from the header the loader already
  parsed. The layout matches what 1.1.x wrote, so old saves still work. Versions come from `About/Manifest.xml`,
  falling back to `<modVersion>` in `About.xml`; unknown versions are omitted, and a version only counts as
  changed when both sides are known.
- **Window.** Drawn with Verse `Widgets`/`Text` (not hand-made Unity `GUIStyle`s), one line per row, only visible
  rows are drawn.
- **Do not rename** the `ModMismatchFormatter` mod class (it is part of the settings file name,
  `Mod_<mod folder>_ModMismatchFormatter.xml`), the `Settings` class (written into that file as `Class="…"`), or the
  assembly name `ModMisMatchWindowPatch` (existing installs replace the old DLL by name).

## RimWorld 1.7 (Waylith) notes

Verified against 1.7.4976 (unstable branch, October 2026).

- The game runs on a modified .NET 10 runtime. `RimWorld.runtimeconfig.json` redirects `UnityEngine*` assemblies
  to `UnityStubs.dll`, which implements IMGUI (`GUI`, `GUIStyle`, …) on the new backend, and enables lenient
  binding, so most net472 mods keep working unchanged.
- Every Verse/RimWorld API this mod uses, and the bodies of the patched methods, are unchanged from 1.6.
- The Harmony mod ships Harmony 3.0.0-rc.2 for 1.7; the API used here (`Harmony`, `HarmonyMethod`,
  `AccessTools`, transpilers with `MoveBlocksFrom`) is unchanged.
- `LoadFolders.xml` must list every version. Without an exact entry RimWorld uses the highest listed version
  below the current one, which is how 1.6 silently loaded the 1.5 DLL before v1.2.0.
- The in-game uploader (`Workshop.BakeAndUpload`) only bakes asset packs for mods with bakeable textures; this
  mod is uploaded as-is.

## Testing in game

Unit tests cover the alignment and the transpiler helper only; the window, the patches and the save metadata
need a game run.

- **Back up your config first.** In 1.7, starting the game with `-mod=a;b;c` writes that list to
  `ModsConfig.xml`, and the 1.7 beta has only English, so `Prefs.xml` is saved with English too. Copy
  `ModsConfig.xml`, `Prefs.xml` and `Knowledge.xml` from
  `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Config` and restore them afterwards.
- Use a copy of a save when testing an older release: 1.1.x re-writes the save file when it reads versions.
- To test a build without clashing with the subscribed Workshop copy, install it under another folder and change
  its `packageId` (for example `Madeline.ModMismatchFormatter.Dev`).
- `-quicktest` starts a small game directly, handy for checking what a save writes.
- The log is `…\RimWorld by Ludeon Studios\Logs\Player.log`.
- Driving 1.7 with synthetic input (UI automation) is unreliable: clicks only register when held for about
  0.25 s, and some buttons (main menu, window close X) ignored them entirely; keyboard input works.

## Checklist used for v1.2.0

On 1.7, with the Workshop copy: version metadata written by default, read back on load; settings window shows the
defaults and saves changes; a 1.6 save with 219 mods opens the window without loading it; search, toggles,
scrolling, tooltips and Esc work; "Load anyway" loads and closes the window; a save with identical mods and
versions loads without the window; no errors in the log. Not yet run on 1.6.
