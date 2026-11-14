using System;
using System.Collections.Generic;
using S14001ASpeak.Audio;
using S14001ASpeak.RomEncoding;
using S14001ASpeak.Synthesis;

namespace S14001ASpeak.Speech;

/// <summary>
/// Renders ROM banks through an emulated S14001A, driven as a host board drives the chip: switch in the bank,
/// put the word number on the bus, pulse START, then wait for /BUSY to fall before the next bank.
/// </summary>
internal sealed class S14001ASpeechRenderer : ISpeechRenderer
{
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="banks"/> or <paramref name="voice"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">A bank's word never ends, so its data is corrupt.</exception>
    public AudioClip Render(IReadOnlyList<SpeechRom> banks, VoiceSettings voice)
    {
#if NET
        ArgumentNullException.ThrowIfNull(banks);
        ArgumentNullException.ThrowIfNull(voice);
#else
        if (banks is null)
        {
            throw new ArgumentNullException(nameof(banks));
        }

        if (voice is null)
        {
            throw new ArgumentNullException(nameof(voice));
        }
#endif

        List<float> samples = [];

        if (banks.Count > 0)
        {
            var chip = new S14001AChip(banks[0]);

            foreach (SpeechRom bank in banks)
            {
                SayWord(chip, bank, samples);
            }
        }

        return new AudioClip((int)Math.Round(voice.ChipClockHz), samples);
    }

    private static void SayWord(S14001AChip chip, SpeechRom bank, List<float> samples)
    {
        // As Berzerk's berzerk_state::audio_w in MAME: latch the word, then clock START.
        chip.SelectRom(bank);
        chip.SelectWord(EncoderConstants.WordNumber);
        chip.SetStart(true);
        chip.SetStart(false);

        // /BUSY rises on the chip's next phase 1, which may be one or two clocks away, so wait for it to rise
        // before waiting for it to fall.
        bool hasStarted = false;
        long wordClocks = 0;

        while (!hasStarted || chip.IsBusy)
        {
            chip.Clock();
            samples.Add((chip.Output - ChipConstants.SilentOutput) / ChipConstants.OutputScale);
            hasStarted |= chip.IsBusy;

            if (++wordClocks > RendererLimits.MaximumClocksPerWord)
            {
                throw new InvalidOperationException("A speech ROM word never ends; its data must be corrupt.");
            }
        }
    }
}
