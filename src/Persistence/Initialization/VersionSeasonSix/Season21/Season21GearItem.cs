// <copyright file="Season21GearItem.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.Interfaces;

/// <summary>
/// An item of Season 21 which a Season 6 character class can use.
/// </summary>
/// <param name="Group">The item group.</param>
/// <param name="Number">The item number.</param>
/// <param name="Name">The name.</param>
/// <param name="IntroducedIn">The version which introduced it; Season 21 Part 1, the version of the data, where it isn't known.</param>
/// <param name="Slot">The inventory slot of a piece of jewelry (9 pendant, 10 ring).</param>
/// <param name="DropLevel">The drop level.</param>
/// <param name="Durability">The durability.</param>
/// <param name="Drops">Whether monsters drop it.</param>
/// <param name="Excellent">The excellent options a piece of jewelry takes.</param>
/// <param name="HealthOption">Whether a piece of jewelry takes the option of automatic health recovery.</param>
/// <param name="Bans">What it may not do.</param>
/// <param name="Specials">The fixed bonuses as the Season 21 data numbers them: 129 attack and wizardry +N%, 97 attack speed +N.</param>
/// <param name="Boxes">The boxes whose excellent item drop holds it.</param>
/// <param name="Stats">The values of a weapon, shield, armor or wing; <c>null</c> for jewelry.</param>
internal sealed record Season21GearItem(
    byte Group,
    short Number,
    LocalizedString Name,
    GameVersion IntroducedIn,
    int Slot,
    byte DropLevel,
    byte Durability,
    bool Drops,
    Season21ExcellentOptions Excellent,
    bool HealthOption,
    Season21ItemBans Bans,
    (int Option, int Value)[] Specials,
    (byte Group, short Number)[] Boxes,
    Season21GearStats? Stats = null)
{
    /// <summary>
    /// Gets the maximum item level: +15 for gear, +4 for jewelry, and none for jewelry with fixed bonuses.
    /// </summary>
    public byte MaximumLevel => this.Stats is not null ? (byte)15 : this.Specials.Length > 0 ? (byte)0 : (byte)4;
}
