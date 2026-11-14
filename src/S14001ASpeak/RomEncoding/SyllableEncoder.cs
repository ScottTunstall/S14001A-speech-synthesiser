using System;
using System.Collections.Generic;
using System.Linq;

namespace S14001ASpeak.RomEncoding;

/// <summary>
/// Groups analysed frames into syllables and encodes their delta data.
/// </summary>
internal static class SyllableEncoder
{
    /// <summary>
    /// Returns the syllables that say <paramref name="levels"/>. Silence before the first sound and after the
    /// last is left out.
    /// </summary>
    /// <param name="levels">The speech, as levels above rest at <see cref="EncoderConstants.DeltaRateHz"/>.</param>
    /// <param name="frames">The analysis of each frame of <paramref name="levels"/>.</param>
    /// <returns>The syllables, in order.</returns>
    /// <exception cref="ArgumentNullException">An argument is <see langword="null"/>.</exception>
    public static IReadOnlyList<EncodedSyllable> Encode(IReadOnlyList<double> levels, IReadOnlyList<FrameAnalysis> frames)
    {
#if NET
        ArgumentNullException.ThrowIfNull(levels);
        ArgumentNullException.ThrowIfNull(frames);
#else
        if (levels is null)
        {
            throw new ArgumentNullException(nameof(levels));
        }

        if (frames is null)
        {
            throw new ArgumentNullException(nameof(frames));
        }
#endif

        int first = FirstSound(frames);
        int last = LastSound(frames);
        List<EncodedSyllable> syllables = [];
        int index = first;

        while (index <= last)
        {
            FrameKind kind = frames[index].Kind;
            int maximum = kind == FrameKind.Voiced ? EncoderConstants.MaximumVoicedFrames : EncoderConstants.MaximumUnvoicedFrames;
            int count = 1;

            while (count < maximum && index + count <= last && frames[index + count].Kind == kind)
            {
                count++;
            }

            byte[] data = [.. frames.Skip(index).Take(count).SelectMany(frame => EncodeFrame(levels, frame))];
            syllables.Add(new EncodedSyllable(kind, count, data));
            index += count;
        }

        return syllables;
    }

    private static int FirstSound(IReadOnlyList<FrameAnalysis> frames)
    {
        int index = 0;

        while (index < frames.Count && frames[index].Kind == FrameKind.Silent)
        {
            index++;
        }

        return index;
    }

    private static int LastSound(IReadOnlyList<FrameAnalysis> frames)
    {
        int index = frames.Count - 1;

        while (index >= 0 && frames[index].Kind == FrameKind.Silent)
        {
            index--;
        }

        return index;
    }

    private static byte[] EncodeFrame(IReadOnlyList<double> levels, FrameAnalysis frame)
    {
        int length = frame.Kind switch
        {
            FrameKind.Voiced => EncoderConstants.DeltasPerQuarter,
            FrameKind.Unvoiced => EncoderConstants.DeltasPerFrame,
            _ => 0,
        };

        if (length == 0)
        {
            return [];
        }

        double[] target = [.. Enumerable.Range(frame.SegmentStart, length).Select(index => index < levels.Count ? levels[index] : 0)];
        return Pack(DeltaEncoder.Encode(target, frame.Kind == FrameKind.Voiced));
    }

    private static byte[] Pack(IReadOnlyList<int> deltas)
    {
        byte[] bytes = new byte[deltas.Count / EncoderConstants.DeltasPerByte];

        for (int index = 0; index < deltas.Count; index++)
        {
            // Big-endian: the first delta of a byte is in bits 7-6.
            int shift = (EncoderConstants.DeltasPerByte - 1 - (index % EncoderConstants.DeltasPerByte)) * EncoderConstants.BitsPerDelta;
            bytes[index / EncoderConstants.DeltasPerByte] |= (byte)(deltas[index] << shift);
        }

        return bytes;
    }
}
