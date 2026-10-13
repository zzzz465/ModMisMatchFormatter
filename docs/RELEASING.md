# Releasing

Assemblies are built locally (the 1.7 target needs game files) and committed. CI never downloads the game.

## Steps

1. Bump the version in `About/Manifest.xml` and `<Version>` in `Source/ModMismatchFormatter/ModMismatchFormatter.csproj`.
2. Build both targets with RimWorld 1.7 available (`dotnet build Source/ModMismatchFormatter.slnx -c Release`)
   and commit `1.6/Assemblies` and `1.7/Assemblies`.
3. Update the changelog in `workshop/description.bbcode`.
4. Push a tag: `git tag -a v1.2.1 -m v1.2.1 && git push origin v1.2.1`.

`.github/workflows/release.yml` then:

- checks that the tag matches `About/Manifest.xml` and the csproj version, that the committed 1.6/1.7 assemblies
  exist (warning if they were committed before the last source change), validates all XML and runs the tests;
- zips the mod files (`About`, `Languages`, `LoadFolders.xml`, `README.md`, the version folders) and creates a
  GitHub release;
- updates the Workshop item from `About/PublishedFileId.txt` with the same files and replaces its description
  with `workshop/description.bbcode`.

Tags with a suffix (`v1.2.1-beta.1`) become GitHub pre-releases and skip the Workshop.

## Steam login for CI

The Workshop job runs steamcmd with a cached login from two repository secrets:

- `STEAM_USERNAME`: the account that owns the Workshop item.
- `STEAM_CONFIG_VDF`: base64 of `config/config.vdf` from a steamcmd that has logged in to that account, so no
  password or Steam Guard confirmation is needed.

To create or refresh it, log in once with steamcmd on your PC (for example from 1Password:
`steamcmd +login (op read "op://<vault>/<item>/username") (op read "op://<vault>/<item>/password") +quit`
and confirm in the Steam mobile app), then
`[Convert]::ToBase64String([IO.File]::ReadAllBytes("<steamcmd>\config\config.vdf")) | gh secret set STEAM_CONFIG_VDF`.

Run the workflow manually with `login-check` to test the login without changing anything.

**A steamcmd login signs your desktop Steam client out.** Steam logs it as `Session Replaced` in
`Steam\logs\connection_log.txt`, the client does not reconnect by itself, and its friends and store pages still look
online. Uploads from that client (in-game uploader, the tags tool) then fail with `Not Logged On` or `No Connection`
in `Steam\logs\workshop_log.txt`. Restart Steam after a release or description sync.

## Workshop description

`workshop/description.bbcode` is the Workshop page text (Steam BBCode). It must stay under 8000 characters and must
not contain `"` or `\` (VDF escapes); use typographic quotes. Every release replaces the Workshop description with it,
and the script reads the published text back through `GetPublishedFileDetails` and fails if it differs. To publish a
description change without a release, run the workflow manually with `sync-description`.

## Workshop tags

steamcmd cannot set tags, and `IPublishedFileService/UpdateTags` needs a publisher key. The version tags
(`Mod` + `supportedVersions`) only need updating when `supportedVersions` changes. Two ways, both from a PC with
the owner's Steam client logged in:

- `dotnet run --project Source/Tools/WorkshopTags -c Release -- --dry-run`, then without `--dry-run`. Uses the
  Steamworks.NET build and `steam_api64.dll` shipped with RimWorld 1.7 (`RimWorld17Dir`), sets only the tags and
  reads them back. Not yet exercised beyond the dry run.
- RimWorld's in-game uploader (dev mode, Mods, right-click the local copy, "Update on Steam Workshop") with a local
  folder identical to the release zip. This is how the 1.7 tag was added for v1.2.0; identical files are reported as
  "No content change detected" and add no changelog entry.

## Troubleshooting

- **Workflow shows its file path instead of `release` and refuses manual runs:** the YAML does not parse. Check it
  with `uv run --no-project --with pyyaml python -c "import yaml; yaml.safe_load(open('.github/workflows/release.yml'))"`.
  A plain scalar containing `: ` was the cause once.
- **A tag push did not start the workflow** (the push printed "failed to push some refs" but the tag exists on
  GitHub): delete the remote tag (`git push origin :refs/tags/vX.Y.Z`) and push it again.
- **Workshop upload fails:** read `Steam\logs\workshop_log.txt` on the machine that uploaded; for CI, the steamcmd
  output in the job log.
