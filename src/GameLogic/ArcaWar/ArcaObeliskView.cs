// <copyright file="ArcaObeliskView.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// An obelisk as a player sees it.
/// </summary>
/// <param name="Index">The index.</param>
/// <param name="Element">The element.</param>
/// <param name="MapNumber">The map.</param>
/// <param name="X">The x coordinate.</param>
/// <param name="Y">The y coordinate.</param>
/// <param name="HealthPercent">The health in percent, 0 while it's destroyed.</param>
/// <param name="AuraMask">The auras the guild of the player stands on: bit i for aura i.</param>
/// <param name="IsOpen">If set to <c>true</c>, the shield is down for the guild of the player.</param>
/// <param name="OwnerGuildId">The guild which holds it, 0 for none.</param>
/// <param name="OwnerGuildName">The name of that guild.</param>
public readonly record struct ArcaObeliskView(byte Index, ArcaElement Element, short MapNumber, byte X, byte Y, byte HealthPercent, byte AuraMask, bool IsOpen, uint OwnerGuildId, string OwnerGuildName);
