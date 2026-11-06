// <copyright file="NpcSpawnPointTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.Pathfinding;
using NUnit.Framework;
using GameMapDefinition = MUnique.OpenMU.Persistence.BasicModel.GameMapDefinition;
using MonsterDefinition = MUnique.OpenMU.Persistence.BasicModel.MonsterDefinition;
using MonsterSpawnArea = MUnique.OpenMU.Persistence.BasicModel.MonsterSpawnArea;

/// <summary>
/// Tests how a non-player character finds the point it spawns on.
/// </summary>
[TestFixture]
public class NpcSpawnPointTest
{
    private const int TerrainHeaderSize = 3;

    private const byte BlockedAttribute = 4;

    /// <summary>
    /// A spawn area with a single walkable tile among many blocked ones always spawns on that tile,
    /// like the single monster spots of Swamp of Darkness, instead of giving up after random tries.
    /// </summary>
    [Test]
    public void AreaWithOneWalkableTileAlwaysSpawnsOnIt()
    {
        var walkable = new Point(110, 120);
        var terrain = Enumerable.Repeat(BlockedAttribute, TerrainHeaderSize + (256 * 256)).ToArray();
        terrain[TerrainHeaderSize + (walkable.Y * 256) + walkable.X] = 0;
        var map = new GameMap(new GameMapDefinition { TerrainData = terrain }, TimeSpan.FromMinutes(1), 8);
        var area = new MonsterSpawnArea { X1 = 100, Y1 = 110, X2 = 114, Y2 = 124, Quantity = 1 };
        var definition = new MonsterDefinition { ObjectKind = NpcObjectKind.Monster };

        for (var i = 0; i < 20; i++)
        {
            var npc = new NonPlayerCharacter(area, definition, map);
            npc.Initialize();
            Assert.That(npc.Position, Is.EqualTo(walkable));
        }
    }
}
