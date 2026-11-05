// <copyright file="Season21GearInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

/// <summary>
/// Adds the <see cref="Season21GearData"/> to a game configuration. Each item copies the Season 6 item of its kind
/// (its template): its slot, options and level tables, with the values of Season 21. Existing items are kept.
/// </summary>
/// <remarks>
/// A piece of jewelry copies the Ring of Ice (rings), the Pendant of Fire (physical pendants) or the Pendant of
/// Lightning (wizardry pendants). Gear copies the template its row names, as the initialization of Season 6 creates
/// it, except the socket, guardian and ancient options, which belong to the template; a socket item gets the seed
/// options of its kind instead. A value which the template has no power-up for (e.g. the defense of an armor) gets
/// the level table which the other items of that value use.
/// </remarks>
internal class Season21GearInitializer
{
    /// <summary>
    /// The option number of the data for a bonus of attack and wizardry damage in percent.
    /// </summary>
    private const int DamagePercentOption = 129;

    /// <summary>
    /// The option number of the data for a bonus of attack speed.
    /// </summary>
    private const int AttackSpeedOption = 97;

    private static readonly HashSet<Guid> TemplateOnlyOptionTypes =
    [
        ItemOptionTypes.SocketOption.Id, ItemOptionTypes.SocketBonusOption.Id, ItemOptionTypes.GuardianOption.Id,
        ItemOptionTypes.AncientOption.Id, ItemOptionTypes.AncientBonus.Id,
    ];

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21GearInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21GearInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Adds the items which don't exist yet, with the monster drop switch of the data, and puts every item into the
    /// boxes of the data. An existing item keeps its drop switch.
    /// </summary>
    public void Initialize()
    {
        foreach (var row in Season21GearData.Items)
        {
            var item = this.Find(row.Group, row.Number);
            if (item is null)
            {
                item = this.Create(row);
                if (item is null)
                {
                    continue;
                }

                item.DropsFromMonsters = row.Drops;
            }

            foreach (var (boxGroup, boxNumber) in row.Boxes)
            {
                if (this.Find(boxGroup, boxNumber)?.DropItems.FirstOrDefault(d => d.ItemType == SpecialItemType.Excellent) is { } excellent
                    && !excellent.PossibleItems.Contains(item))
                {
                    excellent.PossibleItems.Add(item);
                }
            }
        }
    }

    private static Guid OptionTypeOf(ItemOptionDefinition option) => option.PossibleOptions.FirstOrDefault()?.OptionType?.Id ?? Guid.Empty;

    private static CharacterClasses ToClasses(int[] ranks)
    {
        (CharacterClasses All, CharacterClasses Master)[] classes =
        [
            (CharacterClasses.AllWizards, CharacterClasses.GrandMaster),
            (CharacterClasses.AllKnights, CharacterClasses.BladeMaster),
            (CharacterClasses.AllElfs, CharacterClasses.HighElf),
            (CharacterClasses.AllMGs, CharacterClasses.DuelMaster),
            (CharacterClasses.AllLords, CharacterClasses.LordEmperor),
            (CharacterClasses.AllSummoners, CharacterClasses.DimensionMaster),
            (CharacterClasses.AllFighters, CharacterClasses.FistMaster),
        ];
        var result = CharacterClasses.None;
        for (var i = 0; i < classes.Length; i++)
        {
            result |= ranks[i] switch
            {
                <= 0 => CharacterClasses.None,
                >= 3 => classes[i].Master,
                _ => classes[i].All,
            };
        }

        return result;
    }

    private static (byte Group, short Number) TemplateOf(Season21GearItem row) => row.Stats?.Template ?? row.Excellent switch
    {
        Season21ExcellentOptions.Damage => (13, 13), // Pendant of Fire
        Season21ExcellentOptions.Wizardry => (13, 12), // Pendant of Lightning
        _ => (13, 8), // Ring of Ice
    };

    private static float? ValueOf(Season21GearStats stats, AttributeDefinition? attribute)
    {
        if (attribute == Stats.MinimumPhysBaseDmgByWeapon)
        {
            return stats.DamageMinimum;
        }

        if (attribute == Stats.MaximumPhysBaseDmgByWeapon)
        {
            return stats.DamageMaximum;
        }

        if (attribute == Stats.AttackSpeedByWeapon || attribute == Stats.AttackSpeedAny)
        {
            return stats.Speed;
        }

        if (attribute == Stats.StaffRise || attribute == Stats.ScepterRise || attribute == Stats.BookRise)
        {
            return stats.MagicPower / 2f;
        }

        if (attribute == Stats.DefenseShield || attribute == Stats.DefenseBase)
        {
            return stats.Defense;
        }

        if (attribute == Stats.DefenseRatePvm)
        {
            return stats.DefenseRate;
        }

        return null;
    }

