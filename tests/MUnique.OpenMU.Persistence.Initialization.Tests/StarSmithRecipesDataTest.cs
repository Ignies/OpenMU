// <copyright file="StarSmithRecipesDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The Constellation Forge's recipes as data (GameLogic/StarSmithing/Readme.md "Recipes"): the five weapon shapes of the
/// design (The Serpent, Twin Fangs, Ember Ring, Storm Spine, Wyrm Coil), in a new database's Star Smithing configuration
/// and added by an update to an existing one whose list is empty.
/// </summary>
[TestFixture]
internal class StarSmithRecipesDataTest
{
    private const string UpdateTypeName = "MUnique.OpenMU.Persistence.Initialization.Updates.AddStarSmithRecipesUpdatePlugIn, MUnique.OpenMU.Persistence.Initialization";

    private static readonly string[] Names = ["The Serpent", "Twin Fangs", "Ember Ring", "Storm Spine", "Wyrm Coil", "Aegis Weave", "Bulwark Knot", "Featherfall", "Tempest Pinions", "Heart of Embers", "Circle of Tides", "Fire", "Poison", "Frost", "Storm", "Shadow", "Light", "Blood", "Earth", "Water", "Wind", "Arcane", "Nature", "Gold", "Silver", "Obsidian", "Jade", "Crimson Steel", "Starlight", "Prism", "Chroma", "Pearl", "Bronze"];

