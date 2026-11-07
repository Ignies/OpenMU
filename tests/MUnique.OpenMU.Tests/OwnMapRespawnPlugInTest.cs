// <copyright file="OwnMapRespawnPlugInTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.PlugIns;

/// <summary>
/// Tests the <see cref="OwnMapRespawnPlugIn"/>: a listed map (Kanturu Underground 137, Webzen guide 405) respawns on its
/// own spawn gate, every other map on its safezone map.
/// </summary>
[TestFixture]
public class OwnMapRespawnPlugInTest
{
    /// <summary>
    /// A listed map gives its spawn gate, another map none.
    /// </summary>
    [Test]
    public void ListedMapRespawnsOnItsOwnSpawnGate()
    {
        var configuration = new OwnMapRespawnConfiguration();
        Assert.That(configuration.Maps, Is.EqualTo(new short[] { 137 }));

        var underground = CreateMap(137);
        var other = CreateMap(123);

        Assert.That(OwnMapRespawnPlugIn.GetOwnSpawnGate(underground, configuration), Is.SameAs(underground.ExitGates.Single(g => g.IsSpawnGate)));
        Assert.That(OwnMapRespawnPlugIn.GetOwnSpawnGate(other, configuration), Is.Null);
        Assert.That(OwnMapRespawnPlugIn.GetOwnSpawnGate(underground, null), Is.Null);
    }

    private static GameMapDefinition CreateMap(short number)
    {
        var map = new Persistence.BasicModel.GameMapDefinition { Number = number };
        map.ExitGates.Add(new Persistence.BasicModel.ExitGate { Map = map, X1 = 10, X2 = 12, Y1 = 10, Y2 = 12, IsSpawnGate = false });
        map.ExitGates.Add(new Persistence.BasicModel.ExitGate { Map = map, X1 = 100, X2 = 102, Y1 = 100, Y2 = 102, IsSpawnGate = true });
        return map;
    }
}
