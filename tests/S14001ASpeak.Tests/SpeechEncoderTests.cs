using System.Collections.Generic;
using System.Linq;
using S14001ASpeak.Audio;
using S14001ASpeak.RomEncoding;
using S14001ASpeak.Synthesis;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class SpeechEncoderTests
{
    private const int SourceRateHz = 24000;

    [Fact]
    public void EncodesSilenceAsNothing() => Assert.Empty(SpeechEncoder.Encode(new AudioClip(SourceRateHz, new float[SourceRateHz])));

    [Fact]
    public void EncodesNoSamplesAsNothing() => Assert.Empty(SpeechEncoder.Encode(new AudioClip(SourceRateHz, [])));

    [Fact]
    public void EncodesSoundIntoBanksTheChipPlays()
    {
        IReadOnlyList<SpeechRom> banks = SpeechEncoder.Encode(Clip(TestSignals.Join(TestSignals.Sine(150, 0.5, SourceRateHz / 2, SourceRateHz), TestSignals.Noise(0.5, SourceRateHz / 4))));

        var run = ChipRun.Say(Assert.Single(banks), EncoderConstants.WordNumber);

        Assert.Contains(run.Outputs, output => output != ChipConstants.SilentOutput);
    }

    [Fact]
    public void PlaysForAboutAsLongAsTheSpeech()
    {
        // A second of tone is 94 frames of 128 deltas; at two clocks a delta that is about 24,000 clocks.
        IReadOnlyList<SpeechRom> banks = SpeechEncoder.Encode(Clip(TestSignals.Sine(150, 0.5, SourceRateHz, SourceRateHz)));

        int clocks = banks.Sum(bank => ChipRun.Say(bank, EncoderConstants.WordNumber).BusyClocks);

        Assert.InRange(clocks, 23000, 25000);
    }

    [Fact]
    public void LeavesOutLeadingAndTrailingSilence()
    {
        double[] tone = TestSignals.Sine(150, 0.5, SourceRateHz / 2, SourceRateHz);
        int plain = SpeechEncoder.Encode(Clip(tone)).Sum(bank => ChipRun.Say(bank, 0).BusyClocks);
        int padded = SpeechEncoder.Encode(Clip(TestSignals.Join(TestSignals.Silence(SourceRateHz), tone, TestSignals.Silence(SourceRateHz))))
            .Sum(bank => ChipRun.Say(bank, 0).BusyClocks);

        Assert.InRange(padded - plain, -600, 600);
    }

    [Fact]
    public void SplitsLongSpeechAcrossBanks()
    {
        // Noise is stored in full, 32 bytes a frame, so a few seconds of it overflow one 4 KB bank.
        IReadOnlyList<SpeechRom> banks = SpeechEncoder.Encode(Clip(TestSignals.Noise(0.5, 3 * SourceRateHz)));

        Assert.True(banks.Count > 1);
    }

    private static AudioClip Clip(IEnumerable<double> samples) => new(SourceRateHz, TestSignals.ToFloats(samples));
}
