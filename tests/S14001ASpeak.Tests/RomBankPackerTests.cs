using System.Collections.Generic;
using System.Linq;
using S14001ASpeak.RomEncoding;
using S14001ASpeak.Synthesis;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class RomBankPackerTests
{
    /// <summary>Chip clocks per frame: 128 deltas, one every two clocks.</summary>
    private const int ClocksPerFrame = EncoderConstants.DeltasPerFrame * EncoderConstants.ClocksPerDelta;

    [Fact]
    public void PacksNothingIntoNoBanks() => Assert.Empty(RomBankPacker.Pack([]));

    [Fact]
    public void PointsWordZeroAtTheSyllableList()
    {
        SpeechRom bank = RomBankPacker.Pack([Unvoiced(1, 0xFF)])[0];

        // CWAR 002h: 8 MSBs 00h, then 4 LSBs 2h in D7-D4.
        Assert.Equal(0x00, bank.ReadByte(0));
        Assert.Equal(0x20, bank.ReadByte(1));
    }

    [Fact]
    public void PlacesDataOnSixteenByteBoundaries()
    {
        SpeechRom bank = RomBankPacker.Pack([Unvoiced(1, 0xAA), Unvoiced(1, 0x55)])[0];

        int first = bank.ReadByte(2) << 4;
        int second = bank.ReadByte(4) << 4;

        Assert.Equal(0xAA, bank.ReadByte(first));
        Assert.Equal(0x55, bank.ReadByte(second));
        Assert.Equal(first + EncoderConstants.UnvoicedBytesPerFrame, second);
    }

    [Fact]
    public void StopsTheWordAfterTheLastSyllableOnly()
    {
        SpeechRom bank = RomBankPacker.Pack([Unvoiced(1, 0xFF), Unvoiced(1, 0xFF)])[0];

        Assert.Equal(0, bank.ReadByte(3) & ChipConstants.StopBit);
        Assert.Equal(ChipConstants.StopBit, bank.ReadByte(5) & ChipConstants.StopBit);
    }

    [Fact]
    public void StartsANewBankWhenOneIsFull()
    {
        // Each 32-frame unvoiced syllable needs 1 KB of data, so four do not fit beside the syllable list.
        IReadOnlyList<SpeechRom> banks = RomBankPacker.Pack([.. Enumerable.Repeat(Unvoiced(32, 0xFF), 4)]);

        Assert.Equal(2, banks.Count);
    }

    [Theory]
    [InlineData((int)FrameKind.Voiced, 1)]
    [InlineData((int)FrameKind.Voiced, 8)]
    [InlineData((int)FrameKind.Unvoiced, 1)]
    [InlineData((int)FrameKind.Unvoiced, 32)]
    [InlineData((int)FrameKind.Silent, 20)]
    public void EachFrameLastsOneChipPitchPeriod(int kind, int frames)
    {
        int oneFrame = ChipRun.Say(RomBankPacker.Pack([Syllable((FrameKind)kind, 1)])[0], 0).BusyClocks;
        int many = ChipRun.Say(RomBankPacker.Pack([Syllable((FrameKind)kind, frames)])[0], 0).BusyClocks;

        Assert.Equal((frames - 1) * ClocksPerFrame, many - oneFrame);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void PlaysEachStoredPulseForItsPeriods(int periodsPerPulse)
    {
        int onePulse = ChipRun.Say(RomBankPacker.Pack([Syllable(FrameKind.Voiced, periodsPerPulse, periodsPerPulse)])[0], 0).BusyClocks;
        int eightPulses = ChipRun.Say(RomBankPacker.Pack([Syllable(FrameKind.Voiced, 8 * periodsPerPulse, periodsPerPulse)])[0], 0).BusyClocks;

        Assert.Equal(7 * periodsPerPulse * ClocksPerFrame, eightPulses - onePulse);
    }

    [Fact]
    public void RepeatsTheSameStoredPulse()
    {
        var run = ChipRun.Say(RomBankPacker.Pack([Syllable(FrameKind.Voiced, 3, 3)])[0], 0);
        int start = run.Outputs.Count - (3 * ClocksPerFrame) - 2;

        int[] first = [.. run.Outputs.Skip(start).Take(ClocksPerFrame)];
        int[] second = [.. run.Outputs.Skip(start + ClocksPerFrame).Take(ClocksPerFrame)];

        Assert.Equal(first, second);
        Assert.Contains(first, output => output != ChipConstants.SilentOutput);
    }

    [Fact]
    public void PlaysEverySyllableInTheBank()
    {
        int one = ChipRun.Say(RomBankPacker.Pack([Unvoiced(2, 0xFF)])[0], 0).BusyClocks;
        int three = ChipRun.Say(RomBankPacker.Pack([Unvoiced(2, 0xFF), Syllable(FrameKind.Voiced, 3), Syllable(FrameKind.Silent, 4)])[0], 0).BusyClocks;

        // Each extra syllable also spends two cycles (4 clocks) reading its DARMSB and control bytes.
        Assert.Equal(one + (7 * ClocksPerFrame) + (2 * 4), three);
    }

    [Fact]
    public void ASilentSyllableIsSilent()
    {
        var run = ChipRun.Say(RomBankPacker.Pack([Syllable(FrameKind.Silent, 5)])[0], 0);

        Assert.All(run.Outputs, output => Assert.Equal(ChipConstants.SilentOutput, output));
    }

    private static EncodedSyllable Unvoiced(int frames, byte fill) =>
        new(FrameKind.Unvoiced, frames, 1, [.. Enumerable.Repeat(fill, frames * EncoderConstants.UnvoicedBytesPerFrame)]);

    private static EncodedSyllable Syllable(FrameKind kind, int frames, int periodsPerPulse = 1)
    {
        int bytes = kind switch
        {
            FrameKind.Voiced => frames / periodsPerPulse * EncoderConstants.BytesPerPulse,
            FrameKind.Unvoiced => frames * EncoderConstants.UnvoicedBytesPerFrame,
            _ => 0,
        };

        // Rising deltas, so the pulse is audible and a repeat is easy to see.
        return new EncodedSyllable(kind, frames, periodsPerPulse, [.. Enumerable.Repeat((byte)0xFF, bytes)]);
    }
}
