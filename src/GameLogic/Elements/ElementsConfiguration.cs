// <copyright file="ElementsConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Elements;

/// <summary>
/// Configuration of the <see cref="ElementsFeaturePlugIn"/>: the element wheel, the elemental damage rules and the
/// errtel removal rates. See Readme.md.
/// </summary>
public class ElementsConfiguration
{
    private float[,]? _wheel;

    private Dictionary<short, int>? _imprints;

    /// <summary>
    /// Gets or sets the damage multipliers of the element wheel: attacker element against defender element.
    /// A pair which isn't listed multiplies by 1.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ElementsConfiguration_Wheel_Name))]
    public List<ElementMultiplier> Wheel { get; set; } = CreateWheel();

    /// <summary>
    /// Gets or sets the multiplier of the elemental damage against a player (Webzen: "In PVP, elemental damage is increased by 30%").
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ElementsConfiguration_PvpMultiplier_Name))]
    [Range(0, 10)]
    public float PvpMultiplier { get; set; } = 1.3f;

    /// <summary>
    /// Gets or sets the chance of an elemental hit when the elemental defense rate of the target is not below the
    /// elemental attack rate of the attacker, like <c>GetHitChanceTo</c> of the normal hit.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ElementsConfiguration_MinimumHitChance_Name))]
    [Range(0, 1)]
    public float MinimumHitChance { get; set; } = 0.03f;

    /// <summary>
    /// Gets or sets the element imprint bonuses of the skills (S21Emu <c>ElementImprintBonus</c>, percent): a hit with
    /// one of these skills multiplies its elemental damage. A skill which isn't listed has 100 %.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ElementsConfiguration_SkillImprints_Name))]
    public List<SkillImprint> SkillImprints { get; set; } = [];

    /// <summary>
    /// Gets or sets the chance in 1/10000 to take an errtel out of a pentagram undamaged, by its level and rank
    /// (PentagramJewelMixRate.txt). On failure the errtel is destroyed.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ElementsConfiguration_RemovalRates_Name))]
    public List<ErrtelRemovalRate> RemovalRates { get; set; } = [];

    /// <summary>
    /// Gets or sets the level ups of the errtels by level.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ElementsConfiguration_LevelUps_Name))]
    public List<ErrtelLevelUp> LevelUps { get; set; } = [];

    /// <summary>
    /// Gets or sets the rank ups of the errtels by rank.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ElementsConfiguration_RankUps_Name))]
    public List<ErrtelRankUp> RankUps { get; set; } = [];

    /// <summary>
    /// Creates the element wheel of Webzen's guide (https://muonline.webzen.com/en/events/EX700Guide/pop_attribute).
    /// </summary>
    /// <returns>The wheel.</returns>
    /// <remarks>
    /// The guide's table gives wind against wind 120 %; every other element deals 100 % to its own element, so the
    /// wheel takes 100 % there too (the guide of muonline.ai prints 100 %).
    /// </remarks>
    public static List<ElementMultiplier> CreateWheel()
    {
        // Rows: the attacker; columns: fire, water, earth, wind, darkness, none.
        float[][] table =
        [
            [1.0f, 0.8f, 0.9f, 0.9f, 1.2f, 1.2f],
            [1.2f, 1.0f, 0.8f, 0.8f, 1.1f, 1.2f],
            [1.1f, 1.2f, 1.0f, 1.0f, 0.9f, 1.2f],
            [0.9f, 1.1f, 1.2f, 1.0f, 0.8f, 1.2f],
            [0.8f, 0.9f, 1.1f, 1.1f, 1.0f, 1.2f],
        ];
        Element[] columns = [Element.Fire, Element.Water, Element.Earth, Element.Wind, Element.Darkness, Element.None];
        var wheel = new List<ElementMultiplier>();
        for (var row = 0; row < table.Length; row++)
        {
            for (var column = 0; column < columns.Length; column++)
            {
                wheel.Add(new ElementMultiplier { Attacker = (Element)(row + 1), Defender = columns[column], Multiplier = table[row][column] });
            }
        }

        return wheel;
    }

    /// <summary>
    /// Gets the multiplier of the wheel for an attacker element against a defender element.
    /// </summary>
    /// <param name="attacker">The element of the attacker.</param>
    /// <param name="defender">The element of the defender.</param>
    /// <returns>The multiplier; 1 for a pair which isn't configured.</returns>
    public float GetMultiplier(Element attacker, Element defender)
    {
        if (this._wheel is null)
        {
            var size = (int)Element.Random;
            var wheel = new float[size, size];
            for (var a = 0; a < size; a++)
            {
                for (var d = 0; d < size; d++)
                {
                    wheel[a, d] = 1;
                }
            }

            foreach (var cell in this.Wheel.Where(c => c.Attacker < Element.Random && c.Defender < Element.Random))
            {
                wheel[(int)cell.Attacker, (int)cell.Defender] = cell.Multiplier;
            }

            this._wheel = wheel;
        }

        return this._wheel[(int)attacker, (int)defender];
    }

    /// <summary>
    /// Gets the element imprint bonus of a skill.
    /// </summary>
    /// <param name="skillNumber">The skill number; <c>null</c> for a hit without skill.</param>
    /// <returns>The percentage.</returns>
    public int GetImprintPercent(short? skillNumber)
    {
        const int noImprint = 100;
        if (skillNumber is not { } number)
        {
            return noImprint;
        }

        this._imprints ??= this.SkillImprints.GroupBy(i => i.SkillNumber).ToDictionary(g => g.Key, g => g.First().Percent);
        return this._imprints.GetValueOrDefault(number, noImprint);
    }
}
