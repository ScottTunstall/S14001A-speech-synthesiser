// Ported from MAME src/devices/sound/s14001a.h (enum class states).
// license:BSD-3-Clause
// copyright-holders:Ed Bernard, Jonathan Gevaryahu, hap
// thanks-to:Kevin Horton

namespace S14001ASpeak.Synthesis;

/// <summary>
/// The S14001A's 3-bit state register: the steps it walks through to say a word.
/// </summary>
internal enum ChipState
{
    /// <summary>Not speaking; the output rests at mid-scale (MAME <c>IDLE</c>).</summary>
    Idle = 0,

    /// <summary>START is high: latch the word number into the delta address register (MAME <c>WORDWAIT</c>).</summary>
    WordWait = 1,

    /// <summary>Read the 8 most significant bits of the control word address register (MAME <c>CWARMSB</c>).</summary>
    CwarMsb = 2,

    /// <summary>Read the 4 least significant bits of the control word address register from ROM D7-D4 (MAME <c>CWARLSB</c>).</summary>
    CwarLsb = 3,

    /// <summary>Read the 8 most significant bits of the delta address register (MAME <c>DARMSB</c>).</summary>
    DarMsb = 4,

    /// <summary>Read the stop, voiced, silence, length and repeat control bits (MAME <c>CTRLBITS</c>).</summary>
    ControlBits = 5,

    /// <summary>Play the syllable's delta-modulated samples (MAME <c>PLAY</c>).</summary>
    Play = 6,

    /// <summary>One cycle of silence after the last syllable, then idle (MAME <c>DELAY</c>).</summary>
    Delay = 7,
}
