using System.Collections.Generic;
using S14001ASpeak.Audio;

namespace S14001ASpeak.Tests;

/// <summary>
/// An audio output that keeps what it is given.
/// </summary>
internal sealed class RecordingAudioOutput : IAudioOutput
{
    private readonly List<AudioClip> _clips = [];

    /// <summary>Gets the clips delivered so far.</summary>
    public IReadOnlyList<AudioClip> Clips => _clips;

    /// <inheritdoc/>
    public void Deliver(AudioClip clip) => _clips.Add(clip);
}
