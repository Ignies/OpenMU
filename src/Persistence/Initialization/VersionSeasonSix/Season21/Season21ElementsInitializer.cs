// <copyright file="Season21ElementsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the elements of Season 21 (pentagrams, errtels, the element columns of the monsters, the element imprints of
/// the skills) and the earrings, with their equipped slots after the personal store. Every step keeps what the
/// configuration already has, so each update can run again. See Readme.md, "Elements, pentagrams and earrings".
/// </summary>
internal class Season21ElementsInitializer
{
    /// <summary>
    /// The number of the element options of the pentagrams and errtels.
    /// </summary>
    internal const short ElementOptionsNumber = 0x0A00;

    /// <summary>
    /// The number of the rank options of an errtel is this plus the errtel's item number.
    /// </summary>
    internal const short ErrtelOptionsNumber = 0x0B00;

    /// <summary>
    /// The number of the rank 1 options (rolled when an errtel drops) of an errtel is this plus its item number.
    /// </summary>
    internal const short ErrtelFirstRankNumber = 0x0D00;

    /// <summary>
    /// The number of the first option of an earring (always rolled) is this plus its item number.
    /// </summary>
    internal const short EarringOptionsNumber = 0x0F00;

    /// <summary>
    /// The number of the further options of an earring is this plus its item number.
    /// </summary>
    internal const short EarringExtraOptionsNumber = 0x1100;

    /// <summary>
    /// The number of the link options of a pair of earrings is this plus the tier.
    /// </summary>
    internal const short EarringLinkNumber = 0x1300;

    /// <summary>
    /// The number of the options of a pentagram is this plus its item number.
    /// </summary>
    internal const short PentagramOptionsNumber = 0x1500;

    /// <summary>
    /// The number of the set options of the mastery errtels.
    /// </summary>
    internal const short ErrtelSetNumber = 0x1700;

    /// <summary>
    /// The number of the errtel set bonuses of the mastery pentagrams.
    /// </summary>
    internal const short ErrtelSetBonusNumber = 0x1701;

    /// <summary>
    /// The crafting number of the earring enhancement from +0 to +1; the next levels follow.
    /// </summary>
    internal const byte FirstEarringEnhanceCraftingNumber = 165;

    /// <summary>
    /// The crafting number of the first crafting of the elemental refinery at the chaos goblin (after the mixes of
    /// the item options, 110-134).
    /// </summary>
    internal const byte FirstRefineryCraftingNumber = 140;

    private const byte ItemGroup = 12;

    /// <summary>
    /// The level from which a failed earring enhancement resets the earring to +0 (Webzen guide 150: "After +7
    /// enchantment, a failed enchantment attempt resets the enchantment level to 0").
    /// </summary>
    private const int EarringResetLevel = 7;

    private const byte EarringReference = 0x7A;

    private const byte LapidaryStoneGroup = 13;

    private const short LapidaryStoneNumber = 510;

    private const short ElementalRuneNumber = 288;

    private const short AbsorbShieldOption = 210;

    private const short AbsorbLifeOption = 211;

    private const short PunishOption = 223;

    private const byte Season6MaximumItemLevel = 15;

    private const float Percent = 0.01f;

    /// <summary>
    /// The sub number of the item of an earring link set (<see cref="ItemOfItemSet"/> ids are group, number, sub number).
    /// </summary>
    private const byte EarringSetSubNumber = 0xEA;

    /// <summary>
    /// The chance of each of the up to two further options of an earring. EarringItemOption.txt gives one option at
    /// 70 %, two at 20 % and three at 10 % (0.4 further options on average); two tries of 20 % keep that average
    /// (64 / 32 / 4 %), since an option definition rolls each try with the same chance.
    /// </summary>
    private const float EarringExtraOptionChance = 0.2f;

    /// <summary>
    /// The stats of the earring options by their Season 21 option index. Webzen names the options of the first
    /// earrings (guide 150: skill attack, attack/wizardry, excellent and critical damage, maximum health; attack speed,
    /// excellent and critical rate, AG recovery, maximum mana; the pair: enemy defense ignore and all stats), and the
    /// values of EarringItemOption.txt match them. 28-31 are the mastery options of the later earrings (Webzen guides
    /// 275 and 351, muonline.net earrings guide: mastery attack/wizardry, mastery excellent and critical damage, the
    /// pair's mastery damage); OpenMU has no separate mastery damage, so they raise the plain damage stats (design).
    /// </summary>
    private static readonly Dictionary<int, (AttributeDefinition Attribute, bool IsPercentage)> EarringOptionStats = new()
    {
        [9] = (Stats.AttackSpeedAny, false),
        [11] = (Stats.MaximumHealth, false),
        [12] = (Stats.MaximumMana, false),
        [14] = (Stats.AbilityRecoveryAbsolute, false),
        [15] = (Stats.CriticalDamageChance, true),
        [16] = (Stats.CriticalDamageBonus, false),
        [17] = (Stats.ExcellentDamageChance, true),
        [18] = (Stats.ExcellentDamageBonus, false),
        [19] = (Stats.SkillDamageBonus, false),
        [21] = (Stats.DefenseIgnoreChance, true),
        [23] = (Stats.BaseDamageBonus, false),
        [24] = (Stats.AllStatsBonus, false),
        [28] = (Stats.BaseDamageBonus, false),
        [29] = (Stats.ExcellentDamageBonus, false),
        [30] = (Stats.CriticalDamageBonus, false),
        [31] = (Stats.BaseDamageBonus, false),
    };

