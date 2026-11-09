// <copyright file="PandoraBoxConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Pandora Box (19, 0), see <see cref="ItemBagConsumeHandlerPlugInBase"/>.
/// </summary>
[Guid("451CC6C6-284B-4F8B-9A1C-44A09C09610B")]
[PlugIn]
[Display(Name = nameof(PlugInResources.PandoraBoxConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.ItemBagConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class PandoraBoxConsumeHandlerPlugIn : ItemBagConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(0, 19);
}
