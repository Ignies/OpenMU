// <copyright file="NewPowerConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.NewPower;

/// <summary>
/// The configuration of the fifth class New Power (Readme.md, the source or pick of each value).
/// </summary>
public class NewPowerConfiguration
{
    /// <summary>
    /// Gets or sets the kinds.
    /// </summary>
    public List<NewPowerKindSettings> Kinds { get; set; } =
    [
        new() { Kind = NewPowerKind.Nuke, Skill = 2086, Effect = 421, CardOption = 80, Targets = 1, Range = 0, DamageRate = 1 },
        new() { Kind = NewPowerKind.Bolt, Skill = 2087, Effect = 422, CardOption = 81, Targets = 2, Range = 2, DamageRate = 2f / 3 },
        new() { Kind = NewPowerKind.Wide, Skill = 2088, Effect = 423, CardOption = 82, Targets = 10, Range = 3, DamageRate = 1f / 3 },
    ];

    /// <summary>
    /// Gets or sets the time after a strike in which hits don't strike again, so that one cast strikes once.
    /// </summary>
    public int CooldownMilliseconds { get; set; } = 500;

    /// <summary>
    /// Gets or sets the extra damage the effect of a fifth class quest's trial gives (S21Emu Effect.txt 421-423: 50).
    /// </summary>
    public int TrialExtraDamage { get; set; } = 50;

    /// <summary>
    /// Gets the settings of a kind.
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The settings, or <c>null</c>.</returns>
    public NewPowerKindSettings? Get(NewPowerKind kind) => this.Kinds.FirstOrDefault(k => k.Kind == kind);
}
