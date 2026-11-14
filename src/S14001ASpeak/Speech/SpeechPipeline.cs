using System;
using System.Collections.Generic;
using S14001ASpeak.Audio;
using S14001ASpeak.RomEncoding;
using S14001ASpeak.Synthesis;

namespace S14001ASpeak.Speech;

/// <summary>
/// Turns text into S14001A speech: text to recorded speech, recorded speech to ROM banks, ROM banks through the
/// chip to audio, audio to the chosen output.
/// </summary>
internal sealed class SpeechPipeline
{
    private readonly ISpeechSource _source;
    private readonly ISpeechRenderer _renderer;
    private readonly IAudioOutput _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpeechPipeline"/> class.
    /// </summary>
    /// <param name="source">Speaks the text.</param>
    /// <param name="renderer">Plays ROM banks through the chip.</param>
    /// <param name="output">Receives the audio.</param>
    /// <exception cref="ArgumentNullException">An argument is <see langword="null"/>.</exception>
    public SpeechPipeline(ISpeechSource source, ISpeechRenderer renderer, IAudioOutput output)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>
    /// Speaks <paramref name="text"/> to the output, returning once it has been delivered.
    /// </summary>
    /// <param name="text">The text to speak.</param>
    /// <param name="voice">The voice controls.</param>
    /// <exception cref="SpeechSourceException">The text could not be spoken.</exception>
    /// <exception cref="NoSpeechException">The text produced no sound.</exception>
    public void Speak(string text, VoiceSettings voice)
    {
        IReadOnlyList<SpeechRom> banks = SpeechEncoder.Encode(_source.Synthesize(text));

        if (banks.Count == 0)
        {
            throw new NoSpeechException("The text has nothing to say.");
        }

        _output.Deliver(PeakNormalizer.Normalize(_renderer.Render(banks, voice)));
    }
}
