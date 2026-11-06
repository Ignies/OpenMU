// <copyright file="FieldBossGround.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

/// <summary>
/// One of the places a boss may spawn at. Its stages and summons move by the distance from the first stage's point.
/// </summary>
public class FieldBossGround
{
    /// <summary>
    /// Gets or sets the name, e.g. "Ground 3".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the x coordinate of the first stage on this ground.
    /// </summary>
    public byte X { get; set; }

    /// <summary>
    /// Gets or sets the y coordinate of the first stage on this ground.
    /// </summary>
    public byte Y { get; set; }
}
