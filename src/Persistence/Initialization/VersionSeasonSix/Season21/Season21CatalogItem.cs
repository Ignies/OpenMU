// <copyright file="Season21CatalogItem.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// An item of the Season 21 server data, as the item catalog (<c>Resources/Season21/Items.json</c>) holds it:
/// every column of <c>Item.xml</c> and what the option files say about it, see Readme.md in this folder.
/// </summary>
internal sealed class Season21CatalogItem
{
    /// <summary>
    /// Gets the item group (0-21).
    /// </summary>
    public byte Group { get; init; }

    /// <summary>
    /// Gets the item number.
    /// </summary>
    public short Number { get; init; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the key of the name in <see cref="Properties.Season21CatalogItemNames"/>.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Gets the version which introduced the item.
    /// </summary>
    public GameVersion Version { get; init; }

    /// <summary>
    /// Gets a value indicating whether the version is from the MU Online history; otherwise it's the version of the data.
    /// </summary>
    public bool VersionFromHistory { get; init; }

    /// <summary>
    /// Gets the inventory slot of the data: 0-11 as in Season 6, 236 pentagram, 237 and 238 earrings, -1 none.
    /// </summary>
    public int Slot { get; init; }

    /// <summary>
    /// Gets the number of the skill.
    /// </summary>
    public short Skill { get; init; }

    /// <summary>
    /// Gets the width.
    /// </summary>
    public byte Width { get; init; }

    /// <summary>
    /// Gets the height.
    /// </summary>
    public byte Height { get; init; }

    /// <summary>
    /// Gets a value indicating whether it's a two-handed weapon.
    /// </summary>
    public bool TwoHand { get; init; }

    /// <summary>
    /// Gets the drop level.
    /// </summary>
    public int DropLevel { get; init; }

    /// <summary>
    /// Gets the durability.
    /// </summary>
    public int Durability { get; init; }

    /// <summary>
    /// Gets the durability of a staff.
    /// </summary>
    public int MagicDurability { get; init; }

    /// <summary>
    /// Gets the minimum damage.
    /// </summary>
    public int DamageMin { get; init; }

    /// <summary>
    /// Gets the maximum damage.
    /// </summary>
    public int DamageMax { get; init; }

    /// <summary>
    /// Gets the attack speed.
    /// </summary>
    public int Speed { get; init; }

    /// <summary>
    /// Gets the walk speed of boots.
    /// </summary>
    public int WalkSpeed { get; init; }

    /// <summary>
    /// Gets the magic power.
    /// </summary>
    public int MagicPower { get; init; }

    /// <summary>
    /// Gets the defense.
    /// </summary>
    public int Defense { get; init; }

    /// <summary>
    /// Gets the defense rate.
    /// </summary>
    public int DefenseRate { get; init; }

    /// <summary>
    /// Gets the magic defense.
    /// </summary>
    public int MagicDefense { get; init; }

    /// <summary>
    /// Gets the required level, strength, agility, energy, vitality and command.
    /// </summary>
    public int[] Requirements { get; init; } = new int[6];

    /// <summary>
    /// Gets the class rank each class needs (0 none, 1 from the first class, 2 the second, 3 the third, 4 the fourth,
    /// 5 the fifth), in the order DW, DK, FE, MG, DL, SU, RF, GL, RW, SL, GC, KM, LM, IK, AL, CS.
    /// </summary>
    public int[] Classes { get; init; } = new int[16];

    /// <summary>
    /// Gets the kind of the data.
    /// </summary>
    public int KindA { get; init; }

    /// <summary>
    /// Gets the sub kind of the data.
    /// </summary>
    public int KindB { get; init; }

    /// <summary>
    /// Gets the item group column of the data.
    /// </summary>
    public int ItemGroup { get; init; }

    /// <summary>
    /// Gets the stat which an ancient item of it raises.
    /// </summary>
    public int SetStat { get; init; }

    /// <summary>
    /// Gets a value indicating whether it takes options.
    /// </summary>
    public bool TakesOptions { get; init; }

    /// <summary>
    /// Gets a value indicating whether the data lets monsters drop it.
    /// </summary>
    public bool Drops { get; init; }

    /// <summary>
    /// Gets what it may not do.
    /// </summary>
    public Season21ItemBans Bans { get; init; }

    /// <summary>
    /// Gets the stack size, 0 when it doesn't stack.
    /// </summary>
    public int Stack { get; init; }

    /// <summary>
    /// Gets a value indicating whether the data marks it as expensive.
    /// </summary>
    public bool Expensive { get; init; }

    /// <summary>
    /// Gets the legendary option of the data.
    /// </summary>
    public int Legendary { get; init; }

    /// <summary>
    /// Gets the resistances 0-6.
    /// </summary>
    public int[] Resistances { get; init; } = new int[7];

    /// <summary>
    /// Gets the value in zen.
    /// </summary>
    public int Value { get; init; }

    /// <summary>
    /// Gets the buy price of the data.
    /// </summary>
    public int BuyMoney { get; init; }

    /// <summary>
    /// Gets the highest item level which the item values of the data know.
    /// </summary>
    public byte MaximumLevel { get; init; }

    /// <summary>
    /// Gets the options of <c>ItemOption.xml</c>, each as type, index and value. Types: 0 skill, 1 luck, 2 and 3 option,
    /// 4-9 excellent, 10-14 special, 15 more.
    /// </summary>
    public int[][] Options { get; init; } = [];

    /// <summary>
    /// Gets the number of sockets.
    /// </summary>
    public int Sockets { get; init; }

    /// <summary>
    /// Gets a value indicating whether it takes the 380 (guardian) option.
    /// </summary>
    public bool Guardian { get; init; }

    /// <summary>
    /// Gets the mastery group (0 weapon, 1 armor) and tier (0 Bloodangel to 8 Apocalypse), or <c>null</c>.
    /// </summary>
    public int[]? Mastery { get; init; }

    /// <summary>
    /// Gets a value indicating whether it's a lucky item.
    /// </summary>
    public bool Lucky { get; init; }

    /// <summary>
    /// Gets the ancient sets of the data.
    /// </summary>
    public int[] AncientSets { get; init; } = [];

    /// <summary>
    /// Gets what the item waits for before a Season 6 client may get it, e.g. "group" for the groups above 15, or
    /// <c>null</c> when the Season 6 item format carries it.
    /// </summary>
    public string? WaitsFor { get; init; }

    /// <summary>
    /// Gets a value indicating whether it has an option of the given type.
    /// </summary>
    /// <param name="minimumType">The minimum type.</param>
    /// <param name="maximumType">The maximum type.</param>
    /// <returns><c>true</c>, if it has one.</returns>
    public bool HasOption(int minimumType, int maximumType) => this.Options.Any(o => o[0] >= minimumType && o[0] <= maximumType);
}
