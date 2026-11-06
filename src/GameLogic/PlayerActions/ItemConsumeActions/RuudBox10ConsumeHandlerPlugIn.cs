// <copyright file="RuudBox10ConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Ruud box [10] (14, 400).
/// </summary>
[Guid("A14924A8-59E9-5774-9874-6E30D59E5B7C")]
[PlugIn]
[Display(Name = nameof(PlugInResources.RuudBox10ConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.RuudBoxConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class RuudBox10ConsumeHandlerPlugIn : RuudBoxConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(400, 14);
}
