// <copyright file="AddSeason21AbilityCoreUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using System.Text.Json;
using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the Ability Enhancement Core of Season 21 (<see cref="Season21AbilityCoreInitializer"/>): the options of the
/// 45 ability cards and the configuration of the core, which it turns on. Optional: a Season 6 world stays as it is
/// until its owner applies it. Fork feature id <c>ability_core</c>.
/// </summary>
/// <remarks>
/// Existing card options and an existing configuration of the core (and its on/off switch) are the operator's; they stay.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21AbilityCoreUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21AbilityCoreUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("A6C3E9F1-5B27-4D80-9E14-2F7B8C3D6A95")]
public class AddSeason21AbilityCoreUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21AbilityCoreUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21AbilityCoreUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    /// <remarks>
    /// Version 2 adds the card craftings at the chaos goblin; version 3 gives the extra defense and pentagram options
    /// their attributes where an option has none.
    /// </remarks>
    public override int Version => 3;

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 11, 07, 22, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 07, 14, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21AbilityCoreInitializer(context, gameConfiguration).Initialize();
        var typeId = typeof(AbilityCoreFeaturePlugIn).GUID;
        var plugIn = gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeId);
        if (plugIn is null)
        {
            plugIn = context.CreateNew<PlugInConfiguration>();
            plugIn.SetGuid(typeId);
            plugIn.TypeId = typeId;
            gameConfiguration.PlugInConfigurations.Add(plugIn);
        }

        AbilityCoreConfiguration? existing;
        try
        {
            existing = plugIn.GetConfiguration<AbilityCoreConfiguration>(new IdIgnoringReferenceHandler());
        }
        catch (JsonException)
        {
            // A configuration which can't be read is the operator's; it stays.
            return ValueTask.CompletedTask;
        }

        if (existing is not { Cards.Count: > 0 })
        {
            plugIn.SetConfiguration(Season21AbilityCoreInitializer.CreateConfiguration(), null);
            plugIn.IsActive = true;
        }

        return ValueTask.CompletedTask;
    }
}
