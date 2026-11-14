using System.Collections.Generic;

namespace S14001ASpeak.RomEncoding;

/// <summary>
/// A run of frames of one kind, ready to be placed in a ROM bank.
/// </summary>
/// <param name="Kind">How the frames are stored.</param>
/// <param name="FrameCount">How many frames the syllable plays.</param>
/// <param name="Data">The packed delta data; empty for a silent syllable.</param>
internal sealed record EncodedSyllable(FrameKind Kind, int FrameCount, IReadOnlyList<byte> Data);
