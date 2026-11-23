// <copyright file="StarSmithGuidebookTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.GameLogic.Views.StarSmithing;
using Moq;

/// <summary>
/// The smith's Guidebook (Readme.md "Guidebook"): every recipe hidden until made once, then known with its made count,
/// stored on the character; the known set feeds the Forge's guidance and first completion; C2 EE 90 tells the client.
/// And the smith's experience from an attempt (design section 3, step 6).
/// </summary>
[TestFixture]
public class StarSmithGuidebookTest
{
    private const byte HammerSlot = 0;

    private const byte TargetSlot = 12;

    private const byte IngredientSlot = 13;

    private const string PlugIn = "MUnique.OpenMU.GameServer.RemoteView.StarSmithing.StarSmithingBabylonPlugIn, MUnique.OpenMU.GameServer";

    private long _now;

    /// <summary>
    /// The stored text: "number:made" pairs, sorted by number; a record raises the count or adds the recipe; unreadable
    /// parts are skipped.
    /// </summary>
    [Test]
    public void TheStoredGuidebook()
    {
        Assert.That(StarSmithGuidebook.Parse(null), Is.Empty);
        Assert.That(StarSmithGuidebook.Record(null, 7), Is.EqualTo("7:1"));
        Assert.That(StarSmithGuidebook.Record("7:1", 7), Is.EqualTo("7:2"));
        Assert.That(StarSmithGuidebook.Record("7:2", 3), Is.EqualTo("3:1;7:2"));
        Assert.That(StarSmithGuidebook.Parse("3:1;7:2"), Is.EquivalentTo(new Dictionary<short, int> { [3] = 1, [7] = 2 }));
        Assert.That(StarSmithGuidebook.Parse("x;7:2;9;:4;5:-1"), Is.EquivalentTo(new Dictionary<short, int> { [7] = 2 }));

        // PR 6: a third part, the outcomes seen as a bit mask by index.
        Assert.That(StarSmithGuidebook.Record("7:1", 7, 2), Is.EqualTo("7:2:4"));
        Assert.That(StarSmithGuidebook.Record("7:2:4", 7, 0), Is.EqualTo("7:3:5"));
        Assert.That(StarSmithGuidebook.Record("3:1;7:3:5", 3), Is.EqualTo("3:2;7:3:5"));
        Assert.That(StarSmithGuidebook.Parse("7:3:5"), Is.EquivalentTo(new Dictionary<short, int> { [7] = 3 }));
        Assert.That(StarSmithGuidebook.Seen("3:1;7:3:5"), Is.EquivalentTo(new Dictionary<short, int> { [3] = 0, [7] = 5 }));
    }

    /// <summary>
    /// A seal records the outcome it rolled as seen.
    /// </summary>
    [Test]
    public async ValueTask ASealRecordsTheOutcomeSeenAsync()
    {
        var recipe = Sure(7, 50);
        recipe.Outcomes =
        [
            new StarSmithOutcome { Weight = 1, Tier = StarSmithRarity.Mythic, Boosts = [new() { Stat = StarSmithBoostStat.Damage, Minimum = 1, Maximum = 1 }] },
            new StarSmithOutcome { Weight = 1, Tier = StarSmithRarity.Common, Boosts = [new() { Stat = StarSmithBoostStat.Defense, Minimum = 1, Maximum = 1 }] },
        ];
        var (player, plugIn) = await SmithAsync(recipe).ConfigureAwait(false);
        await this.SealAsync(player, plugIn).ConfigureAwait(false);
        Assert.That(player.SelectedCharacter!.StarSmithGuidebook, Is.EqualTo("7:1:2"), "only the Common outcome is in reach of an Epic seal");
    }

    /// <summary>
    /// A found row carries its shape's length, its ingredients and, per stat, the range of the outcomes seen.
    /// </summary>
    [Test]
    public async ValueTask AFoundRowShowsWhatWasSeenAsync()
    {
        var (player, plugIn) = await SmithAsync(null).ConfigureAwait(false);
        player.SelectedCharacter!.StarSmithGuidebook = "1:2:5";

        var serpent = plugIn.GuidebookOf(player)[0];

        Assert.That(IsFound(serpent, 1, 2, "The Serpent"), Is.True);
        Assert.That(serpent.ShapeLength, Is.EqualTo(7));
        Assert.That(serpent.Ingredients, Has.Count.EqualTo(37));
        Assert.That(serpent.BoostsSeen, Is.EqualTo(new[] { new StarSmithBoostSeen(StarSmithBoostStat.Damage, 100, 400), new StarSmithBoostSeen(StarSmithBoostStat.ExcellentRate, 100, 200) }));
    }

