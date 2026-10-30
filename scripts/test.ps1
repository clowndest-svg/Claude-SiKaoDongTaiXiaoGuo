$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
& (Join-Path $root 'build.ps1')

$manifest = Get-Content -LiteralPath (Join-Path $root 'assets\SHA256SUMS.json') -Raw | ConvertFrom-Json
foreach ($entry in $manifest.PSObject.Properties) {
    $file = Join-Path $root ('assets\' + $entry.Name)
    if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $entry.Value) {
        throw ('Asset hash mismatch: ' + $entry.Name)
    }
}
Write-Output 'PASS six asset SHA-256 checks'

$output = Join-Path $root 'verification'
New-Item -ItemType Directory -Path $output -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'ClaudeXiaoKe.exe') -Destination $output -Force
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$wpf = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\WPF'
$references = @(
    '/r:System.Web.Extensions.dll', '/r:System.Xaml.dll',
    ('/r:' + (Join-Path $wpf 'WindowsBase.dll')),
    ('/r:' + (Join-Path $wpf 'PresentationCore.dll')),
    ('/r:' + (Join-Path $wpf 'PresentationFramework.dll')),
    ('/r:' + (Join-Path $root 'ClaudeXiaoKe.exe'))
)
foreach ($name in @('ReadOnlyCheck', 'MotionAudit')) {
    $executable = Join-Path $output ($name + '.exe')
    & $compiler /nologo /target:exe /platform:x64 @references ('/out:' + $executable) (Join-Path $root ('tests\' + $name + '.cs'))
    if ($LASTEXITCODE -ne 0) { throw ('Test build failed: ' + $name) }
    & $executable $root
    if ($LASTEXITCODE -ne 0) { throw ('Test failed: ' + $name) }
}
