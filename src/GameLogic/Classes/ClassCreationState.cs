// <copyright file="ClassCreationState.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// Whether an account may create a character of a class now, see <see cref="ClassCreationRule"/>. The values are
/// sent to the Babylon client (<c>C1 EE 14 CreatableClasses</c>).
/// </summary>
public enum ClassCreationState : byte
{
    /// <summary>
    /// The account may create the class.
    /// </summary>
    Creatable = 0,

    /// <summary>
    /// The class needs another character of the account at its creation level; the server refuses it until then.
    /// </summary>
    LevelRequired = 1,

    /// <summary>
    /// The class has a Season 6 unlock flag which the account doesn't have yet: the client checks it, like in the
    /// original game (the unlock comes with the level or a cash shop card).
    /// </summary>
    UnlockRequired = 2,

    /// <summary>
    /// The class can't get created in this world (not creatable, inactive or its home map is inactive).
    /// </summary>
    Unavailable = 3,
}
