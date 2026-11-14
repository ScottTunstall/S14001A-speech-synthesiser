namespace S14001ASpeak.RomEncoding;

/// <summary>
/// What the encoder found in one frame of speech.
/// </summary>
/// <param name="Kind">How the frame is stored.</param>
/// <param name="SegmentStart">
/// The first sample to encode: the frame's start, or for a voiced frame the start of the pitch pulse it keeps.
/// </param>
internal readonly record struct FrameAnalysis(FrameKind Kind, int SegmentStart);
