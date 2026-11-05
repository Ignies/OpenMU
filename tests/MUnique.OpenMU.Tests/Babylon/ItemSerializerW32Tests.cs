// <copyright file="ItemSerializerW32Tests.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the byte layout of the <see cref="ItemSerializerW32"/> against the Season 6 <see cref="ItemSerializer"/>:
/// every field Season 6 knows keeps its meaning (docs/babylon.md, "W32").
/// </summary>
[TestFixture]
public class ItemSerializerW32Tests
{
    private readonly ItemSerializer _season6 = new();
    private readonly ItemSerializerW32 _w32 = new();
    private GameConfiguration _gameConfiguration = null!;
    private IPersistenceContextProvider _contextProvider = null!;

    /// <summary>
    /// Sets up the Season 6 configuration.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._contextProvider = new InMemoryPersistenceContextProvider();
        await new DataInitialization(this._contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(3, true).ConfigureAwait(false);
        this._gameConfiguration = (await this._contextProvider.CreateNewConfigurationContext().GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
    }

    /// <summary>
    /// Tests that an excellent weapon with skill, luck and an option carries the Season 6 fields in their W32 places.
    /// </summary>
    [Test]
    public void ExcellentWeaponKeepsTheSeason6Fields()
    {
        using var context = this._contextProvider.CreateNewContext(this._gameConfiguration);
        var item = this.CreateItem(context, i => i.Name.ValueInNeutralLanguage == "Blade", level: 13, durability: 40);
        item.HasSkill = true;
        AddOption(context, item, ItemOptionTypes.Option, level: 3);
        AddOption(context, item, ItemOptionTypes.Luck);
        AddOption(context, item, ItemOptionTypes.Excellent, p => p.PowerUpDefinition!.TargetAttribute == Stats.ExcellentDamageChance);
        AddOption(context, item, ItemOptionTypes.Excellent, p => p.PowerUpDefinition!.TargetAttribute == Stats.AttackSpeedAny);

        var (season6, w32) = this.Serialize(item);

        AssertSeason6FieldsMatch(season6, w32);
        Assert.That(w32[5], Is.EqualTo(0b0000_0111), "flags: skill, luck, excellent");
    }

    /// <summary>
    /// Tests that an ancient item keeps its ancient byte.
    /// </summary>
    [Test]
    public void AncientKeepsItsByte()
    {
        using var context = this._contextProvider.CreateNewContext(this._gameConfiguration);
        var item = this.CreateItem(context, i => i.Name.ValueInNeutralLanguage == "Lighting Sword", level: 15, durability: 100);
        var itemOfSet = this._gameConfiguration.ItemSetGroups.First(i => i.Name.ValueInNeutralLanguage == "Hyon").Items.First(i => i.ItemDefinition == item.Definition);
        item.ItemSetGroups.Add(itemOfSet);
        var bonus = context.CreateNew<ItemOptionLink>();
        bonus.ItemOption = itemOfSet.BonusOption;
        bonus.Level = 2;
        item.ItemOptions.Add(bonus);

        var (season6, w32) = this.Serialize(item);

        AssertSeason6FieldsMatch(season6, w32);
        Assert.That(w32[10], Is.Not.Zero);
        Assert.That(w32[5] & 0x08, Is.EqualTo(0x08), "ancient flag");
    }

    /// <summary>
    /// Tests that the sockets keep their options, with the sphere level in its own nibble.
    /// </summary>
    [Test]
    public void SocketsCarryTheLevelInANibble()
    {
        using var context = this._contextProvider.CreateNewContext(this._gameConfiguration);
        var item = this.CreateItem(context, i => i.Group == 7 && i.Number == 46, level: 3, durability: 100);
        item.SocketCount = 3;
        var options = item.Definition!.PossibleItemOptions.SelectMany(o => o.PossibleOptions).Where(o => o.OptionType == ItemOptionTypes.SocketOption).ToList();
        AddSocket(context, item, options[0], slot: 0, level: 2);
        AddSocket(context, item, options[7], slot: 1, level: 5);

        var (season6, w32) = this.Serialize(item);

        AssertSeason6FieldsMatch(season6, w32);
        Assert.That(w32[13..18], Is.EqualTo(new byte[] { (byte)(season6[7] % 50), (byte)(season6[8] % 50), 0xFE, 0xFF, 0xFF }));
        Assert.That(w32[18], Is.EqualTo(0x52), "slot 0 level 2 (low nibble), slot 1 level 5 (high nibble)");
        Assert.That(w32[5] & 0x40, Is.EqualTo(0x40), "sockets flag");
    }

    /// <summary>
    /// Tests that a sphere level above the Season 6 maximum is carried as it is.
    /// </summary>
    [Test]
    public void SphereLevelAboveSeason6IsCarried()
    {
        using var context = this._contextProvider.CreateNewContext(this._gameConfiguration);
        var item = this.CreateItem(context, i => i.Group == 7 && i.Number == 46, level: 0, durability: 100);
        item.SocketCount = 1;
        var option = item.Definition!.PossibleItemOptions.SelectMany(o => o.PossibleOptions).First(o => o.OptionType == ItemOptionTypes.SocketOption);
        AddSocket(context, item, option, slot: 0, level: 9);

        var (_, w32) = this.Serialize(item);
        var deserialized = this._w32.DeserializeItem(w32, this._gameConfiguration, context);

        Assert.That(w32[18] & 0x0F, Is.EqualTo(9));
        Assert.That(deserialized.ItemOptions.Single(o => o.ItemOption!.OptionType == ItemOptionTypes.SocketOption).Level, Is.EqualTo(9));
    }

    /// <summary>
    /// Tests that groups above 15 and numbers above 511, which Season 6 cannot carry, fit.
    /// </summary>
    [Test]
    public void WideGroupAndNumber()
    {
        using var context = this._contextProvider.CreateNewContext(this._gameConfiguration);
        var definition = context.CreateNew<ItemDefinition>();
        definition.Group = 20;
        definition.Number = 766;
        this._gameConfiguration.Items.Add(definition);
        try
        {
            var item = context.CreateNew<Item>();
            item.Definition = definition;
            var target = new byte[ItemSerializerW32.Length];
            this._w32.SerializeItem(target, item);

            Assert.That(target[0], Is.EqualTo(20));
            Assert.That(target[1] | (target[2] << 8), Is.EqualTo(766));
            Assert.That(this._w32.DeserializeItem(target, this._gameConfiguration, context).Definition, Is.SameAs(definition));
        }
        finally
        {
            this._gameConfiguration.Items.Remove(definition);
        }
    }

    /// <summary>
    /// Tests that the fields of later seasons stay zero until their systems exist.
    /// </summary>
    [Test]
    public void LaterSeasonFieldsAreZero()
    {
        using var context = this._contextProvider.CreateNewContext(this._gameConfiguration);
        var item = this.CreateItem(context, i => i.Name.ValueInNeutralLanguage == "Blade", level: 9, durability: 30);
        AddOption(context, item, ItemOptionTypes.Excellent, p => p.PowerUpDefinition!.TargetAttribute == Stats.ExcellentDamageChance);

        var (_, w32) = this.Serialize(item);

        Assert.That(w32[8], Is.Zero, "mastery bits");
        Assert.That(w32[9], Is.Zero, "mastery bonus");
        Assert.That(w32[21..32], Is.All.Zero, "element, kind block and expiry");
    }

    private static void AssertSeason6FieldsMatch(byte[] s6, byte[] w32)
    {
        Assert.Multiple(() =>
        {
            Assert.That(w32[0], Is.EqualTo(s6[5] >> 4), "group");
            Assert.That(w32[1] | (w32[2] << 8), Is.EqualTo(s6[0] | ((s6[3] & 0x80) << 1)), "number");
            Assert.That(w32[3], Is.EqualTo((s6[1] >> 3) & 0x0F), "level");
            Assert.That(w32[4], Is.EqualTo(s6[2]), "durability");
            Assert.That((w32[5] & 0x01) != 0, Is.EqualTo((s6[1] & 0x80) != 0), "skill");
            Assert.That((w32[5] & 0x02) != 0, Is.EqualTo((s6[1] & 0x04) != 0), "luck");
            Assert.That((w32[5] & 0x20) != 0, Is.EqualTo((s6[5] & 0x08) != 0), "380 option");
            Assert.That(w32[6] & 0x0F, Is.EqualTo((s6[1] & 0x03) | ((s6[3] >> 4) & 0x04)), "option level");
            Assert.That(w32[7], Is.EqualTo(s6[3] & 0x3F), "excellent bits");
            Assert.That(w32[10], Is.EqualTo(s6[4]), "ancient");
            Assert.That(w32[11] | w32[12], Is.EqualTo(s6[6]), "harmony or socket bonus");
            for (var slot = 0; slot < 5; slot++)
            {
                var levels = w32[18 + (slot / 2)];
                var level = slot % 2 == 0 ? levels & 0x0F : levels >> 4;
                var expected = w32[13 + slot] >= 0xFE ? w32[13 + slot] : ((level - 1) * 50) + w32[13 + slot];
                Assert.That(expected, Is.EqualTo(s6[7 + slot]), $"socket {slot}");
            }
        });
    }

    private static void AddOption(IContext context, Item item, ItemOptionType type, Func<IncreasableItemOption, bool>? predicate = null, int level = 0)
    {
        var link = context.CreateNew<ItemOptionLink>();
        link.ItemOption = item.Definition!.PossibleItemOptions.SelectMany(o => o.PossibleOptions).First(p => p.OptionType == type && (predicate?.Invoke(p) ?? true));
        link.Level = level;
        item.ItemOptions.Add(link);
    }

    private static void AddSocket(IContext context, Item item, IncreasableItemOption option, int slot, int level)
    {
        var link = context.CreateNew<ItemOptionLink>();
        link.ItemOption = option;
        link.Index = slot;
        link.Level = level;
        item.ItemOptions.Add(link);
    }

    private Item CreateItem(IContext context, Func<ItemDefinition, bool> definition, byte level, byte durability)
    {
        var item = context.CreateNew<Item>();
        item.Definition = this._gameConfiguration.Items.First(definition);
        item.Level = level;
        item.Durability = durability;
        return item;
    }

    private (byte[] Season6, byte[] W32) Serialize(Item item)
    {
        var season6 = new byte[this._season6.NeededSpace];
        var w32 = new byte[ItemSerializerW32.Length];
        this._season6.SerializeItem(season6, item);
        this._w32.SerializeItem(w32, item);
        return (season6, w32);
    }
}
