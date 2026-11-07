// <copyright file="PlayerDamageFactorArgs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

/// <summary>
/// The arguments of the <see cref="IPlayerDamageFactorPlugIn"/>.
/// </summary>
public sealed class PlayerDamageFactorArgs
{
    /// <summary>
    /// Gets or sets the factor of the damage; 1 keeps it, 0 takes it all.
    /// </summary>
    public double Factor { get; set; } = 1;
}
