// <copyright file="ClassEncoding.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Babylon;

/// <summary>
/// The class encoding of the Babylon packets which the <see cref="Network.Packets.Babylon.ForkWelcome"/> announces.
/// </summary>
public enum ClassEncoding : byte
{
    /// <summary>
    /// The 5 bits of the Season 6 character class number.
    /// </summary>
    Season6 = 0,

    /// <summary>
    /// The class code of <see cref="ClassCodes"/>.
    /// </summary>
    ClassCode = 1,
}
