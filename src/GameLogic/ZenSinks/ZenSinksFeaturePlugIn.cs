// <copyright file="ZenSinksFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ZenSinks;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The zen sinks of Season 21 against the zen inflation of the later maps: the tax of the personal store
/// and the reduced money drop of monsters.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ZenSinksFeaturePlugIn_Name), Description = nameof(PlugInResources.ZenSinksFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("A7C25E1B-9D43-4F86-B0E2-5C8F3A1D6E97")]
public class ZenSinksFeaturePlugIn : IFeaturePlugIn, IDisabledByDefault, ISupportCustomConfiguration<ZenSinksConfiguration>, ISupportDefaultCustomConfiguration
{
    /// <summary>
    /// The feature id of the fork marker.
    /// </summary>
    public const string FeatureId = "zen_sinks";

    private const int Percent = 100;

    /// <inheritdoc />
    public ZenSinksConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the zen which the seller of an item of a personal store gets.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <param name="price">The price which the buyer pays.</param>
    /// <returns>The price without the tax.</returns>
    public static int GetSellerIncome(IGameContext gameContext, int price)
    {
        if (Get(gameContext) is not { } configuration || price <= 0)
        {
            return price;
        }

        var tax = (long)price * Math.Clamp(configuration.PersonalStoreTaxPercent, 0, Percent) / Percent;
        return price - (int)tax;
    }

    /// <summary>
    /// Scales the money which a killed monster drops.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <param name="money">The money without this plugin.</param>
    /// <returns>The money to drop; at least 1.</returns>
    public static uint ScaleMoneyDrop(IGameContext gameContext, uint money)
    {
        if (Get(gameContext) is not { } configuration)
        {
            return money;
        }

        var scaled = (ulong)money * (ulong)Math.Max(configuration.MoneyDropPercent, 0) / Percent;
        return (uint)Math.Clamp(scaled, 1, uint.MaxValue);
    }

    /// <inheritdoc />
    public object CreateDefaultConfig() => new ZenSinksConfiguration();

    private static ZenSinksConfiguration? Get(IGameContext gameContext)
        => gameContext.FeaturePlugIns.GetPlugIn<ZenSinksFeaturePlugIn>() is { } plugIn ? plugIn.Configuration ?? new ZenSinksConfiguration() : null;
}
