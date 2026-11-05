// <copyright file="ProgressionFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Feature plugin which provides the progression configuration of a world with the Season 21 maps:
/// original Season 21 numbers, normalized into level 400 plus master level, or stars.
/// </summary>
/// <remarks>
/// It never changes the stored rows. The level caps are set on the running game configuration (and put back in the
/// original mode); requirements, warp zen and drop levels are asked from <see cref="GetWorld(IGameContext)"/>; the monster
/// numbers are multipliers on the attributes of the spawned monsters, shared per monster definition, so a change of the
/// mode reaches the monsters which are alive.
/// </remarks>
[PlugIn]
[Display(Name = nameof(PlugInResources.ProgressionFeaturePlugIn_Name), Description = nameof(PlugInResources.ProgressionFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("624489AB-4623-4AF2-B4CD-4F0482AD05D9")]
public class ProgressionFeaturePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<ProgressionConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault,
    IObjectAddedToMapPlugIn, IObjectRemovedFromMapPlugIn, IExperienceCalculationPlugIn, IPeriodicTaskPlugIn
{
    private static readonly AttributeDefinition[] ShapedStats =
    [
        Stats.Level, Stats.MaximumHealth, Stats.MinimumPhysBaseDmg, Stats.MaximumPhysBaseDmg, Stats.DefenseBase, Stats.AttackRatePvm, Stats.DefenseRatePvm,
    ];

    private static readonly ProgressionConfiguration Unconfigured = new() { Mode = ProgressionMode.Original };

    private readonly ConditionalWeakTable<GameConfiguration, StoredCaps> _storedCaps = [];

    private readonly ConcurrentDictionary<MonsterDefinition, SimpleElement[]> _shapes = new();

    private readonly ConditionalWeakTable<AttackableNpcBase, SimpleElement[]> _shapedMonsters = [];

    private readonly object _lock = new();

    private ProgressionConfiguration? _configuration;

    private WorldEntry? _world;

    private WorldEntry? _walkedWorld;

    /// <inheritdoc/>
    public ProgressionConfiguration? Configuration
    {
        get => this._configuration;
        set
        {
            lock (this._lock)
            {
                this._configuration = value;
                this._world = null;
            }
        }
    }

    /// <summary>
    /// Gets the progression world of a game context, if the feature is active in a normalized mode.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The world, or <c>null</c> in the original mode or when the feature is off.</returns>
    public static ProgressionWorld? GetWorld(IGameContext gameContext)
    {
        return gameContext.FeaturePlugIns.GetPlugIn<ProgressionFeaturePlugIn>()?.GetWorld(gameContext.Configuration);
    }

    /// <inheritdoc />
    public object CreateDefaultConfig() => new ProgressionConfiguration();

    /// <summary>
    /// Gets the progression world of a game configuration, and applies the level caps of the mode to it.
    /// </summary>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <returns>The world, or <c>null</c> in the original mode or without a configuration.</returns>
    public ProgressionWorld? GetWorld(GameConfiguration gameConfiguration)
    {
        if (this._world is { } cached && cached.GameConfiguration == gameConfiguration)
        {
            return cached.World;
        }

        lock (this._lock)
        {
            var configuration = this._configuration ?? Unconfigured;
            this.ApplyCaps(gameConfiguration, configuration);
            var world = configuration.Mode == ProgressionMode.Original ? null : new ProgressionWorld(gameConfiguration, configuration);
            foreach (var (definition, elements) in this._shapes)
            {
                SetFactors(world, definition, elements);
            }

            this._world = new WorldEntry(gameConfiguration, world);
            return world;
        }
    }

    /// <inheritdoc />
    public ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is Player player)
        {
            this.GetWorld(player.GameContext.Configuration);
        }
        else if (addedObject is AttackableNpcBase monster)
        {
            this.Shape(monster);
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask ObjectRemovedFromMapAsync(GameMap map, ILocateable removedObject)
    {
        if (removedObject is AttackableNpcBase monster && this._shapedMonsters.TryGetValue(monster, out var elements))
        {
            for (var i = 0; i < ShapedStats.Length; i++)
            {
                monster.Attributes.RemoveElement(elements[i], ShapedStats[i]);
            }

            this._shapedMonsters.Remove(monster);
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask CalculateExperienceAsync(Player player, ExperienceCalculationArgs args)
    {
        if (this.GetWorld(player.GameContext.Configuration) is { Configuration.Mode: ProgressionMode.Stars } world && player.Attributes is { } attributes)
        {
            args.Experience *= StarRules.GetExperienceFactor((int)attributes[Stats.Resets], world.Configuration);
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    /// <remarks>Builds the world after a start or a change of the configuration, and shapes the monsters which spawned before.</remarks>
    public async ValueTask ExecuteTaskAsync(GameContext gameContext)
    {
        this.GetWorld(gameContext.Configuration);
        if (this._world is not { } current || current == this._walkedWorld)
        {
            return;
        }

        this._walkedWorld = current;
        if (current.World is null)
        {
            return;
        }

        foreach (var map in await gameContext.GetMapsAsync().ConfigureAwait(false))
        {
            foreach (var monster in map.GetNpcsInRange(new Point(128, 128), 256).OfType<AttackableNpcBase>())
            {
                this.Shape(monster);
            }
        }
    }

    /// <inheritdoc />
    public void ForceStart()
    {
        this._walkedWorld = null;
    }

    private static void SetFactors(ProgressionWorld? world, MonsterDefinition definition, SimpleElement[] elements)
    {
        if (world is null || !world.TryGetMonster(definition, out var source, out var normalized))
        {
            elements.ForEach(e => e.Value = 1);
            return;
        }

        int[] sourceValues = [source.Level, source.Health, source.DamageMin, source.DamageMax, source.Defense, source.AttackRate, source.DefenseRate];
        int[] normalizedValues = [normalized.Level, normalized.Health, normalized.DamageMin, normalized.DamageMax, normalized.Defense, normalized.AttackRate, normalized.DefenseRate];
        for (var i = 0; i < elements.Length; i++)
        {
            elements[i].Value = sourceValues[i] > 0 ? (float)normalizedValues[i] / sourceValues[i] : 1;
        }
    }

    private void Shape(AttackableNpcBase monster)
    {
        if (this._world?.World is not { } world
            || !world.TryGetMonster(monster.Definition, out _, out _)
            || this._shapedMonsters.TryGetValue(monster, out _))
        {
            return;
        }

        var elements = this._shapes.GetOrAdd(monster.Definition, definition =>
        {
            var created = ShapedStats.Select(_ => new SimpleElement(1, AggregateType.Multiplicate)).ToArray();
            SetFactors(world, definition, created);
            return created;
        });
        for (var i = 0; i < ShapedStats.Length; i++)
        {
            monster.Attributes.AddElement(elements[i], ShapedStats[i]);
        }

        this._shapedMonsters.AddOrUpdate(monster, elements);
        monster.Health = Math.Min(monster.Health, (int)monster.Attributes[Stats.MaximumHealth]);
    }

    private void ApplyCaps(GameConfiguration gameConfiguration, ProgressionConfiguration configuration)
    {
        var stored = this._storedCaps.GetValue(gameConfiguration, c => new StoredCaps(c.MaximumLevel, c.MaximumMasterLevel));
        var masterLevelCap = configuration.Mode == ProgressionMode.Original && configuration.MasterLevelCap is null
            ? stored.MaximumMasterLevel
            : ProgressionMapping.GetMasterLevelCap(configuration);
        gameConfiguration.MaximumLevel = (short)Math.Min(ProgressionMapping.GetLevelCap(configuration), stored.MaximumLevel);
        gameConfiguration.MaximumMasterLevel = (short)Math.Min(masterLevelCap, stored.MaximumMasterLevel);
    }

    /// <summary>
    /// The caps which the game configuration had before the progression changed them. The experience tables are built
    /// for them, so the progression only lowers a cap.
    /// </summary>
    private sealed record StoredCaps(short MaximumLevel, short MaximumMasterLevel);

    /// <summary>
    /// The world built for a game configuration.
    /// </summary>
    private sealed record WorldEntry(GameConfiguration GameConfiguration, ProgressionWorld? World);
}
