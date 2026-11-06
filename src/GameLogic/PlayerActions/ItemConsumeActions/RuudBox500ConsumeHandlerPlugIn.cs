// <copyright file="RuudBox500ConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Ruud box [500] (14, 485).
/// </summary>
[Guid("54DEF822-B1F3-5098-B489-569E7E58E8C2")]
[PlugIn]
[Display(Name = nameof(PlugInResources.RuudBox500ConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.RuudBoxConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class RuudBox500ConsumeHandlerPlugIn : RuudBoxConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(485, 14);
}
