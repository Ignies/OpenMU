// <copyright file="StarSmithingBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.StarSmithing;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.StarSmithing;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;
using SmithState = MUnique.OpenMU.GameLogic.StarSmithing.StarSmithState;

/// <summary>
/// Sends Star Smithing to the Babylon client (EE 91, EE 92, EE 97).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.StarSmithingBabylonPlugIn_Name), Description = nameof(PlugInResources.StarSmithingBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("782B871C-4E9B-4BAF-AC71-6413DBDA80AE")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class StarSmithingBabylonPlugIn : IStarSmithingViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="StarSmithingBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public StarSmithingBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc />
    public async ValueTask SendStateAsync(SmithState state)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = StarSmithStateRef.Length;
            _ = new StarSmithStateRef(connection.Output.GetSpan(size)[..size])
            {
                Flags = (byte)((state.HammerHeld ? 1 : 0) | (state.HammerBroken ? 2 : 0)),
                Level = (ushort)Math.Clamp(state.Level, 0, ushort.MaxValue),
                Experience = (uint)Math.Clamp(state.Experience, 0, uint.MaxValue),
                NextLevelExperience = (uint)Math.Clamp(state.NextLevelExperience, 0, uint.MaxValue),
                HammerDurability = (byte)Math.Clamp(state.HammerDurability, 0, byte.MaxValue),
            };
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendAnimationAsync(Player smith, StarSmithAction action, byte strikes)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var id = smith.GetId(this._player);
        int Write()
        {
            var size = StarSmithAnimationRef.Length;
            _ = new StarSmithAnimationRef(connection.Output.GetSpan(size)[..size]) { ObjectId = id, Action = (byte)action, Strikes = strikes };
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendResultAsync(StarSmithAction action, StarSmithResultCode result, byte refusal, uint zenCost)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = StarSmithResultRef.Length;
            _ = new StarSmithResultRef(connection.Output.GetSpan(size)[..size]) { Action = (byte)action, Result = (byte)result, Refusal = refusal, ZenCost = zenCost };
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }
}
