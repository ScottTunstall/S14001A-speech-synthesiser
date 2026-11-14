using System;
using System.Collections.Generic;
using System.Globalization;

namespace S14001ASpeak.Synthesis;

/// <summary>
/// The 4 KB speech ROM the S14001A reads: a word table, a syllable table and the delta-modulated sound.
/// </summary>
internal sealed class SpeechRom
{
    private readonly byte[] _image;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpeechRom"/> class.
    /// </summary>
    /// <param name="image">The ROM contents, exactly <see cref="ChipConstants.RomSize"/> bytes. They are copied.</param>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="image"/> is not <see cref="ChipConstants.RomSize"/> bytes long.</exception>
    public SpeechRom(IReadOnlyCollection<byte> image)
    {
#if NET
        ArgumentNullException.ThrowIfNull(image);
#else
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }
#endif

        if (image.Count != ChipConstants.RomSize)
        {
            throw new ArgumentException(
                string.Format(CultureInfo.InvariantCulture, "A speech ROM is {0} bytes, not {1}.", ChipConstants.RomSize, image.Count),
                nameof(image));
        }

        _image = [.. image];
    }

    /// <summary>
    /// Returns the byte at <paramref name="address"/>.
    /// </summary>
    /// <param name="address">The address, 0 to <see cref="ChipConstants.RomAddressMask"/>.</param>
    /// <returns>The byte.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="address"/> is outside the ROM.</exception>
    public int ReadByte(int address) => address is < 0 or > ChipConstants.RomAddressMask
        ? throw new ArgumentOutOfRangeException(nameof(address), address, "The address is outside the speech ROM.")
        : _image[address];
}
