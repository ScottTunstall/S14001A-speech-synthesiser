namespace S14001ASpeak.RomEncoding;

/// <summary>
/// How a frame of speech is stored in the ROM.
/// </summary>
internal enum FrameKind
{
    /// <summary>Silence: a syllable with the silence bit set, which needs no delta data.</summary>
    Silent = 0,

    /// <summary>Voiced speech (vowels, voiced consonants): one mirrored pitch pulse per frame.</summary>
    Voiced = 1,

    /// <summary>Unvoiced speech (s, f, sh, t ...): every delta of the frame is stored.</summary>
    Unvoiced = 2,
}
