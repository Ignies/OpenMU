// <copyright file="RemoveOrphanPlugInConfigurationsPlugIn095d.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Removes the configurations of plugins this server no longer has, for version 0.95d.
/// </summary>
[PlugIn]
[Display(Name = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.RemoveOrphanPlugInConfigurationsPlugInBase_Name), Description = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.RemoveOrphanPlugInConfigurationsPlugInBase_Description), ResourceType = typeof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources))]
[Guid("D4A8D94A-3E27-45D7-89E7-8C479998167F")]
public class RemoveOrphanPlugInConfigurationsPlugIn095d : RemoveOrphanPlugInConfigurationsPlugInBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveOrphanPlugInConfigurationsPlugIn095d"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory; the removed ids are logged with it.</param>
    public RemoveOrphanPlugInConfigurationsPlugIn095d(ILoggerFactory? loggerFactory = null)
        : base(loggerFactory)
    {
    }

    /// <inheritdoc />
    public override string DataInitializationKey => Version095d.DataInitialization.Id;
}
