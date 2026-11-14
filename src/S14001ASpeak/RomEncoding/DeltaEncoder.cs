using System;
using System.Collections.Generic;
using S14001ASpeak.Synthesis;

namespace S14001ASpeak.RomEncoding;

/// <summary>
/// Chooses the 2-bit deltas that make the chip's output follow a target waveform as closely as it can.
/// </summary>
/// <remarks>
/// The chip's next output depends on its current output (16 levels) and the previous delta (4 values), so the
/// search is a Viterbi pass over those 64 states, minimising the squared error. It uses the chip emulation's own
/// <see cref="DeltaModulator"/>, so what the chip plays is exactly what the search measured.
/// </remarks>
internal static class DeltaEncoder
{
    /// <summary>The number of decoder states: output level times previous delta.</summary>
    private const int StateCount = (ChipConstants.MaximumOutput + 1) * EncoderConstants.DeltaValues;

    /// <summary>
    /// Returns the deltas whose playback best follows <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The wanted output, as levels above rest, one per delta, starting at a pitch period.</param>
    /// <param name="voiced">
    /// <see langword="true"/> for the first quarter of a voiced pitch period; <see langword="false"/> for an
    /// unvoiced pitch period.
    /// </param>
    /// <returns>One delta, 0 to 3, per target sample.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<int> Encode(IReadOnlyList<double> target, bool voiced)
    {
#if NET
        ArgumentNullException.ThrowIfNull(target);
#else
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }
#endif

        int steps = target.Count;
        double[] cost = new double[StateCount];
        int[][] cameFrom = new int[steps][];
        Fill(cost, double.PositiveInfinity);

        // Before the first delta the chip rests; the pitch period start resets both the level and the old delta.
        cost[State(ChipConstants.SilentOutput, DeltaConstants.PitchPeriodStartDelta)] = 0;

        for (int step = 0; step < steps; step++)
        {
            cost = Advance(cost, cameFrom, step, target[step], voiced);
        }

        return Trace(cost, cameFrom);
    }

    private static double[] Advance(double[] cost, int[][] cameFrom, int step, double target, bool voiced)
    {
        // Voiced data is only the first quarter, played forwards; unvoiced data runs through all four quarters.
        int pitchQuarter = voiced ? 0 : step / EncoderConstants.DeltasPerQuarter;
        bool isQuarterStart = step % EncoderConstants.DeltasPerQuarter == 0;
        double[] next = new double[StateCount];
        Fill(next, double.PositiveInfinity);
        cameFrom[step] = new int[StateCount];

        for (int state = 0; state < StateCount; state++)
        {
            if (double.IsPositiveInfinity(cost[state]))
            {
                continue;
            }

            int level = state / EncoderConstants.DeltaValues;
            int oldDelta = state % EncoderConstants.DeltaValues;

            for (int delta = 0; delta < EncoderConstants.DeltaValues; delta++)
            {
                DeltaStep move = DeltaModulator.CalculateStep(voiced, pitchQuarter, isQuarterStart, delta, oldDelta);
                int output = DeltaModulator.CalculateOutput(voiced, false, pitchQuarter, isQuarterStart, level, move);
                double error = output - ChipConstants.SilentOutput - target;
                double total = cost[state] + (error * error);
                int nextState = State(output, delta);

                if (total < next[nextState])
                {
                    next[nextState] = total;
                    cameFrom[step][nextState] = state;
                }
            }
        }

        return next;
    }

    private static int[] Trace(double[] cost, int[][] cameFrom)
    {
        int steps = cameFrom.Length;
        int[] deltas = new int[steps];
        int state = 0;

        for (int candidate = 1; candidate < StateCount; candidate++)
        {
            if (cost[candidate] < cost[state])
            {
                state = candidate;
            }
        }

        for (int step = steps - 1; step >= 0; step--)
        {
            deltas[step] = state % EncoderConstants.DeltaValues;
            state = cameFrom[step][state];
        }

        return deltas;
    }

    private static int State(int level, int delta) => (level * EncoderConstants.DeltaValues) + delta;

    private static void Fill(double[] values, double value)
    {
        for (int index = 0; index < values.Length; index++)
        {
            values[index] = value;
        }
    }
}
