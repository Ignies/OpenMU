// <copyright file="ElementalDamageBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.World;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the elemental part of a hit to the Babylon client (<see cref="Network.Packets.Babylon.ElementalDamage"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ElementalDamageBabylonPlugIn_Name), Description = nameof(PlugInResources.ElementalDamageBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("572D22AA-051E-409E-AADE-1013608F3035")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class ElementalDamageBabylonPlugIn : IElementalDamageViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementalDamageBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public ElementalDamageBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowElementalDamageAsync(IAttackable target, Element element, uint damage)
    {
        await this._player.Connection.SendElementalDamageAsync(target.GetId(this._player), (byte)element, damage).ConfigureAwait(false);
    }
}
