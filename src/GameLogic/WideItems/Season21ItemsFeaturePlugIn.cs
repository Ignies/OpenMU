// <copyright file="Season21ItemsFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.WideItems;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The item rules of Season 21 which are configuration data: the grade rates of the wings and the mastery mixes.
/// Off by default; the update "Add the item mixes of Season 21" fills and activates it.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.Season21ItemsFeaturePlugIn_Name), Description = nameof(PlugInResources.Season21ItemsFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("A7C2E5F1-3B84-4D69-9E10-6F2B8D4C7A35")]
public class Season21ItemsFeaturePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<Season21ItemsConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault
{
    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public Season21ItemsConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the configuration of the active plugin.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The configuration, or <c>null</c> when the plugin is off.</returns>
    public static Season21ItemsConfiguration? GetConfiguration(IGameContext gameContext)
        => gameContext.FeaturePlugIns.GetPlugIn<Season21ItemsFeaturePlugIn>()?.Configuration;

    /// <inheritdoc />
    public object CreateDefaultConfig() => new Season21ItemsConfiguration();
}
