// <copyright file="StarSmithDice.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The Forge's own seeded dice (SplitMix64). It does not depend on <see cref="Random"/>, whose sequence a runtime may
/// change, so a sky dealt today is dealt the same after an upgrade and a replay can verify it.
/// </summary>
internal sealed class StarSmithDice
{
    private ulong state;

    /// <summary>
    /// Initializes a new instance of the <see cref="StarSmithDice"/> class.
    /// </summary>
    /// <param name="seed">The seed.</param>
    public StarSmithDice(long seed)
    {
        this.state = unchecked((ulong)seed);
    }

    /// <summary>
    /// The next 64 random bits.
    /// </summary>
    /// <returns>The bits.</returns>
    public ulong NextBits()
    {
        unchecked
        {
            var z = this.state += 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }

    /// <summary>
    /// A number from 0 up to, not including, a maximum.
    /// </summary>
    /// <param name="maximum">The maximum (0 or less gives 0).</param>
    /// <returns>The number.</returns>
    public int Next(int maximum) => maximum <= 0 ? 0 : (int)(this.NextBits() % (ulong)maximum);

    /// <summary>
    /// A number from a minimum up to, not including, a maximum.
    /// </summary>
    /// <param name="minimum">The minimum.</param>
    /// <param name="maximum">The maximum.</param>
    /// <returns>The number.</returns>
    public int Next(int minimum, int maximum) => minimum + this.Next(maximum - minimum);

    /// <summary>
    /// A number from 0 up to, not including, 1.
    /// </summary>
    /// <returns>The number.</returns>
    public double NextDouble() => (this.NextBits() >> 11) * (1.0 / (1UL << 53));

    /// <summary>
    /// Shuffles a list in place (Fisher-Yates).
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="list">The list.</param>
    public void Shuffle<T>(IList<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = this.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
