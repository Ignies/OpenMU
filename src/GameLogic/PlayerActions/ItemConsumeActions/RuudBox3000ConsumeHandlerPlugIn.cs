// <copyright file="RuudBox3000ConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Ruud box [3,000] (14, 448).
/// </summary>
[Guid("51569E89-B591-5B9F-9A42-466C014670C8")]
[PlugIn]
[Display(Name = nameof(PlugInResources.RuudBox3000ConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.RuudBoxConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class RuudBox3000ConsumeHandlerPlugIn : RuudBoxConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(448, 14);
}
