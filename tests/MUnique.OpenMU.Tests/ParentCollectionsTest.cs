// <copyright file="ParentCollectionsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using Microsoft.EntityFrameworkCore;
using MUnique.OpenMU.Persistence.EntityFramework;
using ItemDefinitionEntity = MUnique.OpenMU.Persistence.EntityFramework.Model.ItemDefinition;

/// <summary>
/// Tests for <see cref="ParentCollections"/>: the parent's collection of a changed entry comes from the model, not
/// from the tracked parent, whose collections each detect their changes when they are listed.
/// </summary>
[TestFixture]
public class ParentCollectionsTest
{
    /// <summary>
    /// An item definition belongs to the collection of the game configuration its foreign key points to.
    /// </summary>
    [Test]
    public void FindsTheCollectionOfTheForeignKey()
    {
        using var context = new EntityDataContext();
        var item = context.Model.FindEntityType(typeof(ItemDefinitionEntity))!;
        var foreignKey = item.GetForeignKeys().Single(k => k.PrincipalEntityType.ClrType.Name == "GameConfiguration");

        var collection = ParentCollections.Of(foreignKey.PrincipalEntityType, foreignKey);

        Assert.That(collection, Is.Not.Null);
        Assert.That(collection!.IsCollection, Is.True);
        Assert.That(collection, Is.SameAs(foreignKey.PrincipalToDependent));
    }

    /// <summary>
    /// A foreign key of another type finds no collection on the parent.
    /// </summary>
    [Test]
    public void FindsNothingForAnotherKey()
    {
        using var context = new EntityDataContext();
        var item = context.Model.FindEntityType(typeof(ItemDefinitionEntity))!;
        var foreignKey = item.GetForeignKeys().Single(k => k.PrincipalEntityType.ClrType.Name == "GameConfiguration");
        var other = context.Model.GetEntityTypes().First(t => t.ClrType != foreignKey.PrincipalEntityType.ClrType && t.GetNavigations().Any());

        Assert.That(ParentCollections.Of(other, foreignKey), Is.Null);
    }
}
