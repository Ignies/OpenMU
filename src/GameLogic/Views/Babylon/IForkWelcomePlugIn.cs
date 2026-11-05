// <copyright file="IForkWelcomePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Babylon;

/// <summary>
/// The view which answers the hello of the Babylon client.
/// </summary>
public interface IForkWelcomePlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the client which features of the fork are active for its connection.
    /// </summary>
    /// <param name="activeFeatures">The active features (<see cref="ForkFeatures"/>).</param>
    /// <param name="maximumLevel">The normal level cap of the world.</param>
    /// <param name="maximumStars">The maximum number of stars, 0 when the world has none.</param>
    ValueTask ShowForkWelcomeAsync(uint activeFeatures, short maximumLevel, ushort maximumStars);
}
