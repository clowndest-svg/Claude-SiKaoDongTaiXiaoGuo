$target=Join-Path $PSScriptRoot 'ClaudeXiaoKe.exe'
Get-CimInstance Win32_Process -Filter "Name='ClaudeXiaoKe.exe'" | Where-Object ExecutablePath -eq $target | ForEach-Object {Stop-Process -Id $_.ProcessId}
