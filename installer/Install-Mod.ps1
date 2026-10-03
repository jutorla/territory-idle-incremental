<#
  Territory Idle QoL mod - installer / uninstaller.

  Patches YOUR OWN copy of the game's data.win with the scripts in ..\patches, using
  UndertaleModTool (downloaded once, SHA-256 checked). No game files are included in this mod.

  Normal use: double-click Install.bat (or Uninstall.bat) in the folder above.
  Options:    -GameDir "<path to the folder with game.exe>"   skip auto-detection
              -Yes                                            skip the "export your save" confirmation
              -Uninstall                                      restore the original data.win
              -ToolDir "<folder>"                             where UndertaleModTool is kept
#>
[CmdletBinding()]
param(
    [string]$GameDir,
    [switch]$Uninstall,
    [switch]$Yes,
    [string]$ToolDir = (Join-Path $env:LOCALAPPDATA 'TerritoryIdleMod\tools')
)

$ErrorActionPreference = 'Stop'
$ProgressPreference    = 'SilentlyContinue'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# SHA-256 of the unmodified data.win of Territory Idle VERSION 167, the version these patches are made for.
$OriginalHash = '7D375A28DC2A652D127F6B36086A5DDD8F43D5053028F5A8F3EBCA7011CAD112'
# UndertaleModTool command-line build, official release, pinned by SHA-256.
$UtmtUrl  = 'https://github.com/UnderminersTeam/UndertaleModTool/releases/download/0.9.2.0/UTMT_CLI_v0.9.2.0-Windows.zip'
$UtmtHash = 'E7573E45D107BE34F81F955C6E4AFC3C7C8F2628E5A6F307A871E3825B3DFB40'

$ModRoot  = Split-Path $PSScriptRoot -Parent
$PatchDir = Join-Path $ModRoot 'patches'

