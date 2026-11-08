// <copyright file="Season21ClassRanks.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The class ranks of a row of the data (<see cref="Season21CatalogItem.Classes"/>): which classes of a configuration
/// may use the item.
/// </summary>
internal static class Season21ClassRanks
{
    /// <summary>
    /// Gets whether a class may use an item of these ranks. A rank names the first evolution which may use it (1 the
    /// first class ... 5 the fifth), each later evolution of the class too. The Season 6 classes keep the rule of
    /// their own data, where a rank of 1 or 2 means every evolution.
    /// </summary>
    /// <param name="classNumber">The number of the class.</param>
    /// <param name="ranks">The ranks of the row, one per base class.</param>
    /// <returns><c>true</c> when the class may use it.</returns>
    public static bool Qualifies(byte classNumber, IReadOnlyList<int> ranks)
    {
        if (ClassCodes.GetBaseClass(classNumber) is not { } baseClass
            || ClassCodes.GetEvolution(classNumber) is not { } evolution
            || (int)baseClass >= ranks.Count
            || ranks[(int)baseClass] is not (> 0 and var rank))
        {
            return false;
        }

        var first = baseClass <= BaseClassCode.RageFighter && rank <= 2 ? ClassEvolution.First : (ClassEvolution)(rank - 1);
        return evolution >= first;
    }

    /// <summary>
    /// Adds the classes which may use an item and are missing. An item which is not worn and has no classes (a ticket, a jewel) stays without.
    /// </summary>
    /// <param name="classes">The classes.</param>
    /// <param name="item">The item.</param>
    /// <param name="ranks">The ranks of its row.</param>
    public static void Qualify(IEnumerable<CharacterClass> classes, ItemDefinition item, IReadOnlyList<int> ranks)
    {
        if (item.QualifiedCharacters.Count == 0 && item.ItemSlot is null)
        {
            return;
        }

        foreach (var characterClass in classes.Where(c => Qualifies(c.Number, ranks) && !item.QualifiedCharacters.Contains(c)).ToList())
        {
            item.QualifiedCharacters.Add(characterClass);
        }
    }
}
