# How it works

Part of the [S14001A Speech Synthesiser](../README.md).

```
text ─► WindowsSpeechSource ─► SpeechEncoder ──────────────────────────────► S14001ASpeechRenderer
        (SAPI voice, 24 kHz)   AudioResampler  → 12 kHz delta rate               (bank switch, word 0,
                               FrameAnalyzer   → silent / voiced / unvoiced       START, wait for /BUSY;
                               SyllableEncoder → DeltaEncoder (Viterbi)           S14001AChip, MAME port)
                               RomBankPacker   → 4 KB ROM banks                          │
                                                                                         ▼
                                                  IAudioOutput ◄── PeakNormalizer ◄── chip output
                                                  (speakers / WAV / OGG)
```

| Folder                         | Contents |
|--------------------------------|----------|
| `src/S14001ASpeak/Synthesis`   | The MAME port. `S14001AChip` is the chip: its two-phase registers and its state machine (`ChipState`). `DeltaModulator` is the delta decoder (`Mux8To2`, `CalculateIncrement`, `CalculateOutput`). `SpeechRom` is a 4 KB ROM bank. |
| `src/S14001ASpeak/RomEncoding` | Turns recorded speech into S14001A ROM banks. |
| `src/S14001ASpeak/Speech`      | The speech source, the renderer, the pipeline and the voice settings. |
| `src/S14001ASpeak/Audio`       | Audio clips, WAV and Ogg Vorbis encoding, speaker and file outputs. |
| `src/S14001ASpeak/Cli`         | Command-line parsing and help. |
| `tests/S14001ASpeak.Tests`     | xUnit tests. |

## The chip

The S14001A reads a 4 KB ROM. For a word it reads a 12-bit address from the word table (`CWARMSB`,
`CWARLSB`), then a list of syllables. Each syllable is a data address (`DARMSB`) and a control byte
(`CTRLBITS`): stop, voiced and silence bits, a 3-bit length and a 2-bit repeat. It then plays 2-bit deltas
(`PLAY`), one per clock cycle. Each delta moves the 4-bit output by 0, 1 or 3 steps, depending on the delta
before it.

The chip works in pitch periods of 4 quarters of 32 deltas. The output restarts at mid-scale at the start
of every period. In a **voiced** syllable the chip plays one quarter's 8 bytes forwards, then the same
bytes backwards (mirrored), then stays silent for two quarters. Every voiced sound therefore has the same
pitch, 128 deltas long: about 94 Hz at the default clock. In an **unvoiced** syllable all four quarters play
new data.

## The encoder

`SpeechEncoder` mirrors how TSI produced its ROMs from recordings:

1. **Resample** (`AudioResampler`) the voice to the chip's delta rate: half the default chip clock, about
   12 kHz, with a windowed-sinc low-pass filter against aliasing. Scale it so the loud parts use the chip's
   output range.
2. **Analyse** (`FrameAnalyzer`) each 128-delta frame. Quiet frames are silent. Frames with a high
   zero-crossing rate, or with no clear period between 70 and 400 Hz, are unvoiced. The rest are voiced, and
   for those it finds the zero crossing before the loudest sample of the first pitch period: the pitch
   pulse the chip will repeat.
3. **Encode** (`SyllableEncoder`, `DeltaEncoder`) runs of frames as syllables: up to 8 voiced frames
   (repeat 3, so each stored pulse plays once) or up to 32 unvoiced or silent frames. The deltas come from a
   Viterbi search over the decoder's 64 states (output level × previous delta). The search uses the
   emulator's own `DeltaModulator`, so the chip plays back exactly what the search measured.
4. **Pack** (`RomBankPacker`) the syllables into 4 KB banks laid out as the chip reads them: the word table
   entry, the syllable list, then each syllable's data on a 16-byte boundary. Long speech fills several
   banks; the renderer switches banks between words, as TSI boards with external ROM banking did.

## Checking it

- The chip port was compared with an independent C# port of the same MAME code (the BerzerkSound project),
  playing Berzerk's real speech ROMs. The output matched sample for sample.
- As an intelligibility check, the speech was run back through Windows speech recognition. For eight
  Berzerk phrases, TSI's own ROM recordings were recognised 0 times out of 8, and this encoder's 2 times
  out of 8. The chip's format is hard for modern recognisers, but the encoder holds its own against the
  original.