function Say([string]$Text, [string]$Color = 'Gray') { Write-Host $Text -ForegroundColor $Color }
function Get-Sha([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash }
function Test-GameDir([string]$Dir) {
    if (-not $Dir) { return $false }
    return ((Test-Path -LiteralPath (Join-Path $Dir 'game.exe')) -and (Test-Path -LiteralPath (Join-Path $Dir 'data.win')))
}

function Find-GameDir {
    # 1) next to this mod (the mod folder may sit inside the game folder), 2) Steam libraries, 3) default path
    $candidates = New-Object System.Collections.Generic.List[string]
    $candidates.Add($PSScriptRoot)
    $candidates.Add($ModRoot)
    $candidates.Add((Split-Path $ModRoot -Parent))

    $libraries = New-Object System.Collections.Generic.List[string]
    $steam = $null
    try { $steam = (Get-ItemProperty 'HKCU:\Software\Valve\Steam' -ErrorAction Stop).SteamPath } catch { }
    if ($steam) {
        $libraries.Add($steam)
        $vdf = Join-Path $steam 'steamapps\libraryfolders.vdf'
        if (Test-Path -LiteralPath $vdf) {
            foreach ($m in [regex]::Matches((Get-Content -LiteralPath $vdf -Raw), '"path"\s+"([^"]+)"')) {
                $libraries.Add(($m.Groups[1].Value -replace '\\\\', '\'))
            }
        }
    }
    $libraries.Add('C:\Program Files (x86)\Steam')
    foreach ($lib in $libraries) { $candidates.Add((Join-Path $lib 'steamapps\common\Territory Idle')) }

    foreach ($c in $candidates) {
        if (Test-GameDir $c) { return (Resolve-Path -LiteralPath $c).Path }
    }
    return $null
}

function Assert-GameClosed([string]$Dir) {
    foreach ($p in @(Get-Process -Name game -ErrorAction SilentlyContinue)) {
        $path = $null
        try { $path = $p.Path } catch { }
        if (-not $path -or $path.StartsWith($Dir, [StringComparison]::OrdinalIgnoreCase)) {
            throw 'Territory Idle is running. Close the game completely, then run this again.'
        }
    }
}

function Get-UtmtCli {
    $cli = Join-Path $ToolDir 'UndertaleModCli.exe'
    if (Test-Path -LiteralPath $cli) { return $cli }

    New-Item -ItemType Directory -Force -Path $ToolDir | Out-Null
    $zip = Join-Path $ToolDir 'utmt_cli.zip'
    Say 'Downloading UndertaleModTool (about 60 MB, one time only)...'
    Invoke-WebRequest -Uri $UtmtUrl -OutFile $zip -UseBasicParsing
    if ((Get-Sha $zip) -ne $UtmtHash) {
        Remove-Item -LiteralPath $zip -Force
        throw 'The downloaded UndertaleModTool file failed its SHA-256 check, so it was deleted. Nothing was changed.'
    }
    Say 'Unpacking...'
    Expand-Archive -LiteralPath $zip -DestinationPath $ToolDir -Force
    Remove-Item -LiteralPath $zip -Force
    if (-not (Test-Path -LiteralPath $cli)) { throw 'UndertaleModTool did not unpack correctly.' }
    return $cli
}

function Main {
    Say ''
    Say 'Territory Idle QoL mod' 'Cyan'
    Say 'Unofficial fan mod. Not affiliated with the game''s developer or publisher.'
    Say ''

    # --- locate the game ---
    if ($GameDir) {
        $game = $GameDir.Trim('"', ' ')
        if (-not (Test-GameDir $game)) { throw "'$game' does not look like the Territory Idle folder (game.exe and data.win not found)." }
        $game = (Resolve-Path -LiteralPath $game).Path
    } else {
        $game = Find-GameDir
        while (-not (Test-GameDir $game)) {
            Say 'Could not find the game automatically.' 'Yellow'
            $game = (Read-Host 'Paste the Territory Idle folder path (the folder that contains game.exe)').Trim('"', ' ')
            if (-not $game) { throw 'No game folder given.' }
        }
    }
    Say "Game folder: $game"

    $data = Join-Path $game 'data.win'
    $bak  = Join-Path $game 'data.win.bak'

    Assert-GameClosed $game

    # --- uninstall ---
    if ($Uninstall) {
        if (-not (Test-Path -LiteralPath $bak)) { throw 'No data.win.bak found, so there is nothing to restore. In Steam use: Properties > Installed Files > Verify integrity of game files.' }
        if ((Get-Sha $bak) -ne $OriginalHash) { throw 'data.win.bak is not the original file this mod was made for, so it was not restored. In Steam use: Properties > Installed Files > Verify integrity of game files.' }
        Copy-Item -LiteralPath $bak -Destination $data -Force
        Say 'Done: the original data.win is restored. The mod is removed.' 'Green'
        return
    }

    # --- confirm the save was exported ---
    if (-not $Yes) {
        Say 'Before continuing, export your save in the game (Options > Export Save, or Save/Load to PC).' 'Yellow'
        $answer = Read-Host 'Type YES to continue'
        if ($answer.Trim().ToUpper() -ne 'YES') { Say 'Cancelled. Nothing was changed.'; return }
    }

    # --- backup / source of truth: always patch from the ORIGINAL data.win ---
    if (Test-Path -LiteralPath $bak) {
        if ((Get-Sha $bak) -ne $OriginalHash) {
            throw 'data.win.bak exists but is not the original file this mod was made for (Territory Idle VERSION 167). The game may have been updated, in which case the mod needs an update too. Nothing was changed.'
        }
        Say 'Using the existing backup data.win.bak.'
    } else {
        if ((Get-Sha $data) -ne $OriginalHash) {
            throw 'Your data.win is not the unmodified Territory Idle VERSION 167 this mod was made for. If it was modified, use Steam > Properties > Installed Files > Verify integrity of game files, then run this again. If the game was updated, the mod needs an update too. Nothing was changed.'
        }
        Copy-Item -LiteralPath $data -Destination $bak
        Say 'Backed up the original: data.win.bak (keep this file!).'
    }

    # --- tool + patches ---
    $cli = Get-UtmtCli
    $patches = @(Get-ChildItem -LiteralPath $PatchDir -Filter '*.csx' | Sort-Object Name)
    if ($patches.Count -eq 0) { throw "No patch scripts found in $PatchDir" }

    $out = Join-Path $game 'data.patched.win'
    if (Test-Path -LiteralPath $out) { Remove-Item -LiteralPath $out -Force }

    $cliArgs = @('load', ('"{0}"' -f $bak))
    foreach ($p in $patches) { $cliArgs += '-s'; $cliArgs += ('"{0}"' -f $p.FullName) }
    $cliArgs += '-o'; $cliArgs += ('"{0}"' -f $out); $cliArgs += '-f'

    Say "Applying $($patches.Count) patches (about a minute)..."
    $log = Join-Path $env:TEMP 'territory-idle-mod-patch.log'
    $proc = Start-Process -FilePath $cli -ArgumentList ($cliArgs -join ' ') -Wait -PassThru -NoNewWindow `
            -RedirectStandardOutput $log -RedirectStandardError "$log.err"

    if ($proc.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $out) -or (Get-Item -LiteralPath $out).Length -lt 1MB) {
        if (Test-Path -LiteralPath $out) { Remove-Item -LiteralPath $out -Force }
        $detail = ''
        if (Test-Path -LiteralPath "$log.err") { $detail = (Get-Content -LiteralPath "$log.err" -Tail 8) -join "`n" }
        throw "Patching failed (exit code $($proc.ExitCode)). Your game was not changed. Log: $log`n$detail"
    }

    # --- swap in the patched file (same folder, so this is a quick rename) ---
    Move-Item -LiteralPath $out -Destination $data -Force
    if ((Get-Sha $data) -eq $OriginalHash) { throw 'The patched file was not installed. Run Uninstall.bat to make sure the game is back to normal.' }

    Say ''
    Say 'Done! The mod is installed.' 'Green'
    Say 'Start the game and open: Options (gear icon) > QoL Features...'
    Say 'To remove the mod later, run Uninstall.bat.'
}

$exitCode = 0
try { Main }
catch { Say ('ERROR: ' + $_.Exception.Message) 'Red'; $exitCode = 1 }
exit $exitCode
