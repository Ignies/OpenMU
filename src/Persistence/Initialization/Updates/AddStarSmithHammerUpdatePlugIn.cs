// <copyright file="AddStarSmithHammerUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Items;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// This update adds the Star Smith's Hammer (2/1000), the tool of the star smithing, and puts it on sale at Hanzo the Blacksmith.
/// </summary>
[PlugIn]
[Display(Name = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithHammerUpdatePlugIn_Name), Description = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithHammerUpdatePlugIn_Description), ResourceType = typeof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources))]
[Guid("0311F0DD-054E-43B2-887A-B779B250E8E7")]
public class AddStarSmithHammerUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithHammerUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.AddStarSmithHammerUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        StarSmithHammer.Apply(context, gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
