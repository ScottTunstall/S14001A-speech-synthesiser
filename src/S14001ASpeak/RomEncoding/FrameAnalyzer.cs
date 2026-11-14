using System;
using System.Collections.Generic;
using System.Linq;

namespace S14001ASpeak.RomEncoding;

/// <summary>
/// Splits speech into frames of one chip pitch period and decides how each is stored: silent, voiced or
/// unvoiced. For voiced frames it also finds the peak of the pitch pulse the chip will repeat.
/// </summary>
internal static class FrameAnalyzer
{
    /// <summary>
    /// Analyses <paramref name="levels"/>, speech at <see cref="EncoderConstants.DeltaRateHz"/> scaled to chip
    /// output levels above rest.
    /// </summary>
    /// <param name="levels">The speech.</param>
    /// <returns>One analysis per frame of <see cref="EncoderConstants.DeltasPerFrame"/> samples; the last may be partial.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="levels"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<FrameAnalysis> Analyze(IReadOnlyList<double> levels)
    {
#if NET
        ArgumentNullException.ThrowIfNull(levels);
#else
        if (levels is null)
        {
            throw new ArgumentNullException(nameof(levels));
        }
#endif

        int frameCount = (levels.Count + EncoderConstants.DeltasPerFrame - 1) / EncoderConstants.DeltasPerFrame;
        double[] rms = [.. Enumerable.Range(0, frameCount).Select(frame => Rms(levels, frame * EncoderConstants.DeltasPerFrame))];
        double loudest = rms.DefaultIfEmpty(0).Max();

        return [.. Enumerable.Range(0, frameCount).Select(frame => AnalyzeFrame(levels, frame, rms[frame], loudest))];
    }

    private static FrameAnalysis AnalyzeFrame(IReadOnlyList<double> levels, int frame, double rms, double loudest)
    {
        int start = frame * EncoderConstants.DeltasPerFrame;

        if (loudest == 0 || rms < EncoderConstants.SilenceThreshold * loudest)
        {
            return new FrameAnalysis(FrameKind.Silent, start);
        }

        if (CrossingRate(levels, start) > EncoderConstants.UnvoicedCrossingRate)
        {
            return new FrameAnalysis(FrameKind.Unvoiced, start);
        }

        (double correlation, int lag) = StrongestPeriod(levels, start);

        return correlation >= EncoderConstants.VoicingThreshold
            ? new FrameAnalysis(FrameKind.Voiced, PulsePeak(levels, start, lag))
            : new FrameAnalysis(FrameKind.Unvoiced, start);
    }

    private static double Rms(IReadOnlyList<double> levels, int start)
    {
        int end = Math.Min(start + EncoderConstants.DeltasPerFrame, levels.Count);
        double sum = 0;

        for (int index = start; index < end; index++)
        {
            sum += levels[index] * levels[index];
        }

        return Math.Sqrt(sum / EncoderConstants.DeltasPerFrame);
    }

    private static double CrossingRate(IReadOnlyList<double> levels, int start)
    {
        int end = Math.Min(start + EncoderConstants.DeltasPerFrame, levels.Count);
        int crossings = 0;

        for (int index = start + 1; index < end; index++)
        {
            if ((levels[index - 1] < 0) != (levels[index] < 0))
            {
                crossings++;
            }
        }

        return (double)crossings / EncoderConstants.DeltasPerFrame;
    }

    private static (double Correlation, int Lag) StrongestPeriod(IReadOnlyList<double> levels, int start)
    {
        int shortestLag = (int)Math.Floor(EncoderConstants.DeltaRateHz / EncoderConstants.MaximumPitchHz);
        int longestLag = (int)Math.Ceiling(EncoderConstants.DeltaRateHz / EncoderConstants.MinimumPitchHz);
        int end = Math.Min(start + EncoderConstants.DeltasPerFrame, levels.Count);
        double best = 0;
        int bestLag = longestLag;

        for (int lag = shortestLag; lag <= longestLag; lag++)
        {
            double product = 0;
            double energyNow = 0;
            double energyLater = 0;

            for (int index = start; index < end && index + lag < levels.Count; index++)
            {
                product += levels[index] * levels[index + lag];
                energyNow += levels[index] * levels[index];
                energyLater += levels[index + lag] * levels[index + lag];
            }

            double correlation = energyNow > 0 && energyLater > 0 ? product / Math.Sqrt(energyNow * energyLater) : 0;

            if (correlation > best)
            {
                best = correlation;
                bestLag = lag;
            }
        }

        return (best, bestLag);
    }

    private static int PulsePeak(IReadOnlyList<double> levels, int start, int lag)
    {
        // The highest sample of the first pitch period marks the pitch pulse. The encoder has already turned the
        // speech so its pulses point upwards, so taking the positive peak keeps every stored pulse the same way up.
        int end = Math.Min(start + Math.Min(lag, EncoderConstants.DeltasPerFrame), levels.Count);
        int peak = start;

        for (int index = start; index < end; index++)
        {
            if (levels[index] > levels[peak])
            {
                peak = index;
            }
        }

        return peak;
    }
}
