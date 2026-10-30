$ErrorActionPreference='Stop'
$exe=Join-Path $PSScriptRoot 'ClaudeXiaoKe.exe'
if(!(Test-Path -LiteralPath $exe)){& (Join-Path $PSScriptRoot 'build.ps1')}
$taskName='ClaudeXiaoKeCompanion-ReadOnly'
$old=Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
if($old -and $old.Actions[0].Execute -ne $exe){throw 'Existing task belongs to another installation; use that directory'}
$who=[Security.Principal.WindowsIdentity]::GetCurrent().Name
$action=New-ScheduledTaskAction -Execute $exe -WorkingDirectory $PSScriptRoot
$trigger=New-ScheduledTaskTrigger -AtLogOn -User $who
$principal=New-ScheduledTaskPrincipal -UserId $who -LogonType Interactive -RunLevel Limited
$settings=New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit ([TimeSpan]::Zero) -MultipleInstances IgnoreNew -RestartCount 3 -RestartInterval (New-TimeSpan -Minutes 1)
Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Description 'Independent read-only Claude XiaoKe companion. Visual slider never changes Claude settings.' -Force | Out-Null
Start-ScheduledTask -TaskName $taskName
