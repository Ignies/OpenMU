// <copyright file="PremiumMuunBoxConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Premium Muun Box (19, 3), see <see cref="ItemBagConsumeHandlerPlugInBase"/>.
/// </summary>
[Guid("1798CA7E-0D51-4100-8E82-ADCB77666D30")]
[PlugIn]
[Display(Name = nameof(PlugInResources.PremiumMuunBoxConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.ItemBagConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class PremiumMuunBoxConsumeHandlerPlugIn : ItemBagConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(3, 19);
}
