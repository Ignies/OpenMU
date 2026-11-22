// <copyright file="StarSmithGuidebookEntryKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The kind of a Guidebook row (the Kind byte of an EE 90 entry).
/// </summary>
public enum StarSmithGuidebookEntryKind : byte
{
    /// <summary>A found recipe: number, made count and name.</summary>
    Found,

    /// <summary>A recipe not found yet: its hint only.</summary>
    Sealed,
}
