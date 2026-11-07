// <copyright file="ChangedParentsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.EntityFramework;

/// <summary>
/// Tests for <see cref="ChangedParents"/>: a save announces each changed parent once, not once per changed child.
/// </summary>
[TestFixture]
public class ChangedParentsTest
{
    /// <summary>
    /// A class with a hundred changed children is announced once: each announcement copies the whole class into the
    /// running game's cache, so the Season 21 classes update took minutes per class on a live server.
    /// </summary>
    [Test]
    public void AnnouncesEachParentOnce()
    {
        var knight = new object();
        var wizard = new object();
        var parents = Enumerable.Repeat(knight, 100).Append(wizard).Append(knight).Append(null);

        Assert.That(ChangedParents.ToAnnounce(parents), Is.EqualTo(new[] { knight, wizard }));
    }

    /// <summary>
    /// The game configuration itself and parents known only by id are never announced as a changed parent.
    /// </summary>
    [Test]
    public void SkipsTheConfigurationAndIds()
    {
        var parents = new object?[] { new GameConfiguration(), Guid.NewGuid() };

        Assert.That(ChangedParents.ToAnnounce(parents), Is.Empty);
    }
}
