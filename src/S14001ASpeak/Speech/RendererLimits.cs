using S14001ASpeak.RomEncoding;
using S14001ASpeak.Synthesis;

namespace S14001ASpeak.Speech;

/// <summary>
/// Safety limits of the speech renderer.
/// </summary>
internal static class RendererLimits
{
    /// <summary>
    /// The most clocks a single word may take: a bank full of syllable entries, each the longest a syllable can
    /// be. A word still going after this is running through corrupt data that never sets the stop bit.
    /// </summary>
    public const long MaximumClocksPerWord =
        (long)ChipConstants.RomSize / EncoderConstants.SyllableEntrySize
        * EncoderConstants.MaximumUnvoicedFrames * EncoderConstants.DeltasPerFrame * EncoderConstants.ClocksPerDelta;
}
