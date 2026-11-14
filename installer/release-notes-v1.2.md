## S14001A Speech Synthesiser 1.2

### What's new

- **Stricter code checks.** The Microsoft threading analyzers (`Microsoft.VisualStudio.Threading.Analyzers`)
  now run on every build alongside the .NET analyzers and Roslynator, and, like them, any finding fails the
  build. The code has no findings.
- The README and installer notes no longer refer to another project.
- The speech itself is unchanged from 1.1.

### Downloads

| Installer | For |
|---|---|
| `S14001ASpeakSetup-net10.exe` | 64-bit Windows with the .NET 10 runtime (setup offers a link to it if it is missing). |
| `S14001ASpeakSetup-net48.exe` | Any Windows with .NET Framework 4.8 (built into Windows 10 1903 and later). |

Either replaces an installed 1.0 or 1.1. No ROM is needed.

### Notes

- Windows only: it uses the Windows speech voices (SAPI) and the Windows audio device.
- The installers are not code-signed, so Windows SmartScreen may warn on first run.
