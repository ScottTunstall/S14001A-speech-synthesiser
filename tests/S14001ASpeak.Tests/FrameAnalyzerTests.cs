using System.Collections.Generic;
using System.Linq;
using S14001ASpeak.RomEncoding;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class FrameAnalyzerTests
{
    private const int Frame = EncoderConstants.DeltasPerFrame;

    [Fact]
    public void GivesOneAnalysisPerFrameIncludingAPartialOne()
    {
        IReadOnlyList<FrameAnalysis> frames = FrameAnalyzer.Analyze(TestSignals.Sine(150, 5, (3 * Frame) + 10));

        Assert.Equal(4, frames.Count);
    }

    [Fact]
    public void FindsSilence()
    {
        IReadOnlyList<FrameAnalysis> frames = FrameAnalyzer.Analyze(TestSignals.Join(TestSignals.Sine(150, 5, 4 * Frame), TestSignals.Silence(4 * Frame)));

        Assert.All(frames.Skip(4), frame => Assert.Equal(FrameKind.Silent, frame.Kind));
    }

    [Fact]
    public void TreatsAllSilenceAsSilent()
    {
        IReadOnlyList<FrameAnalysis> frames = FrameAnalyzer.Analyze(TestSignals.Silence(4 * Frame));

        Assert.All(frames, frame => Assert.Equal(FrameKind.Silent, frame.Kind));
    }

    [Fact]
    public void FindsVoicingInAPeriodicSound()
    {
        IReadOnlyList<FrameAnalysis> frames = FrameAnalyzer.Analyze(TestSignals.Sine(150, 5, 8 * Frame));

        Assert.All(frames.Take(6), frame => Assert.Equal(FrameKind.Voiced, frame.Kind));
    }

    [Fact]
    public void FindsNoiseUnvoiced()
    {
        IReadOnlyList<FrameAnalysis> frames = FrameAnalyzer.Analyze(TestSignals.Noise(5, 8 * Frame));

        Assert.All(frames, frame => Assert.Equal(FrameKind.Unvoiced, frame.Kind));
    }

    [Fact]
    public void MarksAVoicedFrameAtThePositivePeakOfItsFirstPitchPeriod()
    {
        double[] levels = TestSignals.Sine(150, 5, 8 * Frame);

        FrameAnalysis frame = FrameAnalyzer.Analyze(levels)[2];

        // 150 Hz at the 12 kHz delta rate is a period of about 80 samples.
        Assert.Equal(FrameKind.Voiced, frame.Kind);
        Assert.InRange(frame.SegmentStart, 2 * Frame, (2 * Frame) + 81);
        Assert.Equal(5, levels[frame.SegmentStart], 1);
    }
}
