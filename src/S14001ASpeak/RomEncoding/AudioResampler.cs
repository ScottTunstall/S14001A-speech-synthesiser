using System;
using System.Collections.Generic;

namespace S14001ASpeak.RomEncoding;

/// <summary>
/// Converts audio to another sample rate: a windowed-sinc low-pass filter against aliasing, then linear
/// interpolation.
/// </summary>
internal static class AudioResampler
{
    /// <summary>
    /// Returns <paramref name="samples"/>, recorded at <paramref name="sourceRateHz"/>, resampled to
    /// <paramref name="targetRateHz"/>.
    /// </summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sourceRateHz">The audio's sample rate.</param>
    /// <param name="targetRateHz">The sample rate wanted.</param>
    /// <returns>The resampled audio.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="samples"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A rate is not positive.</exception>
    public static IReadOnlyList<double> Resample(IReadOnlyList<float> samples, double sourceRateHz, double targetRateHz)
    {
#if NET
        ArgumentNullException.ThrowIfNull(samples);
#else
        if (samples is null)
        {
            throw new ArgumentNullException(nameof(samples));
        }
#endif

        if (sourceRateHz <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceRateHz), sourceRateHz, "The sample rate must be positive.");
        }

        if (targetRateHz <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetRateHz), targetRateHz, "The sample rate must be positive.");
        }

        double ratio = sourceRateHz / targetRateHz;
        // The pass band ends at ResamplerCutoff of the lower rate, expressed as a fraction of the source Nyquist.
        double cutoff = 2 * EncoderConstants.ResamplerCutoff * Math.Min(1, 1 / ratio);
        double[] filtered = LowPass(samples, cutoff);
        int count = (int)Math.Floor((samples.Count - 1) / ratio) + 1;
        double[] output = new double[samples.Count == 0 ? 0 : count];

        for (int index = 0; index < output.Length; index++)
        {
            double position = index * ratio;
            int before = (int)Math.Floor(position);
            int after = Math.Min(before + 1, filtered.Length - 1);
            double fraction = position - before;
            output[index] = (filtered[before] * (1 - fraction)) + (filtered[after] * fraction);
        }

        return output;
    }

    private static double[] LowPass(IReadOnlyList<float> samples, double cutoff)
    {
        double[] taps = DesignLowPass(cutoff);
        double[] output = new double[samples.Count];
        const int HalfTaps = EncoderConstants.ResamplerHalfTaps;

        for (int index = 0; index < samples.Count; index++)
        {
            double sum = 0;

            for (int tap = -HalfTaps; tap <= HalfTaps; tap++)
            {
                int source = index + tap;

                if (source >= 0 && source < samples.Count)
                {
                    sum += samples[source] * taps[tap + HalfTaps];
                }
            }

            output[index] = sum;
        }

        return output;
    }

    private static double[] DesignLowPass(double cutoff)
    {
        // cutoff is a fraction of the source Nyquist frequency: the filter passes cutoff / 2 cycles per sample.
        const int HalfTaps = EncoderConstants.ResamplerHalfTaps;
        double[] taps = new double[(2 * HalfTaps) + 1];
        double sum = 0;

        for (int tap = -HalfTaps; tap <= HalfTaps; tap++)
        {
            double sinc = tap == 0 ? cutoff : Math.Sin(Math.PI * cutoff * tap) / (Math.PI * tap);
            double window = 0.5 + (0.5 * Math.Cos(Math.PI * tap / (HalfTaps + 1)));
            taps[tap + HalfTaps] = sinc * window;
            sum += taps[tap + HalfTaps];
        }

        for (int index = 0; index < taps.Length; index++)
        {
            taps[index] /= sum;
        }

        return taps;
    }
}
