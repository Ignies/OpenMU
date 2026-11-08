// <copyright file="CachedCollectionsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using Microsoft.EntityFrameworkCore;
using MUnique.OpenMU.Persistence.EntityFramework;
using MonsterDefinitionEntity = MUnique.OpenMU.Persistence.EntityFramework.Model.MonsterDefinition;
using QuestDefinitionEntity = MUnique.OpenMU.Persistence.EntityFramework.Model.QuestDefinition;

/// <summary>
/// Tests for <see cref="CachedCollections"/>: an object added to the cached configuration is looked up by its id.
/// </summary>
[TestFixture]
public class CachedCollectionsTest
{
    /// <summary>
    /// A cached parent which got a copy of the new object already (a parent assigned with its children) holds it:
    /// a second copy would put the id twice into the cached configuration and break every later cache refresh.
    /// </summary>
    [Test]
    public void FindsACopyWithTheSameId()
    {
        using var context = new EntityDataContext();
        var quests = context.Model.FindEntityType(typeof(MonsterDefinitionEntity))!.FindNavigation(nameof(MonsterDefinitionEntity.RawQuests))!.GetCollectionAccessor()!;
        var id = Guid.NewGuid();
        var giver = new MonsterDefinitionEntity();
        giver.RawQuests.Add(new QuestDefinitionEntity { Id = id });

        Assert.That(CachedCollections.Find(quests, giver, id), Is.SameAs(giver.RawQuests.Single()));
        Assert.That(CachedCollections.Find(quests, giver, Guid.NewGuid()), Is.Null);
    }
}
