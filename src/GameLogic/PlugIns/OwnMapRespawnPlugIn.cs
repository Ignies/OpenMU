// <copyright file="OwnMapRespawnPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// A plugin which lets a character respawn at the own spawn gate of the maps of <see cref="OwnMapRespawnConfiguration.Maps"/>
/// (the safe zone inside the hunting ground of Season 21) instead of its safezone map.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.OwnMapRespawnPlugIn_Name), Description = nameof(Properties.PlugInResources.OwnMapRespawnPlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("5B9E2D47-8C31-4A6F-9E05-D17C3B8A2F60")]
public class OwnMapRespawnPlugIn : IPlayerSpawnGateSelectionPlugIn, IDisabledByDefault, ISupportCustomConfiguration<OwnMapRespawnConfiguration>, ISupportDefaultCustomConfiguration
{
    /// <inheritdoc />
    public OwnMapRespawnConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the own spawn gate of a map which the configuration lists.
    /// </summary>
    /// <param name="map">The map.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The spawn gate; <c>null</c> when the map isn't listed or has none.</returns>
    public static ExitGate? GetOwnSpawnGate(GameMapDefinition map, OwnMapRespawnConfiguration? configuration)
        => configuration?.Maps.Contains(map.Number) is true ? map.ExitGates.FirstOrDefault(g => g.IsSpawnGate) : null;

    /// <inheritdoc />
    public object CreateDefaultConfig() => new OwnMapRespawnConfiguration();

    /// <inheritdoc />
    public ValueTask SelectSpawnGateAsync(Player player, SpawnGateSelectionArgs args)
    {
        if (args.Gate is null && player.CurrentMap?.Definition is { } map && GetOwnSpawnGate(map, this.Configuration) is { } gate)
        {
            args.Gate = gate;
        }

        return ValueTask.CompletedTask;
    }
}
