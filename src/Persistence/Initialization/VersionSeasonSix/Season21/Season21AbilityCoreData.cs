// <copyright file="Season21AbilityCoreData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// The Ability Enhancement Core data of the Season 21 server data, generated into the embedded resource
/// <c>Resources/Season21/AbilityCore.json</c> by <c>Generator/season21-ability.mjs</c>: the cards with their options and
/// the values by card level, the level ups and the reductions.
/// </summary>
internal sealed class Season21AbilityCoreData
{
    private const string ResourceName = "MUnique.OpenMU.Persistence.Initialization.Resources.Season21.AbilityCore.json";

    private static readonly Lazy<Season21AbilityCoreData> LazyData = new(Load);

    /// <summary>
    /// Gets the data.
    /// </summary>
    public static Season21AbilityCoreData Data => LazyData.Value;

    /// <summary>
    /// Gets or sets the cards.
    /// </summary>
    [JsonPropertyName("cards")]
    public List<CardRow> Cards { get; set; } = [];

    /// <summary>
    /// Gets or sets the level ups.
    /// </summary>
    [JsonPropertyName("levelUps")]
    public List<PriceRow> LevelUps { get; set; } = [];

    /// <summary>
    /// Gets or sets the reductions.
    /// </summary>
    [JsonPropertyName("reductions")]
    public List<PriceRow> Reductions { get; set; } = [];

    private static Season21AbilityCoreData Load()
    {
        using var stream = typeof(Season21AbilityCoreData).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The resource {ResourceName} is missing.");
        return JsonSerializer.Deserialize<Season21AbilityCoreData>(stream)
            ?? throw new InvalidOperationException($"The resource {ResourceName} is empty.");
    }

    /// <summary>
    /// A card (AbilityOption.xml, AbilityParticleSkill.xml).
    /// </summary>
    /// <param name="Group">The item group.</param>
    /// <param name="Number">The item number.</param>
    /// <param name="Kind">The kind: 1 skill, 2 option, 3 elite, 4 extra damage.</param>
    /// <param name="Color">The color: 0 none, 1 blue, 2 green, 3 purple.</param>
    /// <param name="Type">The option card type 1-5, or the skill card type 0-1.</param>
    /// <param name="Skill">The fifth evolution skill of a skill card.</param>
    /// <param name="RequiredSkill">The fourth evolution skill a skill card needs.</param>
    /// <param name="Classes">The classes of a skill card, a bit per class; null for every class.</param>
    /// <param name="Static">The static option of a skill card.</param>
    /// <param name="Options">The options.</param>
    public sealed record CardRow(
        [property: JsonPropertyName("g")] byte Group,
        [property: JsonPropertyName("n")] short Number,
        [property: JsonPropertyName("kind")] byte Kind,
        [property: JsonPropertyName("color")] byte Color,
        [property: JsonPropertyName("type")] byte Type,
        [property: JsonPropertyName("skill")] short Skill,
        [property: JsonPropertyName("req")] short RequiredSkill,
        [property: JsonPropertyName("classes")] int? Classes,
        [property: JsonPropertyName("st")] StaticRow? Static,
        [property: JsonPropertyName("options")] OptionRow[] Options);

    /// <summary>
    /// The static option of a skill card (AbilityParticleSkill.xml).
    /// </summary>
    /// <param name="Id">The option: 0 hits, 1 attack speed, 2 range, 3 splash damage, 4 enemy defense.</param>
    /// <param name="Values">The values by card level 0 to 5.</param>
    public sealed record StaticRow(
        [property: JsonPropertyName("id")] byte Id,
        [property: JsonPropertyName("v")] float[] Values);

    /// <summary>
    /// An option of a card.
    /// </summary>
    /// <param name="Number">The static option index.</param>
    /// <param name="Name">The name (client ParticleOptionText).</param>
    /// <param name="IsRate">A value indicating whether the value is a rate in percent.</param>
    /// <param name="Classes">The classes it takes effect for, a bit per class (AbilityParticleBook.xml).</param>
    /// <param name="Values">The values by card level 0 to 5.</param>
    /// <param name="Chance">The chance of a skill card option in millionths (AbilityAddOption.xml).</param>
    public sealed record OptionRow(
        [property: JsonPropertyName("o")] short Number,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("rate")] bool IsRate,
        [property: JsonPropertyName("classes")] int Classes,
        [property: JsonPropertyName("v")] float[] Values,
        [property: JsonPropertyName("chance")] int Chance);

    /// <summary>
    /// A level up (AbilityLevelUP.xml) or reduction (AbilityReduction.xml) of a card kind.
    /// </summary>
    /// <param name="Kind">The card kind.</param>
    /// <param name="Level">The card level before the level up.</param>
    /// <param name="Cards">The other cards a level up takes.</param>
    /// <param name="Money">The zen.</param>
    /// <param name="StoneGroup">The item group of the stones.</param>
    /// <param name="StoneNumber">The item number of the stones.</param>
    /// <param name="Stones">The stones a level up takes or a reduction gives.</param>
    public sealed record PriceRow(
        [property: JsonPropertyName("kind")] byte Kind,
        [property: JsonPropertyName("level")] byte Level,
        [property: JsonPropertyName("cards")] byte Cards,
        [property: JsonPropertyName("money")] int Money,
        [property: JsonPropertyName("sg")] byte StoneGroup,
        [property: JsonPropertyName("sn")] short StoneNumber,
        [property: JsonPropertyName("stones")] int Stones);
}