    /// <summary>
    /// The default recipes: the design's five weapon shapes, then armour, wings, a pendant and a ring, every one made
    /// from the skill orbs and scrolls, with a name, experience, a hint and an outcome table, and every orb and scroll
    /// of the item data an ingredient.
    /// </summary>
    [Test]
    public async Task TheDefaultRecipesAreTheDesignsAsync()
    {
        var recipes = StarSmithingConfiguration.DefaultRecipes();
        Assert.Multiple(() =>
        {
            Assert.That(recipes.Select(r => r.Name), Is.EqualTo(Names));
            Assert.That(recipes.Select(r => r.Number), Is.EqualTo(Enumerable.Range(1, 11).Concat(Enumerable.Range(101, 22)).Select(n => (short)n)));
            Assert.That(recipes.Take(11).Select(r => r.Geometry!.Nodes), Is.EqualTo(new[] { 6, 3, 3, 4, 4, 4, 3, 4, 5, 3, 3 }));
            Assert.That(recipes.Skip(11).Select(r => (int)r.Look), Is.EqualTo(Enumerable.Range(1, 22)), "the looks in catalogue order");
            Assert.That(recipes.Skip(11).All(r => r.Targets == StarSmithingConfiguration.AnyItem && r.Outcomes.Count == 0 && r.HintKind == StarSmithHintKind.None && r.Experience == 10 * r.Geometry!.Placements), Is.True);
            Assert.That(recipes.Take(11).Select(r => r.Targets), Is.EqualTo(new[]
            {
                StarSmithTargetKind.Weapon, StarSmithTargetKind.Weapon, StarSmithTargetKind.Weapon, StarSmithTargetKind.Weapon, StarSmithTargetKind.Weapon,
                StarSmithTargetKind.Armor, StarSmithTargetKind.Armor, StarSmithTargetKind.Wings, StarSmithTargetKind.Wings, StarSmithTargetKind.Pendant, StarSmithTargetKind.Ring,
            }));
            Assert.That(recipes.All(r => r.Experience > 0 && r.BaseChance > 0) && recipes.Take(11).All(r => r.Outcomes.Count > 0), Is.True);
            Assert.That(recipes.Select(r => r.HintKind).Distinct(), Is.EquivalentTo(new[] { StarSmithHintKind.None, StarSmithHintKind.Riddle, StarSmithHintKind.Ingredient }), "every hint kind");
            Assert.That(recipes.Count(r => r.Risky), Is.EqualTo(1), "one risky recipe; the Guardian protects it");
            Assert.That(recipes.All(r => StarSmithConfigValidator.Validate(new StarSmithingConfiguration { Recipes = [r] }, (_, _) => true).Count == 0), Is.True, "each one valid");
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

    /// <summary>
    /// The PR 6 update adds the default recipes a database lacks by number and gives a stored default recipe without
    /// outcomes its default table; the operator's own recipes and numbers stay as they are.
    /// </summary>
    [Test]
    public async Task ItemRecipesUpdateAddsOnlyWhatIsMissingAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6ConfigurationAsync().ConfigureAwait(false);
        using var _ = context;
        var smithing = PlugIn(gameConfiguration);
        var old = Configuration(gameConfiguration);
        old.Recipes = StarSmithingConfiguration.DefaultRecipes().Take(5).ToList();
        old.Recipes.ForEach(r => r.Outcomes.Clear());
        old.Recipes[1].Name = "Mine";
        old.Recipes.Add(new StarSmithRecipe { Number = 6, Name = "Own six", Outcomes = [new StarSmithOutcome { Weight = 1 }] });
        old.Recipes.Add(new StarSmithRecipe { Number = 77, Name = "Own" });
        smithing.SetConfiguration(old, null);

        var type = Type.GetType("MUnique.OpenMU.Persistence.Initialization.Updates.AddStarSmithItemRecipesUpdatePlugIn, MUnique.OpenMU.Persistence.Initialization");
        Assert.That(type, Is.Not.Null, "The update AddStarSmithItemRecipesUpdatePlugIn is missing.");
        var update = (UpdatePlugInBase)Activator.CreateInstance(type!)!;
        Assert.That(update.IsMandatory, Is.True);
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var recipes = Configuration(gameConfiguration).Recipes;
        var defaults = StarSmithingConfiguration.DefaultRecipes();
        Assert.That(recipes.Select(r => r.Number), Is.EquivalentTo(Enumerable.Range(1, 11).Concat(Enumerable.Range(101, 22)).Append(77).Select(n => (short)n)));
        Assert.That(recipes.Single(r => r.Number == 2).Name, Is.EqualTo("Mine"), "the operator's name stays");
        Assert.That(recipes.Where(r => r.Number <= 5).All(r => r.Outcomes.Count == defaults.Single(d => d.Number == r.Number).Outcomes.Count), Is.True, "the default tables");
        Assert.That(recipes.Single(r => r.Number == 6).Name, Is.EqualTo("Own six"), "never overwritten");
        Assert.That(recipes.Single(r => r.Number == 77).Outcomes, Is.Empty, "not a default number: untouched");
        Assert.That(recipes.Single(r => r.Number == 9).HintKind, Is.EqualTo(StarSmithHintKind.Ingredient));
    }

    /// <summary>
    /// The PR 7a update adds the 22 looks constellations a database lacks and gives a stored default boost recipe
    /// without a shape (a PR 5/6 star shape no longer reads) its default geometry; an operator's own recipes and
    /// numbers stay, and the defaults it writes pass the validator together.
    /// </summary>
    [Test]
    public async Task ConstellationsUpdateAddsTheLooksAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6ConfigurationAsync().ConfigureAwait(false);
        using var _ = context;
        var smithing = PlugIn(gameConfiguration);
        var old = Configuration(gameConfiguration);
        old.Recipes = StarSmithingConfiguration.DefaultRecipes().Take(11).ToList();
        old.Recipes.ForEach(r => r.Geometry = null);
        old.Recipes[1].Name = "Mine";
        old.Recipes.Add(new StarSmithRecipe { Number = 105, Name = "Own five", Look = StarSmithLook.Gold });
        old.Recipes.Add(new StarSmithRecipe { Number = 77, Name = "Own" });
        smithing.SetConfiguration(old, null);

        var type = Type.GetType("MUnique.OpenMU.Persistence.Initialization.Updates.AddStarSmithConstellationsUpdatePlugIn, MUnique.OpenMU.Persistence.Initialization");
        Assert.That(type, Is.Not.Null, "The update AddStarSmithConstellationsUpdatePlugIn is missing.");
        var update = (UpdatePlugInBase)Activator.CreateInstance(type!)!;
        Assert.That(update.IsMandatory, Is.True);
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var recipes = Configuration(gameConfiguration).Recipes;
        var defaults = StarSmithingConfiguration.DefaultRecipes();
        Assert.Multiple(() =>
        {
            Assert.That(recipes.Select(r => r.Number), Is.EqualTo(Enumerable.Range(1, 11).Append(77).Concat(Enumerable.Range(101, 22)).Select(n => (short)n)));
            Assert.That(recipes.Where(r => r.Number <= 11).All(r => r.Geometry!.Turns.SequenceEqual(defaults.Single(d => d.Number == r.Number).Geometry!.Turns)), Is.True, "the default shapes");
            Assert.That(recipes.Single(r => r.Number == 2).Name, Is.EqualTo("Mine"), "the operator's name stays");
            Assert.That(recipes.Single(r => r.Number == 105).Name, Is.EqualTo("Own five"), "never overwritten");
            Assert.That(recipes.Single(r => r.Number == 105).Geometry, Is.Null, "an operator's recipe gets no shape");
            Assert.That(recipes.Single(r => r.Number == 77).Geometry, Is.Null);
            Assert.That(recipes.Single(r => r.Number == 113).Look, Is.EqualTo(StarSmithLook.Gold));
        });
    }

    private static PlugInConfiguration PlugIn(GameConfiguration gameConfiguration)
        => gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(StarSmithingFeaturePlugIn).GUID);

    private static StarSmithingConfiguration Configuration(GameConfiguration gameConfiguration)
        => PlugIn(gameConfiguration).GetConfiguration<StarSmithingConfiguration>(new IdIgnoringReferenceHandler())!;

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
