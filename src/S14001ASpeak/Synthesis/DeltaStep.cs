// Ported from MAME src/devices/sound/s14001a.cpp (the outputs of CalculateIncrement).
// license:BSD-3-Clause
// copyright-holders:Ed Bernard, Jonathan Gevaryahu, hap
// thanks-to:Kevin Horton

namespace S14001ASpeak.Synthesis;

/// <summary>
/// How a 2-bit delta moves the 4-bit output: by 0, 1 or 3 steps, up or down.
/// </summary>
/// <param name="Increment">The size of the step: 0, 1 or 3 (MAME <c>uIncrementP2</c>).</param>
/// <param name="Add">
/// <see langword="true"/> to step up, <see langword="false"/> to step down (MAME <c>bAddP2</c>).
/// </param>
internal readonly record struct DeltaStep(int Increment, bool Add);