    /// <summary>
    /// The packed jewels (group 12) of the jewels (group 14): bless, soul, life, creation.
    /// </summary>
    private static readonly Dictionary<int, short> PackedJewels = new() { [13] = 30, [14] = 31, [16] = 136, [22] = 137 };

    private static readonly short[] MasteryErrtelNumbers = [505, 506, 507, 508];

    /// <summary>
    /// The values of the mastery options of the earrings by tier 1-8, which EarringItemOption.txt gives as an index
    /// only. 28 mastery attack/wizardry and 29/30 mastery excellent/critical damage: Webzen's guides of the 5th (92,
    /// 155) and 7th mastery earrings (127, 215), the other tiers interpolated linearly (design). 31 mastery damage of
    /// a pair: muonlinefanz.com earrings (Blood 60, Allurement 70) and Webzen's 7th guide (90), 10 a tier.
    /// </summary>
    private static readonly Dictionary<int, int[]> EarringMasteryValues = new()
    {
        [28] = [0, 40, 57, 75, 92, 110, 127, 145],
        [29] = [0, 65, 95, 125, 155, 185, 215, 245],
        [30] = [0, 65, 95, 125, 155, 185, 215, 245],
        [31] = [0, 0, 0, 60, 70, 80, 90, 100],
    };

    /// <summary>
    /// The stats of the pentagram options by their number (PentagramOption.xml names): the stat and whether the value
    /// is a percentage. 31-33 are marked unused by the data.
    /// </summary>
    private static readonly Dictionary<int, (AttributeDefinition Attribute, bool IsPercentage)[]> PentagramOptionStats = CreatePentagramOptionStats();

    /// <summary>
    /// The stats of the errtel set bonuses by their index (ErrtelMastery.xml; worldofmu.net mastery errtel guide):
    /// 0 elemental attack, 1 elemental defense, 4 base elemental defense, 5 elemental damage. 2 "supreme attack" and
    /// 3 "supreme defense" are named nowhere with their meaning; they stay without effect.
    /// </summary>
    private static readonly Dictionary<int, AttributeDefinition> ErrtelSetBonusStats = new()
    {
        [0] = Stats.ElementDamageBonus,
        [1] = Stats.ElementDefense,
        [4] = Stats.ElementDefense,
        [5] = Stats.ElementDamageBonus,
    };

    private static readonly AttributeDefinition[] ElementStats =
    [
        Stats.Element, Stats.ElementDamageMin, Stats.ElementDamageMax, Stats.ElementDamageBonus, Stats.ElementDefense,
        Stats.ElementAttackRate, Stats.ElementDefenseRate, Stats.ElementDamagePvp, Stats.ElementDamagePvm,
        Stats.ElementDefensePvp, Stats.ElementDefensePvm, Stats.ElementDamageIncrease, Stats.ElementDefenseIncrease,
        Stats.ElementDamageAgainstFire, Stats.ElementDamageAgainstWater, Stats.ElementDamageAgainstEarth,
        Stats.ElementDamageAgainstWind, Stats.ElementDamageAgainstDarkness, Stats.ElementDefenseAgainstFire,
        Stats.ElementDefenseAgainstWater, Stats.ElementDefenseAgainstEarth, Stats.ElementDefenseAgainstWind,
        Stats.ElementDefenseAgainstDarkness, Stats.ElementCriticalChance, Stats.ElementDodgeChance,
        Stats.ElementDefenseFromDefense, Stats.ElementDamageFromDamage, Stats.ElementAbsorbShield, Stats.ElementAbsorbLife,
        Stats.ElementPunish,
    ];

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21ElementsInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21ElementsInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Adds the element stats, the pentagrams with their slot, element and sockets, the errtels with their element
    /// and rank options, the element columns of the monsters and the configuration of the <see cref="ElementsFeaturePlugIn"/>.
    /// </summary>
    public void AddElements()
    {
        this.EnsureAttributes(ElementStats);
        var data = Season21ElementData.Instance;
        var slot = this.GetOrCreateSlotType(InventoryConstants.PentagramSlot, "Pentagram");
        var elements = this.GetOrCreateDefinition(ElementOptionsNumber, "Element", 1, 1, definition =>
        {
            for (short element = (short)Element.Fire; element <= (short)Element.Darkness; element++)
            {
                definition.PossibleOptions.Add(this.CreateOption(Season21ItemOptionTypes.Element, element, ElementOptionsNumber, Stats.Element, element));
            }
        });

        foreach (var row in data.Pentagrams)
        {
            if (this.Find(row.Number) is not { } pentagram)
            {
                continue;
            }

            pentagram.ItemSlot ??= slot;

            // A pentagram drops with up to four sockets; the Expansion Scroll of Radiance slot opens the fifth.
            pentagram.MaximumSockets = Pentagrams.Sockets - 1;
            AddOnce(pentagram, elements);
            this.AddPentagramOptions(pentagram, row, data.PentagramOptions);
            this.AddBasePowerUp(pentagram, Stats.ElementDamageMin, row.DamageMin);
            this.AddBasePowerUp(pentagram, Stats.ElementDamageMax, row.DamageMax);
            this.AddBasePowerUp(pentagram, Stats.ElementDefense, row.Defense);
            this.AddBasePowerUp(pentagram, Stats.ElementAttackRate, row.AttackRate);
            this.AddBasePowerUp(pentagram, Stats.ElementDefenseRate, row.DefenseRate);
        }

        foreach (var row in data.Errtels)
        {
            if (this.Find(row.Number) is { } errtel)
            {
                this.AddErrtelOptions(errtel, row, elements);
            }
        }

        this.AddErrtelSets(data);

        this.AddMonsterElements(data.Monsters);
        this.AddRefinery(data.Refinery);
        this.ConfigurePlugIn(data);
    }

