// <copyright file="CastleSiegeSwitchMechanics.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.CastleSiege;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.CastleSiege.NPC;
using MUnique.OpenMU.GameLogic.Views.CastleSiege;

/// <summary>
/// Pushes and releases the Castle Siege Crown switches and broadcasts their occupants and the Crown availability.
/// </summary>
public static class CastleSiegeSwitchMechanics
{
    /// <summary>
    /// The range in tiles within which an attacker pushes and keeps a Crown switch.
    /// </summary>
    private const byte SwitchRange = 3;

    /// <summary>
    /// Pushes a Crown switch for an attacker who talks to it. When another attacker already pushes it, the player is
    /// told who.
    /// </summary>
    /// <param name="context">The Castle Siege context.</param>
    /// <param name="siegeSwitch">The switch.</param>
    /// <param name="player">The player who talks to the switch.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask PushAsync(CastleSiegeContext context, CastleSiegeSwitch siegeSwitch, Player player)
    {
        Player? occupant;
        Player? released = null;
        lock (siegeSwitch)
        {
            if (!CanHold(context, siegeSwitch, player))
            {
                return;
            }

            occupant = context.SwitchUsers[siegeSwitch.SwitchIndex];
            if (occupant is null || !CanHold(context, siegeSwitch, occupant))
            {
                released = occupant;
                occupant = null;
                context.SwitchUsers[siegeSwitch.SwitchIndex] = player;
            }
        }

        if (released is not null)
        {
            await ShowAccessAsync(released, siegeSwitch, null, CastleSiegeCrownSwitchAccess.Released).ConfigureAwait(false);
        }

        if (occupant is null)
        {
            await ShowAccessAsync(player, siegeSwitch, null, CastleSiegeCrownSwitchAccess.Pushing).ConfigureAwait(false);
        }
        else if (!ReferenceEquals(occupant, player))
        {
            await ShowAccessAsync(player, siegeSwitch, occupant, CastleSiegeCrownSwitchAccess.PushedByOther).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Releases a Crown switch whose attacker died, left its range or the map, or when the battle is over.
    /// </summary>
    /// <param name="context">The Castle Siege context.</param>
    /// <param name="siegeSwitch">The switch.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask ReleaseIfLeftAsync(CastleSiegeContext context, CastleSiegeSwitch siegeSwitch)
    {
        Player? released;
        lock (siegeSwitch)
        {
            released = context.SwitchUsers[siegeSwitch.SwitchIndex];
            if (released is null || CanHold(context, siegeSwitch, released))
            {
                return;
            }

            context.SwitchUsers[siegeSwitch.SwitchIndex] = null;
        }

        await ShowAccessAsync(released, siegeSwitch, null, CastleSiegeCrownSwitchAccess.Released).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends the current switch and Crown state to all players on the Castle Siege map.
    /// </summary>
    /// <param name="context">The Castle Siege context.</param>
    /// <returns>A task that represents the asynchronous broadcast operation.</returns>
    public static async ValueTask SendSwitchInfoAsync(CastleSiegeContext context)
    {
        var switchInfos = CreateSwitchInfos(context);
        var crownAvailability = UpdateCrownState(context);
        var changedSwitches = switchInfos
            .Where(switchInfo => !context.LastBroadcastSwitchInfos.TryGetValue(switchInfo.ObjectId, out var previous)
                                 || !Equals(switchInfo, previous))
            .ToList();
        var crownStateChanged = context.LastBroadcastCrownAvailability != crownAvailability;
        if (changedSwitches.Count == 0 && !crownStateChanged)
        {
            return;
        }

        await context.ForEachSiegePlayerAsync(async player =>
        {
            foreach (var switchInfo in changedSwitches)
            {
                await player.InvokeViewPlugInAsync<ICastleSiegeSwitchInfoPlugIn>(
                        plugIn => plugIn.ShowSwitchInfoAsync(switchInfo))
                    .ConfigureAwait(false);
            }

            if (crownStateChanged)
            {
                await player.InvokeViewPlugInAsync<ICastleSiegeCrownStatePlugIn>(
                        plugIn => plugIn.ShowCrownStateAsync(crownAvailability))
                    .ConfigureAwait(false);
            }
        }).ConfigureAwait(false);

        context.LastBroadcastSwitchInfos.Clear();
        foreach (var switchInfo in switchInfos)
        {
            context.LastBroadcastSwitchInfos[switchInfo.ObjectId] = switchInfo;
        }

        context.LastBroadcastCrownAvailability = crownAvailability;
    }

    /// <summary>
    /// Sends the current switch and Crown state to one player who entered the siege map.
    /// </summary>
    /// <param name="context">The Castle Siege context.</param>
    /// <param name="player">The player to synchronize.</param>
    /// <returns>A task that represents the asynchronous synchronization operation.</returns>
    public static async ValueTask SynchronizePlayerAsync(CastleSiegeContext context, Player player)
    {
        foreach (var switchInfo in CreateSwitchInfos(context))
        {
            await player.InvokeViewPlugInAsync<ICastleSiegeSwitchInfoPlugIn>(
                    plugIn => plugIn.ShowSwitchInfoAsync(switchInfo))
                .ConfigureAwait(false);
        }

        var crownAvailability = AreSwitchesHeldBySameAttackingSide(context);
        await player.InvokeViewPlugInAsync<ICastleSiegeCrownStatePlugIn>(
                plugIn => plugIn.ShowCrownStateAsync(crownAvailability))
            .ConfigureAwait(false);
    }

    private static bool CanHold(CastleSiegeContext context, CastleSiegeSwitch siegeSwitch, Player player)
    {
        return context.CurrentState == CastleSiegeState.Start
               && player.IsAlive
               && ReferenceEquals(player.CurrentMap, siegeSwitch.CurrentMap)
               && player.IsInRange(siegeSwitch.Position, SwitchRange)
               && context.GetPlayerJoinSide(player) is not CastleSiegeJoinSide.None and not CastleSiegeJoinSide.Defense;
    }

    private static ValueTask ShowAccessAsync(Player player, CastleSiegeSwitch siegeSwitch, Player? occupant, CastleSiegeCrownSwitchAccess access)
    {
        return player.InvokeViewPlugInAsync<ICastleSiegeCrownSwitchStatePlugIn>(
            plugIn => plugIn.ShowCrownSwitchStateAsync(siegeSwitch.Id, occupant, access));
    }

    private static List<CastleSiegeSwitchInfo> CreateSwitchInfos(CastleSiegeContext context)
    {
        return context.NpcController.GetRuntimeSnapshot()
            .Select(runtime => runtime.SpawnedInstance)
            .OfType<CastleSiegeSwitch>()
            .OrderBy(candidate => candidate.SwitchIndex)
            .Select(siegeSwitch => CreateSwitchInfo(
                context,
                siegeSwitch,
                context.SwitchUsers[siegeSwitch.SwitchIndex]))
            .ToList();
    }

    private static CastleSiegeSwitchInfo CreateSwitchInfo(
        CastleSiegeContext context,
        CastleSiegeSwitch siegeSwitch,
        Player? occupant)
    {
        var side = occupant is null
            ? CastleSiegeJoinSide.None
            : context.GetPlayerJoinSide(occupant);
        var guildName = occupant?.GuildStatus is { } guildStatus
                        && context.GetFinalGuild(guildStatus.GuildId) is { } participant
            ? participant.GuildName
            : string.Empty;

        // MuMain resolves the legacy switch-index field through its visible-object table, so the object id is required.
        return new(
            siegeSwitch.Id,
            occupant is not null,
            side,
            guildName,
            occupant?.Name ?? string.Empty);
    }

    private static bool UpdateCrownState(CastleSiegeContext context)
    {
        context.IsCrownAvailable = AreSwitchesHeldBySameAttackingSide(context);
        foreach (var crown in context.NpcController.GetRuntimeSnapshot()
                     .Select(runtime => runtime.SpawnedInstance)
                     .OfType<CastleSiegeCrown>())
        {
            crown.State = context.IsCrownAvailable
                ? CastleSiegeCrownState.Idle
                : CastleSiegeCrownState.Locked;
        }

        return context.IsCrownAvailable;
    }

    private static bool AreSwitchesHeldBySameAttackingSide(CastleSiegeContext context)
    {
        if (context.SwitchesWaived)
        {
            return true;
        }

        if (context.SwitchUsers[0] is not { IsAlive: true, GuildStatus: not null } firstSwitchUser
            || context.SwitchUsers[1] is not { IsAlive: true, GuildStatus: not null } secondSwitchUser)
        {
            return false;
        }

        var firstSide = context.GetPlayerJoinSide(firstSwitchUser);
        return firstSide is not CastleSiegeJoinSide.None and not CastleSiegeJoinSide.Defense
               && context.GetPlayerJoinSide(secondSwitchUser) == firstSide;
    }
}
