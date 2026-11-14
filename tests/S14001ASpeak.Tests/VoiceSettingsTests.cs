using System;
using S14001ASpeak.Speech;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class VoiceSettingsTests
{
    [Theory]
    [InlineData(0, 2_500_000.0 / 16 / 8)]
    [InlineData(3, 2_500_000.0 / 13 / 8)]
    [InlineData(7, 2_500_000.0 / 9 / 8)]
    public void SelectsTheBerzerkBoardsClockDivider(int pitch, double expectedHz) =>
        Assert.Equal(expectedHz, new VoiceSettings(VoiceLimits.DefaultSpeedPercent, pitch).ChipClockHz, 6);

    [Fact]
    public void ScalesTheClockBySpeed() =>
        Assert.Equal(2 * VoiceSettings.Default.ChipClockHz, new VoiceSettings(200, VoiceLimits.DefaultPitch).ChipClockHz, 6);

    [Theory]
    [InlineData(49, 3, "speedPercent")]
    [InlineData(201, 3, "speedPercent")]
    [InlineData(100, -1, "pitch")]
    [InlineData(100, 8, "pitch")]
    public void RejectsValuesOutOfRange(int speedPercent, int pitch, string parameter)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => new VoiceSettings(speedPercent, pitch));

        Assert.Equal(parameter, exception.ParamName);
    }
}
