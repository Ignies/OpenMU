// <copyright file="Season21ElementalFormulas.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The elemental stats of a class from its stats, as the S21Emu calculation defines them
/// (<c>Data/Scripts/LuaCalc/Character/Calculations/Elemental.lua</c>, version S21.1-1). The pentagram adds to them.
/// </summary>
/// <param name="Defense">The elemental defense.</param>
/// <param name="DamageMin">The minimum elemental damage.</param>
/// <param name="DamageMax">The maximum elemental damage.</param>
/// <param name="AttackRate">The elemental attack rate.</param>
/// <param name="DefenseRate">The elemental defense rate.</param>
internal sealed record Season21ElementalFormulas(
    Season21StatFactors Defense,
    Season21StatFactors DamageMin,
    Season21StatFactors DamageMax,
    Season21StatFactors AttackRate,
    Season21StatFactors DefenseRate)
{
    private static readonly Season21StatFactors AttackRateOfMost = new(0.25f, 1.5f, Level: 5);

    private static readonly Season21StatFactors AttackRateOfAgility = new(0.5f, 4.0f / 3, Level: 5);

    private static readonly Season21ElementalFormulas Wizard = new(new(Agility: 1.0f / 4), new(Energy: 1.0f / 9), new(Energy: 1.0f / 6), AttackRateOfMost, new(Agility: 1.0f / 3));

    private static readonly Season21ElementalFormulas Agile = new(new(Agility: 1.0f / 10), new(1.0f / 10, 1.0f / 5), new(1.0f / 6, 1.0f / 3), AttackRateOfAgility, new(Agility: 1.0f / 4));

    /// <summary>
    /// Gets the formulas of every base class.
    /// </summary>
    public static IReadOnlyDictionary<BaseClassCode, Season21ElementalFormulas> Classes { get; } = new Dictionary<BaseClassCode, Season21ElementalFormulas>
    {
        [BaseClassCode.DarkWizard] = Wizard,
        [BaseClassCode.DarkKnight] = new(new(Agility: 1.0f / 3), new(1.0f / 6), new(1.0f / 4), AttackRateOfMost, new(Agility: 1.0f / 3)),
        [BaseClassCode.FairyElf] = new(new(Agility: 1.0f / 10), new(1.0f / 14, 1.0f / 8), new(1.0f / 8, 1.0f / 4), AttackRateOfMost, new(Agility: 1.0f / 4)),
        [BaseClassCode.MagicGladiator] = new(new(Agility: 1.0f / 5), new(1.0f / 10, Energy: 1.0f / 14), new(1.0f / 8, Energy: 1.0f / 4), AttackRateOfMost, new(Agility: 1.0f / 3)),
        [BaseClassCode.DarkLord] = new(new(Agility: 1.0f / 7), new(1.0f / 10, Energy: 1.0f / 14), new(1.0f / 6, Energy: 1.0f / 10), AttackRateOfMost, new(Agility: 1.0f / 7)),
        [BaseClassCode.Summoner] = new(new(Agility: 1.0f / 3), new(Energy: 1.0f / 10), new(Energy: 1.0f / 6), AttackRateOfMost, new(Agility: 1.0f / 4)),
        [BaseClassCode.RageFighter] = new(new(Agility: 1.0f / 8), new(1.0f / 10, Vitality: 1.0f / 20), new(1.0f / 6, Vitality: 1.0f / 15), AttackRateOfMost, new(Agility: 1.0f / 10)),
        [BaseClassCode.GrowLancer] = Agile with { Defense = new(Agility: 1.0f / 7) },
        [BaseClassCode.RuneWizard] = Wizard,
        [BaseClassCode.Slayer] = Agile,
        [BaseClassCode.GunCrusher] = Wizard,
        [BaseClassCode.LightWizard] = Wizard,
        [BaseClassCode.LemuriaMage] = Wizard,
        [BaseClassCode.IllusionKnight] = Agile,
        [BaseClassCode.Alchemist] = new(new(Agility: 1.0f / 3), new(Energy: 1.0f / 6), new(Energy: 1.0f / 4), AttackRateOfMost, new(Agility: 1.0f / 3)),
        [BaseClassCode.Crusader] = new(new(Agility: 1.0f / 3), new(1.0f / 7), new(1.0f / 5), AttackRateOfMost, new(Agility: 1.0f / 3)),
    };
}
