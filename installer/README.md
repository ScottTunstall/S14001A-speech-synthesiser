# The Windows installers

Builds two normal `setup.exe` installers for S14001ASpeak, one for each target framework:

| Installer                      | Program built for              | Runs on |
|--------------------------------|--------------------------------|---------|
| `S14001ASpeakSetup-net48.exe`  | .NET Framework 4.8             | Any Windows with .NET Framework 4.8 (built into Windows 10 1903 and later) |
| `S14001ASpeakSetup-net10.exe`  | .NET 10, framework-dependent   | 64-bit Windows; needs the .NET 10 runtime, and offers a link to it if it is missing |

Each installs the program under `Program Files`, adds it to the PATH so `S14001ASpeak` works from any
Command Prompt, and registers an entry in "Installed apps" / "Programs and Features" so it can be
removed like any other program. Both use the same app identity, so installing one replaces the other
instead of installing both into the same folder.

The installers are built with [Inno Setup](https://jrsoftware.org/isinfo.php):
one script, one free compiler, and the uninstaller comes for free.

## Building them

Needs the .NET 10 SDK and [Inno Setup 6](https://jrsoftware.org/isinfo.php)
(`winget install JRSoftware.InnoSetup`). `build.ps1` looks for the compiler, `ISCC.exe`, on the `PATH`
and in the default install locations.

```console
installer\build.ps1                     # both installers
installer\build.ps1 -Framework net48    # just one
installer\build.ps1 -Version 1.1.0      # stamp a different version
```

For each framework, this publishes S14001ASpeak into `installer\publish\<framework>` and compiles
`S14001ASpeak.iss` into `installer\Output\S14001ASpeakSetup-<suffix>.exe`. The version comes from
`<Version>` in `S14001ASpeak.csproj` unless `-Version` is given.

If PowerShell refuses to run `build.ps1` because script execution is disabled, run it for that one
command with `powershell -ExecutionPolicy Bypass -File installer\build.ps1`.

## No ROM needed

There is no ROM page: S14001ASpeak encodes its own S14001A speech ROM data from the
text, using a Windows speech voice. Every Windows 10 and 11 PC already has one.

## What it does

* Installs the published files, `README.md` and `THIRD-PARTY-NOTICES.md` (the BSD license of the MAME
  code requires its notice to go with the binaries) into `Program Files\S14001ASpeak`.
* Offers to add that folder to the machine `PATH` (checked by default).
* Adds a Start menu shortcut that opens a Command Prompt in the install folder, showing `--help`.
* **.NET Framework 4.8 installer:** stops with an explanation if .NET Framework 4.8 is not installed,
  since setup does not download it.
* **.NET 10 installer:** checks that the .NET 10 runtime is installed. If it is not, it asks whether to
  open Microsoft's download page (setup carries on either way). It does not download or run anything
  itself, so antivirus software has no reason to flag it as a downloader.
* Removes any previously installed version first, so files it no longer ships are not left behind.

## What it does not do

* It does not sign the installers or the program, so Windows SmartScreen may warn about them on a
  machine that has not seen them before. Signing needs a code signing certificate.
