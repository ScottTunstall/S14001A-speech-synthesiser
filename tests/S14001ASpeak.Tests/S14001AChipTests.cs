using System;
using System.Linq;
using S14001ASpeak.Synthesis;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class S14001AChipTests
{
    [Fact]
    public void IsIdleAndSilentWhenCreated()
    {
        var chip = new S14001AChip(SpeechRomBuilder.WithWord(0, new TestSyllable(0xFF)));

        Assert.False(chip.IsBusy);
        Assert.Equal(ChipConstants.SilentOutput, chip.Output);
    }

    [Fact]
    public void StaysSilentWhileIdle()
    {
        var chip = new S14001AChip(SpeechRomBuilder.WithWord(0, new TestSyllable(0xFF)));

        for (int clock = 0; clock < 1000; clock++)
        {
            chip.Clock();
            Assert.Equal(ChipConstants.SilentOutput, chip.Output);
            Assert.False(chip.IsBusy);
        }
    }

    [Fact]
    public void BecomesBusyOnTheFirstClockAfterStart()
    {
        var chip = new S14001AChip(SpeechRomBuilder.WithWord(5, new TestSyllable(0xFF)));
        chip.SelectWord(5);
        chip.SetStart(true);
        chip.SetStart(false);

        chip.Clock();

        Assert.True(chip.IsBusy);
    }

    [Fact]
    public void SaysAWordAndReturnsToRest()
    {
        var run = ChipRun.Say(SpeechRomBuilder.WithWord(3, new TestSyllable(0xFF)), 3);

        Assert.True(run.BusyClocks > 0);
        Assert.Equal(ChipConstants.SilentOutput, run.Outputs[run.Outputs.Count - 1]);
    }

    [Fact]
    public void RisingDeltasDriveTheOutputToFullScale()
    {
        var run = ChipRun.Say(SpeechRomBuilder.WithWord(0, new TestSyllable(0xFF)), 0);

        Assert.Equal(ChipConstants.MaximumOutput, run.Outputs.Max());
        Assert.Equal(ChipConstants.SilentOutput, run.Outputs.Min());
    }

    [Fact]
    public void FallingDeltasDriveTheOutputToZero()
    {
        var run = ChipRun.Say(SpeechRomBuilder.WithWord(0, new TestSyllable(0x00)), 0);

        Assert.Equal(0, run.Outputs.Min());
        Assert.Equal(ChipConstants.SilentOutput, run.Outputs.Max());
    }

    [Fact]
    public void ASilentSyllableStaysAtRest()
    {
        var run = ChipRun.Say(SpeechRomBuilder.WithWord(0, new TestSyllable(0xFF, Silent: true)), 0);

        Assert.All(run.Outputs, output => Assert.Equal(ChipConstants.SilentOutput, output));
    }

    [Fact]
    public void AVoicedSyllableRestsDuringTheSilentHalfOfEachPitchPeriod()
    {
        var run = ChipRun.Say(SpeechRomBuilder.WithWord(0, new TestSyllable(0xFF, Voiced: true)), 0);

        Assert.Equal(ChipConstants.MaximumOutput, run.Outputs.Max());
        Assert.Contains(run.Outputs.Skip(10), output => output == ChipConstants.SilentOutput);
    }

    [Fact]
    public void EachLengthStepIsSixteenQuarterPitchPeriods()
    {
        // The length counter starts at (length << 4) | (repeat << 2) and counts quarter pitch periods up to
        // 0x7F. A quarter is 32 phase 1 cycles, which is 64 clocks.
        int shorter = ChipRun.Say(SpeechRomBuilder.WithWord(0, new TestSyllable(0xFF, Length: 7)), 0).BusyClocks;
        int longer = ChipRun.Say(SpeechRomBuilder.WithWord(0, new TestSyllable(0xFF, Length: 6)), 0).BusyClocks;

        Assert.Equal(16 * 64, longer - shorter);
    }

    [Fact]
    public void PlaysEverySyllableUpToTheStopBit()
    {
        int one = ChipRun.Say(SpeechRomBuilder.WithWord(0, new TestSyllable(0xFF)), 0).BusyClocks;
        int two = ChipRun.Say(SpeechRomBuilder.WithWord(0, new TestSyllable(0xFF), new TestSyllable(0x00)), 0).BusyClocks;

        // The second syllable plays 4 quarter pitch periods (256 clocks) and reads its two ROM bytes in the
        // DARMSB and CTRLBITS states (4 clocks).
        Assert.Equal(one + 256 + 4, two);
    }

    [Fact]
    public void SaysTheWordOnTheBus()
    {
        var builder = new SpeechRomBuilder();
        builder.AddWord(1, [new TestSyllable(0x00)]);
        builder.AddWord(2, [new TestSyllable(0xFF)]);
        SpeechRom rom = builder.Build();

        Assert.Equal(0, ChipRun.Say(rom, 1).Outputs.Min());
        Assert.Equal(ChipConstants.MaximumOutput, ChipRun.Say(rom, 2).Outputs.Max());
    }

    [Fact]
    public void SaysTheWordFromTheSelectedRomBank()
    {
        var chip = new S14001AChip(SpeechRomBuilder.WithWord(0, new TestSyllable(0x00)));
        chip.SelectRom(SpeechRomBuilder.WithWord(0, new TestSyllable(0xFF)));
        chip.SelectWord(0);
        chip.SetStart(true);
        chip.SetStart(false);
        int highest = 0;

        do
        {
            chip.Clock();
            highest = Math.Max(highest, chip.Output);
        }
        while (chip.IsBusy);

        Assert.Equal(ChipConstants.MaximumOutput, highest);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    public void RejectsWordNumbersWiderThanSixBits(int word)
    {
        var chip = new S14001AChip(SpeechRomBuilder.WithWord(0, new TestSyllable(0xFF)));

        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => chip.SelectWord(word));

        Assert.Equal("word", exception.ParamName);
    }
}