    /// <summary>
    /// Adds the earrings: their slots, base stats, random options and the link options of a pair of one tier.
    /// </summary>
    public void AddEarrings()
    {
        this.EnsureAttributes([Stats.ElementDefense]);
        var data = Season21ElementData.Instance.Earrings;
        var slots = new Dictionary<int, ItemSlotType>
        {
            [InventoryConstants.RightEarringSlot] = this.GetOrCreateSlotType(InventoryConstants.RightEarringSlot, "Right Earring"),
            [InventoryConstants.LeftEarringSlot] = this.GetOrCreateSlotType(InventoryConstants.LeftEarringSlot, "Left Earring"),
        };

        foreach (var row in data.Items)
        {
            if (this.Find(row.Number) is not { } earring
                || Season21ItemCatalog.Items.FirstOrDefault(i => i.Group == ItemGroup && i.Number == row.Number) is not { } catalogRow
                || !slots.TryGetValue(catalogRow.Slot, out var slot))
            {
                continue;
            }

            earring.ItemSlot ??= slot;

            // Item.xml damage and defense are the basic options "ATK/Wizardry/Curse min - max" and "Elemental DEF"
            // (Webzen guides 150, 275, 351; muonlinefanz.com earrings: the same numbers for every tier).
            earring.BasePowerUpAttributes.Where(p => p.TargetAttribute == Stats.BaseDamageBonus).ToList().ForEach(p => earring.BasePowerUpAttributes.Remove(p));
            foreach (var attribute in new[] { Stats.MinimumPhysBaseDmg, Stats.MinimumWizBaseDmg, Stats.MinimumCurseBaseDmg })
            {
                this.AddBasePowerUp(earring, attribute, catalogRow.DamageMin);
            }

            foreach (var attribute in new[] { Stats.MaximumPhysBaseDmg, Stats.MaximumWizBaseDmg, Stats.MaximumCurseBaseDmg })
            {
                this.AddBasePowerUp(earring, attribute, catalogRow.DamageMax);
            }

            this.AddBasePowerUp(earring, Stats.ElementDefense, catalogRow.Defense);

            var options = row.Options.Select(o => this.CreateEarringOption(o.Option, EarringValue(o.Option, row.Tier, data.GetValue(row.Tier, o.ValueIndex)), (short)(EarringOptionsNumber + row.Number))).ToList();
            AddOnce(earring, this.GetOrCreateDefinition((short)(EarringOptionsNumber + row.Number), $"Earring Options ({earring.Name.ValueInNeutralLanguage})", 1, 1, d => options.ForEach(d.PossibleOptions.Add)));
            AddOnce(earring, this.GetOrCreateDefinition((short)(EarringExtraOptionsNumber + row.Number), $"Earring Further Options ({earring.Name.ValueInNeutralLanguage})", 2, EarringExtraOptionChance, d => options.ForEach(d.PossibleOptions.Add)));
        }

        foreach (var link in data.Links)
        {
            this.AddEarringLink(link, data);
        }

        this.SetMissingEarringValues(data);
        this.AddEarringEnhancement(data);
    }

    private static float ValueOf(float value, bool isPercentage) => isPercentage ? value * Percent : value;

    private static int EarringValue(int option, int tier, int sourceValue)
        => EarringMasteryValues.TryGetValue(option, out var values) ? values[tier] : sourceValue;

    private static Dictionary<int, (AttributeDefinition Attribute, bool IsPercentage)[]> CreatePentagramOptionStats()
    {
        var result = new Dictionary<int, (AttributeDefinition Attribute, bool IsPercentage)[]>();
        void Add(AttributeDefinition attribute, bool isPercentage, params int[] numbers)
        {
            foreach (var number in numbers)
            {
                result[number] = [.. result.GetValueOrDefault(number) ?? [], (attribute, isPercentage)];
            }
        }

        Add(Stats.ElementDamageIncrease, true, 1, 34, 61, 2, 21, 62);
        Add(Stats.ElementDefenseIncrease, true, 1, 34, 61, 3, 22, 63);
        Add(Stats.ElementCriticalChance, true, 4, 23, 24, 25, 64, 65, 66);
        Add(Stats.ElementDefense, false, 5, 30);
        Add(Stats.ElementDefenseFromDefense, true, 26, 42, 68);
        Add(Stats.ElementDamageFromDamage, true, 27, 43, 69);
        Add(Stats.ElementDodgeChance, true, 28, 41, 67);
        Add(Stats.ElementDamageBonus, false, 29);
        return result;
    }

