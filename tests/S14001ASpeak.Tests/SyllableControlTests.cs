using System;
using S14001ASpeak.RomEncoding;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class SyllableControlTests
{
    [Theory]
    [InlineData(1, 0x5F)]
    [InlineData(8, 0x43)]
    public void PlaysOnePitchPeriodPerLengthStepWhenVoiced(int frames, int expected) =>
        Assert.Equal(expected, SyllableControl.For(FrameKind.Voiced, frames, isLast: false));

    [Theory]
    [InlineData(1, 0x1F)]
    [InlineData(32, 0x00)]
    public void CountsQuartersWhenUnvoiced(int frames, int expected) =>
        Assert.Equal(expected, SyllableControl.For(FrameKind.Unvoiced, frames, isLast: false));

    [Fact]
    public void SetsTheSilenceBitForSilence() => Assert.Equal(0x3F, SyllableControl.For(FrameKind.Silent, 1, isLast: false));

    [Fact]
    public void SetsTheStopBitOnTheLastSyllable() => Assert.Equal(0x9F, SyllableControl.For(FrameKind.Unvoiced, 1, isLast: true));

    [Theory]
    [InlineData((int)FrameKind.Voiced, 0)]
    [InlineData((int)FrameKind.Voiced, 9)]
    [InlineData((int)FrameKind.Unvoiced, 33)]
    public void RejectsFrameCountsASyllableCannotPlay(int kind, int frames)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => SyllableControl.For((FrameKind)kind, frames, isLast: true));

        Assert.Equal("frameCount", exception.ParamName);
    }
}
