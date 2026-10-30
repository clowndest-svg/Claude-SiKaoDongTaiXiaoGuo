$ErrorActionPreference='Stop'
$exe=Join-Path $PSScriptRoot 'ClaudeXiaoKe.exe'
$task=Get-ScheduledTask -TaskName 'ClaudeXiaoKeCompanion-ReadOnly' -ErrorAction SilentlyContinue
if($task){if($task.Actions[0].Execute -ne $exe){throw 'Task belongs to another directory; refusing to change it'};Stop-ScheduledTask -TaskName $task.TaskName;Unregister-ScheduledTask -TaskName $task.TaskName -Confirm:$false}
Get-CimInstance Win32_Process -Filter "Name='ClaudeXiaoKe.exe'" | Where-Object ExecutablePath -eq $exe | ForEach-Object {Stop-Process -Id $_.ProcessId}
Write-Output 'Only XiaoKe task/process removed. Files and settings retained; Claude and GPT companion unchanged.'
