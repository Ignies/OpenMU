// <copyright file="ExitGateExtensionsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.Pathfinding;

/// <summary>
/// Tests the placement-specific landing-point contract of exit gates.
/// </summary>
[TestFixture]
public class ExitGateExtensionsTest
{
    /// <summary>
    /// The possible points must preserve the existing random-placement bounds without defining how other
    /// consumers interpret the gate rectangle.
    /// </summary>
    [Test]
    public void PossibleLandingPointsMatchRandomPlacementBounds()
    {
        var gate = CreateGate(10, 20, 12, 22);
        var expected = new[]
        {
            new Point(10, 20),
            new Point(10, 21),
            new Point(11, 20),
            new Point(11, 21),
        };

        Assert.That(gate.GetPossibleLandingPoints(), Is.EquivalentTo(expected));
    }

    /// <summary>
    /// Equal bounds describe one landing coordinate, including at the edge of the byte coordinate range.
    /// </summary>
    [Test]
    public void PossibleLandingPointsSupportSinglePointGateAtMapEdge()
    {
        var gate = CreateGate(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

        Assert.That(gate.GetPossibleLandingPoints(), Is.EqualTo(new[] { new Point(byte.MaxValue, byte.MaxValue) }));
        Assert.That(gate.GetRandomPoint(), Is.EqualTo(new Point(byte.MaxValue, byte.MaxValue)));
    }

    /// <summary>
    /// Invalid reversed bounds must not be silently normalized into a different landing area.
    /// </summary>
    [Test]
    public void PossibleLandingPointsRejectReversedBounds()
    {
        var gate = CreateGate(12, 22, 10, 20);

        Assert.That(() => gate.GetPossibleLandingPoints().ToList(), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    /// <summary>
    /// Random gate placement and possible-point enumeration must use the same coordinate domain.
    /// </summary>
    [Test]
    public void RandomPointAlwaysBelongsToPossibleLandingPoints()
    {
        var gate = CreateGate(10, 20, 13, 23);
        var possiblePoints = gate.GetPossibleLandingPoints().ToHashSet();

        Assert.That(Enumerable.Range(0, 100).Select(_ => gate.GetRandomPoint()), Is.All.Matches<Point>(possiblePoints.Contains));
    }

    /// <summary>
    /// A warp lands on a walkable tile of the gate when the gate has one: Acheron's spawn gate has three blocked
    /// corner tiles, and a landing on one recovers the player to its safezone map, Devias.
    /// </summary>
    [Test]
    public void RandomWalkablePointAvoidsBlockedTiles()
    {
        var gate = CreateGate(10, 20, 13, 23);
        var terrain = new GameMapTerrain((byte[]?)null);
        foreach (var point in gate.GetPossibleLandingPoints().Where(p => p != new Point(12, 21)))
        {
            terrain.ApplyTerrainAttribute(point.X, point.Y, TerrainAttributeType.Blocked, true);
        }

        Assert.That(Enumerable.Range(0, 100).Select(_ => gate.GetRandomWalkablePoint(terrain)), Is.All.EqualTo(new Point(12, 21)));
    }

    /// <summary>
    /// A gate without a walkable tile, or without a known terrain, keeps the plain random placement.
    /// </summary>
    [Test]
    public void RandomWalkablePointFallsBackToTheGate()
    {
        var gate = CreateGate(10, 20, 13, 23);
        var possiblePoints = gate.GetPossibleLandingPoints().ToHashSet();
        var terrain = new GameMapTerrain((byte[]?)null);
        foreach (var point in possiblePoints)
        {
            terrain.ApplyTerrainAttribute(point.X, point.Y, TerrainAttributeType.Blocked, true);
        }

        Assert.That(Enumerable.Range(0, 20).Select(_ => gate.GetRandomWalkablePoint(terrain)), Is.All.Matches<Point>(possiblePoints.Contains));
        Assert.That(Enumerable.Range(0, 20).Select(_ => gate.GetRandomWalkablePoint(null)), Is.All.Matches<Point>(possiblePoints.Contains));
    }

    private static DataModel.Configuration.ExitGate CreateGate(byte x1, byte y1, byte x2, byte y2)
    {
        return new MUnique.OpenMU.Persistence.BasicModel.ExitGate
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
        };
    }
}