    private static void AddOnce(ItemDefinition definition, ItemOptionDefinition options)
    {
        if (!definition.PossibleItemOptions.Contains(options))
        {
            definition.PossibleItemOptions.Add(options);
        }
    }

    /// <summary>
    /// Gets the stat of an errtel option by its Season 21 option index (PentagramJewelOption.xml): per kind, the
    /// rank 1 option, the five "against element" options of rank 2 and the PvP / raid options of rank 3.
    /// </summary>
    private static AttributeDefinition? GetErrtelStat(int number)
    {
        AttributeDefinition[] damageAgainst = [Stats.ElementDamageAgainstFire, Stats.ElementDamageAgainstWater, Stats.ElementDamageAgainstEarth, Stats.ElementDamageAgainstWind, Stats.ElementDamageAgainstDarkness];
        AttributeDefinition[] defenseAgainst = [Stats.ElementDefenseAgainstFire, Stats.ElementDefenseAgainstWater, Stats.ElementDefenseAgainstEarth, Stats.ElementDefenseAgainstWind, Stats.ElementDefenseAgainstDarkness];
        return number switch
        {
            0 or 250 or 200 or 204 => Stats.ElementDamageBonus,
            AbsorbShieldOption => Stats.ElementAbsorbShield,
            AbsorbLifeOption => Stats.ElementAbsorbLife,
            PunishOption => Stats.ElementPunish,
            >= 10 and <= 14 => damageAgainst[number - 10],
            20 or 120 => Stats.ElementDamagePvp,
            21 or 121 => Stats.ElementDamagePvm,
            50 or 251 or 201 or 205 => Stats.ElementDefense,
            >= 60 and <= 64 => defenseAgainst[number - 60],
            70 or 170 => Stats.ElementDefensePvp,
            71 or 171 => Stats.ElementDefensePvm,
            100 or 252 or 202 => Stats.ElementAttackRate,
            >= 110 and <= 114 => damageAgainst[number - 110],
            150 or 253 or 203 => Stats.ElementDefenseRate,
            >= 160 and <= 164 => defenseAgainst[number - 160],
            _ => null,
        };
    }

    private void AddErrtelOptions(ItemDefinition errtel, Season21ElementData.ErrtelRow row, ItemOptionDefinition elements)
    {
        var name = errtel.Name.ValueInNeutralLanguage;
        var all = this.GetOrCreateDefinition((short)(ErrtelOptionsNumber + row.Number), $"Errtel Rank Options ({name})", 0, 0, definition =>
        {
            foreach (var option in row.Options)
            {
                definition.PossibleOptions.Add(this.CreateErrtelOption(option, (short)(ErrtelOptionsNumber + row.Number)));
            }
        });
        var firstRank = this.GetOrCreateDefinition((short)(ErrtelFirstRankNumber + row.Number), $"Errtel Rank 1 Options ({name})", 1, 1, definition =>
        {
            foreach (var option in all.PossibleOptions.Where(o => o.SubOptionType == 1))
            {
                definition.PossibleOptions.Add(option);
            }
        });

        AddOnce(errtel, elements);
        AddOnce(errtel, firstRank);
        AddOnce(errtel, all);
        foreach (var option in all.PossibleOptions.Where(o => o.PowerUpDefinition is null))
        {
            if (row.Options.FirstOrDefault(o => o.Number == option.Number) is { } source)
            {
                this.SetErrtelStat(option, source);
            }
        }
    }

    private IncreasableItemOption CreateErrtelOption(Season21ElementData.ErrtelOptionRow row, short parentNumber)
    {
        var option = this._context.CreateNew<IncreasableItemOption>();
        option.SetGuid(parentNumber, row.Number);
        option.OptionType = this.GetOptionType(Season21ItemOptionTypes.ErrtelRank);
        option.Number = row.Number;
        option.SubOptionType = row.Rank;
        this.SetErrtelStat(option, row);
        return option;
    }

    private void SetErrtelStat(IncreasableItemOption option, Season21ElementData.ErrtelOptionRow row)
    {
        if (GetErrtelStat(row.Number) is not { } attribute)
        {
            // The radiance effects bastion, bleeding, paralyzing, binding and blinding are no stats; they stay data.
            return;
        }

        var isPercentage = row.Number is AbsorbShieldOption or AbsorbLifeOption or PunishOption;
        option.PowerUpDefinition = this.CreatePowerUp(attribute, ValueOf(row.Values[0], isPercentage));
        for (var level = 1; level < row.Values.Length; level++)
        {
            var optionOfLevel = this._context.CreateNew<ItemOptionOfLevel>();
            optionOfLevel.Level = level;
            optionOfLevel.PowerUpDefinition = this.CreatePowerUp(attribute, ValueOf(row.Values[level], isPercentage));
            option.LevelDependentOptions.Add(optionOfLevel);
        }
    }

    private IncreasableItemOption CreateEarringOption(short number, int value, short parentNumber)
    {
        var option = this._context.CreateNew<IncreasableItemOption>();
        option.SetGuid(parentNumber, number);
        option.OptionType = this.GetOptionType(Season21ItemOptionTypes.EarringOption);
        option.Number = number;
        if (EarringOptionStats.TryGetValue(number, out var stat))
        {
            this.EnsureAttributes([stat.Attribute]);
            option.PowerUpDefinition = this.CreatePowerUp(stat.Attribute, stat.IsPercentage ? value * Percent : value);
        }

        return option;
    }

