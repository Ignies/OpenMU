// <copyright file="FieldBossAbility.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

/// <summary>
/// A special skill of a stage monster, which it uses during the fight.
/// </summary>
public class FieldBossAbility
{
    /// <summary>
    /// Gets or sets the name, for the admin panel and the logs.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets what the skill does.
    /// </summary>
    public FieldBossAbilityKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the seconds between two tries.
    /// </summary>
    [Range(1, 3600)]
    public int IntervalSeconds { get; set; } = 2;

    /// <summary>
    /// Gets or sets the chance of a try in percent.
    /// </summary>
    [Range(0, 100)]
    public int ChancePercent { get; set; } = 100;

    /// <summary>
    /// Gets or sets the distance to the stage monster within which an area strike hits.
    /// </summary>
    [Range(0, 30)]
    public byte Distance { get; set; } = 3;

    /// <summary>
    /// Gets or sets the damage of an area strike in percent of the stage monster's average damage.
    /// </summary>
    [Range(0, 1000)]
    public int DamagePercent { get; set; } = 100;

    /// <summary>
    /// Gets or sets the monsters a summon calls.
    /// </summary>
    public List<FieldBossSummon> Summons { get; set; } = [];

    /// <summary>
    /// Gets or sets how many of the summoned monsters may live at once.
    /// </summary>
    [Range(0, 100)]
    public int MaximumAlive { get; set; } = 8;
}
