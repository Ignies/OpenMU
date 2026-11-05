// <copyright file="BabylonClient.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer;

using MUnique.OpenMU.Network.PlugIns;

/// <summary>
/// The Babylon client in fork mode (docs/babylon.md, "Protocol"): it logs in with its own version bytes, so the
/// view plugins and packet handlers of the fork protocol are chosen for it only.
/// </summary>
/// <remarks>
/// It is no <c>GameClientDefinition</c>: every definition gets its own connect server and game server endpoints
/// on a fresh database, while this client connects to the Season 6 endpoints and only differs by its version bytes.
/// Its language is English, so it gets every plugin of the Season 6 English client, plus the ones marked with
/// <see cref="MinimumVersion"/>.
/// </remarks>
public static class BabylonClient
{
    /// <summary>
    /// The minimum version of the plugins of the fork protocol.
    /// </summary>
    public const byte MinimumSeason = 21;

    /// <summary>
    /// The episode of <see cref="MinimumSeason"/>.
    /// </summary>
    public const byte MinimumEpisode = 1;

    /// <summary>
    /// Gets the version of the client, "21B01" in the login packet.
    /// </summary>
    public static ClientVersion Version { get; } = new(MinimumSeason, MinimumEpisode, ClientLanguage.English);

    /// <summary>
    /// Gets the minimum version of the plugins of the fork protocol.
    /// </summary>
    public static ClientVersion MinimumVersion { get; } = new(MinimumSeason, MinimumEpisode, ClientLanguage.Invariant);

    /// <summary>
    /// Gets the version bytes which the client sends in its login packet.
    /// </summary>
    public static ReadOnlySpan<byte> VersionBytes => "21B01"u8;
}
