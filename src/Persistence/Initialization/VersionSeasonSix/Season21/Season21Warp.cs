// <copyright file="Season21Warp.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.Interfaces;

/// <summary>
/// An entry of the warp list of a <see cref="Season21MapSet"/>.
/// </summary>
/// <param name="Index">The index of the warp list entry.</param>
/// <param name="Name">The name.</param>
/// <param name="Gate">The number of the target gate.</param>
internal sealed record Season21Warp(ushort Index, LocalizedString Name, short Gate);
