namespace S14001ASpeak;

/// <summary>
/// The process exit codes.
/// </summary>
internal enum ExitCode
{
    /// <summary>The text was spoken or written.</summary>
    Success = 0,

    /// <summary>The command line was invalid.</summary>
    InvalidArguments = 2,

    /// <summary>No Windows speech voice could speak the text.</summary>
    SpeechSourceFailed = 3,

    /// <summary>The speech could not be played or written to the file.</summary>
    OutputFailed = 4,

    /// <summary>The text produced no sound, for example because it was only punctuation.</summary>
    NothingToSay = 5,
}
