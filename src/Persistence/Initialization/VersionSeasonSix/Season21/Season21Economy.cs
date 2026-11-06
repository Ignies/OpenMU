// <copyright file="Season21Economy.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// The economy data of the Season 21 server data, generated into the embedded resource <c>Resources/Season21/Economy.json</c>
/// by <c>Generator/season21-economy.mjs</c>: the NPC shops with their tabs, the Ruud shop and the sources of Ruud.
/// </summary>
internal sealed class Season21Economy
{
    private const string ResourceName = "MUnique.OpenMU.Persistence.Initialization.Resources.Season21.Economy.json";

    private static readonly Lazy<Season21Economy> LazyData = new(Load);

    /// <summary>
    /// Gets the data.
    /// </summary>
    public static Season21Economy Data => LazyData.Value;

    /// <summary>
    /// Gets or sets the items of the NPC shops.
    /// </summary>
    [JsonPropertyName("shops")]
    public List<ShopRow> Shops { get; set; } = [];

    /// <summary>
    /// Gets or sets the items of the Ruud shop.
    /// </summary>
    [JsonPropertyName("ruudShop")]
    public List<RuudShopRow> RuudShop { get; set; } = [];

    /// <summary>
    /// Gets or sets the Ruud of monsters.
    /// </summary>
    [JsonPropertyName("ruudMonsters")]
    public List<RuudMonsterRow> RuudMonsters { get; set; } = [];

    /// <summary>
    /// Gets or sets the Ruud of the Ruud boxes.
    /// </summary>
    [JsonPropertyName("ruudBoxes")]
    public List<RuudBoxRow> RuudBoxes { get; set; } = [];

    /// <summary>
    /// Gets or sets the level rewards.
    /// </summary>
    [JsonPropertyName("levelRewards")]
    public List<LevelRewardRow> LevelRewards { get; set; } = [];

    /// <summary>
    /// Gets or sets the boxes of the event contribution.
    /// </summary>
    [JsonPropertyName("contributionBoxes")]
    public List<ContributionBoxRow> ContributionBoxes { get; set; } = [];

    /// <summary>
    /// Gets or sets the points of the event contribution per event.
    /// </summary>
    [JsonPropertyName("contributionRules")]
    public List<ContributionRuleRow> ContributionRules { get; set; } = [];

    private static Season21Economy Load()
    {
        using var stream = typeof(Season21Economy).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The resource {ResourceName} is missing.");
        return JsonSerializer.Deserialize<Season21Economy>(stream)
            ?? throw new InvalidOperationException($"The resource {ResourceName} is empty.");
    }

    /// <summary>
    /// An item of an NPC shop.
    /// </summary>
    /// <param name="Npc">The number of the merchant NPC.</param>
    /// <param name="Tab">The tab, 0 to 4.</param>
    /// <param name="Group">The item group.</param>
    /// <param name="Number">The item number.</param>
    /// <param name="Level">The item level.</param>
    /// <param name="Durability">The durability (count of a stack); 0 for the durability of the definition.</param>
    /// <param name="Skill">1, if the item has its skill.</param>
    /// <param name="Luck">1, if the item has luck.</param>
    /// <param name="Option">The level of the additional option.</param>
    internal sealed record ShopRow(
        [property: JsonPropertyName("n")] short Npc,
        [property: JsonPropertyName("t")] byte Tab,
        [property: JsonPropertyName("g")] byte Group,
        [property: JsonPropertyName("i")] short Number,
        [property: JsonPropertyName("l")] byte Level,
        [property: JsonPropertyName("d")] byte Durability,
        [property: JsonPropertyName("s")] byte Skill,
        [property: JsonPropertyName("k")] byte Luck,
        [property: JsonPropertyName("o")] byte Option);

    /// <summary>
    /// An item of the Ruud shop.
    /// </summary>
    /// <param name="Class">The base class (0-15) which may buy it; <c>null</c> for every class.</param>
    /// <param name="Group">The item group.</param>
    /// <param name="Number">The item number.</param>
    /// <param name="Level">The item level.</param>
    /// <param name="Count">The count; 0 for the durability of the definition.</param>
    /// <param name="LuckChance">The chance of luck in percent.</param>
    /// <param name="OptionChance">The chance of the additional option in percent.</param>
    /// <param name="OptionLevel">The highest level of the additional option.</param>
    /// <param name="Price">The price in Ruud.</param>
    internal sealed record RuudShopRow(
        [property: JsonPropertyName("c")] byte? Class,
        [property: JsonPropertyName("g")] byte Group,
        [property: JsonPropertyName("i")] short Number,
        [property: JsonPropertyName("l")] byte Level,
        [property: JsonPropertyName("n")] byte Count,
        [property: JsonPropertyName("lc")] byte LuckChance,
        [property: JsonPropertyName("oc")] byte OptionChance,
        [property: JsonPropertyName("ol")] byte OptionLevel,
        [property: JsonPropertyName("p")] int Price);

