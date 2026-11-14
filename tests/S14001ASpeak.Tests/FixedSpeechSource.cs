using S14001ASpeak.Audio;
using S14001ASpeak.Speech;

namespace S14001ASpeak.Tests;

/// <summary>
/// A speech source that returns the same recording whatever the text.
/// </summary>
internal sealed class FixedSpeechSource : ISpeechSource
{
    private readonly AudioClip _recording;

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedSpeechSource"/> class.
    /// </summary>
    /// <param name="recording">The recording to return.</param>
    public FixedSpeechSource(AudioClip recording)
    {
        _recording = recording;
    }

    /// <inheritdoc/>
    public AudioClip Synthesize(string text) => _recording;
}
