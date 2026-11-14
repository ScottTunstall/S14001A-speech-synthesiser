using System.Collections.Generic;
using S14001ASpeak.Audio;
using S14001ASpeak.Synthesis;

namespace S14001ASpeak.Speech;

/// <summary>
/// Produces the audio an S14001A plays from a sequence of speech ROM banks.
/// </summary>
internal interface ISpeechRenderer
{
    /// <summary>
    /// Returns the audio the chip produces when it says the word in each of <paramref name="banks"/> in turn.
    /// </summary>
    /// <param name="banks">The ROM banks, in order.</param>
    /// <param name="voice">The voice controls.</param>
    /// <returns>The rendered audio.</returns>
    AudioClip Render(IReadOnlyList<SpeechRom> banks, VoiceSettings voice);
}
