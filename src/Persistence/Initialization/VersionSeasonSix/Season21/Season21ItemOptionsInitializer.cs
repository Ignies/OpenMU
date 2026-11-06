// <copyright file="Season21ItemOptionsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

/// <summary>
/// Adds the item options of Season 21 which only the wide item format carries, and activates the items which wait
/// for it. Every step keeps what the configuration already has, so each update can run again. See Readme.md.
/// </summary>
internal class Season21ItemOptionsInitializer
{
    /// <summary>
    /// The crafting number of the first seed sphere enhancement (level 5 to 6); the next levels follow.
    /// </summary>
    internal const byte FirstSphereEnhanceCraftingNumber = 100;

    private const byte SocketItemGroup = 12;

    private const short FirstSeedSphereNumber = 100;

    private const short FirstWideSeedSphereNumber = 311;

    private const short SphereUpgradeRuneNumber = 401;

    private const int SeedTypes = 6;

    private const int Season6SphereLevels = 5;

    private const int MaximumSphereLevel = 10;

    private const int MountSeedSphereCraftingNumber = 44;

    private const int FourthWingKind = 76;

    private const int FifthWingKind = 106;

    private const float Percent = 0.01f;

    /// <summary>
    /// The values of the socket options of the Season 21 data at the sphere levels 5 to 10 (SocketItemOption.xml,
    /// Value5 to Value10), by element (<see cref="SocketSubOptionType"/>) and option number. A level above 5 scales
    /// the Season 6 value of level 5 by the ratio of these values, so the Season 6 units stay.
    /// </summary>
    private static readonly Dictionary<(SocketSubOptionType Element, int Number), int[]> SphereLevelValues = new()
    {
        [(SocketSubOptionType.Fire, 0)] = [15, 16, 17, 18, 19, 20],
        [(SocketSubOptionType.Fire, 1)] = [11, 12, 13, 14, 15, 16],
        [(SocketSubOptionType.Fire, 2)] = [45, 50, 55, 61, 67, 73],
        [(SocketSubOptionType.Fire, 3)] = [35, 40, 45, 51, 57, 63],
        [(SocketSubOptionType.Fire, 4)] = [35, 40, 45, 51, 57, 63],
        [(SocketSubOptionType.Fire, 5)] = [44, 45, 46, 47, 48, 49],
        [(SocketSubOptionType.Water, 0)] = [14, 15, 16, 17, 18, 19],
        [(SocketSubOptionType.Water, 1)] = [41, 44, 47, 51, 55, 60],
        [(SocketSubOptionType.Water, 2)] = [350, 400, 450, 500, 550, 600],
        [(SocketSubOptionType.Water, 3)] = [8, 9, 10, 11, 12, 13],
        [(SocketSubOptionType.Water, 4)] = [9, 10, 11, 12, 13, 14],
        [(SocketSubOptionType.Ice, 0)] = [372, 404, 436, 471, 506, 541],
        [(SocketSubOptionType.Ice, 1)] = [601, 652, 703, 755, 807, 859],
        [(SocketSubOptionType.Ice, 2)] = [57, 64, 71, 80, 89, 100],
        [(SocketSubOptionType.Ice, 3)] = [42, 49, 56, 65, 74, 83],
        [(SocketSubOptionType.Ice, 4)] = [40, 44, 49, 54, 60, 66],
        [(SocketSubOptionType.Wind, 0)] = [21, 26, 31, 38, 45, 52],
        [(SocketSubOptionType.Wind, 1)] = [165, 185, 205, 235, 265, 295],
        [(SocketSubOptionType.Wind, 2)] = [215, 235, 255, 285, 315, 345],
        [(SocketSubOptionType.Wind, 3)] = [37, 46, 55, 66, 77, 88],
        [(SocketSubOptionType.Wind, 4)] = [126, 143, 162, 182, 203, 226],
        [(SocketSubOptionType.Wind, 5)] = [13, 15, 17, 19, 21, 23],
        [(SocketSubOptionType.Lightning, 0)] = [36, 43, 51, 60, 70, 81],
        [(SocketSubOptionType.Lightning, 1)] = [14, 15, 16, 17, 18, 19],
        [(SocketSubOptionType.Lightning, 2)] = [47, 56, 66, 77, 89, 102],
        [(SocketSubOptionType.Lightning, 3)] = [12, 13, 14, 15, 16, 17],

        // The health option of earth is the third earth option of the data.
        [(SocketSubOptionType.Earth, 0)] = [38, 41, 44, 48, 52, 57],
    };

