param(
    [Parameter(Mandatory = $true)]
    [string]$Path,
    [Parameter(Mandatory = $true)]
    [string]$ReportDirectory
)
$ErrorActionPreference = 'Stop'
$target = (Resolve-Path -LiteralPath $Path).Path
New-Item -ItemType Directory -Path $ReportDirectory -Force | Out-Null
$reportPath = Join-Path $ReportDirectory 'defender-scan.json'
$report = [ordered]@{
    scannedAtUtc = [DateTime]::UtcNow.ToString('o')
    target = $target
    verdict = 'ScanIncomplete'
    engineVersion = $null
    signatureVersion = $null
    signaturesUpdatedAt = $null
    updateExitCode = $null
    scanExitCode = $null
    files = @()
    error = $null
    limitations = 'Microsoft Defender result for these hashes only; does not establish Google Safe Browsing approval or guarantee absence of malware.'
}
try {
    $reportPrefix = [IO.Path]::GetFullPath($ReportDirectory).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    $files = @(Get-ChildItem -LiteralPath $target -File -Recurse | Where-Object { !$_.FullName.StartsWith($reportPrefix, [StringComparison]::OrdinalIgnoreCase) })
    if ($files.Count -eq 0) { throw 'The scan target contains no files.' }
    $report.files = @($files | Sort-Object FullName | ForEach-Object {
        $signature = if ($_.Extension -in @('.exe', '.dll')) { Get-AuthenticodeSignature -LiteralPath $_.FullName } else { $null }
        [ordered]@{
            path = [IO.Path]::GetRelativePath($target, $_.FullName)
            bytes = $_.Length
            sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            authenticode = if ($signature) { $signature.Status.ToString() } else { $null }
            publisher = if ($signature.SignerCertificate) { $signature.SignerCertificate.Subject } else { $null }
        }
    })
    $service = Get-Service WinDefend
    if ($service.Status -ne 'Running') { Start-Service WinDefend }
    $candidates = @(Get-ChildItem "$env:ProgramData/Microsoft/Windows Defender/Platform/*/MpCmdRun.exe" -ErrorAction SilentlyContinue | Sort-Object FullName -Descending)
    $command = if ($candidates.Count) { $candidates[0].FullName } else { Join-Path $env:ProgramFiles 'Windows Defender/MpCmdRun.exe' }
    if (!(Test-Path -LiteralPath $command)) { throw 'Microsoft Defender command-line scanner is not installed.' }
    $updateOutput = @(& $command -SignatureUpdate 2>&1)
    $report.updateExitCode = $LASTEXITCODE
    $updateOutput | Set-Content (Join-Path $ReportDirectory 'definition-update.log') -Encoding UTF8
    if ($report.updateExitCode -ne 0) { throw "Defender definition update failed with exit code $($report.updateExitCode)." }
    $status = Get-MpComputerStatus
    $report.engineVersion = $status.AMEngineVersion
    $report.signatureVersion = $status.AntivirusSignatureVersion
    $report.signaturesUpdatedAt = $status.AntivirusSignatureLastUpdated.ToUniversalTime().ToString('o')
    if (!$status.AMServiceEnabled -or !$status.AntivirusEnabled) { throw 'Microsoft Defender reports that its antivirus engine is unavailable.' }
    if (!$status.AntivirusSignatureVersion -or !$status.AntivirusSignatureLastUpdated -or $status.AntivirusSignatureLastUpdated -lt (Get-Date).AddDays(-2)) { throw 'Defender security intelligence is missing or older than two days.' }
    # Custom read-only scan: includes archive contents, ignores exclusions, and does not remediate files.
    $scanOutput = @(& $command -Scan -ScanType 3 -File $target -DisableRemediation 2>&1)
    $report.scanExitCode = $LASTEXITCODE
    $scanOutput | Tee-Object -FilePath (Join-Path $ReportDirectory 'defender-scan.log') | Write-Output
    if ($report.scanExitCode -ne 0) { throw "Defender detected a threat or could not finish the scan (exit code $($report.scanExitCode)). See defender-scan.log." }
    # Detect any real-time quarantine or other change during scanning rather than reporting a partial scan as clean.
    foreach ($file in $report.files) {
        $fullPath = Join-Path $target $file.path
        if (!(Test-Path -LiteralPath $fullPath) -or (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).Hash.ToLowerInvariant() -ne $file.sha256) {
            throw "A file changed or disappeared during scanning: $($file.path)"
        }
    }
    $report.verdict = 'NoThreatsDetected'
    Write-Host "Microsoft Defender detected no threats in $($files.Count) files. This does not clear a Chrome warning."
} catch {
    $report.error = $_.Exception.Message
    throw
} finally {
    $report | ConvertTo-Json -Depth 6 | Set-Content $reportPath -Encoding UTF8
}
