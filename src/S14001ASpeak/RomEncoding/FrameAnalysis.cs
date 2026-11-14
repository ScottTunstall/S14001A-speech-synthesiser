namespace S14001ASpeak.RomEncoding;

/// <summary>
/// What the encoder found in one frame of speech.
/// </summary>
/// <param name="Kind">How the frame is stored.</param>
/// <param name="SegmentStart">
/// For an unvoiced or silent frame, the frame's first sample. For a voiced frame, the peak of the pitch pulse:
/// the ringing that follows it is what the chip will repeat.
/// </param>
internal readonly record struct FrameAnalysis(FrameKind Kind, int SegmentStart);
