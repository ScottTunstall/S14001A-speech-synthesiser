using System.Collections.Generic;
using System.Linq;
using S14001ASpeak.RomEncoding;
using S14001ASpeak.Synthesis;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class SyllableEncoderTests
{
    private const int Frame = EncoderConstants.DeltasPerFrame;

    [Fact]
    public void StoresOneVoicedPulsePerThreePitchPeriods()
    {
        IReadOnlyList<EncodedSyllable> syllables = Encode(TestSignals.PulseTrain(120, 6, 24 * Frame));

        EncodedSyllable syllable = Assert.Single(syllables);
        Assert.Equal(FrameKind.Voiced, syllable.Kind);
        Assert.Equal(EncoderConstants.PeriodsPerPulse, syllable.PeriodsPerPulse);
        Assert.Equal(24 / 3 * EncoderConstants.BytesPerPulse, syllable.Data.Count);
    }

    [Theory]
    [InlineData(25, 3, 1)]
    [InlineData(26, 3, 2)]
    public void PlaysTheFramesLeftOverAsOneShorterPulse(int frames, int fullPulseFrames, int leftOver)
    {
        IReadOnlyList<EncodedSyllable> syllables = Encode(TestSignals.PulseTrain(120, 6, frames * Frame));

        Assert.Equal(2, syllables.Count);
        Assert.Equal(fullPulseFrames, syllables[0].PeriodsPerPulse);
        Assert.Equal(leftOver, syllables[1].FrameCount);
        Assert.Equal(leftOver, syllables[1].PeriodsPerPulse);
        Assert.Equal(frames, syllables.Sum(syllable => syllable.FrameCount));
    }

    [Fact]
    public void StoresTheRingingReversedSoThePulseEndsOnItsPeak()
    {
        EncodedSyllable syllable = Encode(TestSignals.PulseTrain(120, 6, 6 * Frame))[0];

        int[] pulse = Decode(syllable.Data, 0);

        // Played forwards the stored quarter rises to the peak at its end; the mirror then plays the decay.
        Assert.Equal(pulse.Max(), pulse[pulse.Length - 1]);
        Assert.True(pulse[pulse.Length - 1] >= 4);
        Assert.True(pulse[0] <= 2);
    }

    [Fact]
    public void LeavesSilenceAtTheEdgesOut()
    {
        IReadOnlyList<EncodedSyllable> syllables = Encode(TestSignals.Join(
            TestSignals.Silence(5 * Frame), TestSignals.PulseTrain(120, 6, 6 * Frame), TestSignals.Silence(5 * Frame)));

        Assert.All(syllables, syllable => Assert.NotEqual(FrameKind.Silent, syllable.Kind));
    }

    private static IReadOnlyList<EncodedSyllable> Encode(double[] levels) => SyllableEncoder.Encode(levels, FrameAnalyzer.Analyze(levels));

    /// <summary>Decodes one stored voiced pulse, played forwards, into levels above rest.</summary>
    /// <param name="data">The syllable's data.</param>
    /// <param name="offset">The pulse's first byte.</param>
    /// <returns>The 32 output levels.</returns>
    private static int[] Decode(IReadOnlyList<byte> data, int offset)
    {
        int level = ChipConstants.SilentOutput;
        int oldDelta = DeltaConstants.PitchPeriodStartDelta;
        int[] output = new int[EncoderConstants.DeltasPerQuarter];

        for (int index = 0; index < output.Length; index++)
        {
            int delta = DeltaModulator.SelectDelta(true, 0, index % 4, data[offset + (index / 4)]);
            DeltaStep step = DeltaModulator.CalculateStep(true, 0, index == 0, delta, oldDelta);
            level = DeltaModulator.CalculateOutput(true, false, 0, index == 0, level, step);
            oldDelta = delta;
            output[index] = level - ChipConstants.SilentOutput;
        }

        return output;
    }
}
