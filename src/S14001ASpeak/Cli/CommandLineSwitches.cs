using System.Collections.Generic;

namespace S14001ASpeak.Cli;

/// <summary>
/// The command-line switches the app accepts.
/// </summary>
internal static class CommandLineSwitches
{
    /// <summary>Write the speech to a file instead of playing it.</summary>
    public const string FileShort = "-f";

    /// <summary>Long form of <see cref="FileShort"/>.</summary>
    public const string FileLong = "--file";

    /// <summary>Chip clock as a percentage of the board's clock.</summary>
    public const string SpeedShort = "-s";

    /// <summary>Long form of <see cref="SpeedShort"/>.</summary>
    public const string SpeedLong = "--speed";

    /// <summary>The board's 3-bit speech clock setting.</summary>
    public const string PitchShort = "-p";

    /// <summary>Long form of <see cref="PitchShort"/>.</summary>
    public const string PitchLong = "--pitch";

    /// <summary>Show usage.</summary>
    public const string HelpShort = "-h";

    /// <summary>Long form of <see cref="HelpShort"/>.</summary>
    public const string HelpLong = "--help";

    /// <summary>Windows-style alternative for <see cref="HelpShort"/>.</summary>
    public const string HelpQuestion = "-?";

    /// <summary>Everything after this is text to speak, even if it starts with '-'.</summary>
    public const string EndOfOptions = "@";

    /// <summary>The POSIX spelling of <see cref="EndOfOptions"/>.</summary>
    public const string EndOfOptionsPosix = "--";

    /// <summary>Arguments starting with this character are options.</summary>
    public const char OptionPrefix = '-';

    /// <summary>Gets every long (<c>--name</c>) option, used to suggest the right spelling after a typo such as <c>-file</c>.</summary>
    public static IReadOnlyList<string> LongOptions { get; } = [FileLong, SpeedLong, PitchLong, HelpLong];
}
