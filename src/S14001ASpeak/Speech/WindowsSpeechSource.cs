using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using S14001ASpeak.Audio;

namespace S14001ASpeak.Speech;

/// <summary>
/// Speaks text with the default Windows speech voice (SAPI, through <c>System.Speech</c>).
/// </summary>
internal sealed class WindowsSpeechSource : ISpeechSource
{
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    public AudioClip Synthesize(string text)
    {
#if NET
        ArgumentNullException.ThrowIfNull(text);
#else
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }
#endif

        try
        {
            return new AudioClip(SpeechSourceFormat.SampleRateHz, ToSamples(Record(text)));
        }
        catch (Exception exception) when (exception is PlatformNotSupportedException or InvalidOperationException or COMException)
        {
            throw new SpeechSourceException(
                "Windows could not speak the text: " + exception.Message + " A Windows speech voice must be installed.",
                exception);
        }
    }

    private static byte[] Record(string text)
    {
        using var synthesizer = new SpeechSynthesizer();
        using var stream = new MemoryStream();
        var format = new SpeechAudioFormatInfo(SpeechSourceFormat.SampleRateHz, AudioBitsPerSample.Sixteen, AudioChannel.Mono);

        synthesizer.SetOutputToAudioStream(stream, format);
        synthesizer.Speak(text);
        synthesizer.SetOutputToNull();

        return stream.ToArray();
    }

    private static IEnumerable<float> ToSamples(byte[] pcm)
    {
        for (int offset = 0; offset + 1 < pcm.Length; offset += SpeechSourceFormat.BytesPerSample)
        {
            yield return BitConverter.ToInt16(pcm, offset) / SpeechSourceFormat.FullScale;
        }
    }
}
