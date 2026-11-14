using System;
using S14001ASpeak.Synthesis;

namespace S14001ASpeak.RomEncoding;

/// <summary>
/// Builds a syllable's control byte: stop, voiced and silence bits, and the length and repeat fields that the
/// chip's <c>CTRLBITS</c> state loads into its length counter.
/// </summary>
internal static class SyllableControl
{
    /// <summary>
    /// Returns the control byte that plays <paramref name="syllable"/>.
    /// </summary>
    /// <param name="syllable">The syllable.</param>
    /// <param name="isLast">Whether this is the word's last syllable.</param>
    /// <returns>The control byte.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="syllable"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The syllable plays more, or a different shape of, frames than one can.</exception>
    public static byte For(EncodedSyllable syllable, bool isLast)
    {
#if NET
        ArgumentNullException.ThrowIfNull(syllable);
#else
        if (syllable is null)
        {
            throw new ArgumentNullException(nameof(syllable));
        }
#endif

        int control = syllable.Kind == FrameKind.Voiced ? VoicedLengthAndRepeat(syllable) : UnvoicedLengthAndRepeat(syllable);
        control |= isLast ? ChipConstants.StopBit : 0;
        control |= syllable.Kind == FrameKind.Voiced ? ChipConstants.VoicedBit : 0;
        control |= syllable.Kind == FrameKind.Silent ? ChipConstants.SilenceBit : 0;

        return (byte)control;
    }

    private static int VoicedLengthAndRepeat(EncodedSyllable syllable)
    {
        int periods = syllable.PeriodsPerPulse;

        if (periods is < 1 or > EncoderConstants.MaximumPeriodsPerPulse || syllable.FrameCount % periods != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(syllable), syllable, "A voiced syllable plays whole pulses of 1 to 4 periods.");
        }

        int pulses = syllable.FrameCount / periods;

        if (pulses is < 1 or > EncoderConstants.MaximumVoicedPulses)
        {
            throw new ArgumentOutOfRangeException(nameof(syllable), syllable, "A voiced syllable stores 1 to 8 pulses.");
        }

        // Each length step plays one stored pulse, repeated until the 2-bit repeat counter, loaded with
        // 4 - periods, reaches 3; the length field counts the pulses up to 7.
        return ((EncoderConstants.LengthSteps - pulses) << EncoderConstants.RepeatBits)
            | (EncoderConstants.MaximumPeriodsPerPulse - periods);
    }

    private static int UnvoicedLengthAndRepeat(EncodedSyllable syllable)
    {
        if (syllable.FrameCount is < 1 or > EncoderConstants.MaximumUnvoicedFrames)
        {
            throw new ArgumentOutOfRangeException(nameof(syllable), syllable, "An unvoiced syllable plays 1 to 32 frames.");
        }

        // The length counter counts quarters from (length, repeat, 0) up to 7Fh, so the 5-bit length and
        // repeat field is 32 minus the frame count.
        return EncoderConstants.MaximumUnvoicedFrames - syllable.FrameCount;
    }
}