    private ItemDefinition? Find(byte group, short number)
        => this._gameConfiguration.Items.FirstOrDefault(i => i.Group == group && i.Number == number);

    private ItemDefinition? Create(Season21GearItem row)
    {
        var (templateGroup, templateNumber) = TemplateOf(row);
        if (this.Find(templateGroup, templateNumber) is not { } template)
        {
            return null;
        }

        var item = this._context.CreateNew<ItemDefinition>();
        item.SetGuid(row.Group, row.Number);
        item.Group = row.Group;
        item.Number = row.Number;
        item.Name = row.Name;
        item.IntroducedIn = row.IntroducedIn;
        item.ItemSlot = template.ItemSlot;
        item.DropLevel = row.DropLevel;
        item.Durability = row.Durability;
        item.IsTradable = !row.Bans.HasFlag(Season21ItemBans.Trade);
        item.IsDroppable = !row.Bans.HasFlag(Season21ItemBans.Drop);
        item.IsStorable = !row.Bans.HasFlag(Season21ItemBans.Store);
        item.IsPersonalStoreSellable = !row.Bans.HasFlag(Season21ItemBans.Shop);
        item.IsSellableToNpc = !row.Bans.HasFlag(Season21ItemBans.Sell);
        item.IsRepairable = !row.Bans.HasFlag(Season21ItemBans.Repair);
        if (row.Stats is { } stats)
        {
            this.CopyGear(item, row, stats, template);
        }
        else
        {
            this.CopyJewelry(item, row, template);
        }

        this._gameConfiguration.Items.Add(item);
        return item;
    }

    private void CopyJewelry(ItemDefinition item, Season21GearItem row, ItemDefinition template)
    {
        item.StatProfile = template.StatProfile;
        item.Width = 1;
        item.Height = 1;
        item.MaximumItemLevel = row.MaximumLevel;
        foreach (var characterClass in template.QualifiedCharacters)
        {
            item.QualifiedCharacters.Add(characterClass);
        }

        foreach (var option in template.PossibleItemOptions)
        {
            var type = OptionTypeOf(option);
            if ((type == ItemOptionTypes.Option.Id && row.HealthOption) || (type == ItemOptionTypes.Excellent.Id && row.Excellent != Season21ExcellentOptions.None))
            {
                item.PossibleItemOptions.Add(option);
            }
        }

        foreach (var (option, value) in row.Specials)
        {
            AttributeDefinition[] attributes = option switch
            {
                DamagePercentOption => [Stats.MinimumPhysBaseDmg, Stats.MaximumPhysBaseDmg, Stats.MinimumWizBaseDmg, Stats.MaximumWizBaseDmg],
                AttackSpeedOption => [Stats.AttackSpeedAny],
                _ => throw new NotSupportedException($"Item {row.Group}/{row.Number}: the option {option} of the data has no power-up."),
            };
            foreach (var attribute in attributes)
            {
                var powerUp = this._context.CreateNew<ItemBasePowerUpDefinition>();
                powerUp.TargetAttribute = attribute.GetPersistent(this._gameConfiguration);
                (powerUp.BaseValue, powerUp.AggregateType) = option == DamagePercentOption
                    ? ((100 + value) / 100f, AggregateType.Multiplicate)
                    : (value, AggregateType.AddRaw);
                item.BasePowerUpAttributes.Add(powerUp);
            }
        }
    }

