// <copyright file="ForkWelcomePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Views.Babylon;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The default implementation of the <see cref="IForkWelcomePlugIn"/> which is forwarding everything to the game client with specific data packets.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ForkWelcomePlugIn_Name), Description = nameof(PlugInResources.ForkWelcomePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("0B3C46E1-5D1E-4C3B-9A49-2B7E61D25F0A")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class ForkWelcomePlugIn : IForkWelcomePlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForkWelcomePlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public ForkWelcomePlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowForkWelcomeAsync(uint activeFeatures, short maximumLevel, ushort maximumStars)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var itemFormat = this._player.ItemSerializer.NeededSpace == ItemSerializerW32.Length ? ItemFormat.W32 : ItemFormat.Season6;
        if (itemFormat != ItemFormat.W32)
        {
            activeFeatures &= ~ForkFeatures.BitOf(ItemSerializerW32.FeatureId);
        }

        var classEncoding = (activeFeatures & ForkFeatures.BitOf(ClassCodes.FeatureId)) != 0 ? ClassEncoding.ClassCode : ClassEncoding.Season6;
        await connection.SendForkWelcomeAsync(ForkFeatures.WireVersion, activeFeatures, (byte)itemFormat, (byte)classEncoding, (ushort)maximumLevel, maximumStars).ConfigureAwait(false);
    }
}
