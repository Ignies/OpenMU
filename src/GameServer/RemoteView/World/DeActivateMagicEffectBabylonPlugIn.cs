// <copyright file="DeActivateMagicEffectBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.World;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The magic effects of the Babylon client: an effect numbered past a byte (a later season's, such as the Blessing of
/// Eunice 447) as <c>MagicEffectStatusWide</c> (C1 EE 32) with its remaining time; every other effect as in Season 6
/// (<see cref="DeActivateMagicEffectPlugIn"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.DeActivateMagicEffectBabylonPlugIn_Name), Description = nameof(PlugInResources.DeActivateMagicEffectBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("3F7C2A91-6B4E-4D58-9E13-A8C5D02F7B64")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class DeActivateMagicEffectBabylonPlugIn : IActivateMagicEffectPlugIn, IDeactivateMagicEffectPlugIn
{
    private readonly RemotePlayer _player;

    private readonly DeActivateMagicEffectPlugIn _seasonSix;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeActivateMagicEffectBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public DeActivateMagicEffectBabylonPlugIn(RemotePlayer player)
    {
        this._player = player;
        this._seasonSix = new DeActivateMagicEffectPlugIn(player);
    }

    /// <inheritdoc/>
    public ValueTask ActivateMagicEffectAsync(MagicEffect effect, IAttackable affectedObject)
        => IsWide(effect)
            ? this.SendWideAsync(effect, affectedObject, true, effect.Definition.SendDuration ? effect.Duration : TimeSpan.Zero)
            : this._seasonSix.ActivateMagicEffectAsync(effect, affectedObject);

    /// <inheritdoc/>
    public ValueTask DeactivateMagicEffectAsync(MagicEffect effect, IAttackable affectedObject)
        => IsWide(effect)
            ? this.SendWideAsync(effect, affectedObject, false, TimeSpan.Zero)
            : this._seasonSix.DeactivateMagicEffectAsync(effect, affectedObject);

    private static bool IsWide(MagicEffect effect) => effect.Definition.Number > byte.MaxValue;

    private async ValueTask SendWideAsync(MagicEffect effect, IAttackable affectedObject, bool isActive, TimeSpan duration)
    {
        if (this._player.Connection is not { Connected: true } connection)
        {
            return;
        }

        await connection.SendMagicEffectStatusWideAsync(isActive, affectedObject.GetId(this._player), (ushort)effect.Definition.Number, (uint)duration.TotalSeconds).ConfigureAwait(false);
    }
}
