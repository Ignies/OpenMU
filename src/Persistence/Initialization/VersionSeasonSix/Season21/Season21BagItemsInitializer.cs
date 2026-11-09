// <copyright file="Season21BagItemsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

/// <summary>
/// Gives the items of group 19 their uses: the contents of the item bags (S21Emu <c>EventItemBag</c> 294-297 and 308,
/// opened by <c>ItemBagConsumeHandlerPlugInBase</c>), the [Lvl. 0] lucky tickets in David's exchange, the pentagram
/// talisman in the elemental refinery and the effect of the Blessing of Eunice. What is set already stays.
/// </summary>
internal class Season21BagItemsInitializer
{
    private const byte Group = 19;

    /// <summary>
    /// The first [Lvl. 0] lucky ticket (1st Lucky Armor Ticket); the ten tickets follow the order of group 13 from 135.
    /// </summary>
    private const short FirstLevelZeroTicket = 4;

    private const short FirstSeason6Ticket = 135;

    private const int TicketCount = 10;

    private const short PentagramTalisman = 16;

    private const short BlessingOfEunice = 17;

    /// <summary>
    /// The effect of the Blessing of Eunice (S21Emu <c>Effect.txt</c> 447: 1800 seconds, value 50 %).
    /// </summary>
    private const short EuniceEffect = 447;

    /// <summary>
    /// The effect sub type of the Blessing of Eunice: its own, so it stacks with the seals and the other buffs.
    /// </summary>
    private const byte EuniceSubType = 200;

    private const int EuniceSeconds = 1800;

    private const float EuniceExperienceRate = 1.5f;

    /// <summary>
    /// The success bonus of the Elemental Talisman of Luck (S21Emu <c>ChaosMix.dat</c>, <c>ElementalTalismanOfLuckRate</c>).
    /// </summary>
    private const byte ElementalLuckPercent = 10;

    private static readonly string[] RefineryHandlers =
    [
        typeof(ElementalUpgradeCrafting).FullName!,
        typeof(ErrtelLevelUpCrafting).FullName!,
        typeof(ErrtelRankUpCrafting).FullName!,
    ];

    private static readonly int[] PandoraBox =
    [
        41,
    ];

    private static readonly int[] RedCard =
    [
        7281, 7238, 7239, 7301, 7392, 7431, 7432, 7328, 7329, 7377, 7221,
    ];

    private static readonly int[] BlueCard =
    [
        7281, 7238, 7239, 7301, 7392, 7431, 7432, 7328, 7329, 7377, 7221, 7280, 7253, 7254, 7255,
    ];

    private static readonly int[] PremiumMuunBox =
    [
        8192, 8194, 8196, 8198, 8200, 8202, 8204, 8206, 8208, 8210, 8212, 8214, 8216, 8218, 8220, 8222, 8224, 8226, 8228, 8230,
        8232, 8233, 8235, 8237, 8239, 8241, 8243, 8245, 8247, 8249, 8251, 8253, 8255, 8257, 8259, 8261, 8262, 8263, 8265, 8267,
        8269, 8271, 8273, 8275, 8277, 8279, 8281, 8283, 8285, 8287, 8289, 8291, 8293, 8295, 8297, 8299, 8300, 8301, 8302, 8304,
        8306, 8308, 8310, 8312, 8314, 8316, 8318, 8320, 8322, 8324, 8326, 8327, 8328, 8330, 8331, 8332, 8334, 8336, 8338, 8340,
        8342, 8404, 8406, 8408, 8410, 8415, 8417, 8419, 8421, 8423, 8425, 8427, 8429, 8433, 8435, 8437, 8444, 8446, 8448, 8450,
        8452, 8454, 8456, 8458, 8462, 8464, 8466, 8470, 8472, 8474, 8482, 8484, 8486, 8488, 8490, 8492, 8494, 8496, 8498, 8500,
        8502, 8504, 8506, 8508, 8510, 8512, 8514, 8516, 8518, 8520, 8522, 8524, 8526, 8528, 8530, 8532, 8534, 8536, 8538, 8540,
        8601, 8603, 8605, 8607, 8609, 8611, 8613, 8615, 8617, 8619, 8621, 8623, 8625, 8627, 8629, 8631, 8633, 8635, 8637, 8639,
        8641, 8643, 8645, 8647, 8649, 8651, 8653, 8672, 8674, 8676, 8678, 8680, 8682, 8684, 8686, 8688, 8690, 8692, 8694,
    ];

