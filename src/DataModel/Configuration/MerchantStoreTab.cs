// <copyright file="MerchantStoreTab.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.DataModel.Configuration;

using MUnique.OpenMU.Annotations;
using MUnique.OpenMU.DataModel.Entities;

/// <summary>
/// A further tab of the store of a merchant NPC, as the clients of Season 21 show them.
/// </summary>
[Cloneable]
public partial class MerchantStoreTab
{
    /// <summary>
    /// Gets or sets the index of the tab, 1 to 4. The tab 0 is the <see cref="MonsterDefinition.MerchantStore"/>.
    /// </summary>
    public byte Index { get; set; }

    /// <summary>
    /// Gets or sets the items of the tab.
    /// </summary>
    [MemberOfAggregate]
    public virtual ItemStorage? Items { get; set; }
}
