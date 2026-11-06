// <copyright file="Season21ElementData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Text.Json;

/// <summary>
/// The element data of Season 21 (<c>Resources/Season21/Elements.json</c>, written by <c>Generator/season21-elements.mjs</c>).
/// </summary>
internal sealed class Season21ElementData
{
    private const string ResourceName = "MUnique.OpenMU.Persistence.Initialization.Resources.Season21.Elements.json";

    private static readonly Lazy<Season21ElementData> LazyInstance = new(Load);

    /// <summary>
    /// Gets the data.
    /// </summary>
    public static Season21ElementData Instance => LazyInstance.Value;

    /// <summary>
    /// Gets the pentagrams (Item.xml, slot 236).
    /// </summary>
    public List<PentagramRow> Pentagrams { get; init; } = [];

    /// <summary>
    /// Gets the errtels with their rank options (PentagramJewelOption.xml).
    /// </summary>
    public List<ErrtelRow> Errtels { get; init; } = [];

    /// <summary>
    /// Gets the refinery data.
    /// </summary>
    public RefineryRows Refinery { get; init; } = new();

    /// <summary>
    /// Gets the element columns of the monsters (Monster.xml), only rows with a value.
    /// </summary>
    public List<MonsterRow> Monsters { get; init; } = [];

    /// <summary>
    /// Gets the element imprint bonuses of the skills which aren't 100 % (Skill.xml).
    /// </summary>
    public List<SkillImprintRow> SkillImprints { get; init; } = [];

    /// <summary>
    /// Gets the earring data (EarringItemOption.txt).
    /// </summary>
    public EarringRows Earrings { get; init; } = new();

    private static Season21ElementData Load()
    {
        using var stream = typeof(Season21ElementData).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The resource {ResourceName} is missing.");
        return JsonSerializer.Deserialize<Season21ElementData>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException($"The resource {ResourceName} is empty.");
    }

    /// <summary>A pentagram.</summary>
    /// <param name="Number">The item number (group 12).</param>
    /// <param name="DamageMin">The minimum elemental damage.</param>
    /// <param name="DamageMax">The maximum elemental damage.</param>
    /// <param name="Defense">The elemental defense.</param>
    /// <param name="AttackRate">The elemental attack rate.</param>
    /// <param name="DefenseRate">The elemental defense rate.</param>
    /// <param name="Options">The pentagram options (not applied yet, see Readme.md).</param>
    internal sealed record PentagramRow(short Number, int DamageMin, int DamageMax, int Defense, int AttackRate, int DefenseRate, int[] Options);

    /// <summary>An errtel.</summary>
    /// <param name="Number">The item number (group 12).</param>
    /// <param name="Options">The rank options.</param>
    internal sealed record ErrtelRow(short Number, List<ErrtelOptionRow> Options);

    /// <summary>A rank option of an errtel.</summary>
    /// <param name="Number">The option index of the source.</param>
    /// <param name="Rank">The rank.</param>
    /// <param name="Rate">The chance among the options of the rank (1/10000).</param>
    /// <param name="Name">The name of the source.</param>
    /// <param name="Values">The values of the levels 0 to 10.</param>
    internal sealed record ErrtelOptionRow(short Number, int Rank, int Rate, string Name, int[] Values);

    /// <summary>The element columns of a monster.</summary>
    /// <param name="Number">The monster number.</param>
    /// <param name="Element">The element (6 random).</param>
    /// <param name="Defense">The elemental defense.</param>
    /// <param name="DamageMin">The minimum elemental damage.</param>
    /// <param name="DamageMax">The maximum elemental damage.</param>
    /// <param name="AttackRate">The elemental attack rate.</param>
    /// <param name="DefenseRate">The elemental defense rate.</param>
    internal sealed record MonsterRow(short Number, int Element, int Defense, int DamageMin, int DamageMax, int AttackRate, int DefenseRate);

    /// <summary>The element imprint of a skill.</summary>
    /// <param name="Skill">The skill number.</param>
    /// <param name="Percent">The percentage.</param>
    internal sealed record SkillImprintRow(short Skill, int Percent);

    /// <summary>The refinery data.</summary>
    internal sealed class RefineryRows
    {
        /// <summary>Gets the chances in 1/10000 to take an errtel out of a pentagram, by level and rank.</summary>
        public List<RemovalRateRow> RemovalRates { get; init; } = [];

