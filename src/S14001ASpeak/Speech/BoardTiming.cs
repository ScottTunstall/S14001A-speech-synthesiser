// Speech clock of the Berzerk board, from MAME src/mame/stern/berzerk.cpp
// (https://github.com/mamedev/mame/blob/master/src/mame/stern/berzerk.cpp).
// license:BSD-3-Clause
// copyright-holders:Zsolt Vasvari, Aaron Giles, R. Belmont, Jonathan Gevaryahu

namespace S14001ASpeak.Speech;

/// <summary>
/// The S14001A clock of a real board: Stern's VSU-1000 speech board, used by Berzerk and Frenzy, whose clock
/// circuit is documented in MAME.
/// </summary>
internal static class BoardTiming
{
    /// <summary>The board's 10 MHz master crystal (<c>MASTER_CLOCK</c>).</summary>
    public const double MasterClockHz = 10_000_000;

    /// <summary>The clock fed to the speech clock dividers (<c>S14001_CLOCK</c>, master / 4).</summary>
    public const double SpeechClockHz = MasterClockHz / 4;

    /// <summary>
    /// The first LS161 divides the speech clock by this minus the 3-bit pitch setting, 9 to 16
    /// (<c>berzerk_state::audio_w</c>: <c>clock_divisor = 16 - (data &amp; 0x07)</c>).
    /// </summary>
    public const int PitchDividerBase = 16;

    /// <summary>The second LS161 divides by a further 8, "giving a final clock from 19.5kHz to 34.7kHz".</summary>
    public const int SecondDivider = 8;
}
