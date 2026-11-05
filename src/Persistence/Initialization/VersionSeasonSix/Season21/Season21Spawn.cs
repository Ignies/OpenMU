// <copyright file="Season21Spawn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// A monster spawn area of a <see cref="Season21MapSet"/>, its coordinates are inclusive.
/// </summary>
/// <param name="Monster">The number of the monster.</param>
/// <param name="X1">The first x coordinate.</param>
/// <param name="Y1">The first y coordinate.</param>
/// <param name="X2">The last x coordinate.</param>
/// <param name="Y2">The last y coordinate.</param>
/// <param name="Quantity">The number of monsters.</param>
/// <param name="Map">The number of the map of the set, or <c>0</c> for every map of the set.</param>
internal sealed record Season21Spawn(short Monster, byte X1, byte Y1, byte X2, byte Y2, short Quantity, byte Map = 0);
