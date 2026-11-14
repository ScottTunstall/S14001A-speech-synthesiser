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
| `installer`                    | Inno Setup installers for both frameworks. |

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

`SpeechEncoder` mirrors how TSI produced its ROMs from recordings. The voiced encoding follows what the
Berzerk speech ROM's own data shows:

1. **Resample** (`AudioResampler`) the voice to the chip's delta rate: half the default chip clock, about
   12 kHz, with a windowed-sinc low-pass filter against aliasing. Turn it so its pitch pulses point upwards
   (by the sign of its skew), and scale it so its peaks reach about 5.5 levels above rest. That is where
   TSI's pulses peak; scaling closer to full scale makes the chip clip.
2. **Analyse** (`FrameAnalyzer`) each 128-delta frame. Quiet frames are silent. Frames with a high
   zero-crossing rate, or with no clear period between 70 and 400 Hz, are unvoiced. The rest are voiced, and
   for those it finds the peak of the first pitch period: the pitch pulse.
3. **Encode** (`SyllableEncoder`, `DeltaEncoder`) runs of frames as syllables:
   - **Voiced:** one stored pulse per 3 pitch periods (repeat field 1, as in most of TSI's Berzerk
     syllables), up to 8 pulses per syllable. Each pulse is the average of the 32 samples after the pitch
     pulse's peak in those 3 periods, **time-reversed**. The chip plays a stored quarter forwards and then
     mirrored, so each period sounds as a rise to the peak, then the natural decay, starting and ending at
     rest. Repeating and averaging keep the buzz steady. A new, unaveraged pulse every period varies from
     period to period, which sounds like background noise.
   - **Unvoiced and silent:** up to 32 frames per syllable; unvoiced frames store every delta.

   The deltas come from a Viterbi search over the decoder's 64 states (output level × previous delta). The
   search uses the emulator's own `DeltaModulator`, so the chip plays back exactly what the search measured.
4. **Pack** (`RomBankPacker`) the syllables into 4 KB banks laid out as the chip reads them: the word table
   entry, the syllable list, then each syllable's data on a 16-byte boundary. Long speech fills several
   banks; the renderer switches banks between words, as TSI boards with external ROM banking did.

## Checking it

- The chip port was compared with an independent C# port of the same MAME code (the BerzerkSound project),
  playing Berzerk's real speech ROMs. The output matched sample for sample.
- The voiced encoding was compared with TSI's, on "intruder alert": TSI's ROM and the encoder both give a
  correlation of 0.87 and 0.86 from one pitch period to the next, with 0.2% and 0.5% of the output at the
  clipping rails. Version 1.0 stored a new pulse every period and scored 0.51 and 3.9%, which sounded like
  background noise.
- As an intelligibility check, the speech was run back through Windows speech recognition, picking from
  eight Berzerk phrases. TSI's own ROM recordings were recognised 0 times out of 8, and this encoder's 1 or 2
  times. The chip's format is hard for modern recognisers, but the encoder holds its own against the
  original.
