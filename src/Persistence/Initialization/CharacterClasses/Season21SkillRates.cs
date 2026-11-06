// <copyright file="Season21SkillRates.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The skill damage rates of the S21Emu skill calculation (<c>LuaCalc/Skill/SkillDamageCalc.lua</c>): three rates of a
/// class (attributes up to 500 %), and the skills which take a rate or a formula of their own instead of the class
/// multiplier, as <see cref="Stats.SkillFinalMultiplier"/> relationships of the skill.
/// </summary>
/// <remarks>
/// A hit index factor of the source (harsh strike, magic pin, oversting) is the one of the first hit, as OpenMU sends one
/// hit per use of these skills.
/// </remarks>
internal static class Season21SkillRates
{
    /// <summary>
    /// The highest rate, 500 % in the source.
    /// </summary>
    public const float MaximumRate = 5;

    /// <summary>
    /// The rate of the general skills of a caster (light wizard <c>KMDamageMultiplierRate</c>, lemuria mage
    /// <c>LMDamageMultiplierRate</c>, 200 % for the others). A fighter's general skills take its class multiplier.
    /// </summary>
    public static readonly Guid GeneralRateId = new("B3C1E0A2-5D47-4F8B-9E16-2A7C4D9F0E31");

    /// <summary>
    /// The first own rate of a class (rune wizard <c>RWDamageMultiplierRate</c>, grow lancer <c>GLDamageChastisementRate</c>).
    /// </summary>
    public static readonly Guid FirstRateId = new("C4D2F1B3-6E58-4A9C-8F27-3B8D5E0A1F42");

    /// <summary>
    /// The second own rate of a class (grow lancer <c>GLDamageAttackRageRate</c>).
    /// </summary>
    public static readonly Guid SecondRateId = new("D5E3A2C4-7F69-4BAD-9A38-4C9E6F1B2A53");

    /// <summary>
    /// Gets the inputs of a skill rate.
    /// </summary>
    public enum Input
    {
        /// <summary>The general rate of the class.</summary>
        General,

        /// <summary>The first rate of the class.</summary>
        First,

        /// <summary>The second rate of the class.</summary>
        Second,

        /// <summary>The vitality rate of the rage fighter (<c>RFDamageMultiplierRate[1]</c>).</summary>
        Vitality,

        /// <summary>The total strength.</summary>
        Strength,

        /// <summary>The total agility.</summary>
        Agility,

        /// <summary>The total energy.</summary>
        Energy,

        /// <summary>A constant.</summary>
        Constant,
    }

    /// <summary>
    /// Gets the rates of the skills by skill number.
    /// </summary>
    public static IReadOnlyDictionary<short, Rate[]> Skills { get; } = new Dictionary<short, Rate[]>
    {
        [240] = [new(Input.General)], // magical shot
        [241] = [new(Input.General)], // shining bird
        [242] = [new(Input.General)], // dragon violent
        [243] = [new(Input.General)], // spear storm
        [245] = [new(Input.General)], // marvel burst
        [246] = [new(Input.General)], // unleashed marvel
        [247] = [new(Input.General)], // ultimate force
        [2101] = [new(Input.First)], // rune sphere
        [2102] = [new(Input.First)], // rune praise
        [271] = [new(Input.Second)], // spin step
        [274] = [new(Input.Second, 0.8f)], // magic pin
        [275] = [new(Input.Constant)], // clash
        [276] = [new(Input.First)], // harsh strike
        [277] = [new(Input.First, 0.8f), new(Input.Second, 0.8f)], // shining peak
        [279] = [new(Input.First, 0.5f)], // breche
        [2085] = [new(Input.First)], // wild breche
        [2036] = [new(Input.Second)], // oversting
        [282] = [new(Input.Vitality), new(Input.Energy, 0.1f, IsDamageBonus: true)], // spirit hook
        [739] = [new(Input.Constant, 0.5f), new(Input.Agility, 0.001f)], // dark phoenix shot
        [2012] = [new(Input.Constant, 0.5f), new(Input.Strength, 0.001f)], // chaos blade
        [2013] = [new(Input.Constant, 2), new(Input.Energy, 0.0004f)], // havok spear
        [737] = [new(Input.Constant, 2), new(Input.Energy, 0.0004f)], // wind soul
    };

    /// <summary>
    /// Gets the attribute of a rate of the class, adds it to the configuration when it's missing.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <param name="id">The id, <see cref="GeneralRateId"/>, <see cref="FirstRateId"/> or <see cref="SecondRateId"/>.</param>
    /// <returns>The attribute.</returns>
    public static AttributeDefinition GetAttribute(IContext context, GameConfiguration gameConfiguration, Guid id)
    {
        if (gameConfiguration.Attributes.FirstOrDefault(a => a.Id == id) is { } existing)
        {
            return existing;
        }

        var (designation, description) = id == GeneralRateId
            ? ("Season 21 general skill rate", "The skill damage rate of a Season 21 caster's general skills.")
            : id == FirstRateId
                ? ("Season 21 first skill rate", "The first own skill damage rate of a Season 21 class.")
                : ("Season 21 second skill rate", "The second own skill damage rate of a Season 21 class.");
        var attribute = context.CreateNew<AttributeDefinition>(id, designation, description);
        attribute.MaximumValue = MaximumRate;
        gameConfiguration.Attributes.Add(attribute);
        return attribute;
    }

    /// <summary>
    /// Adds the rates of <see cref="Skills"/> to a skill which has neither a final multiplier nor a final damage bonus.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <param name="skill">The skill.</param>
    public static void AddTo(IContext context, GameConfiguration gameConfiguration, Skill skill)
    {
        if (!Skills.TryGetValue(skill.Number, out var rates)
            || skill.AttributeRelationships.Any(r => r.TargetAttribute == Stats.SkillFinalMultiplier || r.TargetAttribute == Stats.SkillFinalDamageBonus))
        {
            return;
        }

        foreach (var rate in rates)
        {
            var target = rate.IsDamageBonus ? Stats.SkillFinalDamageBonus : Stats.SkillFinalMultiplier;
            var (input, inputOperator) = rate.Source switch
            {
                Input.General => (GetAttribute(context, gameConfiguration, GeneralRateId), InputOperator.Multiply),
                Input.First => (GetAttribute(context, gameConfiguration, FirstRateId), InputOperator.Multiply),
                Input.Second => (GetAttribute(context, gameConfiguration, SecondRateId), InputOperator.Multiply),
                Input.Vitality => (Stats.VitalitySkillMultiplier, InputOperator.Multiply),
                Input.Strength => (Stats.TotalStrength, InputOperator.Multiply),
                Input.Agility => (Stats.TotalAgility, InputOperator.Multiply),
                Input.Energy => (Stats.TotalEnergy, InputOperator.Multiply),

                // Like the plasma storm of Season 6: the minimum of the maximum health and the constant is the constant.
                _ => (Stats.MaximumHealth, InputOperator.Minimum),
            };
            skill.AttributeRelationships.Add(CharacterClassHelper.CreateAttributeRelationship(context, gameConfiguration, target, rate.Factor, input, inputOperator));
        }
    }

    /// <summary>
    /// A rate of a skill: the input times the factor.
    /// </summary>
    /// <param name="Source">The input.</param>
    /// <param name="Factor">The factor, or the value of a constant.</param>
    /// <param name="IsDamageBonus">If set to <c>true</c>, it adds to the damage before the multiplier instead.</param>
    public sealed record Rate(Input Source, float Factor = 1, bool IsDamageBonus = false);
}
