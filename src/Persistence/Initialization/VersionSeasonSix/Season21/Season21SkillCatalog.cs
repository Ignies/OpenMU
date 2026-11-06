// <copyright file="Season21SkillCatalog.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Text.Json;

/// <summary>
/// The skill data of Season 21, generated into the embedded resource <c>Resources/Season21/Skills.json</c> by
/// <c>Generator/season21-skills.mjs</c>, see Readme.md in this folder.
/// </summary>
internal static class Season21SkillCatalog
{
    private const string ResourceName = "MUnique.OpenMU.Persistence.Initialization.Resources.Season21.Skills.json";

    private static readonly Lazy<Season21SkillData> LazyData = new(Load);

    /// <summary>
    /// Gets the data.
    /// </summary>
    public static Season21SkillData Data => LazyData.Value;

    private static Season21SkillData Load()
    {
        using var stream = typeof(Season21SkillCatalog).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The resource {ResourceName} is missing.");
        return JsonSerializer.Deserialize<Season21SkillData>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException($"The resource {ResourceName} is empty.");
    }
}
