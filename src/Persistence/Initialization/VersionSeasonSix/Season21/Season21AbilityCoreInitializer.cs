// <copyright file="Season21AbilityCoreInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

/// <summary>
/// Adds the Ability Enhancement Core of Season 21 (<see cref="Season21AbilityCoreData"/>): the options of the ability
/// cards as item options whose value follows the card level, and the configuration of the
/// <see cref="AbilityCoreFeaturePlugIn"/>. Every step keeps what the configuration already has. See Readme.md,
/// "Ability Enhancement Core".
/// </summary>
internal class Season21AbilityCoreInitializer
{
    /// <summary>
    /// The number of the first item option definition of the cards; each card adds its item number.
    /// </summary>
    internal const short CardOptionsNumber = 0x1900;

    private const float Percent = 0.01f;

    private const int ChanceToWeight = 100_000;

    private const byte CardGroup = 20;

    private const short AbilityCrystalNumber = 164;

    private const short BoundAbilityCrystalNumber = 238;

    private const short UpgradeStoneNumber = 239;

    private const byte JewelGroup = 14;

    private const short JewelOfBlessNumber = 13;

    private const byte UpgradeStonesPerCrafting = 5;

    private static readonly (byte Crystals, byte Blesses, int Money) SkillCardPrice = (50, 10, 100_000_000);

    private static readonly (byte Crystals, byte Blesses, int Money) OptionCardPrice = (5, 1, 10_000_000);

    private static readonly (byte Crystals, byte Blesses, int Money) EliteCardPrice = (25, 5, 50_000_000);

    /// <summary>
    /// The attribute of each option which has one in OpenMU, by the static option index; the other options (the
    /// nuke, bolt and wide skill options, the fifth wings, mastery, bleeding and poison, triple damage, skill slots, AG
    /// recovery, shock defense) are data without effect. The extra defense adds to the defense, the pentagram defense
    /// and attack to the elemental defense and damage.
    /// </summary>
    private static readonly Dictionary<short, (AttributeDefinition Attribute, bool PerTwentyLevels)> Effects = new()
    {
        { 0, (Stats.DefenseBase, false) },
        { 1, (Stats.DefenseShield, false) },
        { 3, (Stats.DefenseBase, false) },
        { 4, (Stats.ElementDefense, false) },
        { 5, (Stats.ElementDefenseAgainstFire, false) },
        { 6, (Stats.ElementDefenseAgainstWater, false) },
        { 7, (Stats.ElementDefenseAgainstEarth, false) },
        { 8, (Stats.ElementDefenseAgainstWind, false) },
        { 9, (Stats.ElementDefenseAgainstDarkness, false) },
        { 10, (Stats.ElementDefense, false) },
        { 11, (Stats.AttackRatePvm, false) },
        { 12, (Stats.DefenseRatePvm, false) },
        { 13, (Stats.ElementAttackRate, false) },
        { 14, (Stats.ElementDefenseRate, false) },
        { 15, (Stats.TotalStrength, false) },
        { 16, (Stats.TotalAgility, false) },
        { 17, (Stats.TotalVitality, false) },
        { 18, (Stats.TotalEnergy, false) },
        { 19, (Stats.MinimumPhysBaseDmg, false) },
        { 20, (Stats.MinimumWizBaseDmg, false) },
        { 21, (Stats.MinimumCurseBaseDmg, false) },
        { 22, (Stats.MaximumPhysBaseDmg, false) },
        { 23, (Stats.MaximumWizBaseDmg, false) },
        { 24, (Stats.MaximumCurseBaseDmg, false) },
        { 28, (Stats.PhysicalBaseDmg, true) },
        { 33, (Stats.CriticalDamageBonus, false) },
        { 34, (Stats.ExcellentDamageBonus, false) },
        { 41, (Stats.SkillDamageBonus, false) },
        { 42, (Stats.ElementDamageAgainstFire, false) },
        { 43, (Stats.ElementDamageAgainstWater, false) },
        { 44, (Stats.ElementDamageAgainstEarth, false) },
        { 45, (Stats.ElementDamageAgainstWind, false) },
        { 46, (Stats.ElementDamageAgainstDarkness, false) },
        { 47, (Stats.ElementDamageBonus, false) },
        { 48, (Stats.ElementDamageBonus, false) },
        { 49, (Stats.CriticalDamageChance, false) },
        { 50, (Stats.CriticalDamageChance, false) },
        { 52, (Stats.FullyRecoverHealthAfterHitChance, false) },
        { 53, (Stats.DoubleDamageChance, false) },
        { 54, (Stats.ExcellentDamageChance, false) },
        { 55, (Stats.ExcellentDamageChance, false) },
        { 57, (Stats.FullyRecoverManaAfterHitChance, false) },
        { 58, (Stats.DefenseIgnoreChance, false) },
        { 59, (Stats.DoubleDamageChance, false) },
        { 65, (Stats.MaximumHealth, false) },
        { 66, (Stats.MaximumMana, false) },
        { 67, (Stats.MaximumAbility, false) },
        { 85, (Stats.WizardryBaseDmg, true) },
        { 86, (Stats.CurseBaseDmg, true) },
    };

