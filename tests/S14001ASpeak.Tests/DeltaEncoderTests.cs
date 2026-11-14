using System;
using System.Collections.Generic;
using System.Linq;
using S14001ASpeak.RomEncoding;
using S14001ASpeak.Synthesis;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class DeltaEncoderTests
{
    [Fact]
    public void ReturnsOneTwoBitDeltaPerTargetSample()
    {
        IReadOnlyList<int> deltas = DeltaEncoder.Encode(TestSignals.Sine(500, 5, 128), voiced: false);

        Assert.Equal(128, deltas.Count);
        Assert.All(deltas, delta => Assert.InRange(delta, 0, 3));
    }

    [Theory]
    [InlineData(false, 128)]
    [InlineData(true, 32)]
    public void FollowsTheTargetBetterThanAnyFixedDelta(bool voiced, int length)
    {
        double[] target = TestSignals.Sine(400, 6, length);

        double encoded = Error(target, DeltaEncoder.Encode(target, voiced), voiced);

        for (int fixedDelta = 0; fixedDelta < 4; fixedDelta++)
        {
            Assert.True(encoded < Error(target, [.. Enumerable.Repeat(fixedDelta, length)], voiced));
        }
    }

    [Fact]
    public void TracksASlowWaveClosely()
    {
        double[] target = TestSignals.Sine(200, 5, 128);

        double meanSquaredError = Error(target, DeltaEncoder.Encode(target, voiced: false), voiced: false) / target.Length;

        Assert.True(meanSquaredError < 1, $"Mean squared error {meanSquaredError} levels².");
    }

    /// <summary>Plays <paramref name="deltas"/> through the chip's decoder and returns the summed squared error.</summary>
    /// <param name="target">The wanted levels above rest.</param>
    /// <param name="deltas">The deltas to play.</param>
    /// <param name="voiced">Whether to play them as a voiced quarter.</param>
    /// <returns>The summed squared error.</returns>
    private static double Error(double[] target, IReadOnlyList<int> deltas, bool voiced)
    {
        int level = ChipConstants.SilentOutput;
        int oldDelta = DeltaConstants.PitchPeriodStartDelta;
        double total = 0;

        for (int step = 0; step < deltas.Count; step++)
        {
            int quarter = voiced ? 0 : step / 32;
            bool isStart = step % 32 == 0;
            DeltaStep move = DeltaModulator.CalculateStep(voiced, quarter, isStart, deltas[step], oldDelta);
            level = DeltaModulator.CalculateOutput(voiced, false, quarter, isStart, level, move);
            oldDelta = deltas[step];
            total += Math.Pow(level - ChipConstants.SilentOutput - target[step], 2);
        }

        return total;
    }
}