    /// <summary>
    /// The money and the runes of the seed sphere enhancement from level 5 to 6, ..., 9 to 10
    /// (SocketSystemCombinationMix.xml, SeedSphereEnhance, levels 5 to 9).
    /// </summary>
    private static readonly (int Money, byte Runes)[] SphereEnhancements =
    [
        (25_000_000, 7),
        (28_000_000, 8),
        (31_000_000, 9),
        (34_000_000, 10),
        (37_000_000, 11),
    ];

    /// <summary>
    /// The grade options of the fourth and fifth wings (ItemGradeOption.xml): the attribute, whether the value is a
    /// percentage, the values of the fourth wings by grade and the value of the fifth wings.
    /// </summary>
    private static readonly (AttributeDefinition Attribute, bool IsPercentage, float[] FourthWing, float FifthWing)[] WingGrades =
    [
        (Stats.FullyRecoverManaAfterHitChance, true, [3, 5, 6, 7], 7),
        (Stats.FullyRecoverHealthAfterHitChance, true, [3, 5, 6, 7], 7),
        (Stats.FullyReflectDamageAfterHitChance, true, [3, 5, 6, 7], 7),
        (Stats.DefenseIgnoreChance, true, [3, 5, 6, 7], 7),
        (Stats.AttackSpeedAny, false, [5, 7, 9, 12], 12),
        (Stats.ExcellentDamageChance, true, [3, 5, 7, 10], 7),
        (Stats.DoubleDamageChance, true, [3, 5, 6, 7], 7),
        (Stats.TotalStrength, false, [10, 25, 40, 65], 65),
        (Stats.TotalVitality, false, [10, 25, 40, 65], 65),
        (Stats.TotalEnergy, false, [10, 25, 40, 65], 65),
        (Stats.TotalAgility, false, [10, 25, 40, 65], 65),
    ];

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21ItemOptionsInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21ItemOptionsInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Activates the items of the catalog which wait for the wide item format and marks those of the item groups
    /// 0 to 15, so that only a client with the wide format gets them.
    /// </summary>
    public void ActivateWideItems()
    {
        var marker = this.GetOrCreateWideFormatMarker();
        foreach (var definition in this.CatalogItems(row => row.WaitsFor is not null))
        {
            definition.IsActive = true;
            if (definition.Group <= 15 && !definition.PossibleItemOptions.Contains(marker))
            {
                definition.PossibleItemOptions.Add(marker);
            }
        }
    }

