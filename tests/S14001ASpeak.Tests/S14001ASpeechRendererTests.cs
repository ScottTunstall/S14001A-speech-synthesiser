using System.Linq;
using S14001ASpeak.Audio;
using S14001ASpeak.Speech;
using S14001ASpeak.Synthesis;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class S14001ASpeechRendererTests
{
    [Theory]
    [InlineData(100, 0, 19531)]
    [InlineData(100, 3, 24038)]
    [InlineData(100, 7, 34722)]
    [InlineData(200, 3, 48077)]
    public void SamplesAtTheChipClock(int speedPercent, int pitch, int expectedHz)
    {
        AudioClip clip = new S14001ASpeechRenderer().Render([Word(0xFF)], new VoiceSettings(speedPercent, pitch));

        Assert.Equal(expectedHz, clip.SampleRateHz);
    }

    [Fact]
    public void RendersNothingForNoBanks() => Assert.Empty(new S14001ASpeechRenderer().Render([], VoiceSettings.Default).Samples);

    [Fact]
    public void PlaysEachBankInTurn()
    {
        var renderer = new S14001ASpeechRenderer();
        int up = renderer.Render([Word(0xFF)], VoiceSettings.Default).Samples.Count;
        int down = renderer.Render([Word(0x00)], VoiceSettings.Default).Samples.Count;

        AudioClip both = renderer.Render([Word(0xFF), Word(0x00)], VoiceSettings.Default);

        // The second START may land on the other clock phase, so its word can begin one clock later.
        Assert.InRange(both.Samples.Count, up + down, up + down + 1);
        Assert.True(both.Samples.Take(up).Max() > 0);
        Assert.True(both.Samples.Skip(up).Min() < 0);
    }

    [Fact]
    public void ScalesTheOutputAsMameDoes()
    {
        AudioClip clip = new S14001ASpeechRenderer().Render([Word(0xFF)], VoiceSettings.Default);

        // Output 15 is 8 levels above rest, which MAME's put_int(..., 8) maps to 1.
        Assert.Equal(1f, clip.Samples.Max());
    }

    private static SpeechRom Word(int fill) => SpeechRomBuilder.WithWord(0, new TestSyllable(fill));
}
