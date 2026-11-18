// <copyright file="RemoveOrphanPlugInConfigurationsPlugIn075.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Removes the configurations of plugins this server no longer has, for version 0.75.
/// </summary>
[PlugIn]
[Display(Name = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.RemoveOrphanPlugInConfigurationsPlugInBase_Name), Description = nameof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources.RemoveOrphanPlugInConfigurationsPlugInBase_Description), ResourceType = typeof(MUnique.OpenMU.Persistence.Initialization.Properties.PlugInResources))]
[Guid("878A5595-26A9-48F4-BBB8-9CB60A8BB2A0")]
public class RemoveOrphanPlugInConfigurationsPlugIn075 : RemoveOrphanPlugInConfigurationsPlugInBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveOrphanPlugInConfigurationsPlugIn075"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory; the removed ids are logged with it.</param>
    public RemoveOrphanPlugInConfigurationsPlugIn075(ILoggerFactory? loggerFactory = null)
        : base(loggerFactory)
    {
    }

    /// <inheritdoc />
    public override string DataInitializationKey => Version075.DataInitialization.Id;
}