    /// <summary>
    /// Adds the mastery options and the mastery bonus options to the mastery items.
    /// </summary>
    public void AddMasteryOptions()
    {
        this.EnsureAllStatsBonus();
        var weapon = this.GetOrCreateDefinition(ItemOptionDefinitionNumbers.MasteryWeapon, "Mastery Weapon Options", 1, 1.0f, definition =>
        {
            definition.PossibleOptions.Add(this.CreateRelatedOption(Season21ItemOptionTypes.Mastery, 6, ItemOptionDefinitionNumbers.MasteryWeapon, Stats.BaseDamageBonus, Stats.TotalLevel, 1f / 10f));
            definition.PossibleOptions.Add(this.CreateOption(Season21ItemOptionTypes.Mastery, 7, ItemOptionDefinitionNumbers.MasteryWeapon, Stats.BaseDamageBonus, 30, AggregateType.AddRaw));
        });
        var armor = this.GetOrCreateDefinition(ItemOptionDefinitionNumbers.MasteryArmor, "Mastery Armor Options", 1, 1.0f, definition =>
        {
            definition.PossibleOptions.Add(this.CreateOption(Season21ItemOptionTypes.Mastery, 6, ItemOptionDefinitionNumbers.MasteryArmor, Stats.MaximumHealth, 165, AggregateType.AddRaw));
            definition.PossibleOptions.Add(this.CreateOption(Season21ItemOptionTypes.Mastery, 7, ItemOptionDefinitionNumbers.MasteryArmor, Stats.MaximumMana, 165, AggregateType.AddRaw));
            definition.PossibleOptions.Add(this.CreateOption(Season21ItemOptionTypes.Mastery, 8, ItemOptionDefinitionNumbers.MasteryArmor, Stats.DefenseFinal, 45, AggregateType.AddRaw));
            definition.PossibleOptions.Add(this.CreateOption(Season21ItemOptionTypes.Mastery, 9, ItemOptionDefinitionNumbers.MasteryArmor, Stats.MoneyAmountRate, 1.45f, AggregateType.Multiplicate));
            definition.PossibleOptions.Add(this.CreateRelatedOption(Season21ItemOptionTypes.Mastery, 10, ItemOptionDefinitionNumbers.MasteryArmor, Stats.DefenseFinal, Stats.TotalLevel, 1f / 20f));
        });
        var weaponBonus = this.GetOrCreateDefinition(ItemOptionDefinitionNumbers.MasteryWeaponBonus, "Mastery Weapon Bonus Option", 1, 0.25f, definition =>
            definition.PossibleOptions.Add(this.CreateLevelOption(Season21ItemOptionTypes.MasteryBonus, 0, ItemOptionDefinitionNumbers.MasteryWeaponBonus, Stats.AllStatsBonus, false, 10, 25, 40)));
        var armorBonus = this.GetOrCreateDefinition(ItemOptionDefinitionNumbers.MasteryArmorBonus, "Mastery Armor Bonus Option", 1, 0.25f, definition =>
            definition.PossibleOptions.Add(this.CreateLevelOption(Season21ItemOptionTypes.MasteryBonus, 1, ItemOptionDefinitionNumbers.MasteryArmorBonus, Stats.DefenseFinal, false, 25, 50, 75)));

        foreach (var row in Season21ItemCatalog.Items.Where(row => row.Mastery is { Length: 2 }))
        {
            if (this.Find(row) is not { } definition)
            {
                continue;
            }

            var isWeapon = row.Mastery![0] == 0;
            AddOnce(definition, isWeapon ? weapon : armor);
            AddOnce(definition, isWeapon ? weaponBonus : armorBonus);
        }
    }

    /// <summary>
    /// Adds the grade options to the fourth and fifth wings.
    /// </summary>
    public void AddWingGrades()
    {
        var fourth = this.GetOrCreateDefinition(ItemOptionDefinitionNumbers.WingGrade4th, "Fourth Wing Grade Options", 3, 0.5f, definition =>
        {
            for (short number = 0; number < WingGrades.Length; number++)
            {
                var (attribute, isPercentage, values, _) = WingGrades[number];
                definition.PossibleOptions.Add(this.CreateLevelOption(Season21ItemOptionTypes.WingGrade, number, ItemOptionDefinitionNumbers.WingGrade4th, attribute, isPercentage, values));
            }
        });
        var fifth = this.GetOrCreateDefinition(ItemOptionDefinitionNumbers.WingGrade5th, "Fifth Wing Grade Options", 3, 0.5f, definition =>
        {
            for (short number = 0; number < WingGrades.Length; number++)
            {
                var (attribute, isPercentage, _, value) = WingGrades[number];
                definition.PossibleOptions.Add(this.CreateLevelOption(Season21ItemOptionTypes.WingGrade, number, ItemOptionDefinitionNumbers.WingGrade5th, attribute, isPercentage, value));
            }
        });

        var wingSlot = this._gameConfiguration.ItemSlotTypes.FirstOrDefault(t => t.ItemSlots.Count == 1 && t.ItemSlots.Contains(InventoryConstants.WingsSlot));
        foreach (var row in Season21ItemCatalog.Items.Where(row => row.KindA is FourthWingKind or FifthWingKind))
        {
            if (this.Find(row) is { } definition)
            {
                AddOnce(definition, row.KindA == FourthWingKind ? fourth : fifth);
                definition.ItemSlot ??= wingSlot;
            }
        }
    }

