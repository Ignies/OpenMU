// <copyright file="AddStarSmithItemRecipesUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using System.Text.Json;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Star Smithing PR 6 for an existing database: adds every default recipe (<see cref="StarSmithingConfiguration.DefaultRecipes"/>)
/// whose number the configuration lacks, the armour, wings, pendant and ring recipes, and gives a stored default recipe
/// without outcomes its default table. Nothing else changes: a number the operator took keeps the operator's recipe.
/// </summary>
[PlugIn]
[Display(Name = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithItemRecipesUpdatePlugIn_Name), Description = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithItemRecipesUpdatePlugIn_Description), ResourceType = typeof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources))]
[Guid("8C1D2E3F-4A5B-4C6D-9E7F-1A2B3C4D5E61")]
public class AddStarSmithItemRecipesUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithItemRecipesUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithItemRecipesUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 23, 12, 0, 0, DateTimeKind.Utc);

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
        foreach (var recipe in StarSmithingConfiguration.DefaultRecipes())
        {
            if (configuration.Recipes.FirstOrDefault(r => r.Number == recipe.Number) is not { } stored)
            {
                configuration.Recipes.Add(recipe);
            }
            else if (stored.Outcomes.Count == 0)
            {
                stored.Outcomes = recipe.Outcomes;
            }
        }

        configuration.Recipes = [.. configuration.Recipes.OrderBy(r => r.Number)];
        smithing.SetConfiguration(configuration, null);
        return ValueTask.CompletedTask;
    }
}
