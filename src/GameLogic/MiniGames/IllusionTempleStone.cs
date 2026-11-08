// <copyright file="IllusionTempleStone.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames;

using MUnique.OpenMU.GameLogic.NPC;

/// <summary>
/// A Captured Stone Statue of the renewed Illusion Temple, into which a team sealed the Cursed Hollow.
/// </summary>
/// <param name="Npc">The statue.</param>
public sealed record IllusionTempleStone(NonPlayerCharacter Npc)
{
    /// <summary>
    /// Gets the index of the stone, 0 to 9 (NPC 659 to 668).
    /// </summary>
    public byte Index => (byte)(this.Npc.Definition.Number - 659);

    /// <summary>
    /// Gets or sets the team which sealed the hollow into the stone, <c>null</c> while it's free.
    /// </summary>
    public IllusionTempleTeam? Holder { get; set; }

    /// <summary>
    /// Gets or sets when the hollow was sealed.
    /// </summary>
    public DateTime SealedAt { get; set; }
}