    /// <summary>
    /// Adds the sphere levels 6 to 10: the levels of the socket options, the options of the seed spheres of these
    /// levels, their mounting and the enhancement of a seed sphere to the next level.
    /// </summary>
    public void AddSphereLevels()
    {
        foreach (var option in this._gameConfiguration.ItemOptions.SelectMany(o => o.PossibleOptions).Where(o => o.OptionType == ItemOptionTypes.SocketOption))
        {
            this.AddSphereLevels(option);
        }

        var mounting = this._gameConfiguration.Monsters.SelectMany(m => m.ItemCraftings).FirstOrDefault(c => c.Number == MountSeedSphereCraftingNumber)
            ?.SimpleCraftingSettings?.RequiredItems.FirstOrDefault(r => r.Reference == MountSeedSphereCrafting.SeedSphereReference);
        for (var level = Season6SphereLevels + 1; level <= MaximumSphereLevel; level++)
        {
            for (var element = 0; element < SeedTypes; element++)
            {
                if (this.SeedSphere(level, element) is not { } sphere || this.SeedSphere(1, element) is not { } template)
                {
                    continue;
                }

                sphere.MaximumItemLevel = template.MaximumItemLevel;
                foreach (var options in template.PossibleItemOptions.Where(o => !sphere.PossibleItemOptions.Contains(o)).ToList())
                {
                    sphere.PossibleItemOptions.Add(options);
                }

                if (mounting is not null && !mounting.PossibleItems.Contains(sphere))
                {
                    mounting.PossibleItems.Add(sphere);
                }
            }
        }

        this.AddSphereEnhancements();
    }

    private static void AddOnce(ItemDefinition definition, ItemOptionDefinition options)
    {
        if (!definition.PossibleItemOptions.Contains(options))
        {
            definition.PossibleItemOptions.Add(options);
        }
    }

    private static float ValueOf(float value, bool isPercentage) => isPercentage ? value * Percent : value;

    private IEnumerable<ItemDefinition> CatalogItems(Func<Season21CatalogItem, bool> predicate)
        => Season21ItemCatalog.Items.Where(predicate).Select(this.Find).OfType<ItemDefinition>();

    private ItemDefinition? Find(Season21CatalogItem row)
        => this._gameConfiguration.Items.FirstOrDefault(i => i.Group == row.Group && i.Number == row.Number);

    private ItemDefinition? SeedSphere(int level, int element)
    {
        var number = level <= Season6SphereLevels
            ? FirstSeedSphereNumber + ((level - 1) * SeedTypes) + element
            : FirstWideSeedSphereNumber + ((level - Season6SphereLevels - 1) * SeedTypes) + element;
        return this._gameConfiguration.Items.FirstOrDefault(i => i.Group == SocketItemGroup && i.Number == number);
    }

    private void AddSphereLevels(IncreasableItemOption option)
    {
        if (!SphereLevelValues.TryGetValue(((SocketSubOptionType)option.SubOptionType, option.Number), out var values)
            || option.LevelDependentOptions.FirstOrDefault(l => l.Level == Season6SphereLevels) is not { PowerUpDefinition: { } top })
        {
            return;
        }

        for (var level = Season6SphereLevels + 1; level <= MaximumSphereLevel; level++)
        {
            if (option.LevelDependentOptions.Any(l => l.Level == level))
            {
                continue;
            }

            var ratio = (float)values[level - Season6SphereLevels] / values[0];
            var optionOfLevel = this._context.CreateNew<ItemOptionOfLevel>();
            optionOfLevel.Level = level;
            optionOfLevel.PowerUpDefinition = this.Scale(top, ratio);
            option.LevelDependentOptions.Add(optionOfLevel);
        }
    }

    private PowerUpDefinition Scale(PowerUpDefinition source, float ratio)
    {
        var powerUp = this._context.CreateNew<PowerUpDefinition>();
        powerUp.TargetAttribute = source.TargetAttribute;
        powerUp.Boost = this._context.CreateNew<PowerUpDefinitionValue>();
        if (source.Boost is not { } boost)
        {
            return powerUp;
        }

        var constant = boost.ConstantValue;
        powerUp.Boost.ConstantValue.AggregateType = constant.AggregateType;
        powerUp.Boost.ConstantValue.Value = constant.AggregateType == AggregateType.Multiplicate
            ? 1 + ((constant.Value - 1) * ratio)
            : constant.Value * ratio;
        foreach (var related in boost.RelatedValues)
        {
            var relationship = this._context.CreateNew<AttributeRelationship>();
            relationship.TargetAttribute = related.TargetAttribute;
            relationship.InputAttribute = related.InputAttribute;
            relationship.InputOperator = related.InputOperator;
            relationship.InputOperand = related.InputOperand * ratio;
            relationship.OperandAttribute = related.OperandAttribute;
            relationship.AggregateType = related.AggregateType;
            powerUp.Boost.RelatedValues.Add(relationship);
        }

        return powerUp;
    }

