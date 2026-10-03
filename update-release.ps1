$ErrorActionPreference = "Stop"

$creds = "protocol=https`nhost=github.com`n" | git credential fill 2>$null | Out-String
$lines = $creds -split "`n" | Where-Object { $_ -match '=' }
$map = @{}
foreach ($l in $lines) { $k,$v = $l -split '=',2; $map[$k] = $v }
$tok = $map['password']

$releaseBody = @"
## Vampire Keyboard v1.0.0 - First Stable Release

**Type Banglish and get Bangla instantly - in any Windows application.**

### Features
- Live Banglish to Bangla conversion (works system-wide, fully offline)
- Smart phonetic engine: 200+ word dictionary, hard letters, hasanta conjuncts
- Bijoy/Avro Compatible Mode for official work
- 20+ languages supported (Hindi, Urdu, Arabic, Spanish, Chinese...)
- Direct translator (Google Translate with offline fallback)
- Draggable top bar with language menu and typing mode toggle
- Runs as Administrator for reliable keyboard hooks

### Installation
1. Download VampireKeyboard-Setup.exe below
2. Run it and accept the UAC prompt
3. Complete the setup wizard and pick your language
4. Start typing Banglish anywhere!

**Requirements:** Windows 10/11 x64. No .NET installation needed (bundled).
"@

$payload = @{
    tag_name = "v1.0.0"
    name = "Vampire Keyboard v1.0.0"
    body = $releaseBody
    draft = $false
    prerelease = $false
} | ConvertTo-Json

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText("d:\Vampire Keyboard\release-payload.json", $payload, $utf8NoBom)

try {
    $r = Invoke-RestMethod -Method Patch -Uri "https://api.github.com/repos/bmjubairdadu/VampireKeyboard/releases/402501946" -Headers @{Authorization="token $tok"; Accept="application/vnd.github+json"} -InFile "d:\Vampire Keyboard\release-payload.json" -ContentType "application/json; charset=utf-8"
    Write-Output "RELEASE_UPDATED: $($r.html_url)"
}
catch {
    Write-Output "ERROR: $($_.ErrorDetails.Message)"
}