    private readonly IContext _context;
    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21AbilityCoreInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21AbilityCoreInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Creates the configuration of the core from the data.
    /// </summary>
    /// <returns>The configuration.</returns>
    public static AbilityCoreConfiguration CreateConfiguration()
    {
        var data = Season21AbilityCoreData.Data;
        var configuration = new AbilityCoreConfiguration();
        foreach (var row in data.Cards)
        {
            var card = new AbilityCardDefinition
            {
                ItemGroup = row.Group,
                ItemNumber = row.Number,
                Kind = (AbilityCardKind)row.Kind,
                Color = (AbilityCardColor)row.Color,
                Type = row.Type,
                BaseClasses = row.Classes ?? 0xFFFF,
                Skill = row.Skill,
                RequiredSkill = row.RequiredSkill,
                StaticOption = row.Static?.Id ?? 0,
                StaticValues = row.Static?.Values.ToList() ?? [],
            };
            card.Options.AddRange(row.Options.Select(o => new AbilityCardOption { Number = o.Number, BaseClasses = o.Classes, Chance = o.Chance }));
            configuration.Cards.Add(card);
        }

        configuration.LevelUps.AddRange(data.LevelUps.Select(r => new AbilityCardLevelUp
        {
            Kind = (AbilityCardKind)r.Kind,
            Level = r.Level,
            Cards = r.Cards,
            Money = r.Money,
            StoneGroup = r.StoneGroup,
            StoneNumber = r.StoneNumber,
            Stones = r.Stones,
        }));
        configuration.Reductions.AddRange(data.Reductions.Select(r => new AbilityCardReduction
        {
            Kind = (AbilityCardKind)r.Kind,
            Money = r.Money,
            StoneGroup = r.StoneGroup,
            StoneNumber = r.StoneNumber,
            Stones = r.Stones,
        }));
        return configuration;
    }

    /// <summary>
    /// Adds the options of the cards to the card items which the configuration defines.
    /// </summary>
    public void Initialize()
    {
        var optionType = this.GetOptionType();
        foreach (var row in Season21AbilityCoreData.Data.Cards)
        {
            if (this._gameConfiguration.Items.FirstOrDefault(i => i.Group == row.Group && i.Number == row.Number) is not { } item)
            {
                continue;
            }

            var definition = this.GetOrCreateDefinition(row, item, optionType);
            if (!item.PossibleItemOptions.Contains(definition))
            {
                item.PossibleItemOptions.Add(definition);
            }
        }

        this.AddCraftings();
    }