    private static readonly int[] ElementalPentagrams =
    [
        6346, 6347, 6350, 6351, 6352, 6353, 6354, 6355, 6356, 6357, 6358, 6361, 6450, 6451, 6452,
    ];

    private static readonly int[] ElementalErrtels =
    [
        6365, 6375, 6385, 6395, 6405, 6268, 6269, 6270, 6271, 6272, 6273,
    ];

    private static readonly int[] ElementalArmors =
    [
        3682, 4194, 4706, 5218, 5730, 3683, 4195, 4707, 5219, 5731, 3684, 4196, 4708, 5220, 5732, 3685, 4197, 4709, 5221, 5733,
        3687, 4199, 4711, 5223, 5735, 4198, 4710, 5222, 5734, 3688, 4200, 4712, 5736, 3689, 4201, 4713, 5225, 5737, 3706, 4218,
        4730, 5242, 5754, 3764, 4276, 4788, 5300, 5812, 3796, 4308, 4820, 5332, 5844,
    ];

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21BagItemsInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21BagItemsInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Applies the uses.
    /// </summary>
    public void Initialize()
    {
        this.AddBag(0, (PandoraBox, SpecialItemType.RandomItem, 0, ItemDropEffect.Fireworks));
        this.AddBag(1, (RedCard, SpecialItemType.RandomItem, 0, ItemDropEffect.Undefined));
        this.AddBag(2, (BlueCard, SpecialItemType.RandomItem, 0, ItemDropEffect.Undefined));
        this.AddBag(3, (PremiumMuunBox, SpecialItemType.RandomItem, 1, ItemDropEffect.Undefined));
        this.AddBag(
            14,
            (ElementalPentagrams, SpecialItemType.RandomItem, 0, ItemDropEffect.Undefined),
            (ElementalErrtels, SpecialItemType.RandomItem, 0, ItemDropEffect.Undefined),
            (ElementalArmors, SpecialItemType.Excellent, 0, ItemDropEffect.Undefined));
        this.AddLevelZeroTickets();
        this.AddTalismans();
        this.AddBlessingOfEunice();
    }

    private ItemDefinition? Find(byte group, short number)
        => this._gameConfiguration.Items.FirstOrDefault(i => i.Group == group && i.Number == number);

    private void AddBag(short number, params (int[] Items, SpecialItemType Type, byte Level, ItemDropEffect Effect)[] sections)
    {
        if (this.Find(Group, number) is not { } bag)
        {
            return;
        }

        bag.Durability = Math.Max(bag.Durability, (byte)1);
        if (bag.DropItems.Count > 0)
        {
            return;
        }

        foreach (var (items, type, level, effect) in sections)
        {
            var group = this._context.CreateNew<ItemDropItemGroup>();
            group.Description = bag.Name;
            group.ItemType = type;
            group.Chance = 1.0;
            group.MinimumLevel = level;
            group.MaximumLevel = level;
            group.DropEffect = effect;
            foreach (var code in items)
            {
                if (this.Find((byte)(code / 512), (short)(code % 512)) is { } item)
                {
                    group.PossibleItems.Add(item);
                }
            }

            bag.DropItems.Add(group);
        }
    }

