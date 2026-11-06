// <copyright file="RadianceSlotScrollConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The Expansion Scroll of Radiance slot (14/316), used on a pentagram of four sockets: it opens the fifth socket, the
/// socket of the radiance errtel (muonlinefanz.com pentagram guide: "Socket 5 is locked by default").
/// </summary>
[Guid("BC7294E1-9CCA-4734-B140-C51A98B30102")]
[PlugIn]
[Display(Name = nameof(PlugInResources.RadianceSlotScrollConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.RadianceSlotScrollConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class RadianceSlotScrollConsumeHandlerPlugIn : ItemModifyConsumeHandlerPlugIn
{
    /// <summary>
    /// The item number of the scroll (group 14), from the Season 21 data.
    /// </summary>
    private const short ScrollNumber = 316;

    private const byte ScrollGroup = 14;

    /// <inheritdoc />
    public override ItemIdentifier Key => new(ScrollNumber, ScrollGroup);

    /// <inheritdoc />
    protected override bool CheckPreconditions(Player player, Item item)
        => ElementsFeaturePlugIn.GetConfiguration(player.GameContext) is not null && base.CheckPreconditions(player, item);

    /// <inheritdoc />
    protected override bool ModifyItem(Item item, IContext persistenceContext)
    {
        if (!item.IsPentagram() || item.SocketCount != Pentagrams.Sockets - 1)
        {
            return false;
        }

        item.SocketCount = Pentagrams.Sockets;
        return true;
    }
}
