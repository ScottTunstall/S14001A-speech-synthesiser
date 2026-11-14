using S14001ASpeak.Synthesis;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class DeltaModulatorTests
{
    /// <summary>Deltas 3, 2, 1, 0 from the most significant bits down.</summary>
    private const int Descending = 0b11_10_01_00;

    [Theory]
    [InlineData(0, 3)]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    [InlineData(3, 0)]
    public void ReadsDeltasFromTheMostSignificantBitsDown(int deltaAddress, int expected) =>
        Assert.Equal(expected, DeltaModulator.SelectDelta(voiced: false, pitchQuarter: 1, deltaAddress, Descending));

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 3)]
    public void ReadsBackwardsInTheMirroredHalfOfAVoicedPeriod(int deltaAddress, int expected) =>
        Assert.Equal(expected, DeltaModulator.SelectDelta(voiced: true, pitchQuarter: 1, deltaAddress, Descending));

    [Fact]
    public void StartsEachPitchPeriodFromAMiddleDelta()
    {
        // At the start of a period the old delta is forced to 2, so delta 3 steps up by uIncrements[3][2] = 3.
        DeltaStep step = DeltaModulator.CalculateStep(voiced: false, pitchQuarter: 0, isQuarterStart: true, delta: 3, previousDelta: 0);

        Assert.Equal(new DeltaStep(3, Add: true), step);
    }

    [Theory]
    [InlineData(0, 0, 3, false)]
    [InlineData(1, 2, 0, false)]
    [InlineData(2, 2, 1, true)]
    [InlineData(3, 3, 3, true)]
    public void LooksUpTheStepFromTheDeltaAndTheOneBefore(int delta, int previousDelta, int increment, bool add)
    {
        DeltaStep step = DeltaModulator.CalculateStep(voiced: false, pitchQuarter: 1, isQuarterStart: false, delta, previousDelta);

        Assert.Equal(new DeltaStep(increment, add), step);
    }

    [Fact]
    public void MakesNoStepWhenMirroringStarts()
    {
        DeltaStep step = DeltaModulator.CalculateStep(voiced: true, pitchQuarter: 1, isQuarterStart: true, delta: 3, previousDelta: 3);

        Assert.Equal(0, step.Increment);
    }

    [Fact]
    public void RestsWhenTheSyllableIsSilent() =>
        Assert.Equal(7, DeltaModulator.CalculateOutput(false, silent: true, 1, false, lastOutput: 12, new DeltaStep(3, Add: true)));

    [Fact]
    public void RestsInTheSilentHalfOfAVoicedPeriod() =>
        Assert.Equal(7, DeltaModulator.CalculateOutput(voiced: true, false, pitchQuarter: 2, false, lastOutput: 12, new DeltaStep(3, Add: true)));

    [Fact]
    public void LimitsTheOutputToFifteen() =>
        Assert.Equal(15, DeltaModulator.CalculateOutput(false, false, 1, false, lastOutput: 14, new DeltaStep(3, Add: true)));

    [Fact]
    public void LimitsTheOutputToZero() =>
        Assert.Equal(0, DeltaModulator.CalculateOutput(false, false, 1, false, lastOutput: 1, new DeltaStep(3, Add: false)));

    [Fact]
    public void RestartsFromMidScaleAtEachPitchPeriod() =>
        Assert.Equal(8, DeltaModulator.CalculateOutput(false, false, pitchQuarter: 0, isQuarterStart: true, lastOutput: 15, new DeltaStep(1, Add: true)));
}
