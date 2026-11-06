// <copyright file="ClassCreationRule.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Classes;

using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The rule whether an account may create a character of a class: <see cref="PlayerActions.Character.CreateCharacterAction"/>
/// enforces it and the Babylon client gets it for every class, so it can grey out what the server refuses.
/// </summary>
/// <remarks>
/// A class with a <see cref="CharacterClass.CreationAllowedFlag"/> is unlocked for the Season 6 client, which checks
/// it, like in the original game. A class without one (the classes after Season 6) is checked by the server: another
/// character of the account has the <see cref="CharacterClass.LevelRequirementByCreation"/>, or the account has
/// unlocked the class.
/// </remarks>
public static class ClassCreationRule
{
    /// <summary>
    /// Gets the creation state of a class for an account.
    /// </summary>
    /// <param name="account">The account.</param>
    /// <param name="characterClass">The character class.</param>
    /// <returns>The state.</returns>
    public static ClassCreationState GetState(Account account, CharacterClass characterClass)
    {
        if (!characterClass.CanGetCreated || !characterClass.IsActive || characterClass.HomeMap is not { IsActive: true })
        {
            return ClassCreationState.Unavailable;
        }

        if (account.UnlockedCharacterClasses.Any(c => c.Number == characterClass.Number))
        {
            return ClassCreationState.Creatable;
        }

        if (characterClass.CreationAllowedFlag != 0)
        {
            return ClassCreationState.UnlockRequired;
        }

        var requirement = characterClass.LevelRequirementByCreation;
        return requirement <= 0 || account.Characters.Any(c => c.Attributes.FirstOrDefault(a => a.Definition == Stats.Level)?.Value >= requirement)
            ? ClassCreationState.Creatable
            : ClassCreationState.LevelRequired;
    }

    /// <summary>
    /// Determines whether the server refuses a creation in the state.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <returns><c>true</c>, if the server refuses the creation.</returns>
    public static bool IsRefused(ClassCreationState state) => state is ClassCreationState.LevelRequired or ClassCreationState.Unavailable;
}
