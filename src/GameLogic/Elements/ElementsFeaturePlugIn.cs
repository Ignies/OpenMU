// <copyright file="ElementsFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Elements;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Pet;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The elements of Season 21: the elemental damage and defense of characters (by their pentagram and errtels) and
/// monsters, with the element wheel. Off by default; the damage calculation asks for it on every hit. See Readme.md.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ElementsFeaturePlugIn_Name), Description = nameof(PlugInResources.ElementsFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("C95DB7E8-CBC5-4A1A-94E4-379B5F1E582A")]
public class ElementsFeaturePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<ElementsConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault,
    IAttackableGotHitPlugIn
{
    /// <summary>
    /// The feature id of the fork (fork.json, the Babylon feature list).
    /// </summary>
    public const string FeatureId = "pentagram_elements";

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public ElementsConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the configuration of the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The configuration, or <c>null</c> when the feature is off.</returns>
    public static ElementsConfiguration? GetConfiguration(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<ElementsFeaturePlugIn>()?.Configuration;

    /// <inheritdoc />
    public object CreateDefaultConfig() => new ElementsConfiguration();

    /// <inheritdoc />
    public void AttackableGotHit(IAttackable attackable, IAttacker attacker, HitInfo hitInfo)
    {
        if (hitInfo.ElementalDamage == 0)
        {
            return;
        }

        var attackerPlayer = attacker is AttackerSurrogate surrogate ? surrogate.Owner : attacker as Player;
        if (attacker is Player player)
        {
            this.ApplyAbsorb(player, hitInfo.ElementalDamage);
        }

        _ = ShowSafeAsync(attackerPlayer, attackable, hitInfo);
        if (attackable is Player defender && defender != attackerPlayer)
        {
            _ = ShowSafeAsync(defender, attackable, hitInfo);
        }
    }

    /// <summary>
    /// Applies the absorb effects of the radiance errtel: by the radiance chance, the attacker gets its absorb shares
    /// of the elemental damage as shield and health.
    /// </summary>
    /// <param name="attacker">The attacker.</param>
    /// <param name="elementalDamage">The elemental damage of the hit.</param>
    internal void ApplyAbsorb(Player attacker, uint elementalDamage)
    {
        if (this.Configuration is not { } configuration || attacker.Attributes is not { } attributes)
        {
            return;
        }

        Absorb(Stats.ElementAbsorbShield, Stats.CurrentShield, Stats.MaximumShield);
        Absorb(Stats.ElementAbsorbLife, Stats.CurrentHealth, Stats.MaximumHealth);

        void Absorb(AttributeSystem.AttributeDefinition share, AttributeSystem.AttributeDefinition current, AttributeSystem.AttributeDefinition maximum)
        {
            if (attributes[share] > 0 && Rand.NextRandomBool(configuration.RadianceChance))
            {
                attributes[current] = Math.Min(attributes[maximum], attributes[current] + (elementalDamage * attributes[share]));
            }
        }
    }

    private static async ValueTask ShowSafeAsync(Player? player, IAttackable target, HitInfo hitInfo)
    {
        if (player is null)
        {
            return;
        }

        try
        {
            await player.InvokeViewPlugInAsync<IElementalDamageViewPlugIn>(p => p.ShowElementalDamageAsync(target, hitInfo.Element, hitInfo.ElementalDamage)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            player.Logger.LogError(ex, "Couldn't show the elemental damage to {player}.", player);
        }
    }
}
