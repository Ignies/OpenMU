// <copyright file="IPlayerDamageFactorPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// A plugin interface which scales the damage of a hit of a player on another player, e.g. a reduction during an event.
/// </summary>
[Guid("5B1E9C27-4A6D-4F3B-8E52-0C7D91A4B6E8")]
[PlugInPoint("Player damage factor", "Plugins which scale the damage of a hit of a player on another player.")]
public interface IPlayerDamageFactorPlugIn
{
    /// <summary>
    /// Modifies the factor of the damage of a hit.
    /// </summary>
    /// <param name="attacker">The attacking player, or the owner of the attacking pet.</param>
    /// <param name="defender">The hit player.</param>
    /// <param name="args">The arguments; a plugin multiplies <see cref="PlayerDamageFactorArgs.Factor"/>.</param>
    void ModifyDamageFactor(Player attacker, Player defender, PlayerDamageFactorArgs args);
}
