// <copyright file="SoulConverter5ConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Activates the monster soul converter of tier 5 (14, 464).
/// </summary>
[Guid("F2836D49-5B7C-4A01-8E3F-3C9B6A2D4E58")]
[PlugIn]
[Display(Name = nameof(PlugInResources.SoulConverter5ConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.SoulConverterConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class SoulConverter5ConsumeHandlerPlugIn : SoulConverterConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(464, 14);
}
