// <copyright file="EnhanceTreeFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The skill enhancement tree of the fourth evolution (fork feature id <c>enhance_tree</c>). Off by default; the update
/// "Add the skills of Season 21" fills and turns it on.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.EnhanceTreeFeaturePlugIn_Name), Description = nameof(PlugInResources.EnhanceTreeFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("3E0C8A0B-6B57-4C3F-9C55-1F2B0A8D7E41")]
public class EnhanceTreeFeaturePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<EnhanceTreeConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault
{
    /// <inheritdoc />
    public EnhanceTreeConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the configuration of the tree of a game context, if the feature is active.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The configuration, or <c>null</c> when the feature is off.</returns>
    public static EnhanceTreeConfiguration? GetConfiguration(IGameContext gameContext)
        => gameContext.FeaturePlugIns.GetPlugIn<EnhanceTreeFeaturePlugIn>()?.Configuration;

    /// <inheritdoc />
    public object CreateDefaultConfig() => new EnhanceTreeConfiguration();
}
