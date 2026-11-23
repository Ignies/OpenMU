// <copyright file="StarSmithChanceBonus.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// What a smith's level adds to a recipe's chance.
/// </summary>
public class StarSmithChanceBonus
{
    /// <summary>Gets or sets what a level adds, in hundredths of a percent.</summary>
    public int PerLevel { get; set; } = StarSmithRecipes.LevelBonus;

    /// <summary>Gets or sets the most the level adds, in hundredths of a percent.</summary>
    public int Maximum { get; set; } = StarSmithRecipes.MaximumLevelBonus;
}
