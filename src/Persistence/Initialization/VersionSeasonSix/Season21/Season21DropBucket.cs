// <copyright file="Season21DropBucket.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// Items of a drop list which share a drop weight; one of them is picked at random.
/// </summary>
/// <param name="Weight">The weight of each item, against the other items of the list.</param>
/// <param name="Items">The items, as <c>group * 512 + number</c>.</param>
internal sealed record Season21DropBucket(int Weight, IReadOnlyList<short> Items);
