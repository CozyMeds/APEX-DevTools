param([string]$Version)
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (!$Version) {
    [xml]$properties = Get-Content (Join-Path $repo 'Directory.Build.props')
    $Version = [string]$properties.Project.PropertyGroup.Version
}
$temporary = Join-Path $repo ('artifacts/_smoke-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporary | Out-Null
try {
    foreach ($package in @('CLI', 'GUI', 'Bundle')) {
        $folder = Join-Path $temporary $package
        $zip = Join-Path $repo "artifacts/APEX-Compiler-$package-v$Version.zip"
        Expand-Archive $zip $folder
        $manifest = Get-Content (Join-Path $folder 'manifest.json') -Raw | ConvertFrom-Json
        if ($manifest.version -ne $Version) { throw 'Manifest version mismatch.' }
        foreach ($file in $manifest.files) {
            $path = Join-Path $folder $file.name
            if ((Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $file.sha256) { throw "Manifest hash mismatch: $path" }
        }
        $cli = Join-Path $folder 'APEX.Compiler.exe'
        if (Test-Path $cli) {
            $output = Join-Path $folder 'smoke.gscc'
            & $cli (Join-Path $repo 'samples/Sample-APEX') $output
            if ($LASTEXITCODE -ne 0 -or !(Test-Path $output)) { throw "$package compilation smoke test failed." }
            & $cli --check --json (Join-Path $repo 'samples/Sample-APEX')
            if ($LASTEXITCODE -ne 0) { throw "$package check smoke test failed." }
        }
        $gui = Join-Path $folder 'APEX.Compiler.GUI.exe'
        if (Test-Path $gui) {
            $process = Start-Process $gui -PassThru
            try {
                if (!$process.WaitForInputIdle(15000) -or $process.HasExited) { throw "$package GUI failed to start." }
                $null = $process.CloseMainWindow()
                if (!$process.WaitForExit(10000)) { throw "$package GUI did not close." }
                if ($process.ExitCode -ne 0) { throw "$package GUI exited with an error." }
            } finally {
                if (!$process.HasExited) { $process.Kill(); $process.WaitForExit() }
                $process.Dispose()
            }
        }
        Write-Host "$package package passed."
    }
} finally {
    Remove-Item $temporary -Recurse -Force
}
