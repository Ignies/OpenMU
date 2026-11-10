// <copyright file="ForkInfoTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using System.IO;
using System.Text.Json;
using MUnique.OpenMU.GameLogic;

/// <summary>
/// Tests the version marker of the fork, <see cref="ForkInfo"/> and the <c>fork.json</c> in the repository root.
/// </summary>
[TestFixture]
public class ForkInfoTest
{
    /// <summary>
    /// The marker file in the repository, which installers read, says the same as the runtime marker.
    /// </summary>
    [Test]
    public void MarkerFileMatchesTheRuntimeMarker()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(FindMarkerFile()));
        var root = document.RootElement;

        Assert.That(root.GetProperty("fork").GetString(), Is.EqualTo(ForkInfo.Name));
        Assert.That(root.GetProperty("line").GetString(), Is.EqualTo(ForkInfo.Line));
        Assert.That(root.GetProperty("markerVersion").GetInt32(), Is.EqualTo(ForkInfo.MarkerVersion));
        Assert.That(root.GetProperty("features").EnumerateArray().Select(f => f.GetString()), Is.EqualTo(ForkInfo.Features));
    }

    /// <summary>
    /// A feature id is lower case snake case of at most 40 characters (what the installer accepts) and listed once,
    /// so clients can compare them as plain strings.
    /// </summary>
    [Test]
    public void FeatureIdsAreUniqueSnakeCase()
    {
        Assert.That(ForkInfo.Features, Is.Unique);
        Assert.That(ForkInfo.Features, Has.All.Matches("^[a-z][a-z0-9_]{0,39}$"));
        Assert.That(ForkInfo.Has("s21_maps"), Is.True);
        Assert.That(ForkInfo.Has("gens"), Is.True);
        Assert.That(ForkInfo.Has("not-a-feature"), Is.False);
    }

    private static string FindMarkerFile()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "fork.json")))
        {
            directory = directory.Parent;
        }

        Assert.That(directory, Is.Not.Null, "fork.json not found above the test directory");
        return Path.Combine(directory!.FullName, "fork.json");
    }
}
