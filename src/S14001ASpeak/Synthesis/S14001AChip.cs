// Ported from MAME src/devices/sound/s14001a.cpp and s14001a.h (s14001a_device: device_start, busy_r,
// data_w, start_w, ReadMem, Clock, sound_stream_update), master branch as of commit d0f1c15 (April 2025):
// https://github.com/mamedev/mame/blob/master/src/devices/sound/s14001a.cpp
// license:BSD-3-Clause
// copyright-holders:Ed Bernard, Jonathan Gevaryahu, hap
// thanks-to:Kevin Horton

using System;

namespace S14001ASpeak.Synthesis;

/// <summary>
/// A cycle-accurate simulation of the SSi TSI S14001A speech chip ("CRC: Custom ROM Controller").
/// </summary>
/// <remarks>
/// <para>
/// Drive it as you would the real chip: put a word number on the speech address bus with
/// <see cref="SelectWord"/>, take START high then low with <see cref="SetStart"/>, then call
/// <see cref="Clock"/> once per output sample and read <see cref="Output"/>, until <see cref="IsBusy"/>
/// (the /BUSY pin) shows the word has ended.
/// </para>
/// <para>
/// As in MAME, the chip's two-phase logic is kept as pairs of registers: phase 1 values (suffix P1) are
/// computed from phase 2 values (suffix P2), which are refreshed from phase 1 on the next half cycle. Each
/// <see cref="Clock"/> call is one half cycle, and MAME's sample rate is the chip clock, so one call gives one
/// output sample. MAME also keeps phase 2 copies of the state, DAR13To05 and XRepeat registers
/// (<c>m_uStateP2</c>, <c>m_uDAR13To05P2</c>, <c>m_uXRepeatP2</c>) but never reads them, so they are left out.
/// </para>
/// </remarks>
internal sealed class S14001AChip
{
    /// <summary>The speech ROM bank currently wired to the address and data pins.</summary>
    private SpeechRom _rom;

    /// <summary>The 1-bit internal clock (<c>m_bPhase1</c>): whether the next half cycle is phase 2.</summary>
    private bool _phase1;

    /// <summary>The 3-bit state register (<c>m_uStateP1</c>).</summary>
    private ChipState _stateP1 = ChipState.Idle;

    /// <summary>
    /// The 9 MSBs of the delta address register (<c>m_uDAR13To05P1</c>); incrementing it advances the ROM address
    /// by 8 bytes.
    /// </summary>
    private int _dar13To05P1;

    /// <summary>
    /// The 5 LSBs of the delta address register (<c>m_uDAR04To00P1</c>): 3 address the ROM, 2 mux 8 bits of data
    /// into a 2-bit delta. Its carry marks the end of a quarter pitch period (32 cycles).
    /// </summary>
    private int _dar04To00P1;

    /// <summary>The phase 2 copy of <see cref="_dar04To00P1"/> (<c>m_uDAR04To00P2</c>).</summary>
    private int _dar04To00P2;

    /// <summary>The 12-bit control word address register, which points at a syllable (<c>m_uCWARP1</c>).</summary>
    private int _cwarP1;

    /// <summary>The phase 2 copy of <see cref="_cwarP1"/> (<c>m_uCWARP2</c>).</summary>
    private int _cwarP2;

    /// <summary>Whether the syllable is the word's last (<c>m_bStopP1</c>).</summary>
    private bool _stopP1;

    /// <summary>The phase 2 copy of <see cref="_stopP1"/> (<c>m_bStopP2</c>).</summary>
    private bool _stopP2;

    /// <summary>Whether the syllable is voiced (<c>m_bVoicedP1</c>).</summary>
    private bool _voicedP1;

    /// <summary>The phase 2 copy of <see cref="_voicedP1"/> (<c>m_bVoicedP2</c>).</summary>
    private bool _voicedP2;

    /// <summary>Whether the syllable is silent (<c>m_bSilenceP1</c>).</summary>
    private bool _silenceP1;

    /// <summary>The phase 2 copy of <see cref="_silenceP1"/> (<c>m_bSilenceP2</c>).</summary>
    private bool _silenceP2;

    /// <summary>
    /// The 7-bit length counter (<c>m_uLengthP1</c>): the upper three bits are loaded from the ROM length, the
    /// middle two from the ROM repeat and/or XRepeat. Bit 0 marks the mirrored half and bit 1 the internal silence
    /// in voiced mode. Incremented each pitch period quarter.
    /// </summary>
    private int _lengthP1;

    /// <summary>The phase 2 copy of <see cref="_lengthP1"/> (<c>m_uLengthP2</c>).</summary>
    private int _lengthP2;