    /// <summary>
    /// Adds the card craftings to the chaos goblin (Webzen guide 343: 100 %, the prices below; the S21Emu ChaosMix.dat
    /// Ability* rates are 100). A crafting which exists stays as the operator left it.
    /// </summary>
    private void AddCraftings()
    {
        if (this._gameConfiguration.Monsters.FirstOrDefault(m => m.NpcWindow == NpcWindow.ChaosMachine) is not { } goblin)
        {
            return;
        }

        var cards = Season21AbilityCoreData.Data.Cards;
        this.AddCrafting(goblin, AbilityCardCrafting.SkillCardNumber, "Skill Ability Card", SkillCardPrice, cards.Where(c => c.Kind == (byte)AbilityCardKind.Skill).Select(c => c.Number), 1);
        for (byte type = 1; type <= 5; type++)
        {
            var optionType = type;
            this.AddCrafting(goblin, (byte)(AbilityCardCrafting.FirstOptionCardNumber + type - 1), $"Option Ability Card Type {type}", OptionCardPrice, cards.Where(c => c.Kind == (byte)AbilityCardKind.Option && c.Type == optionType).Select(c => c.Number), 1);
        }

        foreach (var color in new[] { AbilityCardColor.Blue, AbilityCardColor.Green, AbilityCardColor.Purple })
        {
            this.AddCrafting(goblin, (byte)(AbilityCardCrafting.FirstEliteCardNumber + (byte)color - 1), $"{color} Elite Ability Card", EliteCardPrice, cards.Where(c => c.Kind == (byte)AbilityCardKind.Elite && c.Color == (byte)color).Select(c => c.Number), 1);
        }

        this.AddCrafting(goblin, AbilityCardCrafting.UpgradeStonesNumber, "Ability Upgrade Stones", EliteCardPrice, [UpgradeStoneNumber], UpgradeStonesPerCrafting);
    }

    private void AddCrafting(MonsterDefinition goblin, byte number, string name, (byte Crystals, byte Blesses, int Money) price, IEnumerable<short> results, byte durability)
    {
        var crystals = this.FindItems(CardGroup, [AbilityCrystalNumber, BoundAbilityCrystalNumber]);
        var bless = this.FindItems(JewelGroup, [JewelOfBlessNumber]);
        var resultItems = this.FindItems(CardGroup, results);
        if (goblin.ItemCraftings.Any(c => c.Number == number) || crystals.Count == 0 || bless.Count == 0 || resultItems.Count == 0)
        {
            return;
        }

        var crafting = this._context.CreateNew<ItemCrafting>();
        crafting.Name = name;
        crafting.Number = number;
        crafting.ItemCraftingHandlerClassName = typeof(AbilityCardCrafting).FullName!;
        var settings = this._context.CreateNew<SimpleCraftingSettings>();
        settings.Money = price.Money;
        settings.SuccessPercent = 100;
        settings.ResultItemSelect = ResultItemSelection.Any;
        settings.RequiredItems.Add(this.CreateRequiredItem(crystals, price.Crystals));
        settings.RequiredItems.Add(this.CreateRequiredItem(bless, price.Blesses));
        foreach (var item in resultItems)
        {
            var resultItem = this._context.CreateNew<ItemCraftingResultItem>();
            resultItem.ItemDefinition = item;
            resultItem.Durability = durability;
            settings.ResultItems.Add(resultItem);
        }

        crafting.SimpleCraftingSettings = settings;
        goblin.ItemCraftings.Add(crafting);
    }

    private ItemCraftingRequiredItem CreateRequiredItem(IEnumerable<ItemDefinition> items, byte amount)
    {
        var requiredItem = this._context.CreateNew<ItemCraftingRequiredItem>();
        requiredItem.MinimumAmount = amount;
        requiredItem.MaximumAmount = amount;
        requiredItem.MaximumItemLevel = byte.MaxValue;
        foreach (var item in items)
        {
            requiredItem.PossibleItems.Add(item);
        }

        return requiredItem;
    }

    private List<ItemDefinition> FindItems(byte group, IEnumerable<short> numbers)
    {
        var wanted = numbers.ToHashSet();
        return this._gameConfiguration.Items.Where(i => i.Group == group && wanted.Contains(i.Number)).ToList();
    }

