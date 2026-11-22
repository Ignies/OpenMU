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
        view.Verify(v => v.SendGuidebookAsync(1, It.Is<IReadOnlyList<StarSmithGuidebookEntry>>(f => f.Count == 1 && f[0] == new StarSmithGuidebookEntry(7, 1, "Sure"))), Times.AtLeastOnce);
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
    /// C2 EE 90: the recipe count, the found count, then per found recipe its number, made count and name (32 bytes,
    /// zero padded). Nothing names a recipe not found.
    /// </summary>
    [Test]
    public async ValueTask TheGuidebookBytesAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        var type = Type.GetType(PlugIn)!;
        await ((ValueTask)type.GetMethod("SendGuidebookAsync")!.Invoke(Activator.CreateInstance(type, player), [5, (IReadOnlyList<StarSmithGuidebookEntry>)[new StarSmithGuidebookEntry(1, 4, "The Serpent")]])!).ConfigureAwait(false);

        var expected = new byte[43];
        new byte[] { 0xC2, 0, 43, 0xEE, 0x90, 5, 1, 1, 0, 4, 0 }.CopyTo(expected, 0);
        System.Text.Encoding.UTF8.GetBytes("The Serpent").CopyTo(expected, 11);
        Assert.That(output.ToArray(), Is.EqualTo(expected));
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
        view.Verify(v => v.SendGuidebookAsync(1, It.Is<IReadOnlyList<StarSmithGuidebookEntry>>(f => f.Count == 1 && f[0] == new StarSmithGuidebookEntry(7, 3, "Sure"))), Times.Once, "a removed recipe (99) is not shown");
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