    /// <summary>The 2-bit repeat count, loaded from the ROM repeat (<c>m_uXRepeatP1</c>).</summary>
    private int _xRepeatP1;

    /// <summary>The 2-bit old delta (<c>m_uDeltaOldP1</c>).</summary>
    private int _deltaOldP1;

    /// <summary>The phase 2 copy of <see cref="_deltaOldP1"/> (<c>m_uDeltaOldP2</c>).</summary>
    private int _deltaOldP2;

    /// <summary>The 4-bit audio output, calculated during phase 1 (<c>m_uOutputP1</c>).</summary>
    private int _outputP1 = ChipConstants.SilentOutput;

    /// <summary>Derived signal: DAR04To00 is at its maximum (<c>m_bDAR04To00CarryP2</c>).</summary>
    private bool _dar04To00CarryP2;

    /// <summary>Derived signal: a pitch period ends (<c>m_bPPQCarryP2</c>).</summary>
    private bool _pitchQuarterCarryP2;

    /// <summary>Derived signal: a repeat ends (<c>m_bRepeatCarryP2</c>).</summary>
    private bool _repeatCarryP2;

    /// <summary>Derived signal: the syllable ends (<c>m_bLengthCarryP2</c>).</summary>
    private bool _lengthCarryP2;

    /// <summary>The ROM address (<c>m_uRomAddrP1</c>).</summary>
    private int _romAddressP1;

    /// <summary>The ROM address pins A0-A11, which change on phase 2 (<c>m_uRomAddrP2</c>).</summary>
    private int _romAddressP2;

    /// <summary>The START input pin (<c>m_bStart</c>).</summary>
    private bool _start;

    /// <summary>The 6-bit word number on the speech address bus C0-C5 (<c>m_uWord</c>).</summary>
    private int _word;

    /// <summary>
    /// Initializes a new instance of the <see cref="S14001AChip"/> class, idle and silent, as after MAME's
    /// <c>device_start</c>.
    /// </summary>
    /// <param name="rom">The speech ROM wired to the chip's address and data pins.</param>
    /// <exception cref="ArgumentNullException"><paramref name="rom"/> is <see langword="null"/>.</exception>
    public S14001AChip(SpeechRom rom)
    {
        _rom = rom ?? throw new ArgumentNullException(nameof(rom));
    }

    /// <summary>
    /// Gets a value indicating whether the chip is saying a word: the /BUSY pin, which changes on phase 1
    /// (<c>m_bBusyP1</c>, read by MAME <c>busy_r</c>).
    /// </summary>
    public bool IsBusy { get; private set; }

    /// <summary>
    /// Gets the 4-bit DAC output, 0 to 15, which changes on phase 2 (<c>m_uOutputP2</c>);
    /// <see cref="ChipConstants.SilentOutput"/> is silence.
    /// </summary>
    public int Output { get; private set; } = ChipConstants.SilentOutput;

    /// <summary>
    /// Puts a word number on the speech address bus C0-C5 (MAME <c>data_w</c>).
    /// </summary>
    /// <param name="word">The word number, 0 to <see cref="ChipConstants.MaximumWordNumber"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="word"/> does not fit the 6-bit bus.</exception>
    public void SelectWord(int word)
    {
        if (word is < 0 or > ChipConstants.MaximumWordNumber)
        {
            throw new ArgumentOutOfRangeException(nameof(word), word, "The S14001A's word number is 6 bits.");
        }

        _word = word;
    }

    /// <summary>
    /// Switches the speech ROM wired to the chip, as an external bank switch does. MAME supports this through
    /// <c>device_rom_interface</c> (<c>rom_bank_pre_change</c>), and its notes on the chip mention TSI speech
    /// boards with external speech ROM banking. Switch only while the chip is not busy, as real boards do.
    /// </summary>
    /// <param name="rom">The new ROM bank.</param>
    /// <exception cref="ArgumentNullException"><paramref name="rom"/> is <see langword="null"/>.</exception>
    public void SelectRom(SpeechRom rom) => _rom = rom ?? throw new ArgumentNullException(nameof(rom));

    /// <summary>
    /// Drives the START pin (MAME <c>start_w</c>). A rising edge starts the word on the bus.
    /// </summary>
    /// <param name="isHigh">The new level of the pin.</param>
    public void SetStart(bool isHigh)
    {
        if (isHigh && !_start)
        {
            _stateP1 = ChipState.WordWait;
        }

        _start = isHigh;
    }

