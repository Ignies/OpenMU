// <copyright file="AddSeason21ClassesUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the classes of Season 21: the fourth and fifth evolution of the seven Season 6 classes and the nine new classes
/// (grow lancer, rune wizard, slayer, gun crusher, light wizard, lemuria mage, illusion knight, alchemist, crusader)
/// with all their evolutions. Optional: a Season 6 world stays as it is until its owner applies it.
/// </summary>
/// <remarks>
/// Classes which exist are kept with every change of the operator; only a missing class or a missing link of an
/// evolution chain is added. Version 2 replaces the formulas of the nine new classes, which version 1 took from Season 6
/// classes, with those of the S21Emu character calculation (<see cref="Season21ClassFormulas"/>). Version 3 adds the skill
/// rates, the shield recovery point and the ability recovery of 3 % of that calculation where a class misses them. Fork feature ids <c>class_codes</c>, <c>classes_s21</c> and <c>evolutions_45</c>.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21ClassesUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21ClassesUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("9CD29993-DA3C-4CA0-9E85-1D5E52C418DB")]
public class AddSeason21ClassesUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21ClassesUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21ClassesUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 05, 12, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override int Version => 4;

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 11, 08, 2, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration, int? installedVersion)
    {
        var initialization = new CharacterClassInitialization(context, gameConfiguration);
        if (installedVersion < 2)
        {
            foreach (var characterClass in gameConfiguration.CharacterClasses)
            {
                initialization.ReplaceSeason21Formulas(characterClass);
            }
        }

        if (installedVersion < 3)
        {
            foreach (var characterClass in gameConfiguration.CharacterClasses)
            {
                initialization.AddSeason21SkillRatesAndRecovery(characterClass);
            }
        }

        var existing = gameConfiguration.CharacterClasses.ToHashSet();
        initialization.InitializeSeason21();

        // version 4: the items of the data name the nine classes and the fourth and fifth evolutions too
        var added = gameConfiguration.CharacterClasses.Where(c => !existing.Contains(c)).ToList();
        new Season21ItemCatalogInitializer(context, gameConfiguration).QualifyClasses(installedVersion is > 0 and < 4 ? null : added);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        return this.ApplyAsync(context, gameConfiguration, null);
    }
}
