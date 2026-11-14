using System;
using System.Collections.Generic;
using S14001ASpeak.Synthesis;

namespace S14001ASpeak.Tests;

/// <summary>
/// Says one word on a chip and records what it did.
/// </summary>
internal sealed class ChipRun
{
    /// <summary>The most clocks a test word may take before the test gives up.</summary>
    private const int ClockLimit = 1_000_000;

    private ChipRun(IReadOnlyList<int> outputs, int busyClocks)
    {
        Outputs = outputs;
        BusyClocks = busyClocks;
    }

    /// <summary>Gets the output after every clock, from the START pulse until the chip is no longer busy.</summary>
    public IReadOnlyList<int> Outputs { get; }

    /// <summary>Gets how many clocks the chip was busy for.</summary>
    public int BusyClocks { get; }

    /// <summary>
    /// Says <paramref name="word"/> from <paramref name="rom"/> on a new chip.
    /// </summary>
    /// <param name="rom">The speech ROM.</param>
    /// <param name="word">The word number.</param>
    /// <returns>What the chip did.</returns>
    /// <exception cref="InvalidOperationException">The word never ended.</exception>
    public static ChipRun Say(SpeechRom rom, int word)
    {
        var chip = new S14001AChip(rom);
        chip.SelectWord(word);
        chip.SetStart(true);
        chip.SetStart(false);

        List<int> outputs = [];
        int busyClocks = 0;
        bool hasStarted = false;

        while (outputs.Count < ClockLimit)
        {
            chip.Clock();
            outputs.Add(chip.Output);

            if (chip.IsBusy)
            {
                hasStarted = true;
                busyClocks++;
            }
            else if (hasStarted)
            {
                return new ChipRun(outputs, busyClocks);
            }
        }

        throw new InvalidOperationException("The word never ended.");
    }
}