    private void CopyGear(ItemDefinition item, Season21GearItem row, Season21GearStats stats, ItemDefinition template)
    {
        item.Width = stats.Width;
        item.Height = stats.Height;
        item.MaximumItemLevel = template.MaximumItemLevel;
        item.IsAmmunition = template.IsAmmunition;
        item.Skill = stats.Skill > 0 ? this._gameConfiguration.Skills.FirstOrDefault(s => s.Number == stats.Skill) : null;
        foreach (var characterClass in this._gameConfiguration.DetermineCharacterClasses(ToClasses(stats.Classes), true))
        {
            item.QualifiedCharacters.Add(characterClass);
        }

        var options = template.PossibleItemOptions.Where(o => stats.TakesOptions && !TemplateOnlyOptionTypes.Contains(OptionTypeOf(o)));
        foreach (var option in options.Concat(this.SocketOptions(row, stats)))
        {
            item.PossibleItemOptions.Add(option);
        }

        item.MaximumSockets = stats.Sockets;
        foreach (var source in template.BasePowerUpAttributes)
        {
            var isRise = source.TargetAttribute == Stats.StaffRise || source.TargetAttribute == Stats.ScepterRise || source.TargetAttribute == Stats.BookRise;
            var powerUp = this._context.CreateNew<ItemBasePowerUpDefinition>();
            powerUp.TargetAttribute = source.TargetAttribute;
            powerUp.AggregateType = source.AggregateType;
            powerUp.BaseValue = ValueOf(stats, source.TargetAttribute) ?? source.BaseValue;
            powerUp.BonusPerLevelTable = isRise ? this.RiseTable(source, stats) : source.BonusPerLevelTable;
            item.BasePowerUpAttributes.Add(powerUp);
        }

        this.AddMissingPowerUp(item, stats.Defense, row.Group >= 7 ? Stats.DefenseBase : Stats.DefenseShield);
        this.AddMissingPowerUp(item, row.Group == 10 ? stats.Speed : 0, Stats.AttackSpeedAny);

        AttributeDefinition[] requirements = [Stats.Level, Stats.TotalStrengthRequirementValue, Stats.TotalAgilityRequirementValue, Stats.TotalEnergyRequirementValue, Stats.TotalVitalityRequirementValue, Stats.TotalLeadershipRequirementValue];
        for (var i = 0; i < requirements.Length; i++)
        {
            if (stats.Requirements[i] > 0)
            {
                var requirement = this._context.CreateNew<AttributeRequirement>();
                requirement.Attribute = requirements[i].GetPersistent(this._gameConfiguration);
                requirement.MinimumValue = stats.Requirements[i];
                item.Requirements.Add(requirement);
            }
        }
    }

    /// <summary>
    /// Gets the seed options of a socket item, as the socket system of Season 6 defines them: fire, lightning and ice
    /// with the physical or (for staffs) wizardry bonus for weapons; water, earth and wind with the armor bonus otherwise.
    /// </summary>
    private IEnumerable<ItemOptionDefinition> SocketOptions(Season21GearItem row, Season21GearStats stats)
    {
        if (stats.Sockets <= 0)
        {
            return [];
        }

        string[] names = row.Group <= 5
            ? [ItemOptionNames.SocketOptionsFire, ItemOptionNames.SocketOptionsLightning, ItemOptionNames.SocketOptionsIce, row.Group == 5 ? ItemOptionNames.SocketBonusOptionsWizardry : ItemOptionNames.SocketBonusOptionsPhysical]
            : [ItemOptionNames.SocketOptionsWater, ItemOptionNames.SocketOptionsEarth, ItemOptionNames.SocketOptionsWind, ItemOptionNames.SocketBonusOptionsArmors];
        return names.Select(name => this._gameConfiguration.ItemOptions.FirstOrDefault(o => o.Name.ValueInNeutralLanguage == name)).OfType<ItemOptionDefinition>();
    }

    /// <summary>
    /// Gets the level table of a rise: the rise tables differ by the parity of the magic power.
    /// </summary>
    private ItemLevelBonusTable? RiseTable(ItemBasePowerUpDefinition source, Season21GearStats stats)
    {
        var odd = stats.MagicPower % 2 == 1;
        bool Matches(ItemBasePowerUpDefinition p) => p.TargetAttribute == source.TargetAttribute && ((int)(p.BaseValue * 2) % 2 == 1) == odd;
        return Matches(source)
            ? source.BonusPerLevelTable
            : this._gameConfiguration.Items.SelectMany(i => i.BasePowerUpAttributes).FirstOrDefault(Matches)?.BonusPerLevelTable ?? source.BonusPerLevelTable;
    }

    private void AddMissingPowerUp(ItemDefinition item, int value, AttributeDefinition attribute)
    {
        if (value <= 0 || item.BasePowerUpAttributes.Any(p => p.TargetAttribute == attribute))
        {
            return;
        }

        var powerUp = this._context.CreateNew<ItemBasePowerUpDefinition>();
        powerUp.TargetAttribute = attribute.GetPersistent(this._gameConfiguration);
        powerUp.BaseValue = value;
        powerUp.AggregateType = AggregateType.AddRaw;
        powerUp.BonusPerLevelTable = this._gameConfiguration.Items.SelectMany(i => i.BasePowerUpAttributes)
            .FirstOrDefault(p => p.TargetAttribute == attribute && p.BonusPerLevelTable is not null)?.BonusPerLevelTable;
        item.BasePowerUpAttributes.Add(powerUp);
    }
}
