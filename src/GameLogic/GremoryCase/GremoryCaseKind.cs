// <copyright file="GremoryCaseKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.GremoryCase;

/// <summary>
/// The kind of a gremory case. The values are sent to the client.
/// </summary>
public enum GremoryCaseKind : byte
{
    /// <summary>
    /// The case of the character (<see cref="Character.GremoryCase"/>).
    /// </summary>
    Character,

    /// <summary>
    /// The case of the account (<see cref="Account.GremoryCase"/>), shared by its characters.
    /// </summary>
    Account,
}
