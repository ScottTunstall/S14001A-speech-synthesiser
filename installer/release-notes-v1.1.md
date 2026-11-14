## S14001A Speech Synthesiser 1.1

### What's new

- **Cleaner voice, without the background noise.** Voiced sounds are now encoded as TSI encoded the
  chip's original vocabularies, matching the data in the Berzerk speech ROM:
  - each stored pitch pulse plays for 3 pitch periods and is averaged over them, so the buzz is steady;
  - pulses are stored time-reversed, so the chip's forwards-then-mirrored playback rises to the peak and then
    decays naturally;
  - every pulse is the same way up, and speech is scaled to TSI's level, so the chip no longer clips.

  On "intruder alert", the correlation from one pitch period to the next went from 0.51 to 0.86 (the real
  Berzerk ROM: 0.87). Clipping went from 3.9% of the output to 0.5% (Berzerk: 0.2%). In 1.0 the
  period-to-period variation sounded like noise behind the voice.
- 172 unit tests on each framework.

### Downloads

| Installer | For |
|---|---|
| `S14001ASpeakSetup-net10.exe` | 64-bit Windows with the .NET 10 runtime (setup offers a link to it if it is missing). |
| `S14001ASpeakSetup-net48.exe` | Any Windows with .NET Framework 4.8 (built into Windows 10 1903 and later). |

Either replaces an installed 1.0. No ROM is needed.

### Notes

- Windows only: it uses the Windows speech voices (SAPI) and the Windows audio device.
- The installers are not code-signed, so Windows SmartScreen may warn on first run.
