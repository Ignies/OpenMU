// <copyright file="GateLandingTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

/// <summary>
/// Tests that a warp lands on a walkable tile of the target gate.
/// </summary>
[TestFixture]
public class GateLandingTest
{
    /// <summary>
    /// Acheron's spawn gate (S21Emu gate 417) has three blocked corner tiles; a landing on one recovered the player to
    /// the safezone map of Acheron, Devias.
    /// </summary>
    /// <returns>The task.</returns>
    [Test]
    public async ValueTask WarpToAcheronStaysInAcheronAsync()
    {
        var world = await Season21EventWorld.CreateAsync().ConfigureAwait(false);
        var player = await world.CreatePlayerAsync(CharacterClassNumber.DarkKnight, 400, 0).ConfigureAwait(false);
        var lorencia = world.Configuration.Maps.Single(m => m.Number == 0 && m.Discriminator == 0);
        var acheron = world.Configuration.Maps.Single(m => m.Number == 91 && m.Discriminator == 0);
        var acheronGate = acheron.ExitGates.First(g => g.IsSpawnGate);
        var lorenciaGate = lorencia.ExitGates.First(g => g.IsSpawnGate);

        for (var i = 0; i < 100; i++)
        {
            await player.WarpToAsync(acheronGate).ConfigureAwait(false);
            await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
            Assert.That(player.CurrentMap?.Definition.Number, Is.EqualTo(91), $"warp {i} to {player.Position}");

            await player.WarpToAsync(lorenciaGate).ConfigureAwait(false);
            await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// The first warp into a map which doesn't run on the server yet lands on a walkable tile too: the landing reads
    /// the terrain of the map's definition. Here the gate covers a 16 x 16 area of Acheron with one walkable tile.
    /// </summary>
    /// <returns>The task.</returns>
    [Test]
    public async ValueTask FirstWarpIntoAMapNotRunningYetLandsOnAWalkableTileAsync()
    {
        const int size = 16;
        var world = await Season21EventWorld.CreateAsync().ConfigureAwait(false);
        var player = await world.CreatePlayerAsync(CharacterClassNumber.DarkKnight, 400, 0).ConfigureAwait(false);
        var acheron = world.Configuration.Maps.Single(m => m.Number == 91 && m.Discriminator == 0);
        var walkMap = new GameMapTerrain(acheron).WalkMap;
        var (x1, y1) = Enumerable.Range(0, 256 - size).SelectMany(x => Enumerable.Range(0, 256 - size).Select(y => (x, y)))
            .First(c => Enumerable.Range(c.x, size).Sum(x => Enumerable.Range(c.y, size).Count(y => walkMap[x, y])) == 1);
        var gate = acheron.ExitGates.First(g => g.IsSpawnGate);
        (gate.X1, gate.Y1, gate.X2, gate.Y2) = ((byte)x1, (byte)y1, (byte)(x1 + size), (byte)(y1 + size));

        await player.WarpToAsync(gate).ConfigureAwait(false);
        await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);

        Assert.That(player.CurrentMap?.Definition.Number, Is.EqualTo(91), $"landed at {player.Position}");
        Assert.That(walkMap[player.Position.X, player.Position.Y], Is.True, $"landed at {player.Position}");
    }
}
