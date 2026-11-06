// <copyright file="Season21EffectRow.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// A row of S21Emu <c>Effect.txt</c>.
/// </summary>
internal sealed class Season21EffectRow
{
    /// <summary>
    /// Gets the effect number.
    /// </summary>
    public short Number { get; init; }

    /// <summary>
    /// Gets the group; effects of one group replace each other.
    /// </summary>
    public short Group { get; init; }
}
