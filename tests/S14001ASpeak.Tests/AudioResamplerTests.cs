using System;
using System.Collections.Generic;
using System.Linq;
using S14001ASpeak.RomEncoding;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class AudioResamplerTests
{
    [Fact]
    public void ScalesTheLengthByTheRateRatio()
    {
        IReadOnlyList<double> output = AudioResampler.Resample(new float[24000], 24000, 12000);

        Assert.Equal(12000, output.Count);
    }

    [Fact]
    public void ReturnsNothingForNothing() => Assert.Empty(AudioResampler.Resample([], 24000, 12000));

    [Fact]
    public void KeepsAConstantLevel()
    {
        IReadOnlyList<double> output = AudioResampler.Resample([.. Enumerable.Repeat(0.5f, 4800)], 24000, 12000);

        Assert.All(output.Skip(100).Take(2000), sample => Assert.Equal(0.5, sample, 3));
    }

    [Fact]
    public void KeepsALowTone() => Assert.InRange(PeakAfterResampling(500), 0.95, 1.05);

    [Fact]
    public void RemovesATooHighToneThatWouldAlias() => Assert.True(PeakAfterResampling(9000) < 0.05);

    [Fact]
    public void RejectsANonPositiveRate()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => AudioResampler.Resample([], 0, 12000));

        Assert.Equal("sourceRateHz", exception.ParamName);
    }

    private static double PeakAfterResampling(double frequencyHz)
    {
        float[] tone = TestSignals.ToFloats(TestSignals.Sine(frequencyHz, 1, 24000, 24000));
        IReadOnlyList<double> output = AudioResampler.Resample(tone, 24000, 12000);

        return output.Skip(1000).Take(10000).Max(Math.Abs);
    }
}
