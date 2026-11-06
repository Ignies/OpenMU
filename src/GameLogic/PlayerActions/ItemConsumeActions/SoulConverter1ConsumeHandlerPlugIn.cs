// <copyright file="SoulConverter1ConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Activates the monster soul converter of tier 1 (14, 460).
/// </summary>
[Guid("3E7A1C52-9B4D-4F60-8D21-6A5C3E9B7F14")]
[PlugIn]
[Display(Name = nameof(PlugInResources.SoulConverter1ConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.SoulConverterConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class SoulConverter1ConsumeHandlerPlugIn : SoulConverterConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(460, 14);
}
