// <copyright file="RemoveOrphanPlugInConfigurationsPlugInSeason6.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Removes the configurations of plugins this server no longer has, for season 6.
/// </summary>
[PlugIn]
[Display(Name = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.RemoveOrphanPlugInConfigurationsPlugInBase_Name), Description = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.RemoveOrphanPlugInConfigurationsPlugInBase_Description), ResourceType = typeof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources))]
[Guid("00EDC573-AD63-48EB-909A-C25CFEF3E815")]
public class RemoveOrphanPlugInConfigurationsPlugInSeason6 : RemoveOrphanPlugInConfigurationsPlugInBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveOrphanPlugInConfigurationsPlugInSeason6"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory; the removed ids are logged with it.</param>
    public RemoveOrphanPlugInConfigurationsPlugInSeason6(ILoggerFactory? loggerFactory = null)
        : base(loggerFactory)
    {
    }

    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;
}
