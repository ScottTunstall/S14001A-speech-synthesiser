using System;

namespace S14001ASpeak.Speech;

/// <summary>
/// The voice controls, which both set the S14001A's clock.
/// </summary>
/// <remarks>
/// The chip has no pitch input: its timing and pitch both follow its clock. The Berzerk board sets that clock
/// with a 3-bit field of the byte the game writes to port 44h (<c>01vvvppp</c>), which the disassembly calls
/// the "pitch of speech". Speed scales the clock further, as if the board's crystal were changed.
/// </remarks>
internal sealed class VoiceSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VoiceSettings"/> class.
    /// </summary>
    /// <param name="speedPercent">The clock as a percentage of the board's clock.</param>
    /// <param name="pitch">The board's clock setting, 0 (slowest, lowest) to 7 (fastest, highest).</param>
    /// <exception cref="ArgumentOutOfRangeException">A value is outside the ranges in <see cref="VoiceLimits"/>.</exception>
    public VoiceSettings(int speedPercent, int pitch)
    {
        if (speedPercent is < VoiceLimits.MinimumSpeedPercent or > VoiceLimits.MaximumSpeedPercent)
        {
            throw new ArgumentOutOfRangeException(nameof(speedPercent), speedPercent, "Speed is outside the supported range.");
        }

        if (pitch is < VoiceLimits.MinimumPitch or > VoiceLimits.MaximumPitch)
        {
            throw new ArgumentOutOfRangeException(nameof(pitch), pitch, "Pitch is outside the supported range.");
        }

        SpeedPercent = speedPercent;
        Pitch = pitch;
    }

    /// <summary>Gets the default settings: the board's own speed, and the pitch of the game's fixed phrases.</summary>
    public static VoiceSettings Default { get; } = new(VoiceLimits.DefaultSpeedPercent, VoiceLimits.DefaultPitch);

    /// <summary>Gets the clock as a percentage of the board's clock.</summary>
    public int SpeedPercent { get; }

    /// <summary>Gets the board's clock setting.</summary>
    public int Pitch { get; }

    /// <summary>
    /// Gets the chip clock these settings select, in hertz, which is also the sample rate of its output
    /// (<c>S14001_CLOCK / clock_divisor / 8</c>, scaled by the speed).
    /// </summary>
    public double ChipClockHz =>
        BoardTiming.SpeechClockHz / (BoardTiming.PitchDividerBase - Pitch) / BoardTiming.SecondDivider
        * SpeedPercent / VoiceLimits.PercentScale;
}
