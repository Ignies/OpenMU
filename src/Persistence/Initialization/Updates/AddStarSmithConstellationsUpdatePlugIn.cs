// <copyright file="AddStarSmithConstellationsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using System.Text.Json;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Star Smithing PR 7a for an existing database (GameLogic/StarSmithing/Readme.md "PR 7a contract"): adds every
/// default recipe whose number the configuration lacks, the 22 looks constellations 101 to 122, and gives a stored
/// default boost recipe (1 to 11) without a shape its default geometry, since the PR 5 star shapes no longer draw.
/// Nothing else changes: a number the operator took keeps the operator's recipe.
/// </summary>
[PlugIn]
[Display(Name = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithConstellationsUpdatePlugIn_Name), Description = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithConstellationsUpdatePlugIn_Description), ResourceType = typeof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources))]
[Guid("5E7A1C2B-3D4F-4A6B-8C9D-0E1F2A3B4C72")]
public class AddStarSmithConstellationsUpdatePlugIn : UpdatePlugInBase
{
    /// <summary>The highest number of the boost recipes, which get their default geometry when they have none.</summary>
    private const short LastBoostRecipe = 11;

    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithConstellationsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithConstellationsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 24, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Adds the missing default constellations and the default boost recipes' geometry; the rest stays.
    /// </summary>
    /// <param name="configuration">The configuration, changed in place.</param>
    public static void Apply(StarSmithingConfiguration configuration)
    {
        foreach (var recipe in StarSmithingConfiguration.DefaultRecipes())
        {
            if (configuration.Recipes.FirstOrDefault(r => r.Number == recipe.Number) is not { } stored)
            {
                configuration.Recipes.Add(recipe);
            }
            else if (stored.Geometry is null && stored.Number <= LastBoostRecipe)
            {
                stored.Geometry = recipe.Geometry;
            }
        }

        configuration.Recipes = [.. configuration.Recipes.OrderBy(r => r.Number)];
    }

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        if (gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeof(StarSmithingFeaturePlugIn).GUID) is not { } smithing)
        {
            return ValueTask.CompletedTask;
        }

        StarSmithingConfiguration? configuration;
        try
        {
            configuration = smithing.GetConfiguration<StarSmithingConfiguration>(new IdIgnoringReferenceHandler());
        }
        catch (JsonException)
        {
            // A configuration which can't be read is the operator's; it stays.
            return ValueTask.CompletedTask;
        }

        configuration ??= new StarSmithingConfiguration();
        Apply(configuration);
        smithing.SetConfiguration(configuration, null);
        return ValueTask.CompletedTask;
    }
}
