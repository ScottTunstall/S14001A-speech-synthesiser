namespace S14001ASpeak.Speech;

/// <summary>
/// The allowed ranges and defaults of the voice controls.
/// </summary>
internal static class VoiceLimits
{
    /// <summary>The slowest speed, as a percentage of the board's clock.</summary>
    public const int MinimumSpeedPercent = 50;

    /// <summary>The fastest speed, as a percentage of the board's clock.</summary>
    public const int MaximumSpeedPercent = 200;

    /// <summary>The board's own speed.</summary>
    public const int DefaultSpeedPercent = 100;

    /// <summary>The lowest pitch setting: the board's slowest speech clock, 19.5 kHz.</summary>
    public const int MinimumPitch = 0;

    /// <summary>The highest pitch setting: the board's fastest speech clock, 34.7 kHz.</summary>
    public const int MaximumPitch = 7;

    /// <summary>
    /// The default pitch: 3, the setting of the game's fixed phrases. "Intruder alert! Intruder alert!",
    /// "Got the humanoid, got the intruder!" and "Chicken! Fight like a robot!" start with 7Bh or 73h.
    /// </summary>
    public const int DefaultPitch = 3;

    /// <summary>Speeds are percentages.</summary>
    public const int PercentScale = 100;
}
