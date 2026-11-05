// <copyright file="Season21GateKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// The kind of a <see cref="Season21Gate"/>.
/// </summary>
internal enum Season21GateKind
{
    /// <summary>
    /// A target gate where characters spawn, e.g. the target of a warp list entry.
    /// </summary>
    Spawn = 0,

    /// <summary>
    /// An entrance, which leads to a target gate.
    /// </summary>
    Entrance = 1,

    /// <summary>
    /// A target gate of an entrance.
    /// </summary>
    Target = 2,
}
