// <copyright file="CharacterSoulConverter.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SoulConverter;

/// <summary>
/// The monster soul converter state of a character: the activations of the week and the active converter.
/// </summary>
public class CharacterSoulConverter
{
    /// <summary>
    /// Gets or sets the start of the week the activations count for.
    /// </summary>
    public DateTime Week { get; set; }

    /// <summary>
    /// Gets or sets the converters activated in that week.
    /// </summary>
    public int Activations { get; set; }

    /// <summary>
    /// Gets or sets the tier of the active converter; 0 when none is active.
    /// </summary>
    public byte Tier { get; set; }

    /// <summary>
    /// Gets or sets the point in time at which the active converter expires (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the missions of the active converter.
    /// </summary>
    public List<SoulMissionProgress> Missions { get; set; } = [];
}
