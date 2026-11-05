// <copyright file="Season21Npc.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.Interfaces;

/// <summary>
/// A passive NPC of a <see cref="Season21MapSet"/>, which has no window.
/// </summary>
/// <param name="Number">The number of the NPC.</param>
/// <param name="Name">The name.</param>
/// <param name="Map">The number of the map.</param>
/// <param name="X">The x coordinate.</param>
/// <param name="Y">The y coordinate.</param>
/// <param name="Direction">The direction.</param>
internal sealed record Season21Npc(short Number, LocalizedString Name, byte Map, byte X, byte Y, Direction Direction);
