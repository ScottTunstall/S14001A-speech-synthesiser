using System;

namespace S14001ASpeak.Speech;

/// <summary>
/// Thrown when the text produces no sound to say, for example because it is only punctuation.
/// </summary>
internal sealed class NoSpeechException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoSpeechException"/> class.
    /// </summary>
    public NoSpeechException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoSpeechException"/> class.
    /// </summary>
    /// <param name="message">What went wrong, phrased for the user.</param>
    public NoSpeechException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoSpeechException"/> class.
    /// </summary>
    /// <param name="message">What went wrong, phrased for the user.</param>
    /// <param name="innerException">The underlying failure.</param>
    public NoSpeechException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
