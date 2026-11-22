// <copyright file="AddStarSmithDyesUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Items;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// This update adds the dyes a Star Smith's recolour spends (14/1000-1014): the basic ones on sale at Hanzo, the rare,
/// Prismatic and Radiant ones dropped from level 60 on every map.
/// </summary>
[PlugIn]
[Display(Name = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithDyesUpdatePlugIn_Name), Description = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithDyesUpdatePlugIn_Description), ResourceType = typeof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources))]
[Guid("6C3B9E1F-5A7D-4D2E-9B84-2F1E0C7A9D55")]
public class AddStarSmithDyesUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithDyesUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithDyesUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 21, 12, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        StarSmithDyeItems.Apply(context, gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
