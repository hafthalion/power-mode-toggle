; Power Mode Toggle Setup - per-user installer and uninstaller, built with Inno Setup 6 by build.ps1:
;   ISCC.exe /DAppVersion=x.y.z Setup.iss   ->  PowerModeToggle-Setup.exe
; Inno Setup is a widely used installer, so antivirus programs know its setups well. Before 1.7.0 the
; setup was a custom C# exe that embedded the app, which Trellix flagged as malware.

#ifndef AppVersion
  #error Pass the version: ISCC.exe /DAppVersion=x.y.z Setup.iss
#endif

#define RunKey "Software\Microsoft\Windows\CurrentVersion\Run"
; The uninstall entry of the C# setup used before 1.7.0 (Inno Setup's own is PowerModeToggle_is1).
#define OldUninstallKey "Software\Microsoft\Windows\CurrentVersion\Uninstall\PowerModeToggle"

[Setup]
AppId=PowerModeToggle
AppName=Power Mode Toggle
AppVersion={#AppVersion}
AppVerName=Power Mode Toggle {#AppVersion}
AppPublisher=Hafnium
AppPublisherURL=https://github.com/hafthalion/power-mode-toggle
AppCopyright=Copyright (C) 2026 Hafnium
VersionInfoVersion={#AppVersion}
VersionInfoCompany=Hafnium
VersionInfoDescription=Power Mode Toggle Setup
VersionInfoProductName=Power Mode Toggle
; Per user, without administrator rights, in the same folder as before 1.7.0.
PrivilegesRequired=lowest
DefaultDirName={localappdata}\Programs\Power Mode Toggle
DisableDirPage=auto
DisableProgramGroupPage=yes
DisableReadyPage=yes
UsePreviousTasks=no
WizardStyle=modern
SetupIconFile=app.ico
UninstallDisplayIcon={app}\PowerModeToggle.exe
UninstallDisplayName=Power Mode Toggle
OutputDir=.
OutputBaseFilename=PowerModeToggle-Setup
Compression=lzma2
SolidCompression=yes
; The app is closed with "PowerModeToggle.exe --exit" (below); Restart Manager is the fallback,
; for versions before 1.7.0 that don't know --exit.
CloseApplications=force
RestartApplications=no

[Tasks]
Name: autostart; Description: "Start with Windows"

[Files]
Source: "PowerModeToggle.exe"; DestDir: "{app}"; Flags: ignoreversion

[InstallDelete]
; The uninstaller of the C# setup used before 1.7.0.
Type: files; Name: "{app}\Uninstall.exe"

[Registry]
Root: HKCU; Subkey: "{#OldUninstallKey}"; ValueType: none; Flags: deletekey
Root: HKCU; Subkey: "{#RunKey}"; ValueType: string; ValueName: "PowerModeToggle"; ValueData: """{app}\PowerModeToggle.exe"""; Tasks: autostart
Root: HKCU; Subkey: "{#RunKey}"; ValueType: none; ValueName: "PowerModeToggle"; Flags: deletevalue; Tasks: not autostart

[Run]
; The app creates its Start menu shortcut (needed for notifications) when it starts.
Filename: "{app}\PowerModeToggle.exe"; Flags: nowait skipifsilent

[UninstallRun]
Filename: "{app}\PowerModeToggle.exe"; Parameters: "--exit"; Flags: waituntilterminated; RunOnceId: "ExitApp"

[UninstallDelete]
Type: files; Name: "{userprograms}\Power Mode Toggle.lnk"
Type: filesandordirs; Name: "{%TEMP}\PowerModeToggle"
Type: dirifempty; Name: "{app}"

[Code]
function IsInstalled: Boolean;
begin
  Result := RegKeyExists(HKCU, '{#OldUninstallKey}') or
            RegKeyExists(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\PowerModeToggle_is1');
end;

// "Start with Windows" is checked for a new install and keeps the current choice on an update,
// including a change made in the tray menu since.
procedure CurPageChanged(CurPageID: Integer);
begin
  if (CurPageID = wpSelectTasks) and IsInstalled then
    WizardForm.TasksList.Checked[0] := RegValueExists(HKCU, '{#RunKey}', 'PowerModeToggle');
end;

// Close the running app before its exe is replaced.
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
begin
  if FileExists(ExpandConstant('{app}\PowerModeToggle.exe')) then
    Exec(ExpandConstant('{app}\PowerModeToggle.exe'), '--exit', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := '';
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    // Also when "Start with Windows" was turned on in the tray menu.
    RegDeleteValue(HKCU, '{#RunKey}', 'PowerModeToggle');
    if not UninstallSilent and
       (MsgBox('Also delete your settings?', mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES) then
      DelTree(ExpandConstant('{userappdata}\PowerModeToggle'), True, True, True);
  end;
end;
