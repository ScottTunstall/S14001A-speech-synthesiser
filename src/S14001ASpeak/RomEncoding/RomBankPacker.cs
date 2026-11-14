using System;
using System.Collections.Generic;
using System.Linq;
using S14001ASpeak.Synthesis;

namespace S14001ASpeak.RomEncoding;

/// <summary>
/// Lays syllables out in 4 KB speech ROM banks, each holding one word: word <see cref="EncoderConstants.WordNumber"/>.
/// </summary>
/// <remarks>
/// A bank holds, in the layout the chip reads (MAME <c>s14001a.cpp</c>, states <c>WORDWAIT</c> to
/// <c>CTRLBITS</c>): the word table entry, a 12-bit syllable list address; the syllable list, a DARMSB byte and
/// a control byte per syllable; then each syllable's delta data on a 16-byte boundary. Syllables go into a bank
/// until the next does not fit; the last syllable in each bank gets the stop bit.
/// </remarks>
internal static class RomBankPacker
{
    /// <summary>
    /// Returns the banks that hold <paramref name="syllables"/>, in playing order.
    /// </summary>
    /// <param name="syllables">The syllables.</param>
    /// <returns>The banks; none if there are no syllables.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="syllables"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<SpeechRom> Pack(IReadOnlyList<EncodedSyllable> syllables)
    {
#if NET
        ArgumentNullException.ThrowIfNull(syllables);
#else
        if (syllables is null)
        {
            throw new ArgumentNullException(nameof(syllables));
        }
#endif

        List<SpeechRom> banks = [];
        List<EncodedSyllable> bank = [];

        foreach (EncodedSyllable syllable in syllables)
        {
            if (bank.Count > 0 && SizeOf([.. bank, syllable]) > ChipConstants.RomSize)
            {
                banks.Add(Build(bank));
                bank.Clear();
            }

            bank.Add(syllable);
        }

        if (bank.Count > 0)
        {
            banks.Add(Build(bank));
        }

        return banks;
    }

    private static int SizeOf(List<EncodedSyllable> syllables) =>
        DataStart(syllables.Count) + syllables.Sum(syllable => Align(syllable.Data.Count));

    private static int DataStart(int syllableCount) =>
        Align(EncoderConstants.WordTableSize + (syllableCount * EncoderConstants.SyllableEntrySize));

    private static int Align(int size) =>
        (size + EncoderConstants.DataAlignment - 1) / EncoderConstants.DataAlignment * EncoderConstants.DataAlignment;

    private static SpeechRom Build(List<EncodedSyllable> syllables)
    {
        byte[] image = new byte[ChipConstants.RomSize];
        const int ListAddress = EncoderConstants.WordTableSize;
        int dataAddress = DataStart(syllables.Count);

        // Word table entry: the CWAR's 8 MSBs, then its 4 LSBs in D7-D4.
        const int WordEntry = EncoderConstants.WordNumber * EncoderConstants.WordTableSize;
        image[WordEntry] = ListAddress >> EncoderConstants.ListAddressHighShift;
        image[WordEntry + 1] = (ListAddress & EncoderConstants.ListAddressLowMask) << EncoderConstants.ListAddressHighShift;

        for (int index = 0; index < syllables.Count; index++)
        {
            EncodedSyllable syllable = syllables[index];
            int entry = ListAddress + (index * EncoderConstants.SyllableEntrySize);

            // A silent syllable reads no data that matters, so it points at the start of the bank.
            image[entry] = (byte)(syllable.Data.Count == 0 ? 0 : dataAddress >> EncoderConstants.DataAddressShift);
            image[entry + 1] = SyllableControl.For(syllable.Kind, syllable.FrameCount, isLast: index == syllables.Count - 1);

            for (int offset = 0; offset < syllable.Data.Count; offset++)
            {
                image[dataAddress + offset] = syllable.Data[offset];
            }

            dataAddress += Align(syllable.Data.Count);
        }

        return new SpeechRom(image);
    }
}
