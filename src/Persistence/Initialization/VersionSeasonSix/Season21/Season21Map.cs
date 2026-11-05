// <copyright file="Season21Map.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.Interfaces;

/// <summary>
/// A map of a <see cref="Season21MapSet"/>.
/// </summary>
/// <param name="Number">The map number.</param>
/// <param name="Name">The name of the map.</param>
/// <param name="SafezoneMap">The number of the map on which characters respawn after their death.</param>
/// <param name="TerrainNumber">The number of the embedded terrain file, <c>Resources/Season21/Terrain{TerrainNumber}.att</c>.</param>
/// <param name="IntroducedIn">The version which introduced the map, if it's a later one than the one of its set.</param>
internal sealed record Season21Map(byte Number, LocalizedString Name, byte SafezoneMap, short TerrainNumber, GameVersion? IntroducedIn = null);
