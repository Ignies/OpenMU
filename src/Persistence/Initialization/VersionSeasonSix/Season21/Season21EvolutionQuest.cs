// <copyright file="Season21EvolutionQuest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// A quest of an evolution chain, created once for each class whose next class has the <paramref name="Evolution"/>.
/// </summary>
/// <param name="Number">The quest number (S21Emu <c>Quest/Quest.xml</c> index).</param>
/// <param name="Evolution">The evolution the chain leads to.</param>
/// <param name="Giver">The quest NPC.</param>
/// <param name="Name">The name.</param>
/// <param name="Kills">The monsters to kill, with their count.</param>
/// <param name="LevelUpPoints">The level up points of the reward.</param>
/// <param name="ItemReward">The item of the reward (group, number), if any.</param>
/// <param name="Evolves">A value indicating whether the quest evolves the character.</param>
internal sealed record Season21EvolutionQuest(
    short Number,
    ClassEvolution Evolution,
    short Giver,
    LocalizedString Name,
    (short Monster, int Count)[] Kills,
    int LevelUpPoints,
    (byte Group, short Number)? ItemReward,
    bool Evolves);
