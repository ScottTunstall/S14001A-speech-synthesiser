using System;
using S14001ASpeak.RomEncoding;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class SyllableControlTests
{
    [Theory]
    [InlineData(1, 1, 0x5F)]
    [InlineData(8, 1, 0x43)]
    [InlineData(3, 3, 0x5D)]
    [InlineData(24, 3, 0x41)]
    [InlineData(4, 4, 0x5C)]
    public void PlaysOnePulsePerLengthStepRepeatedForItsPeriodsWhenVoiced(int frames, int periodsPerPulse, int expected) =>
        Assert.Equal(expected, SyllableControl.For(Syllable(FrameKind.Voiced, frames, periodsPerPulse), isLast: false));

    [Theory]
    [InlineData(1, 0x1F)]
    [InlineData(32, 0x00)]
    public void CountsQuartersWhenUnvoiced(int frames, int expected) =>
        Assert.Equal(expected, SyllableControl.For(Syllable(FrameKind.Unvoiced, frames, 1), isLast: false));

    [Fact]
    public void SetsTheSilenceBitForSilence() => Assert.Equal(0x3F, SyllableControl.For(Syllable(FrameKind.Silent, 1, 1), isLast: false));

    [Fact]
    public void SetsTheStopBitOnTheLastSyllable() => Assert.Equal(0x9F, SyllableControl.For(Syllable(FrameKind.Unvoiced, 1, 1), isLast: true));

    [Theory]
    [InlineData((int)FrameKind.Voiced, 0, 1)]
    [InlineData((int)FrameKind.Voiced, 9, 1)]
    [InlineData((int)FrameKind.Voiced, 27, 3)]
    [InlineData((int)FrameKind.Voiced, 4, 3)]
    [InlineData((int)FrameKind.Voiced, 5, 5)]
    [InlineData((int)FrameKind.Unvoiced, 33, 1)]
    public void RejectsSyllablesItCannotPlay(int kind, int frames, int periodsPerPulse)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => SyllableControl.For(Syllable((FrameKind)kind, frames, periodsPerPulse), isLast: true));

        Assert.Equal("syllable", exception.ParamName);
    }

    private static EncodedSyllable Syllable(FrameKind kind, int frames, int periodsPerPulse) => new(kind, frames, periodsPerPulse, []);
}
