## S14001A Speech Synthesiser 1.0

First release. A Windows console app that speaks **any English text** through an emulation of the
**TSI S14001A** speech chip, the voice of Stern's *Berzerk* and *Frenzy* and the TSI Speech+ calculator.
The chip emulation is ported to C# from MAME's `s14001a.cpp`.

```console
S14001ASpeak "Intruder alert! Intruder alert!"
```

### What's in 1.0

- **S14001A chip emulation**, ported clock for clock from MAME: the two-phase registers, the word,
  syllable and play state machine, and the 2-bit delta decoder with its pitch-period mirroring.
- **Say anything.** The S14001A can only play speech stored in its ROM, so a Windows speech voice says the
  text and the app encodes it into S14001A ROM data, as TSI did for the chip's vocabularies: one mirrored
  pitch pulse per period for voiced sounds, every delta for unvoiced sounds, silent syllables for pauses.
  The emulated chip then plays it, switching 4 KB ROM banks for longer speech. No ROM file is needed.
- **Speed and pitch controls:** `-p/--pitch` (0-7) picks one of the Berzerk speech board's 8 clock
  settings, as the game does for its robot voices; `-s/--speed` (50-200%) scales the clock further.
- **`-f/--file <path>`** saves the speech as WAV, or Ogg Vorbis for a `.ogg` name, instead of playing it.
- `-h/--help`; `@` or `--` to speak text starting with a dash.
- Clear exit codes (0 ok, 2 bad arguments, 3 no Windows voice, 4 output failed, 5 nothing to say).
- Builds for **.NET Framework 4.8** and **.NET 10**, which produce byte-identical speech; 156 unit tests on each.

### Downloads

| Installer | For |
|---|---|
| `S14001ASpeakSetup-net10.exe` | 64-bit Windows with the .NET 10 runtime (setup offers a link to it if it is missing). |
| `S14001ASpeakSetup-net48.exe` | Any Windows with .NET Framework 4.8 (built into Windows 10 1903 and later). |

Both install to `Program Files\S14001ASpeak`, can add it to the PATH, and uninstall from "Installed apps".

### Notes

- Windows only: it uses the Windows speech voices (SAPI) and the Windows audio device.
- The voice is the chip's: buzzy, with a fixed pitch of about 94 Hz at the default setting.
- The installers are not code-signed, so Windows SmartScreen may warn on first run.
