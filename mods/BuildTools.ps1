param()

function Update-Definition {
    param(
        [string]$GameModsDir,
        [string]$ProjectDir,
        [string]$Version,
        [string]$ResourceList,  # semicolon-separated
        [string]$Requires       # semicolon-separated
    )

    # 1. Load Definition.json
    $file = Join-Path $ProjectDir 'Definition.json'
    $json = Get-Content $file -Raw | ConvertFrom-Json

    # 2. Update version
    $json | Add-Member -Name 'version'   -Value $Version   -MemberType NoteProperty -Force

    # 3. Update resources
    if ($ResourceList) {
            $resources = @{}

        $paths = $ResourceList -split ';' | Where-Object { $_.Trim() -ne '' }
        foreach ($p in $paths) {
            $p = $p.Trim()
            $logical = ($p -replace '[\\/]', '.')
            $resources[$logical] = $p
        }

         $json | Add-Member -Name 'resources' -Value $resources -MemberType NoteProperty -Force
    }

    # 4. Update requires
    if ($Requires) {
        $requiresDict = @{}
        $reqs = $Requires -split ';' | Where-Object { $_.Trim() -ne '' }

        foreach ($mod in $reqs) {
            # 1. Check local project folder first
            $modDir = Join-Path $(Split-Path $ProjectDir) $mod
            $defFile = Join-Path $modDir 'Definition.json'

            if (-not (Test-Path $defFile)) {
                # 2. Fallback to binary mods folder
                $modDir = Join-Path $GameModsDir $mod
                $defFile = Join-Path $modDir 'Definition.json'
            }

            if (Test-Path $defFile) {
                $modJson = Get-Content $defFile -Raw | ConvertFrom-Json
                $modVersion = if ($modJson.version) { $modJson.version } else { "unknown" }
            } else {
                $modVersion = "unknown"
                Write-Verbose "Required mod '$mod' has no Definition.json → version set to '$modVersion'"
            }

            $requiresDict[$mod] = $modVersion
        }

        $json | Add-Member -Name 'requires' -Value $requiresDict -MemberType NoteProperty -Force
    }

    # 5. Pretty-print back to disk
    $formatted = $json | ConvertTo-Json -Depth 10 | Format-Json
    Set-Content -Path $file -Value $formatted -Encoding UTF8

    Write-Output "Definition.json updated → version=$Version, resources=$($resources.Count) items"
}

# ConvertTo-Json format is wierd ... this code fixes that
function Format-Json([Parameter(Mandatory, ValueFromPipeline)][String] $json) {
  $indent = 0;
  ($json -Split '\n' |
    % {
      if ($_ -match '[\}\]]') {
        $indent--
      }
      $line = (' ' * $indent * 4) + $_.TrimStart().Replace(':  ', ': ')
      if ($_ -match '[\{\[]') {
        $indent++
      }
      $line
  }) -Join "`n"
}