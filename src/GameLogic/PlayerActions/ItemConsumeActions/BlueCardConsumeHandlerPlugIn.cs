// <copyright file="BlueCardConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Blue Card (19, 2), see <see cref="ItemBagConsumeHandlerPlugInBase"/>.
/// </summary>
[Guid("30E9D438-F913-4403-9415-D16F7602575B")]
[PlugIn]
[Display(Name = nameof(PlugInResources.BlueCardConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.ItemBagConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class BlueCardConsumeHandlerPlugIn : ItemBagConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(2, 19);
}
