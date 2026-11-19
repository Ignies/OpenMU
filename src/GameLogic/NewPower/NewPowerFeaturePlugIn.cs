// <copyright file="NewPowerFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.NewPower;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The fifth class New Power (Season 21, fork feature id <see cref="FeatureId"/>): a Nuke, Bolt or Wide hit on the hits of
/// fourth class and card skills. Off by default; the operator turns it on (Admin > Systems). See Readme.md.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.NewPowerFeaturePlugIn_Name), Description = nameof(PlugInResources.NewPowerFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("5E2B7C41-9A86-4D13-B0F5-8C3E6A1D2F74")]
public class NewPowerFeaturePlugIn : IFeaturePlugIn, IDisabledByDefault, ISupportCustomConfiguration<NewPowerConfiguration>, ISupportDefaultCustomConfiguration
{
    /// <summary>
    /// The feature id of the fork marker.
    /// </summary>
    public const string FeatureId = "new_power";

    /// <inheritdoc />
    public NewPowerConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the active plugin of the game, or <c>null</c> when the New Power is off.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The active plugin.</returns>
    public static NewPowerFeaturePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<NewPowerFeaturePlugIn>();

    /// <summary>
    /// Gets the current configuration, or the default one.
    /// </summary>
    /// <returns>The configuration.</returns>
    public NewPowerConfiguration GetConfiguration() => this.Configuration ??= new NewPowerConfiguration();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new NewPowerConfiguration();
}