        /// <summary>Gets the upgrades of the pentagrams to their mastery pentagram.</summary>
        public List<UpgradeRow> PentagramUpgrades { get; init; } = [];

        /// <summary>Gets the upgrades of the errtels to their mastery errtel.</summary>
        public List<UpgradeRow> ErrtelUpgrades { get; init; } = [];

        /// <summary>Gets the chances in percent of an errtel rank up, by rank and level.</summary>
        public List<RankUpRateRow> RankUpRates { get; init; } = [];

        /// <summary>Gets the money of an errtel rank up, by rank.</summary>
        public List<RankUpMoneyRow> RankUpMoney { get; init; } = [];

        /// <summary>Gets the errtel level ups, by level: the money and per rank the chance in percent and the elemental runes.</summary>
        public List<LevelUpRow> LevelUp { get; init; } = [];
    }

    /// <summary>An upgrade to a mastery item.</summary>
    /// <param name="Main">The item number of the upgraded item (group 12).</param>
    /// <param name="Result">The item number of the result (group 12).</param>
    /// <param name="Materials">The materials: group, number and amount (0 for one).</param>
    internal sealed record UpgradeRow(short Main, short Result, int[][] Materials);

    /// <summary>The rank up chances of a rank.</summary>
    /// <param name="Rank">The rank before.</param>
    /// <param name="Rates">The chances by level.</param>
    internal sealed record RankUpRateRow(int Rank, int[] Rates);

    /// <summary>The rank up money of a rank.</summary>
    /// <param name="Rank">The rank before.</param>
    /// <param name="Money">The money.</param>
    internal sealed record RankUpMoneyRow(int Rank, int Money);

    /// <summary>The level up of a level.</summary>
    /// <param name="Level">The level before.</param>
    /// <param name="Money">The money.</param>
    /// <param name="Ranks">Per rank: the chance in percent and the elemental runes.</param>
    internal sealed record LevelUpRow(int Level, int Money, int[][] Ranks);

    /// <summary>The removal chances of one errtel level.</summary>
    /// <param name="Level">The level.</param>
    /// <param name="Rates">The chances by rank.</param>
    internal sealed record RemovalRateRow(int Level, int[] Rates);

    /// <summary>The earring data.</summary>
    internal sealed class EarringRows
    {
        /// <summary>Gets the chances (1/10000) of an earring to have 1 to 5 options.</summary>
        public int[] OptionCountRates { get; init; } = [];

        /// <summary>Gets the earrings.</summary>
        public List<EarringRow> Items { get; init; } = [];

        /// <summary>Gets the link options of a pair of the same tier.</summary>
        public List<EarringLinkRow> Links { get; init; } = [];

        /// <summary>Gets the option values by tier and value index.</summary>
        public List<EarringValueRow> Values { get; init; } = [];

        /// <summary>Gets the value of a tier and value index.</summary>
        /// <param name="tier">The tier.</param>
        /// <param name="index">The value index.</param>
        /// <returns>The value; 0 if the source has none.</returns>
        public int GetValue(int tier, int index) => this.Values.FirstOrDefault(v => v.Tier == tier && v.Index == index)?.Value ?? 0;
    }

    /// <summary>An earring.</summary>
    /// <param name="Number">The item number (group 12).</param>
    /// <param name="Tier">The tier (0 to 7).</param>
    /// <param name="Options">The options it may get.</param>
    internal sealed record EarringRow(short Number, int Tier, List<EarringOptionRow> Options);

    /// <summary>An option of an earring.</summary>
    /// <param name="Option">The option index of the source.</param>
    /// <param name="ValueIndex">The index into the values of the tier.</param>
    /// <param name="Rate">The chance among the options (1/10000).</param>
    internal sealed record EarringOptionRow(short Option, int ValueIndex, int Rate);

    /// <summary>The link options of a tier.</summary>
    /// <param name="Tier">The tier.</param>
    /// <param name="Options">The options.</param>
    internal sealed record EarringLinkRow(int Tier, List<EarringOptionRow> Options);

    /// <summary>A value of a tier.</summary>
    /// <param name="Tier">The tier.</param>
    /// <param name="Index">The value index.</param>
    /// <param name="Value">The value.</param>
    internal sealed record EarringValueRow(int Tier, int Index, int Value);
}
