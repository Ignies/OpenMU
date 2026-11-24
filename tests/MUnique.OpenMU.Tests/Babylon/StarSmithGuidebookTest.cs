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
/// The smith's Guidebook (Readme.md "Guidebook", "PR 7a contract"): every constellation hidden until a figure matches
/// it once, then found (made 0) and counted on every seal, stored on the character; the known set feeds the Forge's
/// guide and finds; C2 EE 90 tells the client, found rows with their shapes. And the smith's experience.
/// </summary>
[TestFixture]
public class StarSmithGuidebookTest
{
    private const byte HammerSlot = 0;

    private const byte TargetSlot = 12;

    private const byte IngredientSlot = 13;

    private const int Interval = 800;

    private const string PlugIn = "MUnique.OpenMU.GameServer.RemoteView.StarSmithing.StarSmithingBabylonPlugIn, MUnique.OpenMU.GameServer";

    private long _now;

    /// <summary>
    /// The stored text: "number:made[:seen]" sorted by number; a find adds "number:0" once, a record raises the count;
    /// unreadable parts are skipped, and a made count of 0 is a found constellation.
    /// </summary>
    [Test]
    public void TheStoredGuidebook()
    {
        Assert.Multiple(() =>
        {
            Assert.That(StarSmithGuidebook.Parse(null), Is.Empty);
            Assert.That(StarSmithGuidebook.Record(null, 7), Is.EqualTo("7:1"));
            Assert.That(StarSmithGuidebook.Record("7:1", 7), Is.EqualTo("7:2"));
            Assert.That(StarSmithGuidebook.Record("7:2", 3), Is.EqualTo("3:1;7:2"));
            Assert.That(StarSmithGuidebook.Parse("3:1;7:2"), Is.EquivalentTo(new Dictionary<short, int> { [3] = 1, [7] = 2 }));
            Assert.That(StarSmithGuidebook.Parse("x;7:2;9;:4;5:-1"), Is.EquivalentTo(new Dictionary<short, int> { [7] = 2 }));
            Assert.That(StarSmithGuidebook.Record("7:1", 7, 2), Is.EqualTo("7:2:4"));
            Assert.That(StarSmithGuidebook.Record("7:2:4", 7, 0), Is.EqualTo("7:3:5"));
            Assert.That(StarSmithGuidebook.Seen("3:1;7:3:5"), Is.EquivalentTo(new Dictionary<short, int> { [3] = 0, [7] = 5 }));

            // PR 7a: a find.
            Assert.That(StarSmithGuidebook.Find(null, 101), Is.EqualTo("101:0"));
            Assert.That(StarSmithGuidebook.Find("7:2", 101), Is.EqualTo("7:2;101:0"));
            Assert.That(StarSmithGuidebook.Find("7:2", 7), Is.EqualTo("7:2"), "found once");
            Assert.That(StarSmithGuidebook.Parse("101:0"), Is.EquivalentTo(new Dictionary<short, int> { [101] = 0 }));
            Assert.That(StarSmithGuidebook.Record("101:0", 101), Is.EqualTo("101:1"));
        });
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
        await this.SealAsync(player, plugIn, late: true).ConfigureAwait(false);
        Assert.That(player.SelectedCharacter!.StarSmithGuidebook, Is.EqualTo("7:1:2"), "only the Common outcome is in reach of a Hue seal");
    }

    /// <summary>
    /// A found row carries its shape (placements, geometry, look), its ingredients and the outcomes seen.
    /// </summary>
    [Test]
    public async ValueTask AFoundRowShowsItsShapeAsync()
    {
        var (player, plugIn) = await SmithAsync(null).ConfigureAwait(false);
        player.SelectedCharacter!.StarSmithGuidebook = "1:2:5;101:0";

        var rows = plugIn.GuidebookOf(player);
        var serpent = rows[0];
        var fire = rows[1];
        Assert.Multiple(() =>
        {
            Assert.That(IsFound(serpent, 1, 2, "The Serpent"), Is.True);
            Assert.That(serpent.ShapeLength, Is.EqualTo(6));
            Assert.That(serpent.Ingredients, Has.Count.EqualTo(37));
            Assert.That(serpent.BoostsSeen, Is.EqualTo(new[] { new StarSmithBoostSeen(StarSmithBoostStat.Damage, 100, 400), new StarSmithBoostSeen(StarSmithBoostStat.ExcellentRate, 100, 200) }));
            Assert.That(IsFound(fire, 101, 0, "Fire"), Is.True, "found, never sealed");
            Assert.That(fire.ShapeLength, Is.EqualTo(4));
            Assert.That(fire.Look, Is.EqualTo(StarSmithLook.Fire));
            Assert.That(fire.Geometry!.Turns, Is.EqualTo(new[] { 120.0, 120.0 }));
        });
    }

