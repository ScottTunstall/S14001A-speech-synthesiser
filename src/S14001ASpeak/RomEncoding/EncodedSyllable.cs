using System.Collections.Generic;

namespace S14001ASpeak.RomEncoding;

/// <summary>
/// A run of frames of one kind, ready to be placed in a ROM bank.
/// </summary>
/// <param name="Kind">How the frames are stored.</param>
/// <param name="FrameCount">How many frames (chip pitch periods) the syllable plays.</param>
/// <param name="PeriodsPerPulse">
/// For a voiced syllable, how many pitch periods each stored pulse plays for (1 to 4); 1 otherwise.
/// </param>
/// <param name="Data">The packed delta data; empty for a silent syllable.</param>
internal sealed record EncodedSyllable(FrameKind Kind, int FrameCount, int PeriodsPerPulse, IReadOnlyList<byte> Data);
