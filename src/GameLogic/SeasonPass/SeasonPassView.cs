// <copyright file="SeasonPassView.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// What a character sees of its season pass.
/// </summary>
/// <param name="Season">The number of the running season, 0 when none runs.</param>
/// <param name="EndsAt">The end of the season.</param>
/// <param name="Level">The pass level.</param>
/// <param name="Experience">The pass experience.</param>
/// <param name="Passes">The owned passes, one bit per <see cref="SeasonPassKind"/>.</param>
/// <param name="ClaimedFree">The claimed levels of the free pass.</param>
/// <param name="ClaimedGold">The claimed levels of the gold pass.</param>
/// <param name="ClaimedPremium">The claimed levels of the premium pass.</param>
/// <param name="Missions">The missions with a progress or a kind.</param>
public sealed record SeasonPassView(ushort Season, DateTime? EndsAt, int Level, int Experience, byte Passes, uint ClaimedFree, uint ClaimedGold, uint ClaimedPremium, IReadOnlyList<SeasonPassMissionState> Missions);
