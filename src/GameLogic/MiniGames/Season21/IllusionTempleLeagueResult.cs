// <copyright file="IllusionTempleLeagueResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The result of a guild in a match of the Illusion Temple League.
/// </summary>
/// <param name="GuildKey">The persistent id of the guild.</param>
/// <param name="GuildName">The name of the guild.</param>
/// <param name="Stones">The occupied stones at the end of the match.</param>
/// <param name="Kills">The kills of enemy players.</param>
/// <param name="LevelSum">The sum of the total levels of the players.</param>
/// <param name="CharacterIds">The characters who played for the guild.</param>
public sealed record IllusionTempleLeagueResult(Guid GuildKey, string GuildName, int Stones, int Kills, int LevelSum, IReadOnlyCollection<Guid> CharacterIds)
{
    /// <summary>
    /// Compares two results by the order of S21Emu: occupied stones, then kills, then the level sum.
    /// </summary>
    /// <param name="other">The other result.</param>
    /// <returns>A positive value when this result is better, a negative when it's worse, 0 for a draw.</returns>
    public int CompareTo(IllusionTempleLeagueResult other)
        => (this.Stones, this.Kills, this.LevelSum).CompareTo((other.Stones, other.Kills, other.LevelSum));
}
