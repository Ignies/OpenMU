// <copyright file="CharacterGuideQuest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.GuideQuests;

/// <summary>
/// The guide quest state of a character, kept in the state file.
/// </summary>
public class CharacterGuideQuest
{
    /// <summary>
    /// Gets or sets the number of the last completed step; 0 before the first.
    /// </summary>
    public short LastFinished { get; set; }

    /// <summary>
    /// Gets or sets the number of the accepted step; 0 when none is accepted.
    /// </summary>
    public short Active { get; set; }

    /// <summary>
    /// Gets or sets the kills of a hunt step, or 1 when a move step reached its spot.
    /// </summary>
    public int Progress { get; set; }
}
