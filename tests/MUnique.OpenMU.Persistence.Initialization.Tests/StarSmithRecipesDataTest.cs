// <copyright file="StarSmithRecipesDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// The Constellation Forge's recipes as data (GameLogic/StarSmithing/Readme.md "Recipes"): the five weapon shapes of the
/// design (The Serpent, Twin Fangs, Ember Ring, Storm Spine, Wyrm Coil), in a new database's Star Smithing configuration
/// and added by an update to an existing one whose list is empty.
/// </summary>
[TestFixture]
internal class StarSmithRecipesDataTest
{
    private const string UpdateTypeName = "MUnique.OpenMU.Persistence.Initialization.Updates.AddStarSmithRecipesUpdatePlugIn, MUnique.OpenMU.Persistence.Initialization";

    private static readonly string[] Names = ["The Serpent", "Twin Fangs", "Ember Ring", "Storm Spine", "Wyrm Coil"];

    /// <summary>
    /// The default recipes: the design's five weapon shapes, every one a weapon recipe made from the skill orbs and
    /// scrolls, with a name and experience, and every orb and scroll of the item data an ingredient.
    /// </summary>
    [Test]
    public async Task TheDefaultRecipesAreTheDesignsAsync()
    {
        var recipes = StarSmithingConfiguration.DefaultRecipes();
        Assert.Multiple(() =>
        {
            Assert.That(recipes.Select(r => r.Name), Is.EqualTo(Names));
            Assert.That(recipes.Select(r => r.Number), Is.EqualTo(new short[] { 1, 2, 3, 4, 5 }));
            Assert.That(recipes.Select(r => r.Shape!.Stars), Is.EqualTo(new[] { new[] { 0, 1, 2, 3, 4, 5, 6 }, [9, 6, 10], [6, 7, 8], [6, 4, 2, 0], [1, 11, 3, 5] }));
            Assert.That(recipes.All(r => r.Targets == StarSmithTargetKind.Weapon), Is.True);
            Assert.That(recipes.All(r => r.Experience > 0 && r.BaseChance > 0), Is.True);
            Assert.That(recipes.Count(r => r.Risky), Is.EqualTo(1), "one risky recipe; the Guardian protects it");
        });

        var (context, gameConfiguration) = await CreateSeason6ConfigurationAsync().ConfigureAwait(false);
        using var _ = context;
        var ingredients = recipes[0].Ingredients.Select(i => (i.Group, i.Number)).ToHashSet();
        Assert.That(ingredients, Has.Count.GreaterThanOrEqualTo(20), "the Season 6 skill orbs and scrolls");
        Assert.That(ingredients.Where(i => !gameConfiguration.Items.Any(d => d.Group == i.Group && d.Number == i.Number && d.Skill is not null)), Is.Empty, "each one a skill item of the data");
        Assert.That(recipes.SelectMany(r => r.Ingredients).All(i => i is { Minimum: 0, Maximum: 10 }), Is.True, "any mix of them, up to ten each");
        Assert.That(recipes.All(r => r.Ingredients.Select(i => (i.Group, i.Number)).ToHashSet().SetEquals(ingredients)), Is.True, "one family, so a sky holds every shape");
    }

    /// <summary>
    /// A new database's Star Smithing configuration holds the recipes.
    /// </summary>
    [Test]
    public async Task NewDatabaseHasTheRecipesAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6ConfigurationAsync().ConfigureAwait(false);
        using var _ = context;
        Assert.That(Configuration(gameConfiguration).Recipes.Select(r => r.Name), Is.EqualTo(Names));
    }

    /// <summary>
    /// The update fills an empty recipe list, once; a list the operator made stays.
    /// </summary>
    [Test]
    public async Task UpdateFillsAnEmptyListOnceAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6ConfigurationAsync().ConfigureAwait(false);
        using var _ = context;
        var smithing = PlugIn(gameConfiguration);
        var emptied = Configuration(gameConfiguration);
        emptied.Recipes = [];
        smithing.SetConfiguration(emptied, null);

        var update = CreateUpdate();
        Assert.That(update.IsMandatory, Is.True);
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        Assert.That(Configuration(gameConfiguration).Recipes.Select(r => r.Name), Is.EqualTo(Names));
        Assert.That(Configuration(gameConfiguration).Dyes, Has.Count.EqualTo(15), "the rest of the configuration stays");

        var own = Configuration(gameConfiguration);
        own.Recipes = [new StarSmithRecipe { Number = 77, Name = "Own" }];
        smithing.SetConfiguration(own, null);
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        Assert.That(Configuration(gameConfiguration).Recipes.Select(r => r.Number), Is.EqualTo(new short[] { 77 }));
    }

    private static PlugInConfiguration PlugIn(GameConfiguration gameConfiguration)
        => gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(StarSmithingFeaturePlugIn).GUID);

    private static StarSmithingConfiguration Configuration(GameConfiguration gameConfiguration)
        => PlugIn(gameConfiguration).GetConfiguration<StarSmithingConfiguration>(null)!;

    private static UpdatePlugInBase CreateUpdate()
    {
        var type = Type.GetType(UpdateTypeName);
        Assert.That(type, Is.Not.Null, "The update AddStarSmithRecipesUpdatePlugIn is missing.");
        return (UpdatePlugInBase)Activator.CreateInstance(type!)!;
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateSeason6ConfigurationAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);

        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        return (context, gameConfiguration);
    }
}
