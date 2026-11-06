// <copyright file="FieldBossHealthPhase.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

/// <summary>
/// Monsters which join the fight once the health of the stage monster falls to a percentage.
/// </summary>
public class FieldBossHealthPhase
{
    /// <summary>
    /// Gets or sets the health percentage at which the phase starts.
    /// </summary>
    [Range(1, 99)]
    public int HealthPercent { get; set; }

    /// <summary>
    /// Gets or sets the monsters which spawn.
    /// </summary>
    public List<FieldBossSummon> Summons { get; set; } = [];
}