    private void AddEarringLink(Season21ElementData.EarringLinkRow link, Season21ElementData.EarringRows data)
    {
        var pair = data.Items.Where(i => i.Tier == link.Tier).Select(i => this.Find(i.Number)).OfType<ItemDefinition>().ToList();
        var id = GuidHelper.CreateGuid<ItemSetGroup>((short)(EarringLinkNumber + link.Tier));
        if (pair.Count < 2 || this._gameConfiguration.ItemSetGroups.Any(g => g.GetId() == id))
        {
            return;
        }

        var number = (short)(EarringLinkNumber + link.Tier);
        var options = this.GetOrCreateDefinition(number, $"Earring Link Options (Tier {link.Tier + 1})", 0, 0, definition =>
        {
            foreach (var option in link.Options)
            {
                definition.PossibleOptions.Add(this.CreateEarringOption(option.Option, EarringValue(option.Option, link.Tier, data.GetValue(link.Tier, option.ValueIndex)), number));
            }
        });

        var group = this._context.CreateNew<ItemSetGroup>();
        group.SetGuid(number);
        group.Name = $"Earring Link (Tier {link.Tier + 1})";
        group.AlwaysApplies = true;
        group.MinimumItemCount = pair.Count;
        group.Options = options;
        foreach (var earring in pair)
        {
            var itemOfSet = this._context.CreateNew<ItemOfItemSet>();
            itemOfSet.SetGuid(earring.Group, earring.Number, EarringSetSubNumber);
            itemOfSet.ItemDefinition = earring;
            group.Items.Add(itemOfSet);
            earring.PossibleItemSetGroups.Add(group);
        }

        this._gameConfiguration.ItemSetGroups.Add(group);
    }

    /// <summary>
    /// Adds the craftings of the elemental refinery to the chaos goblin: the upgrades of the pentagrams and errtels to
    /// their mastery items, the errtel level ups (one per level, since the money depends on it) and the rank up.
    /// </summary>
    private void AddRefinery(Season21ElementData.RefineryRows refinery)
    {
        if (this._gameConfiguration.Monsters.FirstOrDefault(m => m.NpcWindow == NpcWindow.ChaosMachine) is not { } goblin)
        {
            return;
        }

        var number = FirstRefineryCraftingNumber;
        foreach (var upgrade in refinery.PentagramUpgrades.Concat(refinery.ErrtelUpgrades))
        {
            this.AddUpgradeCrafting(goblin, number++, upgrade);
        }

        var rune = this.Find(ElementalRuneNumber);
        var errtels = this._gameConfiguration.Items.Where(i => i.Group == ItemGroup && Pentagrams.GetErrtelSocket(i) is not null).ToList();
        foreach (var levelUp in refinery.LevelUp)
        {
            var crafting = this.GetOrCreateCrafting(goblin, number++, $"Errtel Level Up (Level {levelUp.Level + 1})", typeof(ErrtelLevelUpCrafting), levelUp.Money);
            if (crafting?.SimpleCraftingSettings is { } settings && rune is not null)
            {
                settings.RequiredItems.Add(this.CreateRequiredItem(errtels, 1, 0, ErrtelLevelUpCrafting.ErrtelReference, (byte)levelUp.Level, (byte)levelUp.Level));
                settings.RequiredItems.Add(this.CreateRequiredItem([rune], 1, 0, 0, 0, Season6MaximumItemLevel));
            }
        }

        var rankUpMoney = refinery.RankUpMoney.FirstOrDefault(m => m.Rank == 1)?.Money ?? 0;
        if (this.GetOrCreateCrafting(goblin, number, "Errtel Rank Up", typeof(ErrtelRankUpCrafting), rankUpMoney)?.SimpleCraftingSettings is { } rankUp)
        {
            rankUp.RequiredItems.Add(this.CreateRequiredItem(errtels, 1, 1, ErrtelLevelUpCrafting.ErrtelReference, 0, Season6MaximumItemLevel));
        }
    }

    private void AddUpgradeCrafting(MonsterDefinition goblin, byte number, Season21ElementData.UpgradeRow upgrade)
    {
        if (this.Find(upgrade.Main) is not { } main
            || this.Find(upgrade.Result) is not { } result
            || this.GetOrCreateCrafting(goblin, number, $"{main.Name.ValueInNeutralLanguage} Upgrade", typeof(ElementalUpgradeCrafting), 0)?.SimpleCraftingSettings is not { } settings)
        {
            return;
        }

        settings.RequiredItems.Add(this.CreateRequiredItem([main], 1, 1, ElementalUpgradeCrafting.MainItemReference, 0, Season6MaximumItemLevel));
        foreach (var material in upgrade.Materials)
        {
            if (this.GetMaterial(material[0], material[1], material[2]) is ({ } definition, var level))
            {
                settings.RequiredItems.Add(this.CreateRequiredItem([definition], 1, 1, 0, level, level));
            }
        }

        var resultItem = this._context.CreateNew<ItemCraftingResultItem>();
        resultItem.ItemDefinition = result;
        settings.ResultItems.Add(resultItem);
    }

