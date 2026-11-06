// <copyright file="Season21EliteZone.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.EliteZones;

/// <summary>
/// An elite zone of the data: its entrance, its area and the area in which its elites spawn.
/// </summary>
internal sealed record Season21EliteZone(
    byte Map,
    EliteZoneElement Element,
    byte EntranceX,
    byte EntranceY,
    byte X1,
    byte Y1,
    byte X2,
    byte Y2,
    byte SpawnX1,
    byte SpawnY1,
    byte SpawnX2,
    byte SpawnY2,
    IReadOnlyList<Season21EliteSpawn> Elites);
