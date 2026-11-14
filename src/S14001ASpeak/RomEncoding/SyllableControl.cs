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
    /// Returns the control byte that plays <paramref name="frameCount"/> frames of <paramref name="kind"/>.
    /// </summary>
    /// <param name="kind">How the frames are stored.</param>
    /// <param name="frameCount">The number of frames.</param>
    /// <param name="isLast">Whether this is the word's last syllable.</param>
    /// <returns>The control byte.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameCount"/> is more than one syllable can play.</exception>
    public static byte For(FrameKind kind, int frameCount, bool isLast)
    {
        int maximum = kind == FrameKind.Voiced ? EncoderConstants.MaximumVoicedFrames : EncoderConstants.MaximumUnvoicedFrames;

        if (frameCount < 1 || frameCount > maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(frameCount), frameCount, "A syllable cannot play that many frames.");
        }

        // Voiced: with repeat 3 each length step plays one pitch period, from the length field up to 7.
        // Unvoiced and silent: the length counter counts quarters from (length, repeat, 0) up to 7Fh, so the
        // 5-bit length and repeat field is 32 minus the frame count.
        int control = kind == FrameKind.Voiced
            ? ((EncoderConstants.VoicedLengthSteps - frameCount) << EncoderConstants.RepeatBits) | EncoderConstants.VoicedRepeat
            : EncoderConstants.MaximumUnvoicedFrames - frameCount;

        control |= isLast ? ChipConstants.StopBit : 0;
        control |= kind == FrameKind.Voiced ? ChipConstants.VoicedBit : 0;
        control |= kind == FrameKind.Silent ? ChipConstants.SilenceBit : 0;

        return (byte)control;
    }
}
