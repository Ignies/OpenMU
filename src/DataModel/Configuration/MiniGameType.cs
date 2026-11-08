// <copyright file="MiniGameType.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// Defines the type of a mini game.
/// </summary>
public enum MiniGameType
{
    /// <summary>
    /// Undefined mini game type.
    /// </summary>
    Undefined,

    /// <summary>
    /// The devil square event.
    /// </summary>
    DevilSquare,

    /// <summary>
    /// The blood castle event.
    /// </summary>
    BloodCastle,

    /// <summary>
    /// The chaos castle event.
    /// </summary>
    ChaosCastle,

    /// <summary>
    /// The illusion temple event.
    /// </summary>
    IllusionTemple,

    /// <summary>
    /// The doppelganger event.
    /// </summary>
    Doppelganger,

    /// <summary>
    /// The Kanturu Refinery Tower event.
    /// </summary>
    Kanturu,

    /// <summary>
    /// The imperial guardian event (fortress of the imperial guardian).
    /// </summary>
    ImperialGuardian,

    /// <summary>
    /// A party instance of Season 21: a copy of a map for each party, with its own monsters and timer, e.g. the trial
    /// of the fourth class quest.
    /// </summary>
    PartyInstance,

    /// <summary>
    /// The Tormented Square of Season 21: fifteen rounds of monsters, ranked by points.
    /// </summary>
    TormentedSquare,

    /// <summary>
    /// The Labyrinth of Dimensions of Season 21: a solo instance of floors.
    /// </summary>
    LabyrinthOfDimensions,

    /// <summary>
    /// The Boss Battle Together of Season 21: one boss which everybody fights together.
    /// </summary>
    BossBattle,

    /// <summary>
    /// The Protector of Acheron of Season 21: waves of monsters in Acheron and Deventer.
    /// </summary>
    AcheronGuardian,

    /// <summary>
    /// The Chaos Castle Survival of the Fittest of Season 21: a Chaos Castle on map 97 whose games are the stages of a
    /// monthly tournament.
    /// </summary>
    ChaosCastleSurvival,

    /// <summary>
    /// The Illusion Temple League of Season 21: the renewed Illusion Temple on maps 98 and 99, played by guild parties as
    /// the matches of a monthly tournament.
    /// </summary>
    IllusionTempleLeague,
}