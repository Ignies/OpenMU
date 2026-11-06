// <copyright file="CrywolfSeason21Test.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using MUnique.OpenMU.GameLogic.Crywolf;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;

/// <summary>
/// Tests the Crywolf renewal of Season 21 (S21-1 Update 3): the element of the week, the earlier Balgass and the rewards
/// in the gremory case.
/// </summary>
[TestFixture]
public class CrywolfSeason21Test
{
    private Season21EventWorld _world = null!;

    /// <summary>
    /// Initializes the world with the renewal.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._world = await Season21EventWorld.CreateAsync(new AddSeason21CrywolfUpdatePlugIn()).ConfigureAwait(false);
    }

    /// <summary>
    /// The element changes every Thursday at 00:00 in the order fire, water, earth, wind and darkness.
    /// </summary>
    [Test]
    public void ElementRotatesOnThursday()
    {
        var definition = new CrywolfEventDefinition { WeeklyElements = [Element.Fire, Element.Water, Element.Earth, Element.Wind, Element.Darkness] };
        var thursday = new DateTime(2026, 11, 5, 0, 0, 0, DateTimeKind.Utc);
        var first = definition.GetElementOfTheWeek(thursday);
        Assert.That(definition.GetElementOfTheWeek(thursday.AddDays(6).AddHours(23)), Is.EqualTo(first));
        var next = definition.GetElementOfTheWeek(thursday.AddDays(7));
        Assert.That((int)next, Is.EqualTo(((int)first % 5) + 1));
        Assert.That(definition.GetElementOfTheWeek(thursday.AddDays(35)), Is.EqualTo(first));
        Assert.That(new CrywolfEventDefinition().GetElementOfTheWeek(thursday), Is.EqualTo(Element.None), "Season 6 has none");
    }

    /// <summary>
    /// The update sets the element rotation, Balgass at 6:40 into the battle, the rewards in the gremory case and the
    /// state durations of S21Emu <c>Event/Crywolf.dat</c>.
    /// </summary>
    [Test]
    public void UpdateSetsTheRenewal()
    {
        var definition = this._world.GetStoredConfiguration<CrywolfEventDefinition>(typeof(CrywolfPlugIn).GUID)!;
        Assert.That(definition.WeeklyElements, Is.EqualTo(new[] { Element.Fire, Element.Water, Element.Earth, Element.Wind, Element.Darkness }));
        Assert.That(definition.BalgassAppearanceDelay, Is.EqualTo(TimeSpan.FromMinutes(6) + TimeSpan.FromSeconds(40)));
        Assert.That(definition.RewardsIntoGremoryCase, Is.True);
        Assert.That(
            new[] { definition.Notify1Duration, definition.Notify2Duration, definition.ReadyDuration, definition.BattleDuration, definition.EndDuration, definition.EndCycleDuration },
            Is.EqualTo(new[] { 300, 60, 120, 900, 15, 10 }.Select(seconds => TimeSpan.FromSeconds(seconds))));
    }

    /// <summary>
    /// A reward of a hero goes into the gremory case or the inventory instead of the ground.
    /// </summary>
    [Test]
    public async ValueTask RewardGoesToThePlayerAsync()
    {
        var definition = this._world.GetStoredConfiguration<CrywolfEventDefinition>(typeof(CrywolfPlugIn).GUID)!;
        var player = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 600).ConfigureAwait(false);

        Assert.That(await CrywolfRewards.GiveAsync(player, definition).ConfigureAwait(false), Is.True);

        Assert.That(player.Inventory!.Items.Select(i => (i.Definition?.Group, i.Definition?.Number)), Does.Contain(((byte?)definition.RewardItemGroup, (short?)definition.RewardItemNumber)));
    }
}
