// <copyright file="RedCardConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Red Card (19, 1), see <see cref="ItemBagConsumeHandlerPlugInBase"/>.
/// </summary>
[Guid("BF000FB3-F248-40BE-8F3B-391FD4285A0E")]
[PlugIn]
[Display(Name = nameof(PlugInResources.RedCardConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.ItemBagConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class RedCardConsumeHandlerPlugIn : ItemBagConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(1, 19);
}
