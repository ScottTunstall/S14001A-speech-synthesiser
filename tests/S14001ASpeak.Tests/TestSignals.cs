using System;
using System.Collections.Generic;
using System.Linq;
using S14001ASpeak.RomEncoding;

namespace S14001ASpeak.Tests;

/// <summary>
/// Synthetic signals for encoder tests, at the encoder's delta rate unless a rate is given.
/// </summary>
internal static class TestSignals
{
    /// <summary>The seed for noise, so tests are repeatable.</summary>
    private const int NoiseSeed = 1234;

    /// <summary>
    /// Returns a sine wave.
    /// </summary>
    /// <param name="frequencyHz">The frequency.</param>
    /// <param name="amplitude">The peak value.</param>
    /// <param name="count">The number of samples.</param>
    /// <param name="rateHz">The sample rate.</param>
    /// <returns>The samples.</returns>
    public static double[] Sine(double frequencyHz, double amplitude, int count, double rateHz = EncoderConstants.DeltaRateHz) =>
        [.. Enumerable.Range(0, count).Select(index => amplitude * Math.Sin(2 * Math.PI * frequencyHz * index / rateHz))];

    /// <summary>
    /// Returns uniform white noise.
    /// </summary>
    /// <param name="amplitude">The peak value.</param>
    /// <param name="count">The number of samples.</param>
    /// <returns>The samples.</returns>
    public static double[] Noise(double amplitude, int count) =>
        [.. Enumerable.Range(0, count).Select(index => amplitude * ((HashToUnit(index + NoiseSeed) * 2) - 1))];

    /// <summary>
    /// Returns a voice-like pulse train: at each pitch period a sharp pulse that rings down, like a vowel's
    /// formant excited by the glottis. Unlike a sine wave it is lopsided, so it has a polarity.
    /// </summary>
    /// <param name="pitchHz">The pitch.</param>
    /// <param name="amplitude">The peak value.</param>
    /// <param name="count">The number of samples.</param>
    /// <param name="rateHz">The sample rate.</param>
    /// <returns>The samples.</returns>
    public static double[] PulseTrain(double pitchHz, double amplitude, int count, double rateHz = EncoderConstants.DeltaRateHz)
    {
        const double RingingHz = 700;
        const double DecaySeconds = 0.002;
        double period = rateHz / pitchHz;

        return [.. Enumerable.Range(0, count).Select(index =>
        {
            double seconds = index % period / rateHz;
            return amplitude * Math.Exp(-seconds / DecaySeconds) * Math.Cos(2 * Math.PI * RingingHz * seconds);
        })];
    }

    /// <summary>
    /// Returns silence.
    /// </summary>
    /// <param name="count">The number of samples.</param>
    /// <returns>The samples.</returns>
    public static double[] Silence(int count) => new double[count];

    /// <summary>
    /// Joins signals end to end.
    /// </summary>
    /// <param name="parts">The signals.</param>
    /// <returns>The joined signal.</returns>
    public static double[] Join(params IEnumerable<double>[] parts) => [.. parts.SelectMany(part => part)];

    /// <summary>
    /// Returns <paramref name="samples"/> as single-precision audio samples.
    /// </summary>
    /// <param name="samples">The samples.</param>
    /// <returns>The converted samples.</returns>
    public static float[] ToFloats(IEnumerable<double> samples) => [.. samples.Select(sample => (float)sample)];

    /// <summary>
    /// Returns a repeatable pseudo-random value from 0 to 1 for <paramref name="index"/> (a 32-bit integer hash),
    /// so noise needs no random number generator.
    /// </summary>
    /// <param name="index">The sample index.</param>
    /// <returns>The value.</returns>
    private static double HashToUnit(int index)
    {
        uint hash = unchecked((uint)index * 2_654_435_761u);
        hash ^= hash >> 16;
        hash = unchecked(hash * 2_246_822_519u);
        hash ^= hash >> 13;
        return hash / (double)uint.MaxValue;
    }
}
