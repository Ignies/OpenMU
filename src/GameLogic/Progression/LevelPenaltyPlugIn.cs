// <copyright file="LevelPenaltyPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The level penalties of Season 21 (<see cref="LevelPenalties"/>): a character below a monster's level plus 600 deals
/// less damage to it, takes more from it and, in the Kardamahal temple, gets less experience. Fork feature id
/// <see cref="FeatureId"/>; off by default.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.LevelPenaltyPlugIn_Name), Description = nameof(PlugInResources.LevelPenaltyPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("6C1E9A47-2B8D-4F35-A0C2-7E4D91B3F586")]
public class LevelPenaltyPlugIn : IFeaturePlugIn, IExperienceCalculationPlugIn, ISupportCustomConfiguration<LevelPenaltyConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault
{
    /// <summary>
    /// The id of the feature in the fork marker.
    /// </summary>
    public const string FeatureId = "level_penalties";

    /// <inheritdoc />
    public LevelPenaltyConfiguration? Configuration { get; set; }

    /// <summary>
    /// Applies the damage penalty of the game's active plugin to a hit between a player and a monster.
    /// </summary>
    /// <param name="attackerPlayer">The player who attacks, also through a surrogate; <c>null</c> if a monster attacks.</param>
    /// <param name="attacker">The attacker.</param>
    /// <param name="defender">The defender.</param>
    /// <param name="damage">The damage.</param>
    /// <returns>The damage after the penalty.</returns>
    public static int ApplyToDamage(Player? attackerPlayer, IAttacker attacker, IAttackable defender, int damage)
    {
        var (player, monster) = attackerPlayer is not null
            ? (attackerPlayer, defender as NonPlayerCharacter)
            : (defender as Player, attacker as NonPlayerCharacter);
        if (player is null
            || monster is null
            || player.GameContext.FeaturePlugIns.GetPlugIn<LevelPenaltyPlugIn>()?.Configuration is not { } configuration
            || (attackerPlayer is null && !configuration.MonsterDamageBonusActive)
            || player.Attributes is not { } attributes
            || LevelOf(monster) is not { } monsterLevel)
        {
            return damage;
        }

        var percent = LevelPenalties.GetDamagePenaltyPercent(
            configuration,
            GetProgression(player),
            player.CurrentMap?.Definition.Number ?? (short)-1,
            player.SelectedCharacter?.CharacterClass?.GetEvolution(),
            (int)attributes[Stats.Level],
            (int)attributes[Stats.MasterLevel],
            (int)attributes[Stats.Resets],
            monsterLevel);
        return LevelPenalties.ApplyToDamage(damage, percent, attackerPlayer is not null);
    }

    /// <inheritdoc />
    public object CreateDefaultConfig() => new LevelPenaltyConfiguration();

    /// <inheritdoc />
    public ValueTask CalculateExperienceAsync(Player player, ExperienceCalculationArgs args)
    {
        if (this.Configuration is { } configuration
            && args.KilledObject is NonPlayerCharacter monster
            && player.Attributes is { } attributes
            && LevelOf(monster) is { } monsterLevel)
        {
            var percent = LevelPenalties.GetExperiencePenaltyPercent(
                configuration,
                GetProgression(player),
                monster.CurrentMap.Definition.Number,
                (int)attributes[Stats.Level],
                (int)attributes[Stats.MasterLevel],
                (int)attributes[Stats.Resets],
                monsterLevel);
            args.Experience *= 1 - (percent / 100.0);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>The level of a monster; null for one without (a castle gate, a guardian statue, a life stone).</summary>
    private static int? LevelOf(NonPlayerCharacter monster) =>
        monster.Definition.Attributes.FirstOrDefault(a => a.AttributeDefinition == Stats.Level) is { } level ? (int)level.Value : null;

    private static ProgressionConfiguration? GetProgression(Player player) => ProgressionFeaturePlugIn.GetWorld(player.GameContext)?.Configuration;
}
