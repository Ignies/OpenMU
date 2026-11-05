// <copyright file="Season21MapSet.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// A set of maps which were added after Season 6, as the Season 21 server data defines them.
/// </summary>
/// <param name="IntroducedIn">The version which introduced the maps of the set.</param>
/// <param name="EntryLevel">The total level (level and master level) which the maps require.</param>
/// <param name="EntryMoney">The money which the warp list entries cost.</param>
/// <param name="Maps">The maps.</param>
/// <param name="Monsters">The monsters which are defined by the set. A spawn may refer to a monster of an earlier set.</param>
/// <param name="Spawns">The monster spawn areas.</param>
/// <param name="Gates">The gates of the maps of the set, and the entrances on other maps which lead to them.</param>
/// <param name="Warps">The warp list entries.</param>
internal sealed record Season21MapSet(
    GameVersion IntroducedIn,
    int EntryLevel,
    int EntryMoney,
    IReadOnlyList<Season21Map> Maps,
    IReadOnlyList<Season21Monster> Monsters,
    IReadOnlyList<Season21Spawn> Spawns,
    IReadOnlyList<Season21Gate> Gates,
    IReadOnlyList<Season21Warp> Warps)
{
    /// <summary>
    /// Gets the passive NPCs.
    /// </summary>
    public IReadOnlyList<Season21Npc> Npcs { get; init; } = [];
}
