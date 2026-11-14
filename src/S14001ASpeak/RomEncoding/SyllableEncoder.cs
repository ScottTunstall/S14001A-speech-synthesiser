using System;
using System.Collections.Generic;
using System.Linq;

namespace S14001ASpeak.RomEncoding;

/// <summary>
/// Groups analysed frames into syllables and encodes their delta data.
/// </summary>
/// <remarks>
/// Voiced frames are stored as TSI stored them in its vocabularies (checked against the Berzerk speech ROM):
/// one pulse per <see cref="EncoderConstants.PeriodsPerPulse"/> pitch periods, made from the ringing after the
/// pitch pulse's peak, time-reversed. The chip plays a stored quarter forwards and then mirrored, so each period
/// sounds as a rise to the peak followed by the natural decay, starting and ending at rest.
/// </remarks>
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

        int last = LastSound(frames);
        List<EncodedSyllable> syllables = [];
        int index = FirstSound(frames);

        while (index <= last)
        {
            FrameKind kind = frames[index].Kind;
            int runLength = 1;

            while (index + runLength <= last && frames[index + runLength].Kind == kind)
            {
                runLength++;
            }

            List<FrameAnalysis> run = [.. frames.Skip(index).Take(runLength)];
            syllables.AddRange(kind == FrameKind.Voiced ? EncodeVoiced(levels, run) : EncodeUnvoiced(levels, run, kind));
            index += runLength;
        }

        return syllables;
    }

    private static IEnumerable<EncodedSyllable> EncodeVoiced(IReadOnlyList<double> levels, List<FrameAnalysis> run)
    {
        int index = 0;

        while (index < run.Count)
        {
            int remaining = run.Count - index;

            // Whole groups of PeriodsPerPulse go in syllables of up to 8 pulses; a shorter tail is one pulse
            // repeated for the frames that are left.
            int periods = Math.Min(EncoderConstants.PeriodsPerPulse, remaining);
            int pulses = Math.Min(EncoderConstants.MaximumVoicedPulses, remaining / periods);
            int frameCount = pulses * periods;

            byte[] data = [.. Enumerable.Range(0, pulses)
                .SelectMany(pulse => EncodePulse(levels, run.Skip(index + (pulse * periods)).Take(periods)))];

            yield return new EncodedSyllable(FrameKind.Voiced, frameCount, periods, data);
            index += frameCount;
        }
    }

    private static IEnumerable<EncodedSyllable> EncodeUnvoiced(IReadOnlyList<double> levels, List<FrameAnalysis> run, FrameKind kind)
    {
        for (int index = 0; index < run.Count; index += EncoderConstants.MaximumUnvoicedFrames)
        {
            List<FrameAnalysis> part = [.. run.Skip(index).Take(EncoderConstants.MaximumUnvoicedFrames)];
            byte[] data = kind == FrameKind.Silent ? [] : [.. part.SelectMany(frame => EncodeUnvoicedFrame(levels, frame))];

            yield return new EncodedSyllable(kind, part.Count, 1, data);
        }
    }

    private static byte[] EncodePulse(IReadOnlyList<double> levels, IEnumerable<FrameAnalysis> frames)
    {
        List<int> peaks = [.. frames.Select(frame => frame.SegmentStart)];
        const int Length = EncoderConstants.DeltasPerQuarter;

        // The average of the frames' ringing, from the peak on, time-reversed so the mirror plays it forwards.
        double[] target = [.. Enumerable.Range(0, Length)
            .Select(index => peaks.Average(peak => LevelAt(levels, peak + Length - 1 - index)))];

        return Pack(DeltaEncoder.Encode(target, voiced: true));
    }

    private static byte[] EncodeUnvoicedFrame(IReadOnlyList<double> levels, FrameAnalysis frame)
    {
        double[] target = [.. Enumerable.Range(frame.SegmentStart, EncoderConstants.DeltasPerFrame).Select(index => LevelAt(levels, index))];
        return Pack(DeltaEncoder.Encode(target, voiced: false));
    }

    private static double LevelAt(IReadOnlyList<double> levels, int index) => index < levels.Count ? levels[index] : 0;

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
