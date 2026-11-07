// <copyright file="GateLandingTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

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
}
