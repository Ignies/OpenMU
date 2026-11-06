// <copyright file="GremoryCaseFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.GremoryCase;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The gremory case of Season 21: the account and character storage through which rewards are delivered.
/// Shows the cases to the player when it enters a map.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.GremoryCaseFeaturePlugIn_Name), Description = nameof(PlugInResources.GremoryCaseFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("3B7D1E94-6A2F-4C58-8E03-9F1A5D2C7B64")]
public class GremoryCaseFeaturePlugIn : IFeaturePlugIn, IDisabledByDefault, ISupportCustomConfiguration<GremoryCaseConfiguration>, ISupportDefaultCustomConfiguration, IObjectAddedToMapPlugIn
{
    /// <summary>
    /// The feature id of the fork marker.
    /// </summary>
    public const string FeatureId = "gremory";

    /// <inheritdoc />
    public GremoryCaseConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the active plugin of the game, or <c>null</c> when the case is off.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The active plugin.</returns>
    public static GremoryCaseFeaturePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<GremoryCaseFeaturePlugIn>();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new GremoryCaseConfiguration();

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is Player player)
        {
            await GremoryCaseService.ShowAsync(player).ConfigureAwait(false);
        }
    }
}
