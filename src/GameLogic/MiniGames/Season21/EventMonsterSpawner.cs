// <copyright file="EventMonsterSpawner.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;

/// <summary>
/// Spawns the monsters of a Season 21 event on its map: once, around a point, with optional multipliers.
/// </summary>
internal static class EventMonsterSpawner
{
    /// <summary>
    /// Spawns a monster which doesn't respawn.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="definition">The definition of the monster.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="range">The distance from the point.</param>
    /// <param name="health">The multiplier of the health, or the health when <paramref name="healthOverride"/> is set.</param>
    /// <param name="damage">The multiplier of the damage.</param>
    /// <param name="defense">The multiplier of the defense.</param>
    /// <param name="healthOverride">The absolute health, if any.</param>
    /// <returns>The monster, or <c>null</c> if it couldn't spawn.</returns>
    public static ValueTask<Monster?> SpawnAsync(MiniGameContext game, MonsterDefinition definition, byte x, byte y, byte range, float health = 1, float damage = 1, float defense = 1, long? healthOverride = null)
        => SpawnInAreaAsync(game.Map, game.GameContext, game, definition, (byte)Math.Max(x - range, 0), (byte)Math.Max(y - range, 0), (byte)Math.Min(x + range, byte.MaxValue), (byte)Math.Min(y + range, byte.MaxValue), health, damage, defense, healthOverride);

    /// <summary>
    /// Spawns a monster which doesn't respawn in an area of a map, of an event or not.
    /// </summary>
    /// <param name="map">The map.</param>
    /// <param name="gameContext">The game context.</param>
    /// <param name="eventState">The state of the event, if any.</param>
    /// <param name="definition">The definition of the monster.</param>
    /// <param name="x1">The first x coordinate of the area.</param>
    /// <param name="y1">The first y coordinate of the area.</param>
    /// <param name="x2">The last x coordinate of the area.</param>
    /// <param name="y2">The last y coordinate of the area.</param>
    /// <param name="health">The multiplier of the health, or the health when <paramref name="healthOverride"/> is set.</param>
    /// <param name="damage">The multiplier of the damage.</param>
    /// <param name="defense">The multiplier of the defense.</param>
    /// <param name="healthOverride">The absolute health, if any.</param>
    /// <returns>The monster, or <c>null</c> if it couldn't spawn.</returns>
    public static async ValueTask<Monster?> SpawnInAreaAsync(GameMap map, IGameContext gameContext, IEventStateProvider? eventState, MonsterDefinition definition, byte x1, byte y1, byte x2, byte y2, float health = 1, float damage = 1, float defense = 1, long? healthOverride = null)
    {
        if (!definition.IsActive)
        {
            return null;
        }

        var area = new MonsterSpawnArea
        {
            GameMap = map.Definition,
            MonsterDefinition = definition,
            SpawnTrigger = SpawnTrigger.OnceAtWaveStart,
            Quantity = 1,
            X1 = x1,
            X2 = x2,
            Y1 = y1,
            Y2 = y2,
        };
        if (healthOverride is { } absoluteHealth)
        {
            area.MaximumHealthOverride = (int)Math.Min(absoluteHealth, int.MaxValue);
        }

        var intelligence = new BasicMonsterIntelligence();
        var monster = new Monster(area, definition, map, gameContext.DropGenerator, intelligence, gameContext.PlugInManager, gameContext.PathFinderPool, eventState);
        intelligence.Npc = monster;
        Multiply(monster, Stats.MaximumHealth, healthOverride is null ? health : 1);
        Multiply(monster, Stats.MinimumPhysBaseDmg, damage);
        Multiply(monster, Stats.MaximumPhysBaseDmg, damage);
        Multiply(monster, Stats.DefenseBase, defense);
        try
        {
            monster.Initialize();
        }
        catch (InvalidOperationException ex)
        {
            gameContext.LoggerFactory.CreateLogger(typeof(EventMonsterSpawner)).LogWarning(ex, "{map}: Couldn't spawn {monster} in {x1}, {y1} - {x2}, {y2}.", map, definition, x1, y1, x2, y2);
            monster.Dispose();
            return null;
        }

        await map.AddAsync(monster).ConfigureAwait(false);
        monster.OnSpawn();
        return monster;
    }

    /// <summary>
    /// Removes a monster of the event from its map, if it's still there.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="monster">The monster.</param>
    public static ValueTask RemoveAsync(MiniGameContext game, Monster monster) => RemoveAsync(game.Map, monster);

    /// <summary>
    /// Removes a non-player character from its map, if it's still there.
    /// </summary>
    /// <param name="map">The map.</param>
    /// <param name="npc">The character.</param>
    /// <returns>A task.</returns>
    public static async ValueTask RemoveAsync(GameMap map, NonPlayerCharacter npc)
    {
        if (!npc.IsDisposed && !npc.IsDisposing)
        {
            await map.RemoveAsync(npc).ConfigureAwait(false);
            npc.Dispose();
        }
    }

    private static void Multiply(Monster monster, AttributeDefinition attribute, float multiplier)
    {
        if (Math.Abs(multiplier - 1) > 0.001f)
        {
            monster.Attributes.AddElement(new SimpleElement(multiplier, AggregateType.Multiplicate), attribute);
        }
    }
}