    private ItemOptionDefinition GetOrCreateDefinition(Season21AbilityCoreData.CardRow row, ItemDefinition item, ItemOptionType optionType)
    {
        var number = (short)(CardOptionsNumber + row.Number);
        var id = GuidHelper.CreateGuid<ItemOptionDefinition>(number);
        if (this._gameConfiguration.ItemOptions.FirstOrDefault(o => o.GetId() == id) is { } existing)
        {
            foreach (var optionRow in row.Options)
            {
                if (existing.PossibleOptions.FirstOrDefault(o => o.Number == optionRow.Number && o.OptionType == optionType) is { PowerUpDefinition: null, LevelDependentOptions.Count: 0 } option)
                {
                    this.AddEffect(option, optionRow);
                }
            }

            return existing;
        }

        var definition = this._context.CreateNew<ItemOptionDefinition>();
        definition.SetGuid(number);
        definition.Name = $"Ability Card Options ({item.Name.ValueInNeutralLanguage})";
        definition.AddsRandomly = true;
        definition.AddChance = 1;
        definition.MaximumOptionsPerItem = 1;
        foreach (var optionRow in row.Options)
        {
            definition.PossibleOptions.Add(this.CreateOption(number, optionRow, optionType));
        }

        this._gameConfiguration.ItemOptions.Add(definition);
        return definition;
    }

    private IncreasableItemOption CreateOption(short parentNumber, Season21AbilityCoreData.OptionRow row, ItemOptionType optionType)
    {
        var option = this._context.CreateNew<IncreasableItemOption>();
        option.SetGuid(parentNumber, row.Number);
        option.OptionType = optionType;
        option.Number = row.Number;
        option.LevelType = LevelType.ItemLevel;
        option.Weight = row.Chance > 0 ? (byte)Math.Clamp(row.Chance / ChanceToWeight, 1, byte.MaxValue) : (byte)1;
        this.AddEffect(option, row);
        return option;
    }

    private void AddEffect(IncreasableItemOption option, Season21AbilityCoreData.OptionRow row)
    {
        if (!Effects.TryGetValue(row.Number, out var effect))
        {
            return;
        }

        var values = row.Values.Select(v => row.IsRate ? v * Percent : v).ToArray();
        for (var level = 0; level < values.Length; level++)
        {
            var optionOfLevel = this._context.CreateNew<ItemOptionOfLevel>();
            optionOfLevel.Level = level;
            optionOfLevel.PowerUpDefinition = this.CreatePowerUp(effect.Attribute, values[level], effect.PerTwentyLevels);
            option.LevelDependentOptions.Add(optionOfLevel);
        }

        option.PowerUpDefinition = this.CreatePowerUp(effect.Attribute, values.FirstOrDefault(), effect.PerTwentyLevels);
    }

    private PowerUpDefinition CreatePowerUp(AttributeDefinition attribute, float value, bool perTwentyLevels)
    {
        var powerUp = this._context.CreateNew<PowerUpDefinition>();
        powerUp.TargetAttribute = this.GetPersistent(attribute);
        powerUp.Boost = this._context.CreateNew<PowerUpDefinitionValue>();
        if (perTwentyLevels)
        {
            var relationship = this._context.CreateNew<AttributeRelationship>();
            relationship.TargetAttribute = powerUp.TargetAttribute;
            relationship.InputAttribute = this.GetPersistent(Stats.TotalLevel);
            relationship.InputOperator = InputOperator.Multiply;
            relationship.InputOperand = value / 20;
            powerUp.Boost.RelatedValues.Add(relationship);
        }
        else
        {
            powerUp.Boost.ConstantValue.Value = value;
            powerUp.Boost.ConstantValue.AggregateType = AggregateType.AddRaw;
        }

        return powerUp;
    }

    private AttributeDefinition GetPersistent(AttributeDefinition attribute)
    {
        if (this._gameConfiguration.Attributes.All(a => a.Id != attribute.Id))
        {
            this._gameConfiguration.Attributes.Add(this._context.CreateNew<AttributeDefinition>(attribute.Id, attribute.Designation, attribute.Description));
        }

        return attribute.GetPersistent(this._gameConfiguration);
    }

    private ItemOptionType GetOptionType()
    {
        var type = Season21ItemOptionTypes.AbilityCardOption;
        if (this._gameConfiguration.ItemOptionTypes.FirstOrDefault(t => t == type) is { } existing)
        {
            return existing;
        }

        var persistent = this._context.CreateNew<ItemOptionType>();
        persistent.Id = type.Id;
        persistent.Name = type.Name;
        persistent.Description = type.Description;
        persistent.IsVisible = type.IsVisible;
        this._gameConfiguration.ItemOptionTypes.Add(persistent);
        return persistent;
    }
}
