// <copyright file="ArcaBuff.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// The buff of the members of a guild which holds an obelisk of an element at the end of a war.
/// </summary>
public class ArcaBuff
{
    /// <summary>
    /// Gets or sets the element.
    /// </summary>
    public ArcaElement Element { get; set; }

    /// <summary>
    /// Gets or sets the number of the magic effect which shows the buff.
    /// </summary>
    public short EffectNumber { get; set; }

    /// <summary>
    /// Gets or sets what the buff raises.
    /// </summary>
    public ArcaBuffKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public float Value { get; set; }
}
