# S14001A Speech Synthesiser

A Windows program that speaks any English text through an accurate emulation of the **TSI S14001A**
speech chip, the voice of Stern's *Berzerk* and *Frenzy* and of the TSI Speech+ talking calculator.

```console
S14001ASpeak "Intruder alert! Intruder alert!"
```

The chip emulation is a C# port of MAME's
[`s14001a.cpp`](https://github.com/mamedev/mame/blob/master/src/devices/sound/s14001a.cpp) by Ed Bernard,
Jonathan Gevaryahu and hap. It builds for both **.NET Framework 4.8** and **.NET 10**, and needs no ROM file.

---

## Contents

- [Quick setup](#quick-setup)
- [Usage](#usage)
- [How it can say anything](#how-it-can-say-anything)
- [Speed and pitch](#speed-and-pitch)
- [Building the Solution](docs/building.md)
- [How it works](docs/how-it-works.md)
- [Licensing](#licensing)

---

## Quick setup

You don't need any programming tools or a ROM file.

1. **Download the installer.** Go to the
   [latest release](https://github.com/ScottTunstall/S14001A-speech-synthesiser/releases/latest) and click
   **`S14001ASpeakSetup-net10.exe`**. If you are not sure which one to pick, pick this one.

   *(Only if you have a very old or 32-bit Windows: use `S14001ASpeakSetup-net48.exe` instead.)*

2. **Run the installer.** Double-click the file you downloaded.
   - If Windows says **"Windows protected your PC"**, click **More info**, then **Run anyway**. This
     happens with any new program that isn't from a big company.
   - Leave **Add S14001ASpeak to the PATH** ticked. Then click through to the end.

3. **Make it talk.** Press the Windows key, type `cmd`, and press Enter. In the black window, type this
   and press Enter:

   ```console
   S14001ASpeak "Hello, world!"
   ```

   You should hear the speech. Change the words between the quotes to say anything you like.

To save the speech as a sound file instead, type `S14001ASpeak -f hello.wav "Hello, world!"` (or `hello.ogg`
for Ogg Vorbis). See [Usage](#usage) for more. To remove the program, uninstall *S14001ASpeak* in Windows
Settings > Apps.

## Usage

```
S14001ASpeak [options] [@] <text>
```

| Option                 | Meaning |
|------------------------|---------|
| `<text>`               | **Required.** The text to speak. Quote it, or pass several words, which are joined with spaces. |
| `-f, --file <path>`    | Write the speech to a file instead of playing it. The format follows the extension: `.ogg` writes Ogg Vorbis, anything else writes WAV. A name with no extension gets `.wav` added. |
| `-s, --speed <50-200>` | Chip clock as a percentage of the nominal clock. Default `100`. |
| `-p, --pitch <0-7>`    | The Berzerk speech board's clock setting. Default `3`, the natural speed. |
| `-h, --help`           | Show help. |
| `@` or `--`            | Treat everything after this as text, even if it starts with `-`. |

Options can be given in any order. If an option is given twice, the last value wins.

### Examples

```console
:: Speak through the default audio device
S14001ASpeak "I am an S14001A speech synthesiser."

:: Several words without quotes work too
S14001ASpeak Hello there

:: A higher, faster robot, as Berzerk varies its robots' voices
S14001ASpeak -p 6 "The humanoid must not escape"

:: Slower and lower
S14001ASpeak -s 80 -p 0 "Coins detected in pocket"

:: Save to a WAV or Ogg Vorbis file instead of playing
S14001ASpeak -f hello.wav "Hello, world!"
S14001ASpeak -f hello.ogg "Hello, world!"

:: Text that starts with a dash
S14001ASpeak @ "-5 degrees outside"
```

Numbers, symbols and punctuation are read the way the Windows voice reads them, so "1,905.5" and "20%"
both work.

### Exit codes

| Code | Meaning |
|------|---------|
| `0`  | Spoken, or written to the file. |
| `2`  | Invalid command line. |
| `3`  | No Windows speech voice could speak the text. |
| `4`  | The speech could not be played or written. |
| `5`  | The text had nothing to say (for example, only punctuation). |

**Windows only.** It uses the Windows speech voices (SAPI) and the Windows audio device. Every Windows
10 and 11 PC has a voice (Microsoft David or Zira) already installed.

## How it can say anything

The S14001A is not a phoneme synthesiser. It plays back speech stored in its ROM as
2-bit delta modulation, one word per START pulse. Berzerk's ROM holds only 29 words. TSI made those ROMs
by recording a person and compressing the recording into the chip's format.

This app does the same, live, for any text:

1. A Windows speech voice says the text into a recording.
2. An encoder turns the recording into S14001A ROM data, as TSI did. Voiced sounds are stored as one pitch
   pulse for every 3 chip pitch periods, time-reversed so that the chip's forwards-then-mirrored playback
   rises to the peak and then decays naturally, matching the data in the Berzerk ROM. Unvoiced sounds such
   as *s* and *f* are stored in full, and pauses become silent syllables. The 2-bit deltas are chosen by a
   search that runs the emulator's own decoder.
3. The emulated chip plays the ROM data, switching 4 KB ROM banks for longer speech.

So you hear what a real S14001A would make of that recording, including its buzzing, fixed pitch of about
94 Hz. The voice is the chip's, not a smoothed modern one.

## Speed and pitch

The chip has no pitch input. Its timing and its pitch both follow its clock. The switches set that clock:

- **Pitch** (`-p`) picks one of the 8 clock settings of Stern's VSU-1000 speech board, as Berzerk does for
  each robot voice (19.5 kHz to 34.7 kHz, from MAME's
  [`berzerk.cpp`](https://github.com/mamedev/mame/blob/master/src/mame/stern/berzerk.cpp)). Speech is
  encoded for setting 3 (24 kHz, the setting of the game's fixed phrases), so that setting plays at natural
  speed. Higher settings are faster and higher.
- **Speed** (`-s`) scales that clock further, as if the board's crystal were changed.

## Building the Solution

The requirements, build commands and code rules are in [`docs/building.md`](docs/building.md).

## How it works

The pipeline, the chip emulation, the encoder and the project layout are described in
[`docs/how-it-works.md`](docs/how-it-works.md).

## Licensing

- The chip emulation in `src/S14001ASpeak/Synthesis` is derived from MAME's `s14001a.cpp` and `s14001a.h`,
  BSD-3-Clause, copyright Ed Bernard, Jonathan Gevaryahu and hap, with thanks to Kevin Horton.
- The Berzerk board clock values in `src/S14001ASpeak/Speech/BoardTiming.cs` come from MAME's
  `berzerk.cpp`, BSD-3-Clause, copyright Zsolt Vasvari, Aaron Giles, R. Belmont and Jonathan Gevaryahu.
- No ROM is included or needed.

See [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) for the full notices.
