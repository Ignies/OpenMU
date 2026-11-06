// <copyright file="EliteZonesFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EliteZones;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Progression;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The elite zones of the maps from Scorched Canyon on: five elemental areas per map, which hold its elites.
/// A walk into a zone is refused when the character misses its entry rule.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.EliteZonesFeaturePlugIn_Name), Description = nameof(PlugInResources.EliteZonesFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("5B1F7C3E-2A64-4E8D-9C71-0D3E6A2B8F45")]
public class EliteZonesFeaturePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<EliteZoneConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault,
    IWalkRequestValidationPlugIn, IObjectAddedToMapPlugIn
{
    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public EliteZoneConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or <c>null</c> when the feature is off.</returns>
    public static EliteZonesFeaturePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<EliteZonesFeaturePlugIn>();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new EliteZoneConfiguration();

    /// <summary>
    /// Gets the zones of a map.
    /// </summary>
    /// <param name="mapNumber">The map number.</param>
    /// <returns>The zones.</returns>
    public IEnumerable<EliteZoneDefinition> GetZones(short mapNumber)
        => this.Configuration?.Zones.Where(zone => zone.MapNumber == mapNumber) ?? [];

    /// <summary>
    /// Gets the zone which contains a point of a map.
    /// </summary>
    /// <param name="mapNumber">The map number.</param>
    /// <param name="point">The point.</param>
    /// <returns>The zone, or <c>null</c>.</returns>
    public EliteZoneDefinition? GetZoneAt(short mapNumber, Point point)
        => this.GetZones(mapNumber).FirstOrDefault(zone => zone.Contains(point));

    /// <summary>
    /// Determines whether a player may enter the zones of its current map.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns><c>true</c>, if the player meets the entry rule.</returns>
    public bool CanEnter(Player player)
    {
        var levelAboveMap = this.Configuration?.LevelAboveMap ?? 0;
        if (levelAboveMap <= 0)
        {
            return true;
        }

        var mapLevel = player.CurrentMap?.Definition is { } map ? ProgressionWorld.GetSeason21Requirement(map) ?? 0 : 0;
        return Season21LevelCheck.Meets(player, mapLevel + levelAboveMap);
    }

    /// <inheritdoc />
    public async ValueTask ValidateWalkRequestAsync(Player player, Memory<WalkingStep> steps, WalkRequestValidationArgs eventArgs)
    {
        if (!eventArgs.IsWalkAllowed
            || player.CurrentMap is not { } map
            || this.FindEnteredZone(map.Definition.Number, steps.Span) is null
            || this.CanEnter(player))
        {
            return;
        }

        eventArgs.IsWalkAllowed = false;
        await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.EliteZoneEntryDenied)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is not Player player)
        {
            return;
        }

        var zones = this.GetZones(map.Definition.Number).ToList();
        if (zones.Count == 0)
        {
            return;
        }

        var canEnter = this.CanEnter(player);
        var shown = zones.Select(zone => (zone, canEnter)).ToList();
        await player.InvokeViewPlugInAsync<IEliteZonesViewPlugIn>(p => p.ShowEliteZonesAsync(map.Definition.Number, shown)).ConfigureAwait(false);
    }

    /// <summary>
    /// Finds the first zone which a walk enters from outside.
    /// </summary>
    /// <param name="mapNumber">The map number.</param>
    /// <param name="steps">The steps.</param>
    /// <returns>The zone, or <c>null</c>.</returns>
    internal EliteZoneDefinition? FindEnteredZone(short mapNumber, ReadOnlySpan<WalkingStep> steps)
    {
        foreach (var step in steps)
        {
            if (this.GetZoneAt(mapNumber, step.To) is { } zone && !zone.Contains(step.From))
            {
                return zone;
            }
        }

        return null;
    }
}
