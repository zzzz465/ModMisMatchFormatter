# Logs in to Steam with steamcmd using a cached login and updates the Workshop item.
#
# Modes:
#   LoginCheck       only logs in.
#   Upload           uploads -ContentFolder and, with -DescriptionFile, replaces the description.
#   SyncDescription  only replaces the description with -DescriptionFile.
#
# Environment (repository secrets):
#   STEAM_USERNAME    Steam account that owns the Workshop item.
#   STEAM_CONFIG_VDF  base64 of config/config.vdf from a steamcmd that has logged in to that account,
#                     so no password or Steam Guard confirmation is needed. Refresh it when it expires.
param(
    [Parameter(Mandatory)] [ValidateSet('LoginCheck', 'Upload', 'SyncDescription')] [string] $Mode,
    [string] $ContentFolder,
    [string] $DescriptionFile,
    [string] $PublishedFileIdFile = 'About/PublishedFileId.txt',
    [string] $ChangeNote = ''
)

$ErrorActionPreference = 'Stop'

if (-not $env:STEAM_USERNAME -or -not $env:STEAM_CONFIG_VDF) {
    throw 'The STEAM_USERNAME and STEAM_CONFIG_VDF secrets are required.'
}

# The VDF value is a quoted string; whether steamcmd honours escape sequences is undocumented, so refuse
# characters that would need one instead of guessing. Raw newlines are fine.
function Get-VdfSafeText([string] $Path, [int] $MaxLength) {
    $text = (Get-Content $Path -Raw -Encoding utf8) -replace "`r`n", "`n"
    $text = $text.Trim()
    if ($text -match '["\\]') { throw "$Path must not contain double quotes or backslashes (use typographic quotes)." }
    if ($text.Length -gt $MaxLength) { throw "$Path is $($text.Length) characters; the Workshop limit is $MaxLength." }
    return $text
}

$item = [ordered]@{ appid = '294100' }

if ($Mode -eq 'Upload') {
    $PublishedFileIdFile = Join-Path $ContentFolder 'About/PublishedFileId.txt'
    # Forward slashes avoid any question of backslash escaping in the VDF.
    $item.contentfolder = (Resolve-Path $ContentFolder).Path -replace '\\', '/'
    $item.changenote = $ChangeNote -replace '["\\]', "'"
}
if ($Mode -ne 'LoginCheck') {
    $item.publishedfileid = (Get-Content $PublishedFileIdFile -Raw).Trim()
    if ($DescriptionFile) {
        $item.description = Get-VdfSafeText $DescriptionFile 8000
    }
    elseif ($Mode -eq 'SyncDescription') {
        throw 'SyncDescription needs -DescriptionFile.'
    }
}

$steamArgs = @('+login', $env:STEAM_USERNAME)

if ($Mode -ne 'LoginCheck') {
    # Fields that are not listed (title, preview, visibility, tags) stay as they are on the Workshop.
    $lines = $item.GetEnumerator() | ForEach-Object { "`t`"$($_.Key)`"`t`t`"$($_.Value)`"" }
    $itemVdf = Join-Path $env:RUNNER_TEMP 'workshop_item.vdf'
    "`"workshopitem`"`n{`n$($lines -join "`n")`n}`n" | Set-Content -Path $itemVdf -Encoding utf8 -NoNewline

    Write-Host "Updating Workshop item $($item.publishedfileid): $(($item.Keys | Where-Object { $_ -notin 'appid', 'publishedfileid' }) -join ', ')"
    $steamArgs += @('+workshop_build_item', $itemVdf)
}

$steamArgs += '+quit'

$steamcmdDir = Join-Path $env:RUNNER_TEMP 'steamcmd'
New-Item -ItemType Directory -Force $steamcmdDir, "$steamcmdDir\config" | Out-Null
Invoke-WebRequest https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip -OutFile "$steamcmdDir\steamcmd.zip"
Expand-Archive "$steamcmdDir\steamcmd.zip" $steamcmdDir -Force
# The first run only updates steamcmd itself.
& "$steamcmdDir\steamcmd.exe" +quit | Out-Null

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
if ($Mode -ne 'LoginCheck' -and ($exitCode -ne 0 -or $text -match 'ERROR|FAILED' -or $text -notmatch 'Success')) {
    throw "Workshop update failed (steamcmd exit code $exitCode)."
}

if ($item.Contains('description')) {
    # How steamcmd treats newlines inside the VDF string is undocumented, so read the published text back.
    $expected = $item.description
    $actual = $null
    for ($attempt = 0; $attempt -lt 6 -and $actual -ne $expected; $attempt++) {
        if ($attempt -gt 0) { Start-Sleep -Seconds 10 }
        $details = Invoke-RestMethod -Method Post -Uri 'https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/' `
            -Body @{ itemcount = 1; 'publishedfileids[0]' = $item.publishedfileid }
        $actual = ([string]$details.response.publishedfiledetails[0].description -replace "`r`n", "`n").Trim()
    }
    if ($actual -ne $expected) {
        $at = 0
        while ($at -lt [Math]::Min($actual.Length, $expected.Length) -and $actual[$at] -eq $expected[$at]) { $at++ }
        $context = { param($s) $s.Substring($at, [Math]::Min(60, $s.Length - $at)) }
        throw "The published description differs from $DescriptionFile at character $at.`n  expected: $(& $context $expected)`n  actual:   $(& $context $actual)"
    }
    Write-Host 'Published description matches the file.'
}

Write-Host "Steam $Mode succeeded."
# steamcmd's exit code was checked above; do not let the runner fail the step on it.
$global:LASTEXITCODE = 0
