// <copyright file="AddSeason21SkillsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using System.Text.Json;
using MUnique.OpenMU.GameLogic.EnhanceTree;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the skills of Season 21: the skills of the nine new classes, the fourth and fifth evolution skills of the Season 6
/// classes, the master tree nodes Season 21 adds, the books which teach them and the skill enhancement tree. Optional:
/// a Season 6 world stays as it is until its owner applies it.
/// </summary>
/// <remarks>
/// Existing skills keep every value of the operator; they only get the classes of Season 21 the data names. An existing
/// configuration of the skill enhancement tree is kept. Fork feature ids <c>s21_skills</c> and <c>enhance_tree</c>.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21SkillsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21SkillsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("7F0B5C2E-4A1D-4E8B-9C3F-2D6A8E1B5F70")]
public class AddSeason21SkillsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21SkillsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21SkillsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 12, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21ClassesUpdatePlugIn).GUID, typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21SkillsInitializer(context, gameConfiguration).Initialize();

        var typeId = typeof(EnhanceTreeFeaturePlugIn).GUID;
        var plugIn = gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeId);
        if (plugIn is null)
        {
            plugIn = context.CreateNew<PlugInConfiguration>();
            plugIn.SetGuid(typeId);
            plugIn.TypeId = typeId;
            gameConfiguration.PlugInConfigurations.Add(plugIn);
        }

        EnhanceTreeConfiguration? existing;
        try
        {
            existing = plugIn.GetConfiguration<EnhanceTreeConfiguration>(new IdIgnoringReferenceHandler());
        }
        catch (JsonException)
        {
            // A configuration which can't be read is the operator's; it stays.
            return ValueTask.CompletedTask;
        }

        if (existing is not { Nodes.Count: > 0 })
        {
            plugIn.SetConfiguration(Season21SkillsInitializer.CreateEnhanceTreeConfiguration(), null);
            plugIn.IsActive = true;
        }

        return ValueTask.CompletedTask;
    }
}
