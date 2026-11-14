using S14001ASpeak.Audio;

namespace S14001ASpeak.Speech;

/// <summary>
/// Speaks text as recorded audio, which the encoder then turns into S14001A ROM data.
/// </summary>
internal interface ISpeechSource
{
    /// <summary>
    /// Returns a recording of <paramref name="text"/> being spoken.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The recording.</returns>
    /// <exception cref="SpeechSourceException">The text could not be spoken.</exception>
    AudioClip Synthesize(string text);
}