    /// <summary>
    /// Gets a material of the source: a jewel of 30 (or 10) comes as the packed jewel of that amount, since the chaos
    /// machine holds 32 items (packed jewels: level 0 holds 10, level 1 20, level 2 30).
    /// </summary>
    private (ItemDefinition? Definition, byte Level) GetMaterial(int group, int number, int amount)
    {
        const int jewelGroup = 14;
        const int jewelsPerPackLevel = 10;
        if (group == jewelGroup && amount >= jewelsPerPackLevel && PackedJewels.TryGetValue(number, out var packed))
        {
            return (this.Find(packed), (byte)((amount / jewelsPerPackLevel) - 1));
        }

        return (this._gameConfiguration.Items.FirstOrDefault(i => i.Group == group && i.Number == number), 0);
    }

    private ItemCrafting? GetOrCreateCrafting(MonsterDefinition goblin, byte number, string name, Type? handler, int money)
    {
        if (goblin.ItemCraftings.Any(c => c.Number == number))
        {
            return null;
        }

        var crafting = this._context.CreateNew<ItemCrafting>();
        crafting.Name = name;
        crafting.Number = number;
        crafting.ItemCraftingHandlerClassName = handler?.FullName ?? string.Empty;
        var settings = this._context.CreateNew<SimpleCraftingSettings>();
        settings.Money = money;
        settings.SuccessPercent = 100;
        crafting.SimpleCraftingSettings = settings;
        goblin.ItemCraftings.Add(crafting);
        return crafting;
    }

    private ItemCraftingRequiredItem CreateRequiredItem(IEnumerable<ItemDefinition> items, byte minimumAmount, byte maximumAmount, byte reference, byte minimumLevel, byte maximumLevel)
    {
        var requiredItem = this._context.CreateNew<ItemCraftingRequiredItem>();
        requiredItem.MinimumAmount = minimumAmount;
        requiredItem.MaximumAmount = maximumAmount;
        requiredItem.MinimumItemLevel = minimumLevel;
        requiredItem.MaximumItemLevel = maximumLevel;
        requiredItem.Reference = reference;
        if (reference > 0)
        {
            requiredItem.SuccessResult = MixResult.StaysAsIs;
            requiredItem.FailResult = MixResult.StaysAsIs;
        }

        foreach (var item in items)
        {
            requiredItem.PossibleItems.Add(item);
        }

        return requiredItem;
    }

    private void AddPentagramOptions(ItemDefinition pentagram, Season21ElementData.PentagramRow row, List<Season21ElementData.PentagramOptionRow> settings)
    {
        if (row.Options.Length == 0)
        {
            return;
        }

        var number = (short)(PentagramOptionsNumber + row.Number);
        AddOnce(pentagram, this.GetOrCreateDefinition(number, $"Pentagram Options ({pentagram.Name.ValueInNeutralLanguage})", 0, 0, definition =>
        {
            foreach (var optionNumber in row.Options)
            {
                var value = settings.FirstOrDefault(o => o.Number == optionNumber)?.Value ?? 0;
                var stats = PentagramOptionStats.GetValueOrDefault(optionNumber) ?? [];
                for (var k = 0; k < Math.Max(stats.Length, 1); k++)
                {
                    var option = this._context.CreateNew<IncreasableItemOption>();
                    option.SetGuid(number, (short)((optionNumber * 4) + k));
                    option.OptionType = this.GetOptionType(Season21ItemOptionTypes.PentagramOption);
                    option.Number = (short)optionNumber;
                    if (k < stats.Length)
                    {
                        option.PowerUpDefinition = this.CreatePowerUp(stats[k].Attribute, ValueOf(value, stats[k].IsPercentage));
                    }

                    definition.PossibleOptions.Add(option);
                }
            }
        }));
    }

    private void AddErrtelSets(Season21ElementData data)
    {
        var sets = this.GetOrCreateDefinition(ErrtelSetNumber, "Errtel Set", 1, 1, definition =>
        {
            foreach (var set in data.ErrtelSets)
            {
                var option = this._context.CreateNew<IncreasableItemOption>();
                option.SetGuid(ErrtelSetNumber, (short)set.Number);
                option.OptionType = this.GetOptionType(Season21ItemOptionTypes.ErrtelSet);
                option.Number = (short)set.Number;
                definition.PossibleOptions.Add(option);
            }
        });
        var bonuses = this.GetOrCreateDefinition(ErrtelSetBonusNumber, "Errtel Set Bonuses", 0, 0, definition =>
        {
            foreach (var set in data.ErrtelSets)
            {
                for (var k = 0; k < set.Bonuses.Count; k++)
                {
                    var bonus = set.Bonuses[k];
                    var option = this._context.CreateNew<IncreasableItemOption>();
                    option.SetGuid(ErrtelSetBonusNumber, (short)((set.Number * 10) + k));
                    option.OptionType = this.GetOptionType(Season21ItemOptionTypes.ErrtelSetBonus);
                    option.Number = (short)((set.Number * 10) + k);
                    option.SubOptionType = bonus.Pieces;
                    if (ErrtelSetBonusStats.TryGetValue(bonus.Option, out var attribute))
                    {
                        option.PowerUpDefinition = this.CreatePowerUp(attribute, bonus.Value);
                    }

                    definition.PossibleOptions.Add(option);
                }
            }
        });

        foreach (var errtel in MasteryErrtelNumbers.Select(this.Find).OfType<ItemDefinition>())
        {
            AddOnce(errtel, sets);
        }

        foreach (var pentagram in data.Refinery.PentagramUpgrades.Select(u => this.Find(u.Result)).OfType<ItemDefinition>().Distinct())
        {
            AddOnce(pentagram, bonuses);
        }
    }

