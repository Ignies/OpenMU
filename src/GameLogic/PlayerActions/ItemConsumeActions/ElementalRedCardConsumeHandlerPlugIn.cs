// <copyright file="ElementalRedCardConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Red Card [Elemental] (19, 14), see <see cref="ItemBagConsumeHandlerPlugInBase"/>.
/// </summary>
[Guid("D0C10326-84FF-43A3-8DA8-AB620F82DCAE")]
[PlugIn]
[Display(Name = nameof(PlugInResources.ElementalRedCardConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.ItemBagConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class ElementalRedCardConsumeHandlerPlugIn : ItemBagConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(14, 19);
}
