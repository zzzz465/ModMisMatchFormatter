# Logs in to Steam with steamcmd using a cached login and, in Upload mode, updates the Workshop item.
#
# Environment (repository secrets):
#   STEAM_USERNAME    Steam account that owns the Workshop item.
#   STEAM_CONFIG_VDF  base64 of config/config.vdf from a steamcmd that has logged in to that account,
#                     so no password or Steam Guard confirmation is needed. Refresh it when it expires.
param(
    [Parameter(Mandatory)] [ValidateSet('LoginCheck', 'Upload')] [string] $Mode,
    [string] $ContentFolder,
    [string] $ChangeNote = ''
)

$ErrorActionPreference = 'Stop'

if (-not $env:STEAM_USERNAME -or -not $env:STEAM_CONFIG_VDF) {
    throw 'The STEAM_USERNAME and STEAM_CONFIG_VDF secrets are required.'
}

$steamcmdDir = Join-Path $env:RUNNER_TEMP 'steamcmd'
New-Item -ItemType Directory -Force $steamcmdDir, "$steamcmdDir\config" | Out-Null
Invoke-WebRequest https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip -OutFile "$steamcmdDir\steamcmd.zip"
Expand-Archive "$steamcmdDir\steamcmd.zip" $steamcmdDir -Force
# The first run only updates steamcmd itself.
& "$steamcmdDir\steamcmd.exe" +quit | Out-Null

$steamArgs = @('+login', $env:STEAM_USERNAME)

if ($Mode -eq 'Upload') {
    # Forward slashes avoid any question of backslash escaping in the VDF.
    $content = (Resolve-Path $ContentFolder).Path -replace '\\', '/'
    $publishedId = (Get-Content (Join-Path $ContentFolder 'About/PublishedFileId.txt') -Raw).Trim()
    $note = $ChangeNote -replace '"', "'"

    # Only the content and change note are set; title, description, preview and visibility stay as they are.
    $itemVdf = Join-Path $env:RUNNER_TEMP 'workshop_item.vdf'
    @"
"workshopitem"
{
	"appid"		"294100"
	"publishedfileid"		"$publishedId"
	"contentfolder"		"$content"
	"changenote"		"$note"
}
"@ | Set-Content -Path $itemVdf -Encoding utf8

    Write-Host "Updating Workshop item $publishedId from $content"
    $steamArgs += @('+workshop_build_item', $itemVdf)
}

$steamArgs += '+quit'

[IO.File]::WriteAllBytes("$steamcmdDir\config\config.vdf", [Convert]::FromBase64String($env:STEAM_CONFIG_VDF))
try {
    # Print each line as it arrives and keep it for the checks below.
    $output = & "$steamcmdDir\steamcmd.exe" @steamArgs | ForEach-Object { Write-Host $_; $_ }
    $exitCode = $LASTEXITCODE
}
finally {
    Remove-Item -Force "$steamcmdDir\config\config.vdf" -ErrorAction SilentlyContinue
}

$text = $output -join "`n"
if ($text -notmatch 'Waiting for user info\.\.\.OK') {
    throw 'Steam login failed. The cached login was probably rejected; log in with steamcmd locally and update STEAM_CONFIG_VDF.'
}
if ($Mode -eq 'Upload' -and ($exitCode -ne 0 -or $text -match 'ERROR|FAILED')) {
    throw "Workshop upload failed (steamcmd exit code $exitCode)."
}

Write-Host "Steam $Mode succeeded."
# steamcmd's exit code was checked above; do not let the runner fail the step on it.
$global:LASTEXITCODE = 0
