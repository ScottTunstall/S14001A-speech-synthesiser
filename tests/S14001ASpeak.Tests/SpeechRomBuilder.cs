using System;
using System.Collections.Generic;
using S14001ASpeak.Synthesis;

namespace S14001ASpeak.Tests;

/// <summary>
/// Builds synthetic speech ROMs laid out as the S14001A expects: a word table of 12-bit syllable list addresses,
/// syllable lists of (delta address, control byte) pairs, and blocks of delta data.
/// </summary>
internal sealed class SpeechRomBuilder
{
    /// <summary>Syllable lists start after a full 64-word table.</summary>
    private const int FirstSyllableListAddress = 0x080;

    /// <summary>Delta data blocks start here, one per syllable.</summary>
    private const int FirstDataBlockAddress = 0x400;

    /// <summary>Each syllable gets a block this size, enough for the longest syllable a test uses.</summary>
    private const int DataBlockSize = 0x100;

    /// <summary>A delta address register MSB byte addresses 16-byte units.</summary>
    private const int DataAddressShift = 4;

    private readonly byte[] _image = new byte[ChipConstants.RomSize];
    private int _nextSyllableList = FirstSyllableListAddress;
    private int _nextDataBlock = FirstDataBlockAddress;

    /// <summary>
    /// Adds a word made of <paramref name="syllables"/>; the last one gets the stop bit.
    /// </summary>
    /// <param name="word">The word number.</param>
    /// <param name="syllables">The syllables, in order.</param>
    public void AddWord(int word, IReadOnlyList<TestSyllable> syllables)
    {
        int listAddress = _nextSyllableList;

        _image[word * 2] = (byte)(listAddress >> 4);
        _image[(word * 2) + 1] = (byte)((listAddress & 0x0f) << 4);

        for (int index = 0; index < syllables.Count; index++)
        {
            TestSyllable syllable = syllables[index];
            bool isLast = index == syllables.Count - 1;

            _image[_nextSyllableList] = (byte)(_nextDataBlock >> DataAddressShift);
            _image[_nextSyllableList + 1] = ControlByte(syllable, isLast);
            _nextSyllableList += 2;

            for (int offset = 0; offset < DataBlockSize; offset++)
            {
                _image[_nextDataBlock + offset] = (byte)syllable.Fill;
            }

            _nextDataBlock += DataBlockSize;
        }
    }

    /// <summary>
    /// Returns the ROM built so far.
    /// </summary>
    /// <returns>The ROM.</returns>
    public SpeechRom Build() => new(_image);

    /// <summary>
    /// Returns a ROM holding one word.
    /// </summary>
    /// <param name="word">The word number.</param>
    /// <param name="syllables">The word's syllables.</param>
    /// <returns>The ROM.</returns>
    public static SpeechRom WithWord(int word, params TestSyllable[] syllables)
    {
        var builder = new SpeechRomBuilder();
        builder.AddWord(word, syllables);
        return builder.Build();
    }

    private static byte ControlByte(TestSyllable syllable, bool isLast)
    {
        if (syllable.Length is < 0 or > 7 || syllable.Repeat is < 0 or > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(syllable), syllable, "Length is 3 bits and repeat 2 bits.");
        }

        int control = (syllable.Length << 2) | syllable.Repeat;
        control |= isLast ? 0x80 : 0;
        control |= syllable.Voiced ? 0x40 : 0;
        control |= syllable.Silent ? 0x20 : 0;
        return (byte)control;
    }
}
