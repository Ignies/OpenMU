// <copyright file="SiegeChatCommandPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.CastleSiege;
using MUnique.OpenMU.GameLogic.PlugIns.ChatCommands.Arguments;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// A chat command for game masters to test the Castle Siege: <c>/siege &lt;state&gt;</c> forces a state like the
/// Force State of the admin panel (entered at the next tick of the siege), <c>/siege crown</c> switches the crown test
/// on or off, which waives the two switches so that one attacker alone can capture the Crown.
/// </summary>
[Guid("C6E1F2A8-3B54-4D97-8E0A-5F2D7B19C463")]
[PlugIn]
[Display(Name = nameof(PlugInResources.SiegeChatCommandPlugIn_Name), Description = nameof(PlugInResources.SiegeChatCommandPlugIn_Description), ResourceType = typeof(PlugInResources))]
[ChatCommandHelp(Command, typeof(SiegeChatCommandArgs), CharacterStatus.GameMaster)]
public class SiegeChatCommandPlugIn : ChatCommandPlugInBase<SiegeChatCommandArgs>
{
    /// <summary>The argument of the guild registration state.</summary>
    internal const string Register = "register";

    /// <summary>The argument of the mark registration state.</summary>
    internal const string Mark = "mark";

    /// <summary>The argument of the notification state.</summary>
    internal const string Notify = "notify";

    /// <summary>The argument of the ready state.</summary>
    internal const string Ready = "ready";

    /// <summary>The argument of the battle state.</summary>
    internal const string Start = "start";

    /// <summary>The argument of the end state.</summary>
    internal const string End = "end";

    /// <summary>The argument of the crown test.</summary>
    internal const string Crown = "crown";

    private const string Command = "/siege";

    private readonly Func<Player, CastleSiegeContext?> _getContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="SiegeChatCommandPlugIn"/> class.
    /// </summary>
    public SiegeChatCommandPlugIn()
        : this(CastleSiegeContextResolver.GetContext)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SiegeChatCommandPlugIn"/> class.
    /// </summary>
    /// <param name="getContext">Gets the Castle Siege context of a player.</param>
    internal SiegeChatCommandPlugIn(Func<Player, CastleSiegeContext?> getContext)
    {
        this._getContext = getContext;
    }

    /// <inheritdoc />
    public override string Key => Command;

    /// <inheritdoc />
    public override CharacterStatus MinCharacterStatusRequirement => CharacterStatus.GameMaster;

    /// <inheritdoc />
    protected override async ValueTask DoHandleCommandAsync(Player gameMaster, SiegeChatCommandArgs arguments)
    {
        if (this._getContext(gameMaster) is not { } context)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.CastleSiegeNotActive)).ConfigureAwait(false);
            return;
        }

        if (arguments.State == Crown)
        {
            context.SwitchesWaived = !context.SwitchesWaived;
            await gameMaster.ShowLocalizedBlueMessageAsync(context.SwitchesWaived ? nameof(PlayerMessage.CastleSiegeCrownTestOn) : nameof(PlayerMessage.CastleSiegeCrownTestOff)).ConfigureAwait(false);
            return;
        }

        CastleSiegeState? state = arguments.State switch
        {
            Register => CastleSiegeState.RegisterGuild,
            Mark => CastleSiegeState.RegisterMark,
            Notify => CastleSiegeState.Notify,
            Ready => CastleSiegeState.Ready,
            Start => CastleSiegeState.Start,
            End => CastleSiegeState.End,
            _ => null,
        };
        if (state is not { } requested)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.ChatCommandUsage), CommandExtensions.CreateUsage(typeof(SiegeChatCommandArgs), Command)).ConfigureAwait(false);
            return;
        }

        context.RequestState(requested);
        await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.CastleSiegeStateForced), requested).ConfigureAwait(false);
    }
}
