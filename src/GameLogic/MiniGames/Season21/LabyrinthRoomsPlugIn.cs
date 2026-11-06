// <copyright file="LabyrinthRoomsPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The 10 x 10 rooms of the floors of the Labyrinth of Dimensions (S21Emu <c>LabyrinthOfDimension.xml</c> and
/// <c>LabyrinthOfDimension_Path.xml</c>), played by <see cref="LabyrinthRun"/>. It also counts the damage and the potions
/// of a character in its room for the missions.
/// </summary>
/// <remarks>
/// Only a client with a <see cref="ILabyrinthRoomsViewPlugIn"/> plays the rooms: the room shows an area of another map,
/// which the client has to draw. Other clients play the floors as before.
/// </remarks>
[PlugIn]
[Display(Name = nameof(PlugInResources.LabyrinthRoomsPlugIn_Name), Description = nameof(PlugInResources.LabyrinthRoomsPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("A443C13B-46B8-4C53-9AF6-7A23B1DD9876")]
public class LabyrinthRoomsPlugIn : IFeaturePlugIn, ISupportCustomConfiguration<LabyrinthRoomsConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault, IAttackableGotHitPlugIn, IItemConsumedPlugIn
{
    /// <summary>
    /// The item group of the potions.
    /// </summary>
    internal const byte PotionGroup = 14;

    private const int MapSize = 256;

    private const int TerrainHeaderSize = 3;

    private readonly ConcurrentDictionary<(short Map, byte StartX, byte StartY, byte EndX, byte EndY), byte[]> _terrains = new();

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public LabyrinthRoomsConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or <c>null</c> when the feature is off.</returns>
    public static LabyrinthRoomsPlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<LabyrinthRoomsPlugIn>();

    /// <summary>
    /// Creates the terrain of a room: the area of its map, without safe zones, and everything else blocked. An area without
    /// a walkable tile (its map is missing) is walkable as a whole, so the character never gets stuck.
    /// </summary>
    /// <param name="area">The area.</param>
    /// <param name="sourceTerrain">The terrain data of the map of the area.</param>
    /// <returns>The terrain data, in the format of the <c>*.att</c> files.</returns>
    public static byte[] CreateRoomTerrain(LabyrinthArea area, byte[]? sourceTerrain)
    {
        var data = new byte[TerrainHeaderSize + (MapSize * MapSize)];
        data.AsSpan(TerrainHeaderSize).Fill((byte)TerrainAttributeType.Blocked);
        var walkable = false;
        for (int y = area.StartY; y <= area.EndY; y++)
        {
            for (int x = area.StartX; x <= area.EndX; x++)
            {
                var index = TerrainHeaderSize + (y * MapSize) + x;
                var value = sourceTerrain is { } source && index < source.Length ? source[index] : (byte)0;
                data[index] = (byte)(value & ~(byte)TerrainAttributeType.Safezone);
                walkable |= data[index] == 0;
            }
        }

        if (!walkable)
        {
            for (int y = area.StartY; y <= area.EndY; y++)
            {
                data.AsSpan(TerrainHeaderSize + (y * MapSize) + area.StartX, area.EndX - area.StartX + 1).Clear();
            }
        }

        return data;
    }

    /// <inheritdoc />
    public object CreateDefaultConfig() => new LabyrinthRoomsConfiguration();

    /// <summary>
    /// Creates the rooms of a run, when the configuration has a path and the client of the character draws rooms.
    /// </summary>
    /// <param name="game">The game of the run.</param>
    /// <param name="player">The character of the run.</param>
    /// <returns>The rooms, or <c>null</c> when the run plays without rooms.</returns>
    public LabyrinthRun? CreateRun(MiniGameContext game, Player player)
    {
        if (this.Configuration is not { } configuration
            || !configuration.Paths.Any(p => p.Entrance >= 0)
            || player.ViewPlugIns.GetPlugIn<ILabyrinthRoomsViewPlugIn>() is null)
        {
            return null;
        }

        return new LabyrinthRun(game, player, this, configuration);
    }

    /// <summary>
    /// Gets the terrain of a room, created once per area.
    /// </summary>
    /// <param name="area">The area.</param>
    /// <param name="gameConfiguration">The game configuration with the map of the area.</param>
    /// <returns>The terrain data.</returns>
    public byte[] GetRoomTerrain(LabyrinthArea area, GameConfiguration gameConfiguration)
        => this._terrains.GetOrAdd(
            (area.MapNumber, area.StartX, area.StartY, area.EndX, area.EndY),
            _ => CreateRoomTerrain(area, gameConfiguration.Maps.FirstOrDefault(m => m.Number == area.MapNumber && m.Discriminator == 0)?.TerrainData));

    /// <inheritdoc />
    public void AttackableGotHit(IAttackable attackable, IAttacker attacker, HitInfo hitInfo)
    {
        if (attackable is Monster monster && GetRun(attacker as Player ?? (attacker as IPlayerSurrogate)?.Owner) is { } run)
        {
            run.RegisterHit(monster, hitInfo.HealthDamage);
        }
    }

    /// <inheritdoc />
    public void ItemConsumed(Player player, Item item, Item? targetItem)
    {
        GetRun(player)?.ItemConsumed(item);
    }

    private static LabyrinthRun? GetRun(Player? player)
        => player?.CurrentMiniGame is Season21RoundsContext { Rooms: { } run } && run.Player == player ? run : null;
}
