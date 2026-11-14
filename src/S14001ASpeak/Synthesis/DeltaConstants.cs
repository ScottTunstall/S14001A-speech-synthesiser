// Values taken from MAME src/devices/sound/s14001a.cpp (Mux8To2, CalculateIncrement).
// license:BSD-3-Clause
// copyright-holders:Ed Bernard, Jonathan Gevaryahu, hap
// thanks-to:Kevin Horton

using System.Collections.Generic;

namespace S14001ASpeak.Synthesis;

/// <summary>
/// The constants of the S14001A's delta modulation decoder.
/// </summary>
internal static class DeltaConstants
{
    /// <summary>A delta is two bits.</summary>
    public const int DeltaMask = 0x03;

    /// <summary>
    /// The old delta is forced to this at the start of each pitch period quarter (<c>uDeltaOldP2 = 0x02</c>).
    /// </summary>
    public const int PitchPeriodStartDelta = 0x02;

    /// <summary>Deltas at or above this step up; deltas below it step down (<c>uDelta &gt;= 0x02</c>).</summary>
    public const int FirstRisingDelta = 0x02;

    /// <summary>Selects bit 0 of the pitch period quarter counter: the mirrored half of a voiced period.</summary>
    public const int MirrorBit = 0x01;

    /// <summary>Selects bit 1 of the pitch period quarter counter: the silent half of a voiced period.</summary>
    public const int SilenceBit = 0x02;

    /// <summary>The 8-to-2 multiplexer turns the delta address into a shift of 6, 4, 2 or 0 bits (<c>~adr &lt;&lt; 1 &amp; 0x06</c>).</summary>
    public const int MuxShiftMask = 0x06;

    /// <summary>XOR that turns the 4-bit adder's subtraction into addition and back (<c>uTmp ^= 0x0f</c>).</summary>
    public const int AdderInvertMask = 0x0f;

    /// <summary>
    /// Gets the step sizes, indexed first by one delta and then by the other (MAME <c>uIncrements[4][4]</c>).
    /// Rows and columns are the deltas 00, 01, 10 and 11.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<int>> Increments { get; } =
    [
        [3, 3, 1, 1],
        [1, 1, 0, 0],
        [0, 0, 1, 1],
        [1, 1, 3, 3],
    ];
}
