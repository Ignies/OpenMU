// <copyright file="AbilityCoreFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.AbilityCore;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The Ability Enhancement Core of the fifth evolution (Season 21, fork feature id <see cref="FeatureId"/>): ability
/// cards in the slots of the core give their options and skills. Off by default; the update "Add the Ability
/// Enhancement Core of Season 21" fills and turns it on.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.AbilityCoreFeaturePlugIn_Name), Description = nameof(PlugInResources.AbilityCoreFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("D84E2A3C-61F7-4B9E-8C05-3A7B9E1D4F26")]
public class AbilityCoreFeaturePlugIn : IFeaturePlugIn, IDisabledByDefault, ISupportCustomConfiguration<AbilityCoreConfiguration>, ISupportDefaultCustomConfiguration, IObjectAddedToMapPlugIn
{
    /// <summary>
    /// The feature id of the fork marker.
    /// </summary>
    public const string FeatureId = "ability_core";

    /// <inheritdoc />
    public AbilityCoreConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the active plugin of the game, or <c>null</c> when the core is off.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The active plugin.</returns>
    public static AbilityCoreFeaturePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<AbilityCoreFeaturePlugIn>();

    /// <summary>
    /// Gets the current configuration, or the default one.
    /// </summary>
    /// <returns>The configuration.</returns>
    public AbilityCoreConfiguration GetConfiguration() => this.Configuration ??= new AbilityCoreConfiguration();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new AbilityCoreConfiguration();

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is Player player)
        {
            await AbilityCoreService.RefreshAsync(player).ConfigureAwait(false);
            await AbilityCoreService.ShowAsync(player).ConfigureAwait(false);
        }
    }
}
