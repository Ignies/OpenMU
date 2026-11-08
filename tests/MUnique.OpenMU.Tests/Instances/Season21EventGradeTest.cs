// <copyright file="Season21EventGradeTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;

/// <summary>
/// Tests the grade a player enters on a world with the event grades of Season 21, which the Season 6 client does not know.
/// </summary>
[TestFixture]
public class Season21EventGradeTest
{
    /// <summary>
    /// A master class at total level 400 asks for the master temple (6), which Season 21 has switched off; it enters the
    /// temple whose grade holds its total level (2: 400 to 799).
    /// </summary>
    [Test]
    public async ValueTask MasterTempleRequestEntersTheGradeOfTheTotalLevelAsync()
    {
        var world = await Season21EventWorld.CreateAsync(new AddSeason21EventRulesUpdatePlugIn()).ConfigureAwait(false);
        var player = await world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 0).ConfigureAwait(false);

        var definition = world.Configuration.FindEnterableDefinition(player, MiniGameType.IllusionTemple, 6);

        Assert.That(definition?.GameLevel, Is.EqualTo(2));
    }

    /// <summary>
    /// A Season 6 Blood Castle grade the client picked by the normal level (5) is replaced by the Season 21 grade of the
    /// total level (400 + 400 = 800: grade 3).
    /// </summary>
    [Test]
    public async ValueTask BloodCastleRequestOutOfTheGradeEntersTheGradeOfTheTotalLevelAsync()
    {
        var world = await Season21EventWorld.CreateAsync(new AddSeason21EventRulesUpdatePlugIn()).ConfigureAwait(false);
        var player = await world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 400).ConfigureAwait(false);

        var definition = world.Configuration.FindEnterableDefinition(player, MiniGameType.BloodCastle, 8);

        Assert.That(definition?.GameLevel, Is.EqualTo(3));
    }

    /// <summary>
    /// A request whose grade holds the player stays as it is.
    /// </summary>
    [Test]
    public async ValueTask RequestInTheGradeStaysAsync()
    {
        var world = await Season21EventWorld.CreateAsync(new AddSeason21EventRulesUpdatePlugIn()).ConfigureAwait(false);
        var player = await world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 0).ConfigureAwait(false);

        var definition = world.Configuration.FindEnterableDefinition(player, MiniGameType.IllusionTemple, 2);

        Assert.That(definition?.GameLevel, Is.EqualTo(2));
    }

    /// <summary>
    /// Without a grade that holds the player, the requested one is kept, so the usual refusal names its level.
    /// </summary>
    [Test]
    public async ValueTask NoMatchingGradeKeepsTheRequestAsync()
    {
        var world = await Season21EventWorld.CreateAsync(new AddSeason21EventRulesUpdatePlugIn()).ConfigureAwait(false);
        var player = await world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 100, 0).ConfigureAwait(false);

        var definition = world.Configuration.FindEnterableDefinition(player, MiniGameType.IllusionTemple, 1);

        Assert.That(definition?.GameLevel, Is.EqualTo(1));
    }
}
