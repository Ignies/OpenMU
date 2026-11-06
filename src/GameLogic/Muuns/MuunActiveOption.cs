// <copyright file="MuunActiveOption.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// The option of an equipped Muun as it stands now.
/// </summary>
/// <param name="Slot">The slot of the Muun (<see cref="MuunSlots.Main"/> or <see cref="MuunSlots.Sub"/>).</param>
/// <param name="Type">The type of the option.</param>
/// <param name="Value">The value, with the additional option.</param>
/// <param name="IsActive">If set to <c>true</c>, its condition is met and it applies to the owner.</param>
/// <param name="IsBoosted">If set to <c>true</c>, the additional option raises it.</param>
public record MuunActiveOption(byte Slot, MuunOptionType Type, int Value, bool IsActive, bool IsBoosted);
