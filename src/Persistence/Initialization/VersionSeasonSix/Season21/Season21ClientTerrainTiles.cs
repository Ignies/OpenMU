// <copyright file="Season21ClientTerrainTiles.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// The tiles where the server terrain of the data differs from the walk grid of the Season 21 client, which
/// <c>Resources/Season21</c> now holds, and the repair of a database which still has the data's values
/// (see Readme.md, "Client terrains").
/// </summary>
internal static class Season21ClientTerrainTiles
{
    private const int TerrainHeaderSize = 3;

    /// <summary>
    /// Gets the tiles: map number, coordinates, the value of the data and the value of the client.
    /// </summary>
    /// <remarks>
    /// Twisted Karutan's values of the data are the ones after <see cref="Season21EliteZoneData.TwistedKarutanOpenings"/>.
    /// </remarks>
    public static IReadOnlyList<(byte Map, byte X, byte Y, byte Old, byte New)> Tiles { get; } =
    [
        (112, 24, 124, 2, 0),
        (112, 230, 146, 1, 3),
        (135, 129, 43, 4, 6),
        (135, 139, 49, 12, 4),
        (135, 106, 176, 2, 0),
        (138, 94, 63, 4, 8),
        (138, 225, 213, 4, 8),
        (140, 74, 6, 4, 8),
        (140, 75, 7, 4, 8),
        (140, 84, 12, 4, 8),
        (140, 216, 14, 4, 8),
        (140, 153, 16, 4, 8),
        (140, 168, 17, 4, 8),
        (140, 175, 17, 4, 8),
        (140, 168, 18, 4, 8),
        (140, 168, 19, 4, 8),
        (140, 168, 20, 4, 8),
        (140, 218, 22, 4, 8),
        (140, 216, 23, 4, 8),
        (140, 120, 64, 4, 8),
        (140, 120, 65, 4, 8),
        (140, 212, 80, 4, 8),
        (140, 211, 81, 4, 8),
        (140, 202, 83, 4, 8),
        (140, 207, 83, 4, 8),
        (140, 141, 94, 4, 8),
        (140, 139, 127, 4, 8),
        (140, 140, 129, 4, 8),
        (140, 141, 130, 4, 8),
        (140, 141, 131, 4, 8),
        (140, 142, 137, 4, 8),
        (140, 142, 138, 4, 8),
        (140, 142, 139, 4, 8),
        (140, 210, 145, 4, 8),
        (140, 216, 154, 4, 8),
        (140, 214, 155, 4, 8),
        (140, 245, 176, 4, 8),
        (140, 245, 177, 4, 8),
        (140, 131, 183, 4, 8),
        (140, 245, 187, 4, 8),
        (140, 92, 197, 4, 8),
        (142, 148, 99, 4, 0),
        (142, 148, 100, 4, 0),
        (142, 40, 112, 0, 4),
        (142, 130, 122, 3, 1),
        (142, 39, 124, 0, 4),
        (142, 131, 124, 1, 3),
        (142, 212, 133, 4, 8),
        (142, 212, 134, 4, 8),
        (142, 208, 142, 4, 8),
        (142, 208, 143, 4, 8),
        (142, 208, 144, 4, 8),
        (142, 208, 148, 4, 8),
        (142, 208, 149, 4, 8),
        (142, 240, 151, 4, 0),
        (142, 163, 179, 4, 0),
        (142, 102, 231, 0, 4),
        (142, 103, 231, 0, 4),
        (142, 106, 232, 0, 4),
    ];

    /// <summary>
    /// Sets each tile which still has the value of the data to the value of the client; a value the operator chose stays.
    /// </summary>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Repair(GameConfiguration gameConfiguration)
    {
        foreach (var group in Tiles.GroupBy(t => t.Map))
        {
            if (gameConfiguration.Maps.FirstOrDefault(m => m.Number == group.Key && m.Discriminator == 0) is not { TerrainData: { Length: > TerrainHeaderSize } terrain } map)
            {
                continue;
            }

            var repaired = terrain.ToArray();
            foreach (var tile in group)
            {
                var index = TerrainHeaderSize + (tile.Y * 256) + tile.X;
                if (index < repaired.Length && repaired[index] == tile.Old)
                {
                    repaired[index] = tile.New;
                }
            }

            if (!repaired.AsSpan().SequenceEqual(terrain))
            {
                map.TerrainData = repaired;
            }
        }
    }
}
