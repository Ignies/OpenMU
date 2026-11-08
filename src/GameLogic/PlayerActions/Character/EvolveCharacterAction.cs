// <copyright file="EvolveCharacterAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Character;

using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Views.Character;

/// <summary>
/// Evolves the selected character into its <see cref="CharacterClass.NextGenerationClass"/>. The evolution quests and
/// the game master command use it; the fourth and fifth evolution check the <see cref="EvolutionRules"/>.
/// </summary>
/// <remarks>
/// The attributes of the new class apply when the character gets selected the next time, as after an evolution quest.
/// </remarks>
public class EvolveCharacterAction
{
    /// <summary>
    /// Determines whether the selected character of the player can evolve.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="checkRequirement">If set to <c>true</c>, the level rule of the evolution is checked.</param>
    /// <returns><c>true</c>, if the character can evolve.</returns>
    public static bool CanEvolve(Player player, bool checkRequirement)
    {
        return player.SelectedCharacter is { CharacterClass.NextGenerationClass: { IsActive: true } next }
               && (!checkRequirement || next.GetEvolution() is not { } evolution || player.MeetsEvolutionRule(evolution));
    }

    /// <summary>
    /// Tries to evolve the selected character of the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="checkRequirement">If set to <c>true</c>, the level rule of the evolution is checked.</param>
    /// <returns><c>true</c>, if the character evolved.</returns>
    public bool TryEvolve(Player player, bool checkRequirement)
    {
        if (!CanEvolve(player, checkRequirement)
            || player.SelectedCharacter is not { CharacterClass.NextGenerationClass: { } next } character)
        {
            return false;
        }

        player.Logger.LogInformation("Character {0} evolves from {1} into {2}.", character.Name, character.CharacterClass, next);
        character.CharacterClass = next;
        return true;
    }

    /// <summary>
    /// Tries to evolve the selected character of the player, and shows the evolution to the observers and the new skill
    /// list to the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="checkRequirement">If set to <c>true</c>, the level rule of the evolution is checked.</param>
    /// <param name="reason">The reason of the evolution.</param>
    /// <returns><c>true</c>, if the character evolved.</returns>
    /// <remarks>
    /// The skill list carries the state of the skill enhancement tree to the Babylon client, whose points follow the class.
    /// </remarks>
    public async ValueTask<bool> TryEvolveAsync(Player player, bool checkRequirement, ClassEvolutionReason reason)
    {
        if (!this.TryEvolve(player, checkRequirement))
        {
            return false;
        }

        await player.ForEachWorldObserverAsync<IClassEvolvedPlugIn>(p => p.ClassEvolvedAsync(player, reason), true).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<ISkillListViewPlugIn>(p => p.UpdateSkillListAsync()).ConfigureAwait(false);
        return true;
    }
}