    private void AddLevelZeroTickets()
    {
        var exchange = this._gameConfiguration.Monsters.SelectMany(m => m.ItemCraftings)
            .FirstOrDefault(c => c.ItemCraftingHandlerClassName == typeof(LuckyItemTicketExchangeCrafting).FullName)?.SimpleCraftingSettings;
        for (short i = 0; i < TicketCount && exchange is not null; i++)
        {
            if (this.Find(Group, (short)(FirstLevelZeroTicket + i)) is { } ticket
                && exchange.RequiredItems.FirstOrDefault(r => r.PossibleItems.Any(p => p.Group == 13 && p.Number == FirstSeason6Ticket + i)) is { } required
                && !required.PossibleItems.Contains(ticket))
            {
                required.PossibleItems.Add(ticket);
            }
        }
    }

    private void AddTalismans()
    {
        var chaos = new[] { this.Find(Group, PentagramTalisman), this.Find(14, 190) }.OfType<ItemDefinition>().ToList();
        var luck = this.Find(14, 189);
        foreach (var settings in this._gameConfiguration.Monsters.SelectMany(m => m.ItemCraftings)
                     .Where(c => RefineryHandlers.Contains(c.ItemCraftingHandlerClassName))
                     .Select(c => c.SimpleCraftingSettings).OfType<SimpleCraftingSettings>())
        {
            if (chaos.Count > 0 && settings.RequiredItems.All(r => !r.PossibleItems.Intersect(chaos).Any()))
            {
                settings.RequiredItems.Add(this.CreateTalisman(chaos, 0, MixResult.DisappearAndProtectReferencedItems));
            }

            if (luck is not null && settings.RequiredItems.All(r => !r.PossibleItems.Contains(luck)))
            {
                settings.RequiredItems.Add(this.CreateTalisman([luck], ElementalLuckPercent, MixResult.Disappear));
            }
        }
    }

    private ItemCraftingRequiredItem CreateTalisman(IEnumerable<ItemDefinition> talismans, byte addPercentage, MixResult failResult)
    {
        var requirement = this._context.CreateNew<ItemCraftingRequiredItem>();
        foreach (var talisman in talismans)
        {
            requirement.PossibleItems.Add(talisman);
        }

        requirement.MinimumAmount = 0;
        requirement.MaximumAmount = 1;
        requirement.AddPercentage = addPercentage;
        requirement.SuccessResult = MixResult.Disappear;
        requirement.FailResult = failResult;
        return requirement;
    }

    private void AddBlessingOfEunice()
    {
        if (this.Find(Group, BlessingOfEunice) is not { } blessing)
        {
            return;
        }

        blessing.Durability = Math.Max(blessing.Durability, (byte)1);
        if (this._gameConfiguration.MagicEffects.FirstOrDefault(e => e.Number == EuniceEffect) is not { } effect)
        {
            effect = this._context.CreateNew<MagicEffectDefinition>();
            effect.SetGuid(EuniceEffect);
            effect.Number = EuniceEffect;
            effect.Name = blessing.Name;
            this._gameConfiguration.MagicEffects.Add(effect);
        }

        blessing.ConsumeEffect ??= effect;
        if (effect.PowerUpDefinitions.Count > 0)
        {
            return;
        }

        effect.SubType = EuniceSubType;
        effect.SendDuration = true;
        effect.Duration ??= this._context.CreateNew<PowerUpDefinitionValue>();
        effect.Duration.ConstantValue.Value = EuniceSeconds;
        var powerUp = this._context.CreateNew<PowerUpDefinition>();
        powerUp.TargetAttribute = Stats.ExperienceRate.GetPersistent(this._gameConfiguration);
        powerUp.Boost = this._context.CreateNew<PowerUpDefinitionValue>();
        powerUp.Boost.ConstantValue.AggregateType = AggregateType.Multiplicate;
        powerUp.Boost.ConstantValue.Value = EuniceExperienceRate;
        effect.PowerUpDefinitions.Add(powerUp);
    }
}
