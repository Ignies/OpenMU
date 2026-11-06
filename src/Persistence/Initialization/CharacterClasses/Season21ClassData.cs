// <copyright file="Season21ClassData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The nine classes which Season 21 adds to the seven of Season 6, from the S21Emu data
/// (<c>Character/DefaultClassInfo.txt</c>, version S21.1-1).
/// </summary>
/// <remarks>
/// The S21Emu data marks the Crusader row as unfinished ("Queda hacer: LevelLife, LevelMana, VitalityToLife,
/// EnergyToMana"); its four values equal the dark knight's and no other source has them. They are kept as the source
/// writes them and stay the operator's setting: the health and mana relationships of the crusader classes in the admin
/// panel. Its damage, defense, rates, speeds and ability come from the S21Emu character calculation, which has a
/// complete crusader (<see cref="Season21ClassFormulas"/>).
/// The first evolution of each class can get created without a level (S21Emu <c>CreateType = 0</c>,
/// <c>CreateLevel = 0</c> in <c>GameServerInfo - Common.dat</c>).
/// </remarks>
public static class Season21ClassData
{
    /// <summary>
    /// Gets the classes.
    /// </summary>
    public static IReadOnlyList<Season21ClassInfo> Classes { get; } =
    [
        new(BaseClassCode.GrowLancer, Season21ClassArchetype.Knight, 30, 30, 25, 24, 110, 40, 2.0f, 1.0f, 2.0f, 1.0f, 7, GameVersion.Season10Episode2),
        new(BaseClassCode.RuneWizard, Season21ClassArchetype.Wizard, 13, 18, 14, 40, 60, 60, 1.2f, 1.8f, 2.0f, 1.5f, 5, GameVersion.Season14Episode1),
        new(BaseClassCode.Slayer, Season21ClassArchetype.Agility, 28, 30, 15, 10, 130, 10, 1.5f, 1.0f, 3.0f, 1.0f, 5, GameVersion.Season15Episode1),
        new(BaseClassCode.GunCrusher, Season21ClassArchetype.Wizard, 20, 18, 20, 25, 100, 80, 1.5f, 1.5f, 2.0f, 2.0f, 5, GameVersion.Season16Episode1),
        new(BaseClassCode.LightWizard, Season21ClassArchetype.Wizard, 19, 19, 15, 30, 70, 70, 2.0f, 2.0f, 2.0f, 2.0f, 5, GameVersion.Season17Episode1),
        new(BaseClassCode.LemuriaMage, Season21ClassArchetype.Wizard, 18, 18, 19, 30, 88, 65, 1.0f, 1.5f, 2.0f, 2.0f, 5, GameVersion.Season17Episode1),
        new(BaseClassCode.IllusionKnight, Season21ClassArchetype.Agility, 25, 28, 15, 10, 100, 20, 2.0f, 1.5f, 2.0f, 1.2f, 5, GameVersion.Season18Episode1),
        new(BaseClassCode.Alchemist, Season21ClassArchetype.Wizard, 15, 20, 23, 25, 110, 80, 2.0f, 2.0f, 3.0f, 2.0f, 5, GameVersion.Season19Episode1),
        new(BaseClassCode.Crusader, Season21ClassArchetype.Knight, 26, 18, 19, 20, 150, 40, 2.0f, 0.5f, 3.0f, 1.0f, 5, GameVersion.Season20Episode1, IsUnfinishedInSource: true),
    ];

    /// <summary>
    /// Gets the version which introduced the fourth evolution.
    /// </summary>
    public static GameVersion FourthEvolutionIntroducedIn => GameVersion.Season13Episode1;

    /// <summary>
    /// Gets the version which introduced the fifth evolution.
    /// </summary>
    public static GameVersion FifthEvolutionIntroducedIn => GameVersion.Season19Episode2;
}