    /// <summary>
    /// Toggles the internal clock once: half a cycle (MAME <c>Clock</c>). Read <see cref="Output"/> afterwards
    /// for the next sample.
    /// </summary>
    public void Clock()
    {
        // On the actual chip all phase 1 values have to be refreshed from phase 2 values or lose their charge.
        // Here, as in MAME, phase 1 values are only changed when they differ from the preceding phase 2 values.
        if (_phase1)
        {
            TransferToPhase2();
            return;
        }

        _phase1 = true;
        RunPhase1();
    }

    private void TransferToPhase2()
    {
        _phase1 = false;

        _dar04To00P2 = _dar04To00P1;
        _cwarP2 = _cwarP1;
        _stopP2 = _stopP1;
        _voicedP2 = _voicedP1;
        _silenceP2 = _silenceP1;
        _lengthP2 = _lengthP1;
        _deltaOldP2 = _deltaOldP1;

        Output = _outputP1;
        _romAddressP2 = _romAddressP1;

        // Set up the carries from the phase 2 values.
        _dar04To00CarryP2 = _dar04To00P2 == ChipConstants.Dar04To00Maximum;
        _pitchQuarterCarryP2 = _dar04To00CarryP2 && (_lengthP2 & ChipConstants.PitchQuarterMask) == ChipConstants.PitchQuarterMask;
        _repeatCarryP2 = _pitchQuarterCarryP2 && (_lengthP2 & ChipConstants.RepeatCountMask) == ChipConstants.RepeatCountMask;
        _lengthCarryP2 = _repeatCarryP2 && _lengthP2 == ChipConstants.LengthMaximum;
    }

    private void RunPhase1()
    {
        switch (_stateP1)
        {
            case ChipState.Idle:
                RunIdle();
                break;

            case ChipState.WordWait:
                RunWordWait();
                break;

            case ChipState.CwarMsb:
                RunCwarMsb();
                break;

            case ChipState.CwarLsb:
                RunCwarLsb();
                break;

            case ChipState.DarMsb:
                RunDarMsb();
                break;

            case ChipState.ControlBits:
                RunControlBits();
                break;

            case ChipState.Play:
                RunPlay();
                break;

            case ChipState.Delay:
                _outputP1 = ChipConstants.SilentOutput;
                _stateP1 = NextStateUnlessStarted(ChipState.Idle);
                break;

            default:
                throw new InvalidOperationException($"The chip is in an unknown state, {_stateP1}.");
        }
    }

    private void RunIdle()
    {
        _outputP1 = ChipConstants.SilentOutput;

        if (_start)
        {
            _stateP1 = ChipState.WordWait;
        }

        IsBusy = false;
    }

    private void RunWordWait()
    {
        // The delta address register latches the word number into bits 03 to 08; all other bits are forced
        // to 0. 04 to 08 makes a multiply by two.
        _dar13To05P1 = (_word & ChipConstants.WordHighBitsMask) >> ChipConstants.WordHighBitsShift;
        _dar04To00P1 = (_word & ChipConstants.WordLowBitsMask) << ChipConstants.WordLowBitsShift;
        _romAddressP1 = DeltaAddress(_dar13To05P1, _dar04To00P1);

        _outputP1 = ChipConstants.SilentOutput;
        _stateP1 = _start ? ChipState.WordWait : ChipState.CwarMsb;

        IsBusy = true;
    }

    private void RunCwarMsb()
    {
        // Use the DAR to load the 8 MSBs of the CWAR, from the ROM address set up in the previous state.
        _cwarP1 = ReadRom(_romAddressP2) << ChipConstants.CwarMsbShift;

        // Increment the DAR by 4; its 2 LSBs count deltas within a byte. Emulate a 5-bit counter.
        _dar04To00P1 += ChipConstants.Dar04To00ByteStep;

        if (_dar04To00P1 >= ChipConstants.Dar04To00Modulus)
        {
            _dar04To00P1 = 0;
        }

        _romAddressP1 = DeltaAddress(_dar13To05P1, _dar04To00P1);

        _outputP1 = ChipConstants.SilentOutput;
        _stateP1 = NextStateUnlessStarted(ChipState.CwarLsb);
    }

    private void RunCwarLsb()
    {
        // The 4 LSBs come from ROM D7-D4, at the address set up in the previous state.
        _cwarP1 = _cwarP2 | (ReadRom(_romAddressP2) >> ChipConstants.CwarLsbShift);
        _romAddressP1 = _cwarP1;

        _outputP1 = ChipConstants.SilentOutput;
        _stateP1 = NextStateUnlessStarted(ChipState.DarMsb);
    }