    /// <summary>
    /// The first match of a constellation finds it at once; the first seal counts it made and doubles its experience.
    /// </summary>
    [Test]
    public async ValueTask TheFirstMatchFindsAndTheFirstSealDoublesAsync()
    {
        var (player, plugIn) = await SmithAsync(Sure(7, 50)).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);

        await this.SealAsync(player, plugIn).ConfigureAwait(false);

        view.Verify(v => v.SendForgeNodeAsync(It.Is<StarSmithFigureAnswer>(a => a.Flags.HasFlag(StarSmithFigureFlags.Found) && a.Match!.Number == 7)), Times.Once);
        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Sealed, It.IsAny<StarSmithForgeResult>(), 100, true), Times.Once);
        Assert.That(player.SelectedCharacter!.StarSmithGuidebook, Is.EqualTo("7:1"));
        Assert.That((player.SelectedCharacter.StarSmithLevel, player.SelectedCharacter.StarSmithExperience), Is.EqualTo(((short)2, 0L)), "50 doubled reaches level 2");
        view.Verify(v => v.SendGuidebookAsync(1, It.Is<IReadOnlyList<StarSmithGuidebookEntry>>(f => f.Count == 1 && IsFound(f[0], 7, 1, "Sure"))), Times.AtLeastOnce);
    }

    /// <summary>
    /// A known constellation is no find, guides a longer figure, and its made count rises without doubling.
    /// </summary>
    [Test]
    public async ValueTask AKnownConstellationGuidesAsync()
    {
        var (player, plugIn) = await SmithAsync(Sure(7, 50)).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        player.SelectedCharacter!.StarSmithGuidebook = "7:1";

        await this.SealAsync(player, plugIn).ConfigureAwait(false);

        view.Verify(v => v.SendForgeNodeAsync(It.Is<StarSmithFigureAnswer>(a => a.FigurePlacements == 3 && a.Guide.Contains((short)7))), Times.Once);
        view.Verify(v => v.SendForgeNodeAsync(It.Is<StarSmithFigureAnswer>(a => a.Flags.HasFlag(StarSmithFigureFlags.Found))), Times.Never);
        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Sealed, It.IsAny<StarSmithForgeResult>(), 50, false), Times.Once);
        Assert.That(player.SelectedCharacter.StarSmithGuidebook, Is.EqualTo("7:2"));
    }

    /// <summary>
    /// A crack gives 30% of the constellation the figure was on and finds nothing; an abandon gives none.
    /// </summary>
    [Test]
    public async ValueTask ACrackGivesThirtyPercentAsync()
    {
        var (player, plugIn) = await SmithAsync(Sure(8, 50), faults: 1).ConfigureAwait(false);
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        var fault = player.StarSmithForge!.Forge.Faults[0];
        foreach (var point in Points(Sure(8, 50)).Take(3))
        {
            this._now += Interval;
            await plugIn.PlaceForgeNodeAsync(player, point).ConfigureAwait(false);
        }

        this._now += Interval;
        await plugIn.PlaceForgeNodeAsync(player, fault.A).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(player.StarSmithForge, Is.Null, "cracked");
            Assert.That(player.SelectedCharacter!.StarSmithExperience, Is.EqualTo(15));
            Assert.That(player.SelectedCharacter.StarSmithGuidebook, Is.Null.Or.Empty);
        });

        var (other, otherPlugIn) = await SmithAsync(Sure(7, 50)).ConfigureAwait(false);
        await otherPlugIn.StartForgeAsync(other, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        await otherPlugIn.AbandonForgeAsync(other).ConfigureAwait(false);
        Assert.That(other.SelectedCharacter!.StarSmithExperience, Is.Zero);
    }

    /// <summary>
    /// C2 EE 90 v3: the recipe count, the entry count (u16), then 75-byte entries. A found row (kind 0: number, made,
    /// placements, the lines after it, the name) is followed by its shape line (kind 4: nodes, flags, look, turns and
    /// lengths, the extension), its ingredient lines (kind 2) and its boost lines (kind 3); a sealed row (kind 1)
    /// carries only its hint.
    /// </summary>
    [Test]
    public async ValueTask TheGuidebookBytesAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        var type = Type.GetType(PlugIn)!;
        var found = StarSmithGuidebookEntry.Found(101, 0, "Fire") with
        {
            ShapeLength = 4,
            Geometry = StarSmithGeometry.Of(3, true, [120, 120], [1, 1, 1]),
            Look = StarSmithLook.Fire,
            Ingredients = [new StarSmithIngredient { Group = 12, Number = 7, Minimum = 0, Maximum = 10 }],
            BoostsSeen = [new StarSmithBoostSeen(StarSmithBoostStat.Damage, 100, 400)],
        };
        IReadOnlyList<StarSmithGuidebookEntry> entries =
        [
            found,
            StarSmithGuidebookEntry.Sealed(StarSmithHintKind.Riddle, "Two fangs bite across the guard.", 0, 0),
        ];
        await ((ValueTask)type.GetMethod("SendGuidebookAsync")!.Invoke(Activator.CreateInstance(type, player), [5, entries])!).ConfigureAwait(false);

        var expected = new byte[8 + (5 * 75)];
        new byte[] { 0xC2, 0x01, 0x7F, 0xEE, 0x90, 5, 5, 0 }.CopyTo(expected, 0);
        new byte[] { 0, 101, 0, 0, 0, 4, 0, 0, 0, 0, 3 }.CopyTo(expected, 8);
        System.Text.Encoding.UTF8.GetBytes("Fire").CopyTo(expected, 19);
        new byte[] { 4, 3, 3, 1, 0, 0, 0xB0, 0x04, 0xB0, 0x04 }.CopyTo(expected, 83);
        new byte[] { 0xE8, 0x03, 0xE8, 0x03, 0xE8, 0x03 }.CopyTo(expected, 83 + 28);
        new byte[] { 0x7C, 0xFC, 0xF4, 0x01 }.CopyTo(expected, 83 + 52);
        new byte[] { 2, 1, 12, 7, 0, 0, 10 }.CopyTo(expected, 158);
        new byte[] { 3, 1, 0, 100, 0, 0, 0, 0x90, 1, 0, 0 }.CopyTo(expected, 233);
        new byte[] { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 }.CopyTo(expected, 308);
        System.Text.Encoding.UTF8.GetBytes("Two fangs bite across the guard.").CopyTo(expected, 319);
        Assert.That(output.ToArray(), Is.EqualTo(expected));
    }

    /// <summary>
    /// The constellations not found go out as sealed rows after the found ones, with their hint only; the default
    /// looks constellations have none.
    /// </summary>
    [Test]
    public async ValueTask TheUnfoundAreSealedAsync()
    {
        var (player, plugIn) = await SmithAsync(null).ConfigureAwait(false);
        player.SelectedCharacter!.StarSmithGuidebook = "2:1";
        var recipes = StarSmithingConfiguration.DefaultRecipes();
        Assert.That(recipes.Where(r => r.HintKind == StarSmithHintKind.Riddle).All(r => r.Hint.Length is > 0 and <= 64 && !r.Hint.Contains(r.Name, StringComparison.OrdinalIgnoreCase)), Is.True);
        Assert.That(recipes.Where(r => r.Number > 100).All(r => r.HintKind == StarSmithHintKind.None), Is.True, "no hints for the looks");

        var entries = plugIn.GuidebookOf(player);

        Assert.That(IsFound(entries[0], 2, 1, "Twin Fangs"), Is.True);
        var sealedRows = entries.Skip(1).ToList();
        var others = recipes.Where(r => r.Number != 2).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(sealedRows.Select(e => e.Text), Is.EqualTo(others.Select(r => r.HintKind == StarSmithHintKind.Riddle ? r.Hint : string.Empty)));
            Assert.That(sealedRows.Select(e => e.HintKind), Is.EqualTo(others.Select(r => r.HintKind)));
            Assert.That(sealedRows.All(e => e is { Kind: StarSmithGuidebookEntryKind.Sealed, Recipe: 0, Made: 0, ShapeLength: 0, Geometry: null, Ingredients.Count: 0, BoostsSeen.Count: 0 }), Is.True);
            Assert.That(sealedRows.Any(e => recipes.Any(r => System.Text.RegularExpressions.Regex.IsMatch(e.Text, $@"\b{r.Name}\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))), Is.False, "no sealed row names a recipe");
        });
    }

    /// <summary>
    /// The smith's state goes out with the Guidebook while the hammer is in a hand; a removed constellation is left out.
    /// </summary>
    [Test]
    public async ValueTask TheStateSendsTheGuidebookAsync()
    {
        var (player, plugIn) = await SmithAsync(Sure(7, 50)).ConfigureAwait(false);
        player.SelectedCharacter!.StarSmithGuidebook = "7:3;99:1";
        await plugIn.SendStateAsync(player).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        view.Verify(v => v.SendGuidebookAsync(1, It.Is<IReadOnlyList<StarSmithGuidebookEntry>>(f => f.Count == 1 && IsFound(f[0], 7, 3, "Sure"))), Times.Once);
    }

    /// <summary>
    /// With the default recipes, one skill orb on a sword starts the Forge with the weapon recipes and every looks
    /// constellation; no ingredient at all is refused.
    /// </summary>
    [Test]
    public async ValueTask TheDefaultRecipesStartWithAnOrbAsync()
    {
        var (player, plugIn) = await SmithAsync(null).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        await AddAsync(player, 14, 12, 7).ConfigureAwait(false);

        await plugIn.StartForgeAsync(player, TargetSlot, [], []).ConfigureAwait(false);
        view.Verify(v => v.SendForgeAttemptAsync(It.Is<StarSmithForgeAttemptMessage>(m => m.Result == StarSmithResultCode.NoIngredients)), Times.Once);

        await plugIn.StartForgeAsync(player, TargetSlot, [14], []).ConfigureAwait(false);
        view.Verify(v => v.SendForgeAttemptAsync(It.Is<StarSmithForgeAttemptMessage>(m => m.Result == StarSmithResultCode.Done && m.Strikes == 4)), Times.Once);
        var names = player.StarSmithForge!.Forge.Constellations.Select(r => r.Number).Order().ToList();
        Assert.That(names, Is.EqualTo(new short[] { 1, 2, 3, 4, 5 }.Concat(Enumerable.Range(101, 22).Select(n => (short)n))));
    }

    private static bool IsFound(StarSmithGuidebookEntry entry, short recipe, int made, string name)
        => entry is { Kind: StarSmithGuidebookEntryKind.Found } && entry.Recipe == recipe && entry.Made == made && entry.Text == name;

    /// <summary>
    /// A four-node open shape: three placements are a prefix of it, four a match.
    /// </summary>
    private static StarSmithRecipe Sure(short number, int experience) => new()
    {
        Number = number,
        Name = "Sure",
        Experience = experience,
        Targets = StarSmithTargetKind.Weapon,
        Ingredients = [new StarSmithIngredient { Group = 14, Number = 13, Minimum = 0, Maximum = 10 }],
        Geometry = StarSmithGeometry.Of(4, false, [90, -90], [1, 1, 1]),
    };

    private static List<StarSmithPoint> Points(StarSmithRecipe recipe) => StarSmithGeometryTest.Draw(recipe.Geometry!, 0, 0.06, false, false, 0.02, 0.07);

    private static async ValueTask<Item> AddAsync(Player player, byte slot, byte group, short number, byte durability = 1)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = new MUnique.OpenMU.Persistence.BasicModel.ItemDefinition { Group = group, Number = number, Width = 1, Height = 1, Durability = durability };
        item.Durability = durability;
        await player.Inventory!.AddItemAsync(slot, item).ConfigureAwait(false);
        return item;
    }

    private async ValueTask SealAsync(Player player, StarSmithingFeaturePlugIn plugIn, bool late = false)
    {
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        foreach (var point in Points(Sure(7, 50)))
        {
            this._now += Interval;
            await plugIn.PlaceForgeNodeAsync(player, point).ConfigureAwait(false);
            if (late)
            {
                this._now += Interval / 2;
            }
        }

        await plugIn.SealForgeAsync(player).ConfigureAwait(false);
    }

    private async ValueTask<(Player Player, StarSmithingFeaturePlugIn PlugIn)> SmithAsync(StarSmithRecipe? recipe, int faults = 0)
    {
        this._now = 0;
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        await AddAsync(player, HammerSlot, 2, 1000, 255).ConfigureAwait(false);
        await AddAsync(player, TargetSlot, 0, 1).ConfigureAwait(false);
        await AddAsync(player, IngredientSlot, 14, 13).ConfigureAwait(false);
        var configuration = new StarSmithingConfiguration { Forge = new StarSmithForgeRules { FaultCount = faults, BaseStrikes = 3 } };
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
