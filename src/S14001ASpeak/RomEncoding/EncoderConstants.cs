using S14001ASpeak.Speech;

namespace S14001ASpeak.RomEncoding;

/// <summary>
/// The constants of the S14001A speech encoder: the chip's data layout (from MAME's <c>s14001a.cpp</c>) and the
/// encoder's own analysis settings.
/// </summary>
internal static class EncoderConstants
{
    /// <summary>The chip reads one 2-bit delta per full clock cycle: every second half cycle (<c>Clock</c> call).</summary>
    public const int ClocksPerDelta = 2;

    /// <summary>
    /// The rate speech is encoded at: one delta per cycle of the chip clock the board's default pitch selects,
    /// about 12 kHz. Played at that clock, the speech has its natural speed.
    /// </summary>
    public const double DeltaRateHz =
        BoardTiming.SpeechClockHz / (BoardTiming.PitchDividerBase - VoiceLimits.DefaultPitch) / BoardTiming.SecondDivider
        / ClocksPerDelta;

    /// <summary>A pitch period quarter is 32 deltas: one full count of the 5-bit DAR04To00 counter.</summary>
    public const int DeltasPerQuarter = 32;

    /// <summary>A pitch period is four quarters, counted by the length counter's two low bits.</summary>
    public const int QuartersPerFrame = 4;

    /// <summary>
    /// The encoder works in frames of one chip pitch period, 128 deltas (about 10.6 ms). A voiced pitch period
    /// always lasts this long, which gives the chip its fixed, buzzing pitch.
    /// </summary>
    public const int DeltasPerFrame = DeltasPerQuarter * QuartersPerFrame;

    /// <summary>A ROM byte holds four 2-bit deltas, the first in bits 7-6.</summary>
    public const int DeltasPerByte = 4;

    /// <summary>The bits of one delta.</summary>
    public const int BitsPerDelta = 2;

    /// <summary>The number of different deltas.</summary>
    public const int DeltaValues = 4;

    /// <summary>A quarter's deltas fill 8 ROM bytes.</summary>
    public const int BytesPerQuarter = DeltasPerQuarter / DeltasPerByte;

    /// <summary>A voiced frame stores one quarter: played forwards, then mirrored, then silent for two quarters.</summary>
    public const int VoicedBytesPerFrame = BytesPerQuarter;

    /// <summary>An unvoiced frame stores all four quarters, as unvoiced speech advances 8 bytes every quarter.</summary>
    public const int UnvoicedBytesPerFrame = BytesPerQuarter * QuartersPerFrame;

    /// <summary>
    /// The most frames in a voiced syllable: with repeat 3 each length step plays one pitch period, and the
    /// 3-bit length field allows 1 to 8 steps.
    /// </summary>
    public const int MaximumVoicedFrames = 8;

    /// <summary>
    /// The most frames in an unvoiced or silent syllable: the 5-bit length and repeat field counts 4 to 128
    /// quarters, in steps of one frame.
    /// </summary>
    public const int MaximumUnvoicedFrames = 32;

    /// <summary>The voiced syllable's repeat field: 3 plays each stored pitch period once.</summary>
    public const int VoicedRepeat = 3;

    /// <summary>The length field's steps that a syllable of one frame leaves unplayed, plus one.</summary>
    public const int VoicedLengthSteps = 8;

    /// <summary>The bits of the repeat field in a control byte.</summary>
    public const int RepeatBits = 2;

    /// <summary>The word number every encoded ROM bank says.</summary>
    public const int WordNumber = 0;

    /// <summary>The word table entry for <see cref="WordNumber"/> fills the first two bytes of a bank.</summary>
    public const int WordTableSize = 2;

    /// <summary>A syllable list entry is two bytes: the delta address MSBs and the control byte.</summary>
    public const int SyllableEntrySize = 2;

    /// <summary>Delta data must start on a 16-byte boundary: the DARMSB byte is the address shifted right 4.</summary>
    public const int DataAlignment = 16;

    /// <summary>Shift from a data address to its DARMSB byte.</summary>
    public const int DataAddressShift = 4;

    /// <summary>Shift from a syllable list address to the word table's first byte (the CWAR's 8 MSBs).</summary>
    public const int ListAddressHighShift = 4;

    /// <summary>Mask of the syllable list address's 4 LSBs, stored in bits 7-4 of the word table's second byte.</summary>
    public const int ListAddressLowMask = 0x0f;

    /// <summary>The level above rest that the loudest of the speech is scaled to (rest is 7, full scale 15).</summary>
    public const double TargetPeakLevel = 7.5;

    /// <summary>The highest level above rest the chip can output.</summary>
    public const double MaximumLevelAboveRest = 8;

    /// <summary>The lowest level below rest the chip can output.</summary>
    public const double MaximumLevelBelowRest = -7;

    /// <summary>
    /// The fraction of samples allowed to clip when scaling: the scale is set by this quantile of the sample
    /// magnitudes, so a few spikes do not make the rest of the speech quiet.
    /// </summary>
    public const double ScalingQuantile = 0.995;

    /// <summary>A frame quieter than this fraction of the loudest frame's RMS level is silent.</summary>
    public const double SilenceThreshold = 0.04;

    /// <summary>A frame whose zero-crossing rate (crossings per sample) is above this is unvoiced.</summary>
    public const double UnvoicedCrossingRate = 0.3;

    /// <summary>A frame whose normalized autocorrelation peak is at least this is voiced.</summary>
    public const double VoicingThreshold = 0.4;

    /// <summary>The highest voice pitch looked for, in hertz.</summary>
    public const double MaximumPitchHz = 400;

    /// <summary>The lowest voice pitch looked for, in hertz.</summary>
    public const double MinimumPitchHz = 70;

    /// <summary>The audio fed to the encoder is low-pass filtered to this fraction of the delta rate.</summary>
    public const double ResamplerCutoff = 0.45;

    /// <summary>Taps on each side of the resampler's low-pass filter.</summary>
    public const int ResamplerHalfTaps = 32;
}