    private void AddSphereEnhancements()
    {
        var researcher = this._gameConfiguration.Monsters.FirstOrDefault(m => m.ItemCraftings.Any(c => c.Number == MountSeedSphereCraftingNumber));
        var rune = this._gameConfiguration.Items.FirstOrDefault(i => i.Group == SocketItemGroup && i.Number == SphereUpgradeRuneNumber);
        if (researcher is null || rune is null)
        {
            return;
        }

        for (var step = 0; step < SphereEnhancements.Length; step++)
        {
            var number = (byte)(FirstSphereEnhanceCraftingNumber + step);
            if (researcher.ItemCraftings.Any(c => c.Number == number))
            {
                continue;
            }

            var fromLevel = Season6SphereLevels + step;
            var spheres = Enumerable.Range(0, SeedTypes).Select(element => this.SeedSphere(fromLevel, element)).OfType<ItemDefinition>().ToList();
            if (spheres.Count == 0)
            {
                continue;
            }

            var (money, runes) = SphereEnhancements[step];
            var crafting = this._context.CreateNew<ItemCrafting>();
            crafting.Name = $"Seed Sphere Enhancement (Level {fromLevel + 1})";
            crafting.Number = number;
            crafting.ItemCraftingHandlerClassName = typeof(SeedSphereEnhanceCrafting).FullName!;
            var settings = this._context.CreateNew<SimpleCraftingSettings>();
            crafting.SimpleCraftingSettings = settings;
            settings.SuccessPercent = 100;
            settings.Money = money;

            var seedSphere = this._context.CreateNew<ItemCraftingRequiredItem>();
            seedSphere.MinimumAmount = 1;
            seedSphere.MaximumAmount = 1;
            seedSphere.MaximumItemLevel = 15;
            seedSphere.Reference = SeedSphereEnhanceCrafting.SeedSphereReference;
            spheres.ForEach(seedSphere.PossibleItems.Add);
            settings.RequiredItems.Add(seedSphere);

            var runeItems = this._context.CreateNew<ItemCraftingRequiredItem>();
            runeItems.MinimumAmount = runes;
            runeItems.MaximumAmount = runes;
            runeItems.PossibleItems.Add(rune);
            settings.RequiredItems.Add(runeItems);

            researcher.ItemCraftings.Add(crafting);
        }
    }

    private void EnsureAllStatsBonus()
    {
        if (this._gameConfiguration.Attributes.All(a => a.Id != Stats.AllStatsBonus.Id))
        {
            this._gameConfiguration.Attributes.Add(this._context.CreateNew<AttributeDefinition>(Stats.AllStatsBonus.Id, Stats.AllStatsBonus.Designation, Stats.AllStatsBonus.Description));
        }

        var bonus = Stats.AllStatsBonus.GetPersistent(this._gameConfiguration);
        var raised = new[] { Stats.TotalStrength, Stats.TotalAgility, Stats.TotalVitality, Stats.TotalEnergy, Stats.TotalLeadership }
            .Select(a => a.GetPersistent(this._gameConfiguration))
            .ToList();
        foreach (var characterClass in this._gameConfiguration.CharacterClasses)
        {
            foreach (var target in raised)
            {
                if (target == Stats.TotalLeadership.GetPersistent(this._gameConfiguration)
                    && characterClass.StatAttributes.All(s => s.Attribute != Stats.BaseLeadership.GetPersistent(this._gameConfiguration)))
                {
                    continue;
                }

                if (characterClass.AttributeCombinations.Any(r => r.TargetAttribute == target && r.InputAttribute == bonus))
                {
                    continue;
                }

                var relationship = this._context.CreateNew<AttributeRelationship>();
                relationship.TargetAttribute = target;
                relationship.InputAttribute = bonus;
                relationship.InputOperator = InputOperator.Multiply;
                relationship.InputOperand = 1;
                characterClass.AttributeCombinations.Add(relationship);
            }
        }
    }

