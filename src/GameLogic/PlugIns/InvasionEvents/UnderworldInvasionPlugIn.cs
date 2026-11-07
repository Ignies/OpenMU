// <copyright file="UnderworldInvasionPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.InvasionEvents;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The Underworld invasion of Season 21: the Death King and his Death Bones on Lorencia or Noria.
/// </summary>
[PlugIn]
[Display(Name = nameof(MUnique.OpenMU.GameLogic.Properties.PlugInResources.UnderworldInvasionPlugIn_Name), Description = nameof(MUnique.OpenMU.GameLogic.Properties.PlugInResources.UnderworldInvasionPlugIn_Description), ResourceType = typeof(MUnique.OpenMU.GameLogic.Properties.PlugInResources))]
[Guid("6D2B7E41-93C5-4F0A-8B61-2E7D4C9A1F35")]
public sealed class UnderworldInvasionPlugIn : SimpleInvasionPlugIn, IDisabledByDefault
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnderworldInvasionPlugIn"/> class.
    /// </summary>
    public UnderworldInvasionPlugIn()
        : base(() => InvasionConfigurationDefaults.Underworld)
    {
    }

    /// <inheritdoc />
    protected override ushort? AnnouncedMonsterId => InvasionMonsters.DeathKing;
}
