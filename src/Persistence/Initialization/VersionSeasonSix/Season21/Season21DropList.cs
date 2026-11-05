// <copyright file="Season21DropList.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// The items which some monsters drop, from the Season 21 server data.
/// </summary>
/// <param name="Monsters">The numbers of the monsters which drop the items.</param>
/// <param name="Buckets">The items, by their drop weight.</param>
internal sealed record Season21DropList(IReadOnlyList<short> Monsters, IReadOnlyList<Season21DropBucket> Buckets);
