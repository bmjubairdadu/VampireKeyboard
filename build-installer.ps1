$iscc = Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe"
$out = & $iscc "d:\Vampire Keyboard\installer.iss" 2>&1
$out | Select-Object -Last 3
Get-Item "d:\Vampire Keyboard\installer\VampireKeyboard-Setup.exe" | Select-Object Name, @{N='SizeMB';E={[math]::Round($_.Length/1MB,1)}}, LastWriteTime
