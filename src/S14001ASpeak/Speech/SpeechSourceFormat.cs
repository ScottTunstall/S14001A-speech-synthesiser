namespace S14001ASpeak.Speech;

/// <summary>
/// The audio format requested from the Windows speech voice.
/// </summary>
internal static class SpeechSourceFormat
{
    /// <summary>
    /// The sample rate: 24 kHz, comfortably above the encoder's 12 kHz delta rate, so the voice's own
    /// resampling does not limit the high frequencies the chip can store.
    /// </summary>
    public const int SampleRateHz = 24000;

    /// <summary>Bytes per 16-bit sample.</summary>
    public const int BytesPerSample = 2;

    /// <summary>The largest 16-bit sample value, which maps to 1.</summary>
    public const float FullScale = short.MaxValue;
}
