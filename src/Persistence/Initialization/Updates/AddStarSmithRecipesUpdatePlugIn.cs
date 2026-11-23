// <copyright file="AddStarSmithRecipesUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using System.Text.Json;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the Constellation Forge recipes (<see cref="StarSmithingConfiguration.DefaultRecipes"/>) to the Star Smithing
/// configuration of an existing database whose recipe list is empty. A new database gets them with the plug-in's
/// default configuration; a list the operator made stays.
/// </summary>
[PlugIn]
[Display(Name = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithRecipesUpdatePlugIn_Name), Description = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithRecipesUpdatePlugIn_Description), ResourceType = typeof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources))]
[Guid("5E658BDD-D94F-4C7D-910E-15D4F0A2A596")]
public class AddStarSmithRecipesUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithRecipesUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithRecipesUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 22, 12, 0, 0, DateTimeKind.Utc);

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
        if (configuration.Recipes.Count > 0 && !string.IsNullOrWhiteSpace(smithing.CustomConfiguration))
        {
            return ValueTask.CompletedTask;
        }

        configuration.Recipes = StarSmithingConfiguration.DefaultRecipes();
        smithing.SetConfiguration(configuration, null);
        return ValueTask.CompletedTask;
    }
}
