// <copyright file="IDeactivationAwareFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic;

/// <summary>
/// A feature plugin which changes the running game and puts it back when it's deactivated.
/// </summary>
public interface IDeactivationAwareFeaturePlugIn : IFeaturePlugIn
{
    /// <summary>
    /// Is called by the <see cref="FeaturePlugInContainer"/> after the plugin was deactivated.
    /// </summary>
    void OnDeactivated();
}