    private void SetMissingEarringValues(Season21ElementData.EarringRows data)
    {
        foreach (var row in data.Items)
        {
            var definition = this._gameConfiguration.ItemOptions.FirstOrDefault(o => o.GetId() == GuidHelper.CreateGuid<ItemOptionDefinition>((short)(EarringOptionsNumber + row.Number)));
            foreach (var option in definition?.PossibleOptions.Where(o => o.PowerUpDefinition is null) ?? [])
            {
                if (EarringOptionStats.TryGetValue(option.Number, out var stat) && row.Options.FirstOrDefault(o => o.Option == option.Number) is { } source)
                {
                    this.EnsureAttributes([stat.Attribute]);
                    var value = EarringValue(option.Number, row.Tier, data.GetValue(row.Tier, source.ValueIndex));
                    option.PowerUpDefinition = this.CreatePowerUp(stat.Attribute, stat.IsPercentage ? value * Percent : value);
                }
            }
        }

        foreach (var link in data.Links)
        {
            var definition = this._gameConfiguration.ItemOptions.FirstOrDefault(o => o.GetId() == GuidHelper.CreateGuid<ItemOptionDefinition>((short)(EarringLinkNumber + link.Tier)));
            foreach (var option in definition?.PossibleOptions.Where(o => o.PowerUpDefinition is null) ?? [])
            {
                if (EarringOptionStats.TryGetValue(option.Number, out var stat) && link.Options.FirstOrDefault(o => o.Option == option.Number) is { } source)
                {
                    var value = EarringValue(option.Number, link.Tier, data.GetValue(link.Tier, source.ValueIndex));
                    option.PowerUpDefinition = this.CreatePowerUp(stat.Attribute, stat.IsPercentage ? value * Percent : value);
                }
            }
        }
    }

    /// <summary>
    /// Adds the earring enhancement +0 to +15 at the chaos goblin (EarringItemOption.txt section 4: chance, lapidary
    /// stones, zen; muonline.net earrings guide: 55 stones and 67.5 million zen in total). One crafting per level.
    /// </summary>
    private void AddEarringEnhancement(Season21ElementData.EarringRows data)
    {
        if (this._gameConfiguration.Monsters.FirstOrDefault(m => m.NpcWindow == NpcWindow.ChaosMachine) is not { } goblin
            || this._gameConfiguration.Items.FirstOrDefault(i => i.Group == LapidaryStoneGroup && i.Number == LapidaryStoneNumber) is not { } stone)
        {
            return;
        }

        var earrings = data.Items.Select(i => this.Find(i.Number)).OfType<ItemDefinition>().ToList();
        foreach (var step in data.Enhancement)
        {
            var level = (byte)step.Level;
            if (this.GetOrCreateCrafting(goblin, (byte)(FirstEarringEnhanceCraftingNumber + level), $"Earring Enhancement (+{level + 1})", null, step.Money)?.SimpleCraftingSettings is not { } settings)
            {
                continue;
            }

            settings.SuccessPercent = (byte)step.Rate;
            var earring = this.CreateRequiredItem(earrings, 1, 1, EarringReference, level, level);
            earring.FailResult = level >= EarringResetLevel ? MixResult.LevelResetToZero : MixResult.StaysAsIs;
            settings.RequiredItems.Add(earring);
            settings.RequiredItems.Add(this.CreateRequiredItem([stone], (byte)step.Stones, (byte)step.Stones, 0, 0, 0));
            var result = this._context.CreateNew<ItemCraftingResultItem>();
            result.Reference = EarringReference;
            result.AddLevel = 1;
            settings.ResultItems.Add(result);
        }
    }

    private void AddMonsterElements(IEnumerable<Season21ElementData.MonsterRow> rows)
    {
        foreach (var row in rows)
        {
            foreach (var monster in this._gameConfiguration.Monsters.Where(m => m.Number == row.Number))
            {
                this.SetMonsterAttribute(monster, Stats.Element, row.Element);
                this.SetMonsterAttribute(monster, Stats.ElementDefense, row.Defense);
                this.SetMonsterAttribute(monster, Stats.ElementDamageMin, row.DamageMin);
                this.SetMonsterAttribute(monster, Stats.ElementDamageMax, row.DamageMax);
                this.SetMonsterAttribute(monster, Stats.ElementAttackRate, row.AttackRate);
                this.SetMonsterAttribute(monster, Stats.ElementDefenseRate, row.DefenseRate);
            }
        }
    }

    private void SetMonsterAttribute(MonsterDefinition monster, AttributeDefinition attribute, float value)
    {
        var persistent = attribute.GetPersistent(this._gameConfiguration);
        if (monster.Attributes.FirstOrDefault(a => a.AttributeDefinition == persistent) is { } existing)
        {
            existing.Value = value;
        }
        else if (value != 0)
        {
            monster.AddAttributes(new Dictionary<AttributeDefinition, float> { [attribute] = value }, this._context, this._gameConfiguration);
        }
        else
        {
            // Nothing to add: a missing attribute reads as 0.
        }
    }

