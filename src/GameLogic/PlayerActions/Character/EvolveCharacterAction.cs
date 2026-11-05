// <copyright file="EvolveCharacterAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Character;

using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Progression;

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
    /// Tries to evolve the selected character of the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="checkRequirement">If set to <c>true</c>, the level rule of the evolution is checked.</param>
    /// <returns><c>true</c>, if the character evolved.</returns>
    public bool TryEvolve(Player player, bool checkRequirement)
    {
        if (player.SelectedCharacter is not { CharacterClass.NextGenerationClass: { IsActive: true } next } character)
        {
            return false;
        }

        if (checkRequirement
            && next.GetEvolution() is { } evolution
            && player.Attributes is { } attributes
            && !EvolutionRules.IsMet(
                evolution,
                (int)attributes[Stats.Level],
                (int)attributes[Stats.MasterLevel],
                (int)attributes[Stats.Resets],
                ProgressionFeaturePlugIn.GetWorld(player.GameContext)?.Configuration))
        {
            return false;
        }

        player.Logger.LogInformation("Character {0} evolves from {1} into {2}.", character.Name, character.CharacterClass, next);
        character.CharacterClass = next;
        return true;
    }
}
