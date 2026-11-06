// <copyright file="FieldBossDefinition.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

/// <summary>
/// A field boss: its map, its stages and its arena.
/// </summary>
public class FieldBossDefinition
{
    /// <summary>
    /// Gets or sets the name, which the game master command takes.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the boss spawns.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the map number.
    /// </summary>
    public short MapNumber { get; set; }

    /// <summary>
    /// Gets or sets the stages; the last one is the boss.
    /// </summary>
    public List<FieldBossStage> Stages { get; set; } = [];

    /// <summary>
    /// Gets or sets the arena, or <c>null</c> for a boss in the open field.
    /// </summary>
    public FieldBossArena? Arena { get; set; }

    /// <summary>
    /// Gets or sets the number of the NPC which leads into the arena; 0 for none.
    /// </summary>
    public short EntryNpcNumber { get; set; }

    /// <summary>
    /// Gets or sets the minutes the players have from the first hit; 0 for no limit.
    /// </summary>
    [Range(0, 1440)]
    public int BattleMinutes { get; set; }

    /// <summary>
    /// Gets the monster number of the boss itself, the last stage.
    /// </summary>
    public short BossNumber => this.Stages.Count > 0 ? this.Stages[^1].MonsterNumber : (short)0;

    /// <inheritdoc />
    public override string ToString() => this.Name;
}