    private ItemOptionDefinition GetOrCreateWideFormatMarker()
        => this.GetOrCreateDefinition(ItemOptionDefinitionNumbers.WideFormat, "Wide Item Format", 0, 0, definition =>
        {
            var marker = this._context.CreateNew<IncreasableItemOption>();
            marker.SetGuid(ItemOptionDefinitionNumbers.WideFormat, 0);
            marker.OptionType = this.GetOptionType(Season21ItemOptionTypes.WideFormat);
            definition.PossibleOptions.Add(marker);
        });

    private ItemOptionDefinition GetOrCreateDefinition(short number, string name, int maximumOptions, float addChance, Action<ItemOptionDefinition> addOptions)
    {
        var id = GuidHelper.CreateGuid<ItemOptionDefinition>(number);
        if (this._gameConfiguration.ItemOptions.FirstOrDefault(o => o.GetId() == id) is { } existing)
        {
            return existing;
        }

        var definition = this._context.CreateNew<ItemOptionDefinition>();
        definition.SetGuid(number);
        definition.Name = name;
        definition.AddsRandomly = maximumOptions > 0;
        definition.AddChance = addChance;
        definition.MaximumOptionsPerItem = maximumOptions;
        addOptions(definition);
        this._gameConfiguration.ItemOptions.Add(definition);
        return definition;
    }

    private ItemOptionType GetOptionType(ItemOptionType type)
    {
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

    private IncreasableItemOption CreateOption(ItemOptionType type, short number, short parentNumber, AttributeDefinition attribute, float value, AggregateType aggregateType)
    {
        var option = this.CreateOptionBase(type, number, parentNumber);
        option.PowerUpDefinition = this.CreatePowerUp(attribute, value, aggregateType);
        return option;
    }

    private IncreasableItemOption CreateRelatedOption(ItemOptionType type, short number, short parentNumber, AttributeDefinition attribute, AttributeDefinition input, float multiplier)
    {
        var option = this.CreateOptionBase(type, number, parentNumber);
        var powerUp = this._context.CreateNew<PowerUpDefinition>();
        powerUp.TargetAttribute = attribute.GetPersistent(this._gameConfiguration);
        powerUp.Boost = this._context.CreateNew<PowerUpDefinitionValue>();
        var relationship = this._context.CreateNew<AttributeRelationship>();
        relationship.TargetAttribute = powerUp.TargetAttribute;
        relationship.InputAttribute = input.GetPersistent(this._gameConfiguration);
        relationship.InputOperator = InputOperator.Multiply;
        relationship.InputOperand = multiplier;
        powerUp.Boost.RelatedValues.Add(relationship);
        option.PowerUpDefinition = powerUp;
        return option;
    }

    private IncreasableItemOption CreateLevelOption(ItemOptionType type, short number, short parentNumber, AttributeDefinition attribute, bool isPercentage, params float[] values)
    {
        var option = this.CreateOptionBase(type, number, parentNumber);
        for (var i = 0; i < values.Length; i++)
        {
            var optionOfLevel = this._context.CreateNew<ItemOptionOfLevel>();
            optionOfLevel.Level = i + 1;
            optionOfLevel.PowerUpDefinition = this.CreatePowerUp(attribute, ValueOf(values[i], isPercentage), AggregateType.AddRaw);
            option.LevelDependentOptions.Add(optionOfLevel);
        }

        option.PowerUpDefinition = this.CreatePowerUp(attribute, ValueOf(values[0], isPercentage), AggregateType.AddRaw);
        return option;
    }

    private IncreasableItemOption CreateOptionBase(ItemOptionType type, short number, short parentNumber)
    {
        var option = this._context.CreateNew<IncreasableItemOption>();
        option.SetGuid(parentNumber, number);
        option.OptionType = this.GetOptionType(type);
        option.Number = number;
        return option;
    }

    private PowerUpDefinition CreatePowerUp(AttributeDefinition attribute, float value, AggregateType aggregateType)
    {
        var powerUp = this._context.CreateNew<PowerUpDefinition>();
        powerUp.TargetAttribute = attribute.GetPersistent(this._gameConfiguration);
        powerUp.Boost = this._context.CreateNew<PowerUpDefinitionValue>();
        powerUp.Boost.ConstantValue.Value = value;
        powerUp.Boost.ConstantValue.AggregateType = aggregateType;
        return powerUp;
    }
}
