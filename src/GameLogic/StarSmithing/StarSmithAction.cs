// <copyright file="StarSmithAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A smithing action (EE 93-96, the Action byte of EE 97).
/// </summary>
public enum StarSmithAction : byte
{
    /// <summary>A recolour (Zen, never random).</summary>
    Recolor = 1,

    /// <summary>A name.</summary>
    Rename,

    /// <summary>A lore.</summary>
    Lore,

    /// <summary>A lock: permanent for players.</summary>
    Lock,

    /// <summary>A recipe attempt in the Constellation Forge (EE 99-9F); EE 92 shows it when sealed.</summary>
    Forge,
}
