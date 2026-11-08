// <copyright file="Season21MovedGates.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// The entrances of <see cref="Season21MapData"/> which moved after a version of the maps update had added them, with
/// their old areas, and the repair of a database which still has them (see Readme.md, "Moved entrances").
/// </summary>
internal static class Season21MovedGates
{
    /// <summary>
    /// Gets the old areas: gate number and area.
    /// </summary>
    public static IReadOnlyList<(short Number, byte X1, byte Y1, byte X2, byte Y2)> OldAreas { get; } =
    [
        (635, 223, 118, 226, 120),
        (719, 125, 175, 126, 176),
    ];

    /// <summary>
    /// Moves each entrance which still has its old area to the area of the data; an area the operator chose stays.
    /// </summary>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Repair(GameConfiguration gameConfiguration)
    {
        foreach (var old in OldAreas)
        {
            var row = Season21MapData.All.SelectMany(set => set.Gates).Single(g => g.Number == old.Number);
            var gate = gameConfiguration.Maps
                .Where(m => m.Number == row.Map && m.Discriminator == 0)
                .SelectMany(m => m.EnterGates)
                .FirstOrDefault(g => g.Number == old.Number && (g.X1, g.Y1, g.X2, g.Y2) == (old.X1, old.Y1, old.X2, old.Y2));
            if (gate is not null)
            {
                (gate.X1, gate.Y1, gate.X2, gate.Y2) = (row.X1, row.Y1, row.X2, row.Y2);
            }
        }
    }
}
