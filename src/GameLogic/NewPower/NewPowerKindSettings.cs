// <copyright file="NewPowerKindSettings.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.NewPower;

/// <summary>
/// The settings of one kind of the New Power.
/// </summary>
public class NewPowerKindSettings
{
    /// <summary>
    /// Gets or sets the kind.
    /// </summary>
    public NewPowerKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the skill which the extra damage card gives ("Nuke(Activated)" 2086, ...); its skill animation shows a hit.
    /// </summary>
    public short Skill { get; set; }

    /// <summary>
    /// Gets or sets the magic effect which gives the power for a while ("Extra Nuke Damage" 421, ...).
    /// </summary>
    public short Effect { get; set; }

    /// <summary>
    /// Gets or sets the card option of the extra damage card of the kind ("[Extra DMG] Nuke" 80, ...).
    /// </summary>
    public short CardOption { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of targets of a hit, the first included.
    /// </summary>
    public int Targets { get; set; } = 1;

    /// <summary>
    /// Gets or sets the range in tiles around the first target in which the others are hit.
    /// </summary>
    public int Range { get; set; }

    /// <summary>
    /// Gets or sets the part of the extra damage a hit deals (Nuke 1, Bolt 2/3, Wide 1/3: the 3:2:1 of the kind options).
    /// </summary>
    public float DamageRate { get; set; } = 1;
}
