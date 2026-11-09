// <copyright file="CastleSiegeCrownSwitchTalkPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.CastleSiege;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.CastleSiege.NPC;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Lets an attacker push a Castle Siege Crown switch by talking to it.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.CastleSiegeCrownSwitchTalkPlugIn_Name), Description = nameof(PlugInResources.CastleSiegeCrownSwitchTalkPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("C5E2B7A4-3D61-4F0E-8B9A-2E7D4C1F6A35")]
public sealed class CastleSiegeCrownSwitchTalkPlugIn : IPlayerTalkToNpcPlugIn
{
    private readonly Func<Player, CastleSiegeContext?> _contextResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="CastleSiegeCrownSwitchTalkPlugIn"/> class.
    /// </summary>
    public CastleSiegeCrownSwitchTalkPlugIn()
        : this(CastleSiegeContextResolver.GetContext)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CastleSiegeCrownSwitchTalkPlugIn"/> class.
    /// </summary>
    /// <param name="contextResolver">The Castle Siege context resolver.</param>
    internal CastleSiegeCrownSwitchTalkPlugIn(Func<Player, CastleSiegeContext?> contextResolver)
    {
        this._contextResolver = contextResolver;
    }

    /// <inheritdoc />
    public async ValueTask PlayerTalksToNpcAsync(Player player, NonPlayerCharacter npc, NpcTalkEventArgs eventArgs)
    {
        if (npc is not CastleSiegeSwitch siegeSwitch)
        {
            return;
        }

        eventArgs.HasBeenHandled = true;
        if (this._contextResolver.Invoke(player) is { Configuration.Enabled: true } context)
        {
            await CastleSiegeSwitchMechanics.PushAsync(context, siegeSwitch, player).ConfigureAwait(false);
        }
    }
}
