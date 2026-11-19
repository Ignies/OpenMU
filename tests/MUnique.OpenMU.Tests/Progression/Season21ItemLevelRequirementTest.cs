// <copyright file="Season21ItemLevelRequirementTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Progression;

using Moq;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// A Season 21 item's level requirement above the normal level cap (Wings of Virtue 12/180: <c>RequireLevel="1200"</c>)
/// is a Season 21 total level, read through the world's progression mode like an evolution: the live run of 2026-11-18
/// refused it to a character of level 400 and master level 1000.
/// </summary>
[TestFixture]
public class Season21ItemLevelRequirementTest
{
    /// <summary>
    /// The total level counts: level 400 plus master level 1000 meets 1200, plus 700 doesn't.
    /// </summary>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="expected">Whether the item can be worn.</param>
    [TestCase(1000, true)]
    [TestCase(700, false)]
    public async Task TotalLevelMeetsASeason21RequirementAsync(int masterLevel, bool expected)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.Attributes![Stats.Level] = 400;
        player.Attributes[Stats.MasterLevel] = masterLevel;

        Assert.That(player.CompliesRequirements(Wing(player, 1200)), Is.EqualTo(expected));
    }

    /// <summary>
    /// In the original mode of the progression the same holds; a requirement within the level cap stays a normal level.
    /// </summary>
    [Test]
    public async Task OriginalModeReadsTheTotalLevelAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.GameContext.FeaturePlugIns.AddPlugIn(new ProgressionFeaturePlugIn { Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Original } }, true);
        player.Attributes![Stats.Level] = 380;
        player.Attributes[Stats.MasterLevel] = 1000;

        Assert.That(player.CompliesRequirements(Wing(player, 1200)), Is.True);
        Assert.That(player.CompliesRequirements(Wing(player, 400)), Is.False);
    }

    private static Item Wing(Player player, int level)
    {
        var definition = new Persistence.BasicModel.ItemDefinition { Group = 12, Number = 180, Width = 4, Height = 3 };
        player.SelectedCharacter!.CharacterClass = new Persistence.BasicModel.CharacterClass();
        definition.Requirements.Add(new Persistence.BasicModel.AttributeRequirement { Attribute = Stats.Level, MinimumValue = level });
        definition.QualifiedCharacters.Add(player.SelectedCharacter!.CharacterClass!);
        Mock.Get(player.GameContext.Configuration).Setup(c => c.ItemStatProfiles).Returns(TestItemStatProfiles.Configuration.ItemStatProfiles);
        TestItemStatProfiles.ForItem(definition);
        return new Persistence.BasicModel.Item { Definition = definition, Durability = 220 };
    }
}