    /// <summary>
    /// The first seal of a recipe writes it into the Guidebook, flags the first completion, doubles the experience and
    /// sends the Guidebook.
    /// </summary>
    [Test]
    public async ValueTask TheFirstSealFindsTheRecipeAsync()
    {
        var (player, plugIn) = await SmithAsync(Sure(7, 50)).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);

        await this.SealAsync(player, plugIn).ConfigureAwait(false);

        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Sealed, It.Is<StarSmithForgeResult>(r => r.FirstCompletion)), Times.Once);
        Assert.That(player.SelectedCharacter!.StarSmithGuidebook, Is.EqualTo("7:1"));
        Assert.That((player.SelectedCharacter.StarSmithLevel, player.SelectedCharacter.StarSmithExperience), Is.EqualTo(((short)2, 0L)), "50 doubled reaches level 2");
        view.Verify(v => v.SendGuidebookAsync(1, It.Is<IReadOnlyList<StarSmithGuidebookEntry>>(f => f.Count == 1 && IsFound(f[0], 7, 1, "Sure"))), Times.AtLeastOnce);
    }

    /// <summary>
    /// A known recipe guides the path and is no longer a first completion; its made count rises.
    /// </summary>
    [Test]
    public async ValueTask AKnownRecipeGuidesAsync()
    {
        var (player, plugIn) = await SmithAsync(Sure(7, 50)).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        player.SelectedCharacter!.StarSmithGuidebook = "7:1";

        await this.SealAsync(player, plugIn).ConfigureAwait(false);

        view.Verify(v => v.SendForgeStrikeAsync(0, It.IsAny<StarSmithStrike>(), It.IsAny<int>(), It.Is<IReadOnlyList<short>>(g => g.Contains((short)7))), Times.Once);
        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Sealed, It.Is<StarSmithForgeResult>(r => !r.FirstCompletion)), Times.Once);
        Assert.That(player.SelectedCharacter.StarSmithGuidebook, Is.EqualTo("7:2"));
        Assert.That(player.SelectedCharacter.StarSmithExperience, Is.EqualTo(50));
    }

    /// <summary>
    /// A crack gives 30% of the recipe's experience and finds nothing; an abandon gives none.
    /// </summary>
    [Test]
    public async ValueTask ACrackGivesThirtyPercentAsync()
    {
        var (player, plugIn) = await SmithAsync(Doomed(8, 50)).ConfigureAwait(false);
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        var attempt = player.StarSmithForge!;
        foreach (var star in new byte[] { 0, 1, 2 })
        {
            this._now += 800;
            await plugIn.StrikeForgeAsync(player, star).ConfigureAwait(false);
            if (attempt.Sky.Stars[star].IsVoid)
            {
                break;
            }
        }

        Assert.That(player.StarSmithForge, Is.Null, "cracked");
        Assert.That(player.SelectedCharacter!.StarSmithExperience, Is.EqualTo(15));
        Assert.That(player.SelectedCharacter.StarSmithGuidebook, Is.Null.Or.Empty);

        var (other, otherPlugIn) = await SmithAsync(Sure(7, 50)).ConfigureAwait(false);
        await otherPlugIn.StartForgeAsync(other, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        await otherPlugIn.AbandonForgeAsync(other).ConfigureAwait(false);
        Assert.That(other.SelectedCharacter!.StarSmithExperience, Is.Zero);
    }

    /// <summary>
    /// C2 EE 90 v2 (Readme.md "PR 6 contract"): the recipe count, the entry count (u16), then 75-byte entries. A found
    /// row (kind 0: number, made, shape length, the lines after it, the name) is followed by its ingredient lines (kind
    /// 2, up to 12 ingredients each) and its boost lines (kind 3, up to 8 stats each); a sealed row (kind 1) carries
    /// only its hint: its kind, an ingredient's item or a riddle's text.
    /// </summary>
    [Test]
    public async ValueTask TheGuidebookBytesAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        var type = Type.GetType(PlugIn)!;
        var found = StarSmithGuidebookEntry.Found(1, 4, "The Serpent") with
        {
            ShapeLength = 7,
            Ingredients = [new StarSmithIngredient { Group = 12, Number = 7, Minimum = 0, Maximum = 10 }, new StarSmithIngredient { Group = 15, Number = 3, Minimum = 1, Maximum = 2 }],
            BoostsSeen = [new StarSmithBoostSeen(StarSmithBoostStat.Damage, 100, 400)],
        };
        IReadOnlyList<StarSmithGuidebookEntry> entries =
        [
            found,
            StarSmithGuidebookEntry.Sealed(StarSmithHintKind.Ingredient, string.Empty, 12, 7),
            StarSmithGuidebookEntry.Sealed(StarSmithHintKind.Riddle, "Two fangs bite across the guard.", 0, 0),
        ];
        await ((ValueTask)type.GetMethod("SendGuidebookAsync")!.Invoke(Activator.CreateInstance(type, player), [5, entries])!).ConfigureAwait(false);

        var expected = new byte[8 + (5 * 75)];
        new byte[] { 0xC2, 0x01, 0x7F, 0xEE, 0x90, 5, 5, 0 }.CopyTo(expected, 0);
        new byte[] { 0, 1, 0, 4, 0, 7, 0, 0, 0, 0, 2 }.CopyTo(expected, 8);
        System.Text.Encoding.UTF8.GetBytes("The Serpent").CopyTo(expected, 19);
        new byte[] { 2, 2, 12, 7, 0, 0, 10, 15, 3, 0, 1, 2 }.CopyTo(expected, 83);
        new byte[] { 3, 1, 0, 100, 0, 0, 0, 0x90, 1, 0, 0 }.CopyTo(expected, 158);
        new byte[] { 1, 0, 0, 0, 0, 0, 2, 12, 7, 0, 0 }.CopyTo(expected, 233);
        new byte[] { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 }.CopyTo(expected, 308);
        System.Text.Encoding.UTF8.GetBytes("Two fangs bite across the guard.").CopyTo(expected, 319);
        Assert.That(output.ToArray(), Is.EqualTo(expected));
    }

    /// <summary>
    /// The recipes not found yet go out as sealed rows after the found ones, in recipe order, with their hint only: no
    /// number, no made count and nothing that names them. Every default recipe has a hint that doesn't name it.
    /// </summary>
    [Test]
    public async ValueTask TheUnfoundAreSealedAsync()
    {
        var (player, plugIn) = await SmithAsync(null).ConfigureAwait(false);
        player.SelectedCharacter!.StarSmithGuidebook = "2:1";
        var recipes = StarSmithingConfiguration.DefaultRecipes();
        Assert.That(recipes.Where(r => r.HintKind == StarSmithHintKind.Riddle).All(r => r.Hint.Length is > 0 and <= 64 && !r.Hint.Contains(r.Name, StringComparison.OrdinalIgnoreCase)), Is.True);

        var entries = plugIn.GuidebookOf(player);

        Assert.That(IsFound(entries[0], 2, 1, "Twin Fangs"), Is.True);
        var sealedRows = entries.Skip(1).ToList();
        var others = recipes.Where(r => r.Number != 2).ToList();
        Assert.That(sealedRows.Select(e => e.Text), Is.EqualTo(others.Select(r => r.HintKind == StarSmithHintKind.Riddle ? r.Hint : string.Empty)));
        Assert.That(sealedRows.Select(e => e.HintKind), Is.EqualTo(others.Select(r => r.HintKind)));
        Assert.That(sealedRows.Select(e => (e.HintGroup, e.HintNumber)), Is.EqualTo(others.Select(r => r.HintKind == StarSmithHintKind.Ingredient ? (r.HintGroup, r.HintNumber) : ((byte)0, (short)0))));
        Assert.That(sealedRows.All(e => e is { Kind: StarSmithGuidebookEntryKind.Sealed, Recipe: 0, Made: 0, ShapeLength: 0, Ingredients.Count: 0, BoostsSeen.Count: 0 }), Is.True);
        Assert.That(sealedRows.Any(e => recipes.Any(r => e.Text.Contains(r.Name, StringComparison.OrdinalIgnoreCase))), Is.False);
    }

    /// <summary>
    /// The smith's state goes out with the Guidebook while the hammer is in a hand.
    /// </summary>
    [Test]
    public async ValueTask TheStateSendsTheGuidebookAsync()
    {
        var (player, plugIn) = await SmithAsync(Sure(7, 50)).ConfigureAwait(false);
        player.SelectedCharacter!.StarSmithGuidebook = "7:3;99:1";
        await plugIn.SendStateAsync(player).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        view.Verify(v => v.SendGuidebookAsync(1, It.Is<IReadOnlyList<StarSmithGuidebookEntry>>(f => f.Count == 1 && IsFound(f[0], 7, 3, "Sure"))), Times.Once, "a removed recipe (99) is not shown");
    }

    /// <summary>
    /// With the default recipes, one skill orb on a sword starts the Forge; no ingredient at all makes no recipe.
    /// </summary>
    [Test]
    public async ValueTask TheDefaultRecipesStartWithAnOrbAsync()
    {
        var (player, plugIn) = await SmithAsync(null).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        await AddAsync(player, 14, 12, 7).ConfigureAwait(false);

        await plugIn.StartForgeAsync(player, TargetSlot, [], []).ConfigureAwait(false);
        view.Verify(v => v.SendForgeSkyAsync(It.Is<StarSmithForgeSkyMessage>(m => m.Result == StarSmithResultCode.NoRecipe)), Times.Once);

        await plugIn.StartForgeAsync(player, TargetSlot, [14], []).ConfigureAwait(false);
        view.Verify(v => v.SendForgeSkyAsync(It.Is<StarSmithForgeSkyMessage>(m => m.Result == StarSmithResultCode.Done && m.Strikes == 3)), Times.Once);
        Assert.That(player.StarSmithForge!.Sky.Recipes.Select(r => r.Name), Is.EquivalentTo(new[] { "The Serpent", "Twin Fangs", "Ember Ring", "Storm Spine", "Wyrm Coil" }));
    }

    private static bool IsFound(StarSmithGuidebookEntry entry, short recipe, int made, string name)
        => entry is { Kind: StarSmithGuidebookEntryKind.Found } && entry.Recipe == recipe && entry.Made == made && entry.Text == name;

    private static StarSmithRecipe Sure(short number, int experience) => new()
    {
        Number = number,
        Name = "Sure",
        Experience = experience,
        Targets = StarSmithTargetKind.Weapon,
        Ingredients = [new StarSmithIngredient { Group = 14, Number = 13 }],
        BaseChance = 10_000,
        Shape = new() { Stars = [0, 1, 2] },
    };

    private static StarSmithRecipe Doomed(short number, int experience) => new()
    {
        Number = number,
        Name = "Doomed",
        Experience = experience,
        Targets = StarSmithTargetKind.Weapon,
        Ingredients = [new StarSmithIngredient { Group = 14, Number = 13 }],
        MaximumChance = 0,
        Shape = new() { Stars = [0, 1, 2] },
    };

    private static async ValueTask<Item> AddAsync(Player player, byte slot, byte group, short number, byte durability = 1)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = new MUnique.OpenMU.Persistence.BasicModel.ItemDefinition { Group = group, Number = number, Width = 1, Height = 1, Durability = durability };
        item.Durability = durability;
        await player.Inventory!.AddItemAsync(slot, item).ConfigureAwait(false);
        return item;
    }

    private async ValueTask SealAsync(Player player, StarSmithingFeaturePlugIn plugIn)
    {
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        foreach (var star in new byte[] { 0, 1, 2 })
        {
            this._now += 800;
            await plugIn.StrikeForgeAsync(player, star).ConfigureAwait(false);
        }

        await plugIn.SealForgeAsync(player).ConfigureAwait(false);
    }

    private async ValueTask<(Player Player, StarSmithingFeaturePlugIn PlugIn)> SmithAsync(StarSmithRecipe? recipe)
    {
        this._now = 0;
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        await AddAsync(player, HammerSlot, 2, 1000, 255).ConfigureAwait(false);
        await AddAsync(player, TargetSlot, 0, 1).ConfigureAwait(false);
        await AddAsync(player, IngredientSlot, 14, 13).ConfigureAwait(false);
        var configuration = new StarSmithingConfiguration();
        if (recipe is not null)
        {
            configuration.Recipes = [recipe];
        }

        var plugIn = new StarSmithingFeaturePlugIn
        {
            Configuration = configuration,
            Clock = () => this._now,
            NewSeed = () => 4711,
        };
        return (player, plugIn);
    }
}