    private void ConfigurePlugIn(Season21ElementData data)
    {
        var configuration = Season21PlugInConfigurations.GetOrCreate<ElementsConfiguration>(this._context, this._gameConfiguration, typeof(ElementsFeaturePlugIn).GUID, out var plugIn);
        if (configuration is null)
        {
            return;
        }

        var isNew = configuration.SkillImprints.Count == 0 && configuration.RemovalRates.Count == 0;
        var changed = false;
        if (configuration.SkillImprints.Count == 0)
        {
            configuration.SkillImprints.AddRange(data.SkillImprints.Select(s => new SkillImprint { SkillNumber = s.Skill, Percent = s.Percent }));
            changed = true;
        }

        if (configuration.RemovalRates.Count == 0)
        {
            configuration.RemovalRates.AddRange(data.Refinery.RemovalRates.Select(r => new ErrtelRemovalRate { Level = r.Level, Rates = [.. r.Rates] }));
            changed = true;
        }

        if (configuration.LevelUps.Count == 0)
        {
            configuration.LevelUps.AddRange(data.Refinery.LevelUp.Select(l => new ErrtelLevelUp { Level = l.Level, Rates = [.. l.Ranks.Select(r => r[0])], Runes = [.. l.Ranks.Select(r => r[1])] }));
            changed = true;
        }

        if (configuration.PentagramOptions.Count == 0)
        {
            configuration.PentagramOptions.AddRange(data.PentagramOptions.Select(o => new PentagramOptionRule
            {
                Number = o.Number,
                Needs = [.. o.Needs.Select(n => new ErrtelRequirement { Socket = n[0], Rank = n[1], Level = n[2] })],
            }));
            changed = true;
        }

        if (configuration.PentagramUpgradeRates.Count == 0)
        {
            configuration.PentagramUpgradeRates.AddRange(data.Refinery.PentagramUpgradeRates);
            configuration.ErrtelUpgradeRates.AddRange(data.Refinery.ErrtelUpgradeRates);
            changed = true;
        }

        if (configuration.RankUps.Count == 0)
        {
            configuration.RankUps.AddRange(data.Refinery.RankUpRates.Select(r => new ErrtelRankUp { Rank = r.Rank, Rates = [.. r.Rates] }));
            changed = true;
        }

        if (changed)
        {
            // The first application turns the feature on; later ones leave the operator's switch alone.
            plugIn.IsActive |= isNew;
            plugIn.SetConfiguration(configuration, null);
        }
    }

    private ItemDefinition? Find(short number) => this._gameConfiguration.Items.FirstOrDefault(i => i.Group == ItemGroup && i.Number == number);

    private void EnsureAttributes(IEnumerable<AttributeDefinition> attributes)
    {
        foreach (var attribute in attributes.Where(a => this._gameConfiguration.Attributes.All(c => c.Id != a.Id)))
        {
            var persistent = this._context.CreateNew<AttributeDefinition>(attribute.Id, attribute.Designation, attribute.Description);
            persistent.MaximumValue = attribute.MaximumValue;
            this._gameConfiguration.Attributes.Add(persistent);
        }
    }

    private ItemSlotType GetOrCreateSlotType(byte slot, string description)
    {
        if (this._gameConfiguration.ItemSlotTypes.FirstOrDefault(t => t.ItemSlots.Count == 1 && t.ItemSlots.Contains(slot)) is { } existing)
        {
            return existing;
        }

        var slotType = this._context.CreateNew<ItemSlotType>();
        slotType.SetGuid(slot);
        slotType.Description = description;
        slotType.ItemSlots.Add(slot);
        this._gameConfiguration.ItemSlotTypes.Add(slotType);
        return slotType;
    }

    private void AddBasePowerUp(ItemDefinition item, AttributeDefinition attribute, float value)
    {
        var persistent = attribute.GetPersistent(this._gameConfiguration);
        if (value == 0 || item.BasePowerUpAttributes.Any(p => p.TargetAttribute == persistent))
        {
            return;
        }

        var powerUp = this._context.CreateNew<ItemBasePowerUpDefinition>();
        powerUp.TargetAttribute = persistent;
        powerUp.BaseValue = value;
        powerUp.AggregateType = AggregateType.AddRaw;
        item.BasePowerUpAttributes.Add(powerUp);
    }

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

    private IncreasableItemOption CreateOption(ItemOptionType type, short number, short parentNumber, AttributeDefinition attribute, float value)
    {
        var option = this._context.CreateNew<IncreasableItemOption>();
        option.SetGuid(parentNumber, number);
        option.OptionType = this.GetOptionType(type);
        option.Number = number;
        option.PowerUpDefinition = this.CreatePowerUp(attribute, value);
        return option;
    }

    private PowerUpDefinition CreatePowerUp(AttributeDefinition attribute, float value)
    {
        var powerUp = this._context.CreateNew<PowerUpDefinition>();
        powerUp.TargetAttribute = attribute.GetPersistent(this._gameConfiguration);
        powerUp.Boost = this._context.CreateNew<PowerUpDefinitionValue>();
        powerUp.Boost.ConstantValue.Value = value;
        powerUp.Boost.ConstantValue.AggregateType = AggregateType.AddRaw;
        return powerUp;
    }
}
