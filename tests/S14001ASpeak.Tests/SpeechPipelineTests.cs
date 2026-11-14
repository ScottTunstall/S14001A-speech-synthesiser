using System;
using System.Linq;
using S14001ASpeak.Audio;
using S14001ASpeak.Speech;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class SpeechPipelineTests
{
    private const int SourceRateHz = 24000;

    [Fact]
    public void DeliversTheChipsSpeechNormalized()
    {
        var output = new RecordingAudioOutput();
        var pipeline = new SpeechPipeline(Source(TestSignals.Sine(150, 0.5, SourceRateHz / 2, SourceRateHz)), new S14001ASpeechRenderer(), output);

        pipeline.Speak("anything", VoiceSettings.Default);

        AudioClip clip = Assert.Single(output.Clips);
        Assert.Equal(AudioLevels.NormalizedPeak, clip.Samples.Max(Math.Abs), 3);
        Assert.Equal((int)Math.Round(VoiceSettings.Default.ChipClockHz), clip.SampleRateHz);
    }

    [Fact]
    public void ReportsTextThatMakesNoSound()
    {
        var output = new RecordingAudioOutput();
        var pipeline = new SpeechPipeline(Source(TestSignals.Silence(SourceRateHz)), new S14001ASpeechRenderer(), output);

        _ = Assert.Throws<NoSpeechException>(() => pipeline.Speak("...", VoiceSettings.Default));
        Assert.Empty(output.Clips);
    }

    private static FixedSpeechSource Source(double[] samples) => new(new AudioClip(SourceRateHz, TestSignals.ToFloats(samples)));
}
