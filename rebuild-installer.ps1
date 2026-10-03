$iscc = Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe'
$log = & $iscc 'd:\Vampire Keyboard\installer.iss' 2>&1
$log | Select-Object -Last 2 | Out-File 'd:\Vampire Keyboard\iscc-result.txt' -Encoding utf8
Get-Item 'd:\Vampire Keyboard\installer\VampireKeyboard-Setup.exe' |
    Select-Object Name, @{N='SizeMB';E={[math]::Round($_.Length/1MB,1)}}, LastWriteTime |
    Out-File 'd:\Vampire Keyboard\iscc-result.txt' -Append -Encoding utf8
