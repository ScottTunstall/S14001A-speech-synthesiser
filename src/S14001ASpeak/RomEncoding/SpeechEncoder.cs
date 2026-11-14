using System;
using System.Collections.Generic;
using System.Linq;
using S14001ASpeak.Audio;
using S14001ASpeak.Synthesis;

namespace S14001ASpeak.RomEncoding;

/// <summary>
/// Encodes recorded speech into S14001A speech ROM banks, as TSI did for the chip's vocabularies: frames of
/// speech become voiced syllables (one mirrored pitch pulse per chip pitch period), unvoiced syllables (every
/// delta stored) or silent syllables.
/// </summary>
internal static class SpeechEncoder
{
    /// <summary>
    /// Returns the ROM banks that say <paramref name="speech"/>. Each bank says word
    /// <see cref="EncoderConstants.WordNumber"/>; play them in order.
    /// </summary>
    /// <param name="speech">The speech to encode.</param>
    /// <returns>The banks; none if <paramref name="speech"/> is silent.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="speech"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<SpeechRom> Encode(AudioClip speech)
    {
#if NET
        ArgumentNullException.ThrowIfNull(speech);
#else
        if (speech is null)
        {
            throw new ArgumentNullException(nameof(speech));
        }
#endif

        IReadOnlyList<double> samples = AudioResampler.Resample(speech.Samples, speech.SampleRateHz, EncoderConstants.DeltaRateHz);
        IReadOnlyList<double> levels = ToLevels(samples);
        IReadOnlyList<FrameAnalysis> frames = FrameAnalyzer.Analyze(levels);

        return RomBankPacker.Pack(SyllableEncoder.Encode(levels, frames));
    }

    private static IReadOnlyList<double> ToLevels(IReadOnlyList<double> samples)
    {
        double[] magnitudes = [.. samples.Select(Math.Abs)];
        Array.Sort(magnitudes);

        if (magnitudes.Length == 0)
        {
            return [];
        }

        double reference = magnitudes[(int)Math.Floor((magnitudes.Length - 1) * EncoderConstants.ScalingQuantile)];

        if (reference == 0)
        {
            return [.. samples.Select(_ => 0.0)];
        }

        // Turn the speech so its pitch pulses point upwards, as TSI's do; the sign of the third moment (the
        // skew) shows which way they point. Every stored pulse is then the same way up.
        double skew = samples.Sum(sample => sample * sample * sample);
        double scale = EncoderConstants.TargetPeakLevel / reference * (skew < 0 ? -1 : 1);

        return [.. samples.Select(sample => Math.Max(
            EncoderConstants.MaximumLevelBelowRest,
            Math.Min(EncoderConstants.MaximumLevelAboveRest, sample * scale)))];
    }
}
