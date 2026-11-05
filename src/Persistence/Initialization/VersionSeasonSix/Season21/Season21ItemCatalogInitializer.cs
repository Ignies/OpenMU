// <copyright file="Season21ItemCatalogInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

/// <summary>
/// Adds every item of the <see cref="Season21ItemCatalog"/> which a game configuration doesn't define yet.
/// Defined items, the Season 6 ones and the ones an operator changed, are kept as they are.
/// </summary>
/// <remarks>
/// <para>
/// A weapon, shield, armor or wing copies the defined item of its kind (its template), as the gear of Season 21 does
/// (<see cref="Season21GearInitializer"/>): the item of the same group and slot whose row in the catalog matches its
/// kind, two-handedness, sub kind and skill best, then the nearest drop level. Rings and pendants copy the Season 6
/// ring or pendant of their options. Everything else gets the values of its row only.
/// </para>
/// <para>
/// No new item joins the random drops of the monsters (<see cref="ItemDefinition.DropsFromMonsters"/>), which would
/// change the drops of every Season 6 map: they drop by the drop lists of the later maps (<see cref="Season21DropGroups"/>).
/// An item which the Season 6 item format can't carry (<see cref="Season21CatalogItem.WaitsFor"/>) is defined, but
/// inactive: it doesn't drop and can't be made, so no Season 6 client gets it.
/// </para>
/// </remarks>
internal class Season21ItemCatalogInitializer
{
    /// <summary>
    /// The number of classes of Season 6 at the start of the class ranks of a row.
    /// </summary>
    private const int SeasonSixClasses = 7;

    /// <summary>
    /// The highest class rank of Season 6: the master class.
    /// </summary>
    private const int MasterRank = 3;

    /// <summary>
    /// The highest gear slot of the data (wings).
    /// </summary>
    private const int WingSlot = 7;

    /// <summary>
    /// The pet slot of the data.
    /// </summary>
    private const int PetSlot = 8;

    /// <summary>
    /// The first kind of the data of rings and pendants.
    /// </summary>
    private const int FirstJewelryKind = 29;

    /// <summary>
    /// The last kind of the data of rings and pendants.
    /// </summary>
    private const int LastJewelryKind = 35;

    /// <summary>
    /// The kind of the data of the decoration rings.
    /// </summary>
    private const int DecorationRingKind = 61;

    /// <summary>
    /// The highest item level.
    /// </summary>
    private const byte MaximumGearLevel = 15;

    /// <summary>
    /// The option numbers of the data which a piece of jewelry can have as fixed bonus, see <see cref="Season21GearInitializer"/>.
    /// </summary>
    private static readonly HashSet<int> SupportedSpecials = [129, 97];

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    private readonly Season21GearInitializer _gear;

