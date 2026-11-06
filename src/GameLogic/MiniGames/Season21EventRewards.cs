// <copyright file="Season21EventRewards.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames;

using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// Gives the rewards of the Season 21 events.
/// </summary>
public static class Season21EventRewards
{
    /// <summary>
    /// Gives event experience: master experience to a master class at the maximum level, as the characters of
    /// Season 21 events mostly are; normal experience otherwise. <see cref="Player.AddExperienceAsync"/> alone gives a
    /// character at the maximum level nothing.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="experience">The experience.</param>
    public static ValueTask AddEventExperienceAsync(this Player player, long experience)
    {
        if (experience <= 0 || player.Attributes is not { } attributes)
        {
            return ValueTask.CompletedTask;
        }

        var amount = (int)Math.Min(experience, int.MaxValue);
        var isMaximumLevel = attributes[Stats.Level] >= player.GameContext.Configuration.MaximumLevel;
        return isMaximumLevel && player.SelectedCharacter?.CharacterClass?.IsMasterClass is true
            ? player.AddMasterExperienceAsync(amount, null)
            : player.AddExperienceAsync(amount, null);
    }
}
