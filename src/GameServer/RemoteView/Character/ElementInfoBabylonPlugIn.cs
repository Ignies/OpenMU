// <copyright file="ElementInfoBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Character;

using System.Runtime.InteropServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the elemental stats of the own character to the Babylon client (<see cref="ElementInfo"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ElementInfoBabylonPlugIn_Name), Description = nameof(PlugInResources.ElementInfoBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("E54109B8-D9C0-449E-B29E-B11F105DA845")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class ElementInfoBabylonPlugIn : IElementInfoViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementInfoBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public ElementInfoBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowElementInfoAsync()
    {
        if (this._player.Attributes is { } attributes)
        {
            await this.ShowElementInfoAsync(attributes).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Shows the elemental stats of these attributes.
    /// </summary>
    /// <param name="attributes">The attributes of the own character.</param>
    internal async ValueTask ShowElementInfoAsync(IAttributeSystem attributes)
    {
        const float percent = 100f;
        var bonus = attributes[Stats.ElementDamageBonus];
        await this._player.Connection.SendElementInfoAsync(
            (byte)attributes.GetElement(this._player.Id),
            (uint)(attributes[Stats.ElementDamageMin] + bonus),
            (uint)(attributes[Stats.ElementDamageMax] + bonus),
            (uint)attributes[Stats.ElementDefense],
            (uint)attributes[Stats.ElementAttackRate],
            (uint)attributes[Stats.ElementDefenseRate],
            (byte)Math.Round(attributes[Stats.ElementCriticalChance] * percent),
            (byte)Math.Round(attributes[Stats.ElementDodgeChance] * percent)).ConfigureAwait(false);
    }
}
