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
    public static async ValueTask<Monster?> SpawnAsync(MiniGameContext game, MonsterDefinition definition, byte x, byte y, byte range, float health = 1, float damage = 1, float defense = 1, long? healthOverride = null)
    {
        if (!definition.IsActive)
        {
            return null;
        }

        var area = new MonsterSpawnArea
        {
            GameMap = game.Map.Definition,
            MonsterDefinition = definition,
            SpawnTrigger = SpawnTrigger.OnceAtWaveStart,
            Quantity = 1,
            X1 = (byte)Math.Max(x - range, 0),
            X2 = (byte)Math.Min(x + range, byte.MaxValue),
            Y1 = (byte)Math.Max(y - range, 0),
            Y2 = (byte)Math.Min(y + range, byte.MaxValue),
        };
        if (healthOverride is { } absoluteHealth)
        {
            area.MaximumHealthOverride = (int)Math.Min(absoluteHealth, int.MaxValue);
        }

        var gameContext = game.GameContext;
        var intelligence = new BasicMonsterIntelligence();
        var monster = new Monster(area, definition, game.Map, gameContext.DropGenerator, intelligence, gameContext.PlugInManager, gameContext.PathFinderPool, game);
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
            game.GameContext.LoggerFactory.CreateLogger(typeof(EventMonsterSpawner)).LogWarning(ex, "{game}: Couldn't spawn {monster} at {x}, {y}.", game, definition, x, y);
            monster.Dispose();
            return null;
        }

        await game.Map.AddAsync(monster).ConfigureAwait(false);
        monster.OnSpawn();
        return monster;
    }

    /// <summary>
    /// Removes a monster of the event from its map, if it's still there.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="monster">The monster.</param>
    public static async ValueTask RemoveAsync(MiniGameContext game, Monster monster)
    {
        if (!monster.IsDisposed && !monster.IsDisposing)
        {
            await game.Map.RemoveAsync(monster).ConfigureAwait(false);
            monster.Dispose();
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
