// <copyright file="NewPowerStats.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.NewPower;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The attributes of a kind of the New Power.
/// </summary>
/// <param name="ExtraDamage">The extra damage, which takes the kind's rate.</param>
/// <param name="AttackIncrease">The damage which is added as it is.</param>
/// <param name="CriticalChance">The critical chance.</param>
/// <param name="ExcellentChance">The excellent chance.</param>
/// <param name="DoubleChance">The double damage chance.</param>
/// <param name="TripleChance">The triple damage chance.</param>
public sealed record NewPowerStats(
    AttributeDefinition ExtraDamage,
    AttributeDefinition AttackIncrease,
    AttributeDefinition CriticalChance,
    AttributeDefinition ExcellentChance,
    AttributeDefinition DoubleChance,
    AttributeDefinition TripleChance)
{
    private static readonly NewPowerStats Nuke = new(Stats.NukeExtraDamage, Stats.NukeAttackIncrease, Stats.NukeCriticalChance, Stats.NukeExcellentChance, Stats.NukeDoubleChance, Stats.NukeTripleChance);

    private static readonly NewPowerStats Bolt = new(Stats.BoltExtraDamage, Stats.BoltAttackIncrease, Stats.BoltCriticalChance, Stats.BoltExcellentChance, Stats.BoltDoubleChance, Stats.BoltTripleChance);

    private static readonly NewPowerStats Wide = new(Stats.WideExtraDamage, Stats.WideAttackIncrease, Stats.WideCriticalChance, Stats.WideExcellentChance, Stats.WideDoubleChance, Stats.WideTripleChance);

    /// <summary>
    /// Gets the attributes of a kind.
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The attributes.</returns>
    public static NewPowerStats Of(NewPowerKind kind) => kind switch
    {
        NewPowerKind.Nuke => Nuke,
        NewPowerKind.Bolt => Bolt,
        _ => Wide,
    };
}