    private void RunDarMsb()
    {
        // A 9-bit counter: 8 MSBs from the ROM, LSB zeroed.
        _dar13To05P1 = ReadRom(_romAddressP2) << ChipConstants.DarMsbShift;
        _dar04To00P1 = 0;
        _cwarP1++;
        _romAddressP1 = _cwarP1;

        _outputP1 = ChipConstants.SilentOutput;
        _stateP1 = NextStateUnlessStarted(ChipState.ControlBits);
    }

    private void RunControlBits()
    {
        int data = ReadRom(_romAddressP2);

        _stopP1 = (data & ChipConstants.StopBit) != 0;
        _voicedP1 = (data & ChipConstants.VoicedBit) != 0;
        _silenceP1 = (data & ChipConstants.SilenceBit) != 0;
        _xRepeatP1 = data & ChipConstants.RepeatMask;

        // Includes the external length and repeat.
        _lengthP1 = (data & ChipConstants.LengthAndRepeatMask) << ChipConstants.LengthShift;
        _dar04To00P1 = 0;

        // Gets ready for the next DARMSB.
        _cwarP1++;
        _romAddressP1 = DeltaAddress(_dar13To05P1, _dar04To00P1);

        _outputP1 = ChipConstants.SilentOutput;
        _stateP1 = NextStateUnlessStarted(ChipState.Play);
    }

    private void RunPlay()
    {
        UpdateOutput();
        AdvanceCounters();

        // Construct the next ROM address; a voiced mirrored half counts backwards.
        int deltaCounter = _dar04To00P1;

        if (_voicedP2 && (_lengthP1 & DeltaConstants.MirrorBit) != 0)
        {
            deltaCounter ^= ChipConstants.MirrorAddressMask;
        }

        _romAddressP1 = DeltaAddress(_dar13To05P1, deltaCounter);

        if (_start)
        {
            _stateP1 = ChipState.WordWait;
        }
        else if (_stopP2 && _lengthCarryP2)
        {
            _stateP1 = ChipState.Delay;
        }
        else if (_lengthCarryP2)
        {
            _stateP1 = ChipState.DarMsb;

            // Output the correct address.
            _romAddressP1 = _cwarP1;
        }
        else
        {
            _stateP1 = ChipState.Play;
        }
    }

    private void UpdateOutput()
    {
        int pitchQuarter = _lengthP2 & ChipConstants.PitchQuarterMask;
        bool isQuarterStart = _dar04To00P2 == 0;

        int delta = DeltaModulator.SelectDelta(
            _voicedP2,
            pitchQuarter,
            _dar04To00P2 & ChipConstants.DeltaSelectMask,
            ReadRom(_romAddressP2));

        DeltaStep step = DeltaModulator.CalculateStep(_voicedP2, pitchQuarter, isQuarterStart, delta, _deltaOldP2);
        _deltaOldP1 = delta;

        _outputP1 = DeltaModulator.CalculateOutput(_voicedP2, _silenceP2, pitchQuarter, isQuarterStart, Output, step);
    }

    private void AdvanceCounters()
    {
        _dar04To00P1++;

        if (_dar04To00CarryP2)
        {
            // Pitch period quarter end: emulate a 5-bit counter. The length counter's two low bits count
            // quarter pitch periods; emulate a 7-bit counter.
            _dar04To00P1 = 0;
            _lengthP1++;

            if (_lengthP1 >= ChipConstants.LengthModulus)
            {
                _lengthP1 = 0;
            }
        }

        if (_voicedP2 && _repeatCarryP2)
        {
            // Repeat complete: keep the current length, reload the repeat from the external repeat, and
            // advance the ROM address 8 bytes.
            _lengthP1 &= ChipConstants.LengthKeepMask;
            _lengthP1 |= _xRepeatP1 << ChipConstants.LengthShift;
            AdvanceDar13To05();
        }

        if (!_voicedP2 && _dar04To00CarryP2)
        {
            // Unvoiced speech advances each quarter pitch period; its repeat counter is not reloaded.
            AdvanceDar13To05();
        }
    }

    private void AdvanceDar13To05()
    {
        // Advances the ROM address 8 bytes; emulate a 9-bit counter.
        _dar13To05P1++;

        if (_dar13To05P1 >= ChipConstants.Dar13To05Modulus)
        {
            _dar13To05P1 = 0;
        }
    }

    private ChipState NextStateUnlessStarted(ChipState next) => _start ? ChipState.WordWait : next;

    private int ReadRom(int address) => _rom.ReadByte(address & ChipConstants.RomAddressMask);

    private static int DeltaAddress(int dar13To05, int dar04To00) =>
        (dar13To05 << ChipConstants.Dar13To05AddressShift) | (dar04To00 >> ChipConstants.Dar04To00AddressShift);
}
