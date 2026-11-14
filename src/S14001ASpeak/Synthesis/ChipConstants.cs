// Values taken from MAME src/devices/sound/s14001a.cpp and s14001a.h.
// license:BSD-3-Clause
// copyright-holders:Ed Bernard, Jonathan Gevaryahu, hap
// thanks-to:Kevin Horton

namespace S14001ASpeak.Synthesis;

/// <summary>
/// Register widths, bit fields and levels of the S14001A, as modelled by MAME's <c>s14001a_device</c>.
/// </summary>
internal static class ChipConstants
{
    /// <summary>The chip addresses 4 KB of speech ROM through pins A0-A11 (<c>device_rom_interface&lt;12&gt;</c>).</summary>
    public const int RomSize = 0x1000;

    /// <summary>Mask applied to every ROM address (<c>ReadMem</c>: <c>offset &amp;= 0xfff</c>).</summary>
    public const int RomAddressMask = RomSize - 1;

    /// <summary>The highest word number on the 6-bit speech address bus C0-C5 (<c>data_w</c>: <c>data &amp; 0x3f</c>).</summary>
    public const int MaximumWordNumber = 0x3f;

    /// <summary>The 4-bit DAC output at rest, and the level every non-playing state outputs.</summary>
    public const int SilentOutput = 7;

    /// <summary>The highest 4-bit DAC output; <c>CalculateOutput</c> limits the adder to this.</summary>
    public const int MaximumOutput = 0x0f;

    /// <summary>
    /// Divides <c>output - 7</c> (-7 to 8) into the -1 to 1 range, as MAME's <c>stream.put_int(0, i, sample, 8)</c> does.
    /// </summary>
    public const float OutputScale = 8f;

    /// <summary>Bits 03-08 of the delta address register latch the word number (<c>WORDWAIT</c>): word bits 2-5.</summary>
    public const int WordHighBitsMask = 0x3c;

    /// <summary>Word bits 0-1, latched into the delta address register's low counter (<c>WORDWAIT</c>).</summary>
    public const int WordLowBitsMask = 0x03;

    /// <summary>Shift that drops the word number's two low bits (<c>(m_uWord &amp; 0x3c) &gt;&gt; 2</c>).</summary>
    public const int WordHighBitsShift = 2;

    /// <summary>Shift that places the word number's two low bits in the 5-bit counter (<c>(m_uWord &amp; 0x03) &lt;&lt; 3</c>).</summary>
    public const int WordLowBitsShift = 3;

    /// <summary>The highest value of the 5-bit counter DAR04To00; reaching it is the quarter pitch period carry.</summary>
    public const int Dar04To00Maximum = 0x1f;

    /// <summary>The 5-bit counter DAR04To00 wraps to zero at this value (<c>if (m_uDAR04To00P1 &gt;= 32)</c>).</summary>
    public const int Dar04To00Modulus = 32;

    /// <summary><c>CWARMSB</c> advances DAR04To00 by four: one ROM byte, as the two low bits count deltas within a byte.</summary>
    public const int Dar04To00ByteStep = 4;

    /// <summary>The 9-bit counter DAR13To05 wraps to zero at this value (<c>if (m_uDAR13To05P1 &gt;= 0x200)</c>).</summary>
    public const int Dar13To05Modulus = 0x200;

    /// <summary>Shift that turns DAR13To05 into a ROM address: each step is 8 bytes.</summary>
    public const int Dar13To05AddressShift = 3;

    /// <summary>Shift that drops DAR04To00's two delta-select bits to give a byte offset.</summary>
    public const int Dar04To00AddressShift = 2;

    /// <summary>DAR04To00's two low bits select one of the four 2-bit deltas in a ROM byte.</summary>
    public const int DeltaSelectMask = 0x03;

    /// <summary>Reversing DAR04To00 (<c>^= 0x1f</c>) plays a voiced pitch period backwards (mirroring).</summary>
    public const int MirrorAddressMask = 0x1f;

    /// <summary>The CWARMSB read places the ROM byte in the top 8 bits of the 12-bit control word address.</summary>
    public const int CwarMsbShift = 4;

    /// <summary>The CWARLSB read takes the low 4 bits of the control word address from ROM D7-D4.</summary>
    public const int CwarLsbShift = 4;

    /// <summary>DARMSB loads the 8 most significant bits of the 9-bit DAR13To05 and zeroes its least significant bit.</summary>
    public const int DarMsbShift = 1;

    /// <summary>Control byte bit 7: this is the word's last syllable.</summary>
    public const int StopBit = 0x80;

    /// <summary>Control byte bit 6: the syllable is voiced (pitch periods are mirrored and half silent).</summary>
    public const int VoicedBit = 0x40;

    /// <summary>Control byte bit 5: the syllable is silent.</summary>
    public const int SilenceBit = 0x20;

    /// <summary>Control byte bits 0-1: the external repeat count.</summary>
    public const int RepeatMask = 0x03;

    /// <summary>Control byte bits 0-4: length and repeat, loaded into the length counter.</summary>
    public const int LengthAndRepeatMask = 0x1f;

    /// <summary>Shift that places the control byte's length and repeat above the pitch period quarter bits.</summary>
    public const int LengthShift = 2;

    /// <summary>The highest value of the 7-bit length counter; reaching it ends the syllable.</summary>
    public const int LengthMaximum = 0x7f;

    /// <summary>The 7-bit length counter wraps to zero at this value.</summary>
    public const int LengthModulus = 0x80;

    /// <summary>The length counter's two low bits count pitch period quarters.</summary>
    public const int PitchQuarterMask = 0x03;

    /// <summary>The length counter's middle two bits count repeats.</summary>
    public const int RepeatCountMask = 0x0c;

    /// <summary>The length counter's top three bits, kept when a voiced repeat completes (<c>&amp;= 0x70</c>).</summary>
    public const int LengthKeepMask = 0x70;
}