    private readonly Dictionary<(byte Group, short Number), Season21CatalogItem> _rows;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21ItemCatalogInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21ItemCatalogInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
        this._gear = new Season21GearInitializer(context, gameConfiguration);
        this._rows = Season21ItemCatalog.Items.ToDictionary(row => (row.Group, row.Number));
    }

    /// <summary>
    /// Adds the items which aren't defined yet, then the newly defined items to the drop lists of the later maps.
    /// </summary>
    public void Initialize()
    {
        var defined = this._gameConfiguration.Items.Select(item => (item.Group, item.Number)).ToHashSet();
        var templates = this._gameConfiguration.Items
            .Where(item => item.ItemSlot is not null && this._rows.ContainsKey((item.Group, item.Number)))
            .GroupBy(item => (item.Group, this._rows[(item.Group, item.Number)].Slot))
            .ToDictionary(g => g.Key, g => g.ToList());
        foreach (var row in Season21ItemCatalog.Items.Where(row => !defined.Contains((row.Group, row.Number))))
        {
            var item = this.Create(row, templates);
            item.IntroducedIn = row.Version;
            item.DropsFromMonsters = false;
            item.IsActive = row.WaitsFor is null;
            item.Value = row.Value;
        }

        new Season21DropGroups(this._context, this._gameConfiguration).Apply();
    }

    private static int[] SeasonSixRanks(Season21CatalogItem row)
        => row.Classes.Take(SeasonSixClasses).Select(rank => rank > MasterRank ? 0 : rank).ToArray();

    private static Season21ExcellentOptions ExcellentOf(Season21CatalogItem row) => !row.HasOption(4, 9) ? Season21ExcellentOptions.None : row.KindA switch
    {
        29 or 34 => Season21ExcellentOptions.Damage,
        30 => Season21ExcellentOptions.Wizardry,
        _ => Season21ExcellentOptions.Defense,
    };

    private static Season21OptionKinds OptionKindsOf(Season21CatalogItem row)
        => (row.HasOption(1, 1) ? Season21OptionKinds.Luck : Season21OptionKinds.None)
           | (row.HasOption(2, 3) ? Season21OptionKinds.Option : Season21OptionKinds.None)
           | (row.HasOption(4, 9) ? Season21OptionKinds.Excellent : Season21OptionKinds.None);

    private static ItemDefinition? TemplateOf(Season21CatalogItem row, List<ItemDefinition> candidates, Dictionary<(byte Group, short Number), Season21CatalogItem> rows)
        => candidates
            .Select(item => (Item: item, Row: rows[(item.Group, item.Number)]))
            .OrderByDescending(c => (c.Row.KindA == row.KindA ? 8 : 0) + (c.Row.TwoHand == row.TwoHand ? 4 : 0) + (c.Row.KindB == row.KindB ? 2 : 0) + (c.Row.Skill == row.Skill ? 1 : 0))
            .ThenBy(c => Math.Abs(c.Row.DropLevel - row.DropLevel))
            .ThenBy(c => c.Item.Number)
            .Select(c => c.Item)
            .FirstOrDefault();

    private static Season21GearItem GearRowOf(Season21CatalogItem row, LocalizedString name, Season21GearStats? stats)
        => new(
            row.Group,
            row.Number,
            name,
            row.Version,
            row.Slot,
            (byte)Math.Min(row.DropLevel, byte.MaxValue),
            (byte)Math.Min(row.Durability > 0 ? row.Durability : row.MagicDurability, byte.MaxValue),
            false,
            ExcellentOf(row),
            row.HasOption(2, 3),
            row.Bans,
            row.Options.Where(o => o[0] >= 10 && SupportedSpecials.Contains(o[1])).Select(o => (o[1], o[2])).ToArray(),
            [],
            stats);

    private ItemDefinition Create(Season21CatalogItem row, Dictionary<(byte Group, int Slot), List<ItemDefinition>> templates)
    {
        var name = Properties.Season21CatalogItemNames.ResourceManager.GetLocalizedString(row.Key);
        if (row.WaitsFor is null && row.Group <= 12 && row.Slot is >= 0 and <= WingSlot
            && templates.TryGetValue((row.Group, row.Slot), out var candidates)
            && TemplateOf(row, candidates, this._rows) is { } template)
        {
            var stats = new Season21GearStats(
                (template.Group, template.Number),
                row.Skill,
                row.Width,
                row.Height,
                row.DamageMin,
                row.DamageMax,
                row.Speed,
                row.MagicPower,
                row.Defense,
                row.DefenseRate,
                row.Requirements,
                SeasonSixRanks(row),
                row.Sockets,
                row.TakesOptions,
                OptionKindsOf(row));
            if (this._gear.Create(GearRowOf(row, name, stats)) is { } gear)
            {
                this.AddGuardianOption(gear, row);
                return gear;
            }
        }

        if (row.WaitsFor is null && row.Slot is 9 or 10 && row.KindA is (>= FirstJewelryKind and <= LastJewelryKind) or DecorationRingKind && this._gear.Create(GearRowOf(row, name, null)) is { } jewelry)
        {
            this.AddRequirements(jewelry, row);
            return jewelry;
        }

        return this.CreatePlain(row, name);
    }

    private ItemDefinition CreatePlain(Season21CatalogItem row, LocalizedString name)
    {
        var item = this._context.CreateNew<ItemDefinition>();
        item.SetGuid(row.Group, row.Number);
        item.Group = row.Group;
        item.Number = row.Number;
        item.Name = name;
        item.Width = row.Width;
        item.Height = row.Height;
        item.DropLevel = (byte)Math.Min(row.DropLevel, byte.MaxValue);
        item.Durability = (byte)Math.Min(Math.Max(Math.Max(row.Durability, row.MagicDurability), row.Stack), byte.MaxValue);
        item.MaximumItemLevel = row.Slot is >= 0 and <= WingSlot ? MaximumGearLevel : row.MaximumLevel;
        item.IsTradable = !row.Bans.HasFlag(Season21ItemBans.Trade);
        item.IsDroppable = !row.Bans.HasFlag(Season21ItemBans.Drop);
        item.IsStorable = !row.Bans.HasFlag(Season21ItemBans.Store);
        item.IsPersonalStoreSellable = !row.Bans.HasFlag(Season21ItemBans.Shop);
        item.IsSellableToNpc = !row.Bans.HasFlag(Season21ItemBans.Sell);
        item.IsRepairable = !row.Bans.HasFlag(Season21ItemBans.Repair);
        item.Skill = row.Skill > 0 ? this._gameConfiguration.Skills.FirstOrDefault(s => s.Number == row.Skill) : null;
        if (row.WaitsFor is null && row.Slot == PetSlot)
        {
            item.ItemSlot = this._gameConfiguration.ItemSlotTypes.FirstOrDefault(slot => slot.ItemSlots.Contains(PetSlot));
        }

        this.AddClasses(item, row);
        this.AddRequirements(item, row);
        this._gameConfiguration.Items.Add(item);
        return item;
    }

    private void AddClasses(ItemDefinition item, Season21CatalogItem row)
    {
        foreach (var characterClass in this._gameConfiguration.DetermineCharacterClasses(Season21GearInitializer.ToClasses(SeasonSixRanks(row)), true))
        {
            item.QualifiedCharacters.Add(characterClass);
        }
    }

    private void AddRequirements(ItemDefinition item, Season21CatalogItem row)
    {
        AttributeDefinition[] requirements = [Stats.Level, Stats.TotalStrengthRequirementValue, Stats.TotalAgilityRequirementValue, Stats.TotalEnergyRequirementValue, Stats.TotalVitalityRequirementValue, Stats.TotalLeadershipRequirementValue];
        for (var i = 0; i < requirements.Length; i++)
        {
            if (row.Requirements[i] > 0 && item.Requirements.All(r => r.Attribute?.Id != requirements[i].Id))
            {
                var requirement = this._context.CreateNew<AttributeRequirement>();
                requirement.Attribute = requirements[i].GetPersistent(this._gameConfiguration);
                requirement.MinimumValue = row.Requirements[i];
                item.Requirements.Add(requirement);
            }
        }
    }

    private void AddGuardianOption(ItemDefinition item, Season21CatalogItem row)
    {
        var name = row.Group switch
        {
            <= 5 => ItemOptionNames.GuardianOptionWeapon,
            7 => ItemOptionNames.GuardianOptionHelm,
            8 => ItemOptionNames.GuardianOptionArmor,
            9 => ItemOptionNames.GuardianOptionPants,
            10 => ItemOptionNames.GuardianOptionGloves,
            11 => ItemOptionNames.GuardianOptionBoots,
            _ => null,
        };
        if (row.Guardian && name is not null
            && this._gameConfiguration.ItemOptions.FirstOrDefault(o => o.Name.ValueInNeutralLanguage == name) is { } option
            && !item.PossibleItemOptions.Contains(option))
        {
            item.PossibleItemOptions.Add(option);
        }
    }
}
