namespace S14001ASpeak.Tests;

/// <summary>
/// One syllable of a synthetic speech ROM word.
/// </summary>
/// <param name="Fill">The byte every delta byte of the syllable holds; 0xFF steps up, 0x00 steps down.</param>
/// <param name="Length">The 3-bit length field; higher is shorter.</param>
/// <param name="Repeat">The 2-bit repeat field.</param>
/// <param name="Voiced">Whether the syllable is voiced.</param>
/// <param name="Silent">Whether the syllable is silent.</param>
internal sealed record TestSyllable(int Fill, int Length = 7, int Repeat = 3, bool Voiced = false, bool Silent = false);
