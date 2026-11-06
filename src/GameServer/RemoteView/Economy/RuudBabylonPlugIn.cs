// <copyright file="RuudBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Economy;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Ruud;
using MUnique.OpenMU.GameLogic.Views.Economy;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the Ruud of the character and the Ruud shop to the Babylon client (C1 EE 70, C2 EE 71, C1 EE 73).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.RuudBabylonPlugIn_Name), Description = nameof(PlugInResources.RuudBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("7D2A5F38-1B9C-4E06-8C73-4A1E6D9B2F51")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class RuudBabylonPlugIn : IRuudViewPlugIn
{
    private const byte LuckFlag = 0x01;
    private const byte OptionFlag = 0x02;

    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuudBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public RuudBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc />
    public async ValueTask ShowRuudAsync(int ruud, int change)
    {
        if (this._player.Connection is { } connection)
        {
            await connection.SendRuudChangedAsync((uint)Math.Max(ruud, 0), unchecked((uint)change)).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask ShowRuudShopAsync(IReadOnlyList<(int Index, RuudShopEntry Entry)> entries)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var count = Math.Min(entries.Count, byte.MaxValue);

        int Write()
        {
            var size = RuudShopListRef.GetRequiredSize(count);
            var packet = new RuudShopListRef(connection.Output.GetSpan(size)[..size])
            {
                ItemCount = (byte)count,
            };

            for (var i = 0; i < count; i++)
            {
                var (index, entry) = entries[i];
                var item = packet[i];
                item.Index = (ushort)index;
                item.ItemGroup = entry.Group;
                item.ItemNumber = (ushort)entry.Number;
                item.ItemLevel = entry.Level;
                item.Count = entry.Count;
                item.Flags = (byte)((entry.LuckChance > 0 ? LuckFlag : 0) | (entry.OptionChance > 0 && entry.OptionLevel > 0 ? OptionFlag : 0));
                item.Price = (uint)Math.Max(entry.Price, 0);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask ShowRuudShopBuyResultAsync(RuudShopBuyResult result, int index)
    {
        if (this._player.Connection is { } connection)
        {
            await connection.SendRuudShopBuyResponseAsync((byte)result, (ushort)index, (uint)Math.Max(this._player.GetRuud(), 0)).ConfigureAwait(false);
        }
    }
}
