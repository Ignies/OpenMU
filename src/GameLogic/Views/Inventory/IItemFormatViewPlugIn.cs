// <copyright file="IItemFormatViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Inventory;

/// <summary>
/// Interface of a view which tells what the item format of the client can carry.
/// </summary>
/// <remarks>The item serializer of a client implements it; see <see cref="WideItems.WideItemRules"/>.</remarks>
public interface IItemFormatViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Gets a value indicating whether the item format carries the items and options of the later seasons
    /// (item groups above 15, mastery and wing grade options, sphere levels above 5).
    /// </summary>
    bool CarriesWideItems { get; }
}
