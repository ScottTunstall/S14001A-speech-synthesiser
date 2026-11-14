; Inno Setup script for S14001ASpeak.
;
; Inno Setup is used because it is the simplest way to get a normal
; Windows setup.exe: one script, one free compiler (https://jrsoftware.org/isinfo.php), and an
; "Installed apps" entry with a working uninstaller for free.
;
; One script builds both installers. TargetFramework picks which published build goes in:
;   /DTargetFramework=net48    -> Output\S14001ASpeakSetup-net48.exe  (.NET Framework 4.8)
;   /DTargetFramework=net10.0  -> Output\S14001ASpeakSetup-net10.exe  (.NET 10, framework-dependent)
;
; This script does not build the program. Run build.ps1, which publishes S14001ASpeak for both
; frameworks and then compiles this script once for each; see installer\README.md.
;
; No ROM is needed: S14001ASpeak encodes its own speech ROM data from the text.

#define MyAppName "S14001ASpeak"
#define MyAppExeName "S14001ASpeak.exe"
#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#ifndef TargetFramework
  #error Pass /DTargetFramework=net48 or /DTargetFramework=net10.0 (build.ps1 does this).
#endif

#if TargetFramework == "net48"
  #define FrameworkName ".NET Framework 4.8"
  #define FileSuffix "net48"
#elif TargetFramework == "net10.0"
  #define FrameworkName ".NET 10"
  #define FileSuffix "net10"
#else
  #error TargetFramework must be net48 or net10.0.
#endif

#define MyPublishDir "publish\" + TargetFramework

[Setup]
; A fixed, random GUID that identifies S14001ASpeak across versions. Both installers share it, so
; installing either one replaces whichever is already there instead of installing side by side into
; the same folder. It must never change. The doubled "{{" is Inno's escape for a literal "{".
AppId={{5F242604-85C3-4F6F-B7B9-6F929712B300}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
; Shown in "Installed apps", so it is clear which of the two builds is installed.
AppVerName={#MyAppName} {#MyAppVersion} ({#FrameworkName})
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
; Keep the destination page showing even when a previous install exists, so the folder is always
; visible and changeable (the default, "auto", silently reuses the old location).
DisableDirPage=no
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=Output
OutputBaseFilename=S14001ASpeakSetup-{#FileSuffix}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
#if TargetFramework == "net10.0"
; The .NET 10 build is published for win-x64.
ArchitecturesAllowed=x64compatible
#endif
; The .NET Framework build is AnyCPU, so it installs on any Windows; on 64-bit Windows it still goes
; into the 64-bit Program Files.
ArchitecturesInstallIn64BitMode=x64compatible
; Program Files and the machine PATH both need administrator rights to write to.
PrivilegesRequired=admin
; Tells Windows Explorer to notice the PATH change, so a newly opened Command Prompt sees it without a reboot.
ChangesEnvironment=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "addtopath"; Description: "Add {#MyAppName} to the PATH, so ""{#MyAppExeName}"" can be run from any Command Prompt"

[Files]
; publish\<framework> is created by build.ps1 (dotnet publish).
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
; The MAME port is BSD-licensed, which requires its notice to accompany binary distributions.
Source: "..\THIRD-PARTY-NOTICES.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; S14001ASpeak takes the text to speak as an argument, so a plain shortcut would only flash a usage
; message and close. This one opens a prompt in the install folder instead, ready to type into.
Name: "{group}\{#MyAppName} Command Prompt"; Filename: "{cmd}"; \
    Parameters: "/K ""cd /d ""{app}"" && {#MyAppExeName} --help"""; WorkingDir: "{app}"; \
    IconFilename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

; The .NET 10 build needs the .NET 10 runtime. This installer deliberately does not download or install
; it itself: a setup program that quietly runs a hidden PowerShell to fetch and run a script is exactly
; what antivirus software flags as a downloader. It checks for the runtime and, if it is missing, offers
; to open Microsoft's download page instead (see [Code]).

[Code]
const
  EnvironmentKey = 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment';
  NetFrameworkKey = 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full';
  { The "Release" value Microsoft documents for .NET Framework 4.8. }
  NetFramework48Release = 528040;
  DotNetRuntimeKey = 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App';

function GetInstallDir(): string;
begin
  Result := ExpandConstant('{app}');
end;

{ Finds the uninstaller a previously installed version registered, by AppId, so it can be removed
  before this one is put down. Otherwise files an older version wrote but this one no longer includes
  would be left behind, since the same AppId makes Inno install over the old copy in place. }
function GetPreviousUninstallString(): string;
var
  UninstallKey: string;
  UninstallCommand: string;
begin
  UninstallKey := 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}_is1';
  if not RegQueryStringValue(HKLM, UninstallKey, 'UninstallString', UninstallCommand) then
    RegQueryStringValue(HKCU, UninstallKey, 'UninstallString', UninstallCommand);
  Result := UninstallCommand;
end;

{ Runs a previously installed version's uninstaller silently. Errors are ignored (best effort): a
  failed removal still leaves the new install to overwrite what it can. }
procedure UninstallPreviousVersion();
var
  UninstallCommand: string;
  ExitCode: Integer;
begin
  UninstallCommand := GetPreviousUninstallString();
  if UninstallCommand = '' then
    Exit;

  UninstallCommand := RemoveQuotes(UninstallCommand);
  Exec(UninstallCommand, '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART', '', SW_HIDE,
      ewWaitUntilTerminated, ExitCode);
end;

#if TargetFramework == "net48"
{ The .NET Framework build cannot run without .NET Framework 4.8, so setup stops with an explanation
  instead of installing a program that will not start. Windows 10 (May 2019 Update) and later include it. }
function IsNetFramework48Installed(): Boolean;
var
  Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, NetFrameworkKey, 'Release', Release)
      and (Release >= NetFramework48Release);
end;
#endif

#if TargetFramework == "net10.0"
{ The installed .NET runtimes are listed as value names ("10.0.0" and so on) under this key. The x64
  runtime's installer registers it in the 32-bit registry view, hence HKLM32. }
function IsDotNet10RuntimeRegistered(): Boolean;
var
  Versions: TArrayOfString;
  I: Integer;
begin
  Result := False;
  if RegGetValueNames(HKLM32, DotNetRuntimeKey, Versions) then
    for I := 0 to GetArrayLength(Versions) - 1 do
      if Pos('10.', Versions[I]) = 1 then
      begin
        Result := True;
        Exit;
      end;
end;

{ A runtime that was unzipped or installed without registering itself still sits in this folder. }
function IsDotNet10RuntimeFolderPresent(): Boolean;
var
  FindRec: TFindRec;
begin
  Result := FindFirst(ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.NETCore.App\10.*'), FindRec);
  if Result then
    FindClose(FindRec);
end;

function IsDotNet10RuntimeInstalled(): Boolean;
begin
  Result := IsDotNet10RuntimeRegistered() or IsDotNet10RuntimeFolderPresent();
end;

{ Setup carries on either way, because the runtime can be installed afterwards; this only tells the
  user what is missing and takes them to Microsoft's download page if they want. A silent install
  answers No, so nothing is ever opened unattended. }
procedure OfferDotNet10Download();
var
  ErrorCode: Integer;
begin
  if SuppressibleMsgBox('{#MyAppName} needs the .NET 10 runtime, which is not installed on this computer.' + #13#10#13#10 +
      'Open Microsoft''s download page now? Choose the ".NET Runtime" for x64 under Windows, install it, and ' +
      '{#MyAppName} will work. Setup will carry on either way.', mbConfirmation, MB_YESNO, IDNO) = IDYES then
    ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/10.0', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
end;
#endif

function InitializeSetup(): Boolean;
begin
#if TargetFramework == "net48"
  if not IsNetFramework48Installed() then
  begin
    SuppressibleMsgBox('This version of {#MyAppName} needs .NET Framework 4.8, which is not installed.' + #13#10#13#10 +
        'Install it from https://dotnet.microsoft.com/download/dotnet-framework/net48, or use ' +
        'S14001ASpeakSetup-net10.exe instead.', mbCriticalError, MB_OK, IDOK);
    Result := False;
    Exit;
  end;
#endif
#if TargetFramework == "net10.0"
  if not IsDotNet10RuntimeInstalled() then
    OfferDotNet10Download();
#endif

  UninstallPreviousVersion();
  Result := True;
end;

{ Appends the install directory to the machine PATH, unless it is already there. }
procedure AddToPath();
var
  Path: string;
  InstallDir: string;
begin
  InstallDir := GetInstallDir();
  if not RegQueryStringValue(HKEY_LOCAL_MACHINE, EnvironmentKey, 'Path', Path) then
    Path := '';

  if (Pos(';' + Uppercase(InstallDir) + ';', ';' + Uppercase(Path) + ';') > 0) then
    Exit;

  if (Length(Path) > 0) and (Path[Length(Path)] <> ';') then
    Path := Path + ';';
  Path := Path + InstallDir;

  RegWriteExpandStringValue(HKEY_LOCAL_MACHINE, EnvironmentKey, 'Path', Path);
end;

{ Removes the install directory from the machine PATH, leaving everything else in it untouched. }
procedure RemoveFromPath();
var
  Path: string;
  InstallDir: string;
  MarkedPath: string;
  Position: Integer;
begin
  InstallDir := GetInstallDir();
  if not RegQueryStringValue(HKEY_LOCAL_MACHINE, EnvironmentKey, 'Path', Path) then
    Exit;

  MarkedPath := ';' + Path + ';';
  Position := Pos(';' + Uppercase(InstallDir) + ';', Uppercase(MarkedPath));
  if Position = 0 then
    Exit;

  Delete(MarkedPath, Position, Length(InstallDir) + 1);
  { Drop the leading and trailing markers added above; they are all that is left when the install
    directory was the only entry in the PATH. }
  if Length(MarkedPath) <= 2 then
    Path := ''
  else
    Path := Copy(MarkedPath, 2, Length(MarkedPath) - 2);

  RegWriteExpandStringValue(HKEY_LOCAL_MACHINE, EnvironmentKey, 'Path', Path);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep = ssPostInstall) and WizardIsTaskSelected('addtopath') then
    AddToPath();
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
    RemoveFromPath();

  { Only succeeds when empty, so nothing else the user put in the install folder is touched. }
  if (CurUninstallStep = usPostUninstall) and not UninstallSilent() then
    RemoveDir(GetInstallDir());
end;
