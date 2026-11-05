// <copyright file="ItemFormat.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Babylon;

/// <summary>
/// The item data format which the <see cref="Network.Packets.Babylon.ForkWelcome"/> announces.
/// </summary>
public enum ItemFormat : byte
{
    /// <summary>
    /// The 12 bytes of the Season 6 client.
    /// </summary>
    Season6 = 0,

    /// <summary>
    /// The 32 bytes of <see cref="ItemSerializerW32"/>.
    /// </summary>
    W32 = 1,
}
