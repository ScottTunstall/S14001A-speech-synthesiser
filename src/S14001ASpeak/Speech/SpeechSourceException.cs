using System;

namespace S14001ASpeak.Speech;

/// <summary>
/// Thrown when the speech source cannot speak the text, for example because no Windows voice is installed.
/// </summary>
internal sealed class SpeechSourceException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpeechSourceException"/> class.
    /// </summary>
    public SpeechSourceException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpeechSourceException"/> class.
    /// </summary>
    /// <param name="message">What went wrong, phrased for the user.</param>
    public SpeechSourceException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpeechSourceException"/> class.
    /// </summary>
    /// <param name="message">What went wrong, phrased for the user.</param>
    /// <param name="innerException">The underlying failure.</param>
    public SpeechSourceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
