using System.Globalization;
using S14001ASpeak.Speech;

namespace S14001ASpeak.Cli;

/// <summary>
/// The help text shown for --help, and the hint shown after a command-line error.
/// </summary>
internal static class UsageText
{
    /// <summary>The line shown after a command-line error.</summary>
    public const string HelpHint = "Try 'S14001ASpeak --help' for more information.";

    /// <summary>Gets the help text.</summary>
    public static string Text { get; } = string.Format(
        CultureInfo.InvariantCulture,
        """
        Usage: S14001ASpeak [options] [@] <text>

        Speaks <text> through an emulation of the TSI S14001A speech chip (ported from MAME), the voice
        of Berzerk, Frenzy and the TSI Speech+ calculator. The chip can only play speech stored in its
        ROM, so a Windows speech voice says the text first, the app encodes that into S14001A ROM data,
        and the emulated chip plays it back.

        Options:
          -f, --file <path>      Write the speech to a file instead of playing it: Ogg Vorbis if the name
                                 ends in .ogg, otherwise WAV (.wav is added if the name has no extension).
          -s, --speed <{0}-{1}>   Chip clock as a percentage of the nominal clock (default {2}).
                                 As on the real chip, a faster clock also raises the pitch.
          -p, --pitch <{3}-{4}>      The speech clock setting of the Berzerk board; higher is higher pitched
                                 and faster (default {5}, the natural speed).
          -h, --help             Show this help.
          @                      Treat everything after this as text, even if it starts with '-'.
                                 -- does the same.

        Exit codes: 0 success, 2 invalid arguments, 3 no Windows speech voice could speak the text,
                    4 speech could not be played or written, 5 the text had nothing to say.

        Examples:
          S14001ASpeak "Intruder alert! Intruder alert!"
          S14001ASpeak Hello there
          S14001ASpeak -p 6 "The humanoid must not escape"
          S14001ASpeak -s 80 -p 0 "Coins detected in pocket"
          S14001ASpeak -f hello.wav "Hello, world!"
          S14001ASpeak -f hello.ogg "Hello, world!"

        """,
        VoiceLimits.MinimumSpeedPercent,
        VoiceLimits.MaximumSpeedPercent,
        VoiceLimits.DefaultSpeedPercent,
        VoiceLimits.MinimumPitch,
        VoiceLimits.MaximumPitch,
        VoiceLimits.DefaultPitch);
}
