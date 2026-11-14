// Ported from MAME src/devices/sound/s14001a.cpp (Mux8To2, CalculateIncrement, CalculateOutput).
// https://github.com/mamedev/mame/blob/master/src/devices/sound/s14001a.cpp
// license:BSD-3-Clause
// copyright-holders:Ed Bernard, Jonathan Gevaryahu, hap
// thanks-to:Kevin Horton

namespace S14001ASpeak.Synthesis;

/// <summary>
/// The S14001A's delta modulation decoder: picks a 2-bit delta out of a ROM byte and turns it into a new
/// 4-bit output level. Implemented, as in MAME, to mimic the silicon a bit.
/// </summary>
internal static class DeltaModulator
{
    /// <summary>
    /// Returns the two bits of <paramref name="romData"/> selected by <paramref name="deltaAddress"/>
    /// (MAME <c>Mux8To2</c>). Deltas are big-endian: address 0 is bits 7-6.
    /// </summary>
    /// <param name="voiced">Whether the syllable is voiced (<c>bVoicedP2</c>).</param>
    /// <param name="pitchQuarter">The pitch period quarter counter, 0 to 3 (<c>uPPQtrP2</c>).</param>
    /// <param name="deltaAddress">Which delta in the byte, 0 to 3 (<c>uDeltaAdrP2</c>).</param>
    /// <param name="romData">The ROM byte (<c>uRomDataP2</c>).</param>
    /// <returns>The delta, 0 to 3.</returns>
    public static int SelectDelta(bool voiced, int pitchQuarter, int deltaAddress, int romData)
    {
        int address = deltaAddress;

        if (voiced && (pitchQuarter & DeltaConstants.MirrorBit) != 0)
        {
            // Mirroring: count backwards through the byte.
            address ^= DeltaConstants.DeltaMask;
        }

        return (romData >> ((~address << 1) & DeltaConstants.MuxShiftMask)) & DeltaConstants.DeltaMask;
    }

    /// <summary>
    /// Returns the step that <paramref name="delta"/> makes, given the delta before it (MAME
    /// <c>CalculateIncrement</c>). MAME also copies <paramref name="delta"/> into <c>uDeltaOldP1</c> here;
    /// the caller does that.
    /// </summary>
    /// <param name="voiced">Whether the syllable is voiced (<c>bVoicedP2</c>).</param>
    /// <param name="pitchQuarter">The pitch period quarter counter, 0 to 3 (<c>uPPQtrP2</c>).</param>
    /// <param name="isQuarterStart">Whether this is the first delta of a pitch period quarter (<c>bPPQStartP2</c>).</param>
    /// <param name="delta">The delta just read (<c>uDelta</c>).</param>
    /// <param name="previousDelta">The delta before it (<c>uDeltaOldP2</c>).</param>
    /// <returns>The step.</returns>
    public static DeltaStep CalculateStep(bool voiced, int pitchQuarter, bool isQuarterStart, int delta, int previousDelta)
    {
        int oldDelta = previousDelta;

        // Beginning of a pitch period; note this is done for voiced and unvoiced.
        if (pitchQuarter == 0 && isQuarterStart)
        {
            oldDelta = DeltaConstants.PitchPeriodStartDelta;
        }

        bool mirror = (pitchQuarter & DeltaConstants.MirrorBit) != 0;

        // Calculate the increment from the delta; always done, even if silent, to update the old delta.
        // In silicon a PLA determined 0, 1, 3 and add/subtract.
        DeltaStep step = !voiced || !mirror
            ? new DeltaStep(DeltaConstants.Increments[delta][oldDelta], delta >= DeltaConstants.FirstRisingDelta)
            : new DeltaStep(DeltaConstants.Increments[oldDelta][delta], oldDelta < DeltaConstants.FirstRisingDelta);

        // No change when first starting mirroring.
        return voiced && isQuarterStart && mirror ? step with { Increment = 0 } : step;
    }

    /// <summary>
    /// Returns the next 4-bit output level, limited to 0 to 15 (MAME <c>CalculateOutput</c>).
    /// </summary>
    /// <param name="voiced">Whether the syllable is voiced (<c>bVoiced</c>).</param>
    /// <param name="silent">Whether the syllable is silent (<c>bXSilence</c>).</param>
    /// <param name="pitchQuarter">The pitch period quarter counter, 0 to 3 (<c>uPPQtr</c>).</param>
    /// <param name="isQuarterStart">Whether this is the first delta of a pitch period quarter (<c>bPPQStart</c>).</param>
    /// <param name="lastOutput">The previous output level (<c>uLOutput</c>).</param>
    /// <param name="step">The step to apply (<c>uIncrementP2</c>, <c>bAddP2</c>).</param>
    /// <returns>The output level, 0 to 15.</returns>
    public static int CalculateOutput(bool voiced, bool silent, int pitchQuarter, bool isQuarterStart, int lastOutput, DeltaStep step)
    {
        bool silentHalf = (pitchQuarter & DeltaConstants.SilenceBit) != 0;

        if (silent || (voiced && silentHalf))
        {
            return ChipConstants.SilentOutput;
        }

        // Beginning of a pitch period; note this is done for voiced and unvoiced.
        int level = pitchQuarter == 0 && isQuarterStart ? ChipConstants.SilentOutput : lastOutput;

        // The adder only adds: subtraction inverts the level first and afterwards.
        if (!step.Add)
        {
            level ^= DeltaConstants.AdderInvertMask;
        }

        // Add 0, 1 or 3; limit at 15.
        level += step.Increment;

        if (level > ChipConstants.MaximumOutput)
        {
            level = ChipConstants.MaximumOutput;
        }

        if (!step.Add)
        {
            level ^= DeltaConstants.AdderInvertMask;
        }

        return level;
    }
}
