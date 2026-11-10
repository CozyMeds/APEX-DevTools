param(
    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+$')]
    [string]$Version
)
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
[xml]$properties = Get-Content (Join-Path $repo 'Directory.Build.props')
$sourceVersion = [string]$properties.Project.PropertyGroup.Version
if (!$Version) { $Version = $sourceVersion }
if ($Version -ne $sourceVersion) { throw 'Package version must match Directory.Build.props. Update that file and rebuild first.' }
$compilerDir = Join-Path $repo 'src/APEX.Compiler/bin/Release/net48'
$guiDir = Join-Path $repo 'src/APEX.Compiler.GUI/bin/Release/net48'
$releaseDir = Join-Path $repo 'artifacts'
$stageDir = Join-Path $releaseDir ('_staging-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $stageDir | Out-Null
try {
    $shared = @('APEX.Compiler.Core.dll', 'Irony.dll', 'apex_ps4_ops.bin')
    $cliFiles = @('APEX.Compiler.exe', 'APEX.Compiler.exe.config') + $shared
    $guiFiles = @('APEX.Compiler.GUI.exe', 'APEX.Compiler.GUI.exe.config') + $shared
    foreach ($definition in @(@{ Directory = $compilerDir; Files = $cliFiles }, @{ Directory = $guiDir; Files = $guiFiles })) {
        foreach ($file in $definition.Files) {
            $path = Join-Path $definition.Directory $file
            if (!(Test-Path $path -PathType Leaf)) { throw "Missing runtime file: $path. Build the solution in Release first." }
            if ($file -like 'APEX.*.exe' -or $file -eq 'APEX.Compiler.Core.dll') {
                if ([Diagnostics.FileVersionInfo]::GetVersionInfo($path).FileVersion -ne "$Version.0") {
                    throw "Stale binary version: $path. Rebuild the solution."
                }
            }
        }
    }
    $sourceCommit = (& git -C $repo rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Could not determine the source commit.' }
    $dirty = [bool](& git -C $repo status --porcelain)
    $assets = @(
        @{ Name = "APEX-Compiler-CLI-v$Version"; CLI = $true; GUI = $false },
        @{ Name = "APEX-Compiler-GUI-v$Version"; CLI = $false; GUI = $true },
        @{ Name = "APEX-Compiler-Bundle-v$Version"; CLI = $true; GUI = $true }
    )
    $checksums = @()
    foreach ($asset in $assets) {
        $folder = Join-Path $stageDir $asset.Name
        New-Item -ItemType Directory -Path $folder | Out-Null
        if ($asset.CLI) { foreach ($file in $cliFiles) { Copy-Item (Join-Path $compilerDir $file) $folder } }
        if ($asset.GUI) { foreach ($file in $guiFiles) { Copy-Item (Join-Path $guiDir $file) $folder } }
        foreach ($notice in @('README.md', 'CREDITS_AND_RIGHTS.md', 'THIRD_PARTY_NOTICES.md')) { Copy-Item (Join-Path $repo $notice) $folder }
        $files = @(Get-ChildItem $folder -File | Sort-Object Name | ForEach-Object {
            [ordered]@{ name = $_.Name; bytes = $_.Length; sha256 = (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
        })
        [ordered]@{
            version = $Version; sourceCommit = $sourceCommit; workingTreeDirty = $dirty
            target = 'PS4 BO3 1.33'; runtime = '.NET Framework 4.8'; files = $files
        } | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $folder 'manifest.json') -Encoding UTF8
        $zip = Join-Path $releaseDir ($asset.Name + '.zip')
        Compress-Archive -Path (Join-Path $folder '*') -DestinationPath $zip -CompressionLevel Optimal -Force
        $checksums += (Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant() + '  ' + [IO.Path]::GetFileName($zip)
        Write-Host "Created $zip"
    }
    $checksums | Set-Content (Join-Path $releaseDir 'SHA256SUMS.txt') -Encoding ASCII
} finally {
    if (Test-Path $stageDir) { Remove-Item $stageDir -Recurse -Force }
}
