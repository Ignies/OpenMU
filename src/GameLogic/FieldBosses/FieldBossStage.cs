// <copyright file="FieldBossStage.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

/// <summary>
/// One monster of a field boss fight. When it dies, the next stage spawns; the last one is the boss itself.
/// </summary>
public class FieldBossStage
{
    /// <summary>
    /// Gets or sets the monster number.
    /// </summary>
    public short MonsterNumber { get; set; }

    /// <summary>
    /// Gets or sets the x coordinate it spawns at.
    /// </summary>
    public byte X { get; set; }

    /// <summary>
    /// Gets or sets the y coordinate it spawns at.
    /// </summary>
    public byte Y { get; set; }

    /// <summary>
    /// Gets or sets the monsters which spawn with it.
    /// </summary>
    public List<FieldBossSummon> Summons { get; set; } = [];

    /// <summary>
    /// Gets or sets the health phases.
    /// </summary>
    public List<FieldBossHealthPhase> Phases { get; set; } = [];
}
