[Setup]
AppName=Vampire Keyboard
AppVersion=1.0.0
AppVerName=Vampire Keyboard 1.0.0
AppPublisher=Vampire Keyboard Team
AppCopyright=© 2026 Vampire Keyboard Team
DefaultDirName={autopf}\VampireKeyboard
DefaultGroupName=Vampire Keyboard
UninstallDisplayName=Vampire Keyboard
UninstallDisplayIcon={app}\VampireKeyboard.exe
OutputDir=installer
OutputBaseFilename=VampireKeyboard-Setup
SetupIconFile=Assets\dracula.ico
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes

[Messages]
WelcomeLabel2=This will install [name/ver] on your computer.%n%nIt lets you type Banglish and get Bangla instantly — in any application, including MS Office, browsers, and code editors.%n%nIt is recommended that you close all other applications before continuing.

[Files]
Source: "publish\VampireKeyboard.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Assets\dracula.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Vampire Keyboard"; Filename: "{app}\VampireKeyboard.exe"; IconFilename: "{app}\dracula.ico"
Name: "{group}\Vampire Keyboard Help"; Filename: "{app}\VampireKeyboard.exe"; Parameters: "--help"; IconFilename: "{app}\dracula.ico"
Name: "{group}\Uninstall Vampire Keyboard"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Vampire Keyboard"; Filename: "{app}\VampireKeyboard.exe"; IconFilename: "{app}\dracula.ico"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"
Name: "quicklaunch"; Description: "Pin to &taskbar"; GroupDescription: "Additional icons:"; Flags: unchecked
Name: "autostart"; Description: "Start Vampire Keyboard when Windows starts"; GroupDescription: "Startup:"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "VampireKeyboard"; ValueData: """{app}\VampireKeyboard.exe"""; Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\VampireKeyboard.exe"; Description: "Launch Vampire Keyboard"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{cmd}"; Parameters: "/C taskkill /IM VampireKeyboard.exe /F"; Flags: runhidden; RunOnceId: "KillApp"

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\VampireKeyboard"
