// <copyright file="BabylonGroupHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Packet handler for the packets of the Babylon protocol (0xEE identifier), which only the Babylon client sends.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.BabylonGroupHandlerPlugIn_Name), Description = nameof(PlugInResources.BabylonGroupHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("8F0A6C2D-3B71-4E5A-9C1F-6D2E8B4A7C30")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
internal class BabylonGroupHandlerPlugIn : GroupPacketHandlerPlugIn
{
    /// <summary>
    /// The group key, the head code of every packet of the Babylon protocol.
    /// </summary>
    internal const byte GroupKey = 0xEE;

    /// <summary>
    /// Initializes a new instance of the <see cref="BabylonGroupHandlerPlugIn" /> class.
    /// </summary>
    /// <param name="clientVersionProvider">The client version provider.</param>
    /// <param name="manager">The manager.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public BabylonGroupHandlerPlugIn(IClientVersionProvider clientVersionProvider, PlugInManager manager, ILoggerFactory loggerFactory)
        : base(clientVersionProvider, manager, loggerFactory)
    {
    }

    /// <inheritdoc/>
    public override bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public override byte Key => GroupKey;
}
