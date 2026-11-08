// <copyright file="Season21IllusionTempleLeagueData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// The statues of the Illusion Temple League (S21Emu <c>Event/IlusionTempleLeague/IllusionTemple_League_NPC.xml</c>), the
/// same on both maps.
/// </summary>
internal static class Season21IllusionTempleLeagueData
{
    /// <summary>
    /// The maps of the league temples, by game level.
    /// </summary>
    internal static readonly byte[] MapNumbers = [98, 99];

    /// <summary>
    /// The statues: the Cursed Statue (658) and the ten Captured Stone Statues (659-668).
    /// </summary>
    internal static readonly (short Number, byte X, byte Y)[] Statues =
    [
        (658, 169, 84),
        (659, 136, 101),
        (660, 151, 118),
        (661, 151, 87),
        (662, 166, 102),
        (663, 173, 66),
        (664, 187, 81),
        (665, 187, 51),
        (666, 203, 67),
        (667, 133, 121),
        (668, 207, 47),
    ];
}
