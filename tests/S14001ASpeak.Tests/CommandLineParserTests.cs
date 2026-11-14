using S14001ASpeak.Cli;
using S14001ASpeak.Speech;
using Xunit;

namespace S14001ASpeak.Tests;

public sealed class CommandLineParserTests
{
    [Fact]
    public void UsesDefaultsWhenOnlyTextIsGiven()
    {
        CommandLineOptions options = ParseSuccessfully("Hello world");

        Assert.Equal("Hello world", options.Text);
        Assert.Equal(VoiceLimits.DefaultSpeedPercent, options.Voice.SpeedPercent);
        Assert.Equal(VoiceLimits.DefaultPitch, options.Voice.Pitch);
        Assert.Null(options.OutputFile);
        Assert.False(options.ShowHelp);
    }

    [Fact]
    public void JoinsSeparateWordsIntoOneText()
    {
        CommandLineOptions options = ParseSuccessfully("Hello", "-s", "120", "there");

        Assert.Equal("Hello there", options.Text);
    }

    [Theory]
    [InlineData("-f")]
    [InlineData("--file")]
    public void ReadsTheOutputFile(string option)
    {
        CommandLineOptions options = ParseSuccessfully(option, "out.wav", "Hi");

        Assert.Equal("out.wav", options.OutputFile);
    }

    [Theory]
    [InlineData("-s", "-p")]
    [InlineData("--speed", "--pitch")]
    public void ReadsTheVoiceControls(string speedOption, string pitchOption)
    {
        CommandLineOptions options = ParseSuccessfully(speedOption, "150", pitchOption, "6", "Hi");

        Assert.Equal(150, options.Voice.SpeedPercent);
        Assert.Equal(6, options.Voice.Pitch);
    }

    [Fact]
    public void LetsTheLastRepeatedOptionWin()
    {
        CommandLineOptions options = ParseSuccessfully("-p", "1", "-p", "2", "Hi");

        Assert.Equal(2, options.Voice.Pitch);
    }

    [Theory]
    [InlineData("@")]
    [InlineData("--")]
    public void TreatsEverythingAfterTheEndOfOptionsAsText(string marker)
    {
        CommandLineOptions options = ParseSuccessfully("-p", "1", marker, "-5", "degrees", "-p");

        Assert.Equal("-5 degrees -p", options.Text);
        Assert.Equal(1, options.Voice.Pitch);
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    [InlineData("-?")]
    public void AllowsHelpWithoutText(string option)
    {
        CommandLineOptions options = ParseSuccessfully(option);

        Assert.True(options.ShowHelp);
    }

    [Fact]
    public void RequiresText() => Assert.Contains("No text", ParseError(), System.StringComparison.Ordinal);

    [Fact]
    public void RejectsWhitespaceOnlyText() => Assert.Contains("No text", ParseError("   "), System.StringComparison.Ordinal);

    [Theory]
    [InlineData("-s", "49")]
    [InlineData("-s", "201")]
    [InlineData("-s", "fast")]
    [InlineData("-p", "-1")]
    [InlineData("-p", "8")]
    public void RejectsOutOfRangeValues(string option, string value) =>
        Assert.Contains(option, ParseError(option, value, "Hi"), System.StringComparison.Ordinal);

    [Fact]
    public void RejectsAMissingValue() => Assert.Contains("needs a value", ParseError("Hi", "-f"), System.StringComparison.Ordinal);

    [Fact]
    public void RejectsAnUnknownOption() => Assert.Contains("Unknown option '-x'", ParseError("-x", "Hi"), System.StringComparison.Ordinal);

    [Theory]
    [InlineData("-gorf")]
    [InlineData("--gorf")]
    [InlineData("--rom")]
    public void DoesNotAcceptVotraxOrRomOptions(string option) =>
        Assert.Contains($"Unknown option '{option}'", ParseError(option, "PA1", "Hi"), System.StringComparison.Ordinal);

    [Fact]
    public void SuggestsTheDoubleDashSpellingOfALongOption() =>
        Assert.Contains("Did you mean '--speed'?", ParseError("-speed", "100", "Hi"), System.StringComparison.Ordinal);

    [Fact]
    public void DoesNotSuggestAnythingForAnUnrelatedOption() =>
        Assert.DoesNotContain("Did you mean", ParseError("-x", "Hi"), System.StringComparison.Ordinal);

    private static CommandLineOptions ParseSuccessfully(params string[] arguments)
    {
        CommandLineParseResult result = CommandLineParser.Parse(arguments);

        Assert.Equal(string.Empty, result.Error);
        Assert.NotNull(result.Options);
        return result.Options;
    }

    private static string ParseError(params string[] arguments)
    {
        CommandLineParseResult result = CommandLineParser.Parse(arguments);

        Assert.Null(result.Options);
        return result.Error;
    }
}