    /// <summary>
    /// The Ruud of a monster.
    /// </summary>
    /// <param name="Monster">The monster number.</param>
    /// <param name="Minimum">The minimum amount.</param>
    /// <param name="Maximum">The maximum amount.</param>
    internal sealed record RuudMonsterRow(
        [property: JsonPropertyName("m")] short Monster,
        [property: JsonPropertyName("min")] int Minimum,
        [property: JsonPropertyName("max")] int Maximum);

    /// <summary>
    /// The Ruud of a Ruud box.
    /// </summary>
    /// <param name="Group">The item group.</param>
    /// <param name="Number">The item number.</param>
    /// <param name="Amount">The amount.</param>
    internal sealed record RuudBoxRow(
        [property: JsonPropertyName("g")] byte Group,
        [property: JsonPropertyName("i")] short Number,
        [property: JsonPropertyName("a")] int Amount);

    /// <summary>
    /// A level reward.
    /// </summary>
    /// <param name="Id">The id (the data's rewardID plus 1).</param>
    /// <param name="TotalLevel">The total level.</param>
    /// <param name="Group">The item group.</param>
    /// <param name="Number">The item number.</param>
    /// <param name="Level">The item level.</param>
    /// <param name="Count">The count; 0 for the durability of the definition.</param>
    /// <param name="Classes">The base classes; empty for every class.</param>
    internal sealed record LevelRewardRow(
        [property: JsonPropertyName("id")] byte Id,
        [property: JsonPropertyName("lv")] int TotalLevel,
        [property: JsonPropertyName("g")] byte Group,
        [property: JsonPropertyName("i")] short Number,
        [property: JsonPropertyName("l")] byte Level,
        [property: JsonPropertyName("n")] byte Count,
        [property: JsonPropertyName("c")] List<byte> Classes);

    /// <summary>
    /// A box of the event contribution.
    /// </summary>
    /// <param name="Group">The item group.</param>
    /// <param name="Number">The item number.</param>
    /// <param name="Count">The count.</param>
    /// <param name="Minimum">The least points.</param>
    /// <param name="Maximum">The most points; 0 for no bound.</param>
    internal sealed record ContributionBoxRow(
        [property: JsonPropertyName("g")] byte Group,
        [property: JsonPropertyName("i")] short Number,
        [property: JsonPropertyName("n")] byte Count,
        [property: JsonPropertyName("min")] int Minimum,
        [property: JsonPropertyName("max")] int Maximum);

    /// <summary>
    /// The points of an event.
    /// </summary>
    /// <param name="Type">The name of the <see cref="MiniGameType"/>.</param>
    /// <param name="MonsterKill">The points of a killed monster.</param>
    /// <param name="SundayMonsterKill">The points of a killed monster on Sundays; 0 for the same.</param>
    /// <param name="Winner">The points of the winner.</param>
    /// <param name="PlayerKill">The points of a killed player.</param>
    /// <param name="RankPoints">The points per rank, rank 1 first.</param>
    /// <param name="RelicClaim">The points of taking a relic.</param>
    /// <param name="RelicScore">The points of delivering a relic.</param>
    /// <param name="SpecialMonsters">Pairs of a monster number and its points.</param>
    internal sealed record ContributionRuleRow(
        [property: JsonPropertyName("t")] string Type,
        [property: JsonPropertyName("m")] int MonsterKill,
        [property: JsonPropertyName("sun")] int SundayMonsterKill,
        [property: JsonPropertyName("w")] int Winner,
        [property: JsonPropertyName("p")] int PlayerKill,
        [property: JsonPropertyName("r")] List<int> RankPoints,
        [property: JsonPropertyName("rc")] int RelicClaim,
        [property: JsonPropertyName("rs")] int RelicScore,
        [property: JsonPropertyName("s")] List<int[]> SpecialMonsters);
}
