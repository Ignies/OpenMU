// <copyright file="ProgressionFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Feature plugin which provides the progression configuration of a world with the Season 21 maps:
/// original Season 21 numbers, normalized into level 400 plus master level, or stars.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ProgressionFeaturePlugIn_Name), Description = nameof(PlugInResources.ProgressionFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("624489AB-4623-4AF2-B4CD-4F0482AD05D9")]
public class ProgressionFeaturePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<ProgressionConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault
{
    /// <inheritdoc/>
    public ProgressionConfiguration? Configuration { get; set; }

    /// <inheritdoc />
    public object CreateDefaultConfig() => new ProgressionConfiguration();
}
