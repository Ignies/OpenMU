// <copyright file="IllusionTempleLeagueChatCommandPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.PlugIns.ChatCommands.Arguments;
using MUnique.OpenMU.GameLogic.PlugIns.PeriodicTasks;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// A chat command for game masters to test the Illusion Temple League: <c>/itleague &lt;stage&gt;</c> plays the stage
/// (elim, semi, final, reward) on any day and opens the temples of the league at once; <c>/itleague off</c> gives the
/// stages back to the days of the month.
/// </summary>
[Guid("4B7E0C93-1D68-4A25-B9F4-6E2A8D51C037")]
[PlugIn]
[Display(Name = nameof(PlugInResources.IllusionTempleLeagueChatCommandPlugIn_Name), Description = nameof(PlugInResources.IllusionTempleLeagueChatCommandPlugIn_Description), ResourceType = typeof(PlugInResources))]
[ChatCommandHelp(Command, typeof(IllusionTempleLeagueChatCommandArgs), CharacterStatus.GameMaster)]
public class IllusionTempleLeagueChatCommandPlugIn : ChatCommandPlugInBase<IllusionTempleLeagueChatCommandArgs>
{
    /// <summary>The argument of the eliminations.</summary>
    internal const string Elimination = "elim";

    /// <summary>The argument of the semi-finals.</summary>
    internal const string SemiFinal = "semi";

    /// <summary>The argument of the final.</summary>
    internal const string Final = "final";

    /// <summary>The argument of the reward day.</summary>
    internal const string Reward = "reward";

    /// <summary>The argument which gives the stages back to the days.</summary>
    internal const string Off = "off";

    private const string Command = "/itleague";

    /// <inheritdoc />
    public override string Key => Command;

    /// <inheritdoc />
    public override CharacterStatus MinCharacterStatusRequirement => CharacterStatus.GameMaster;

    /// <inheritdoc />
    protected override async ValueTask DoHandleCommandAsync(Player gameMaster, IllusionTempleLeagueChatCommandArgs arguments)
    {
        if (IllusionTempleLeaguePlugIn.Get(gameMaster.GameContext) is not { } league
            || gameMaster.GameContext.PlugInManager.GetStrategy<MiniGameType, IPeriodicMiniGameStartPlugIn>(MiniGameType.IllusionTempleLeague) is not { } start)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.MiniGameNotConfigured)).ConfigureAwait(false);
            return;
        }

        if (arguments.Stage == Off)
        {
            league.ForcedStage = null;
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.IllusionTempleLeagueStageReleased)).ConfigureAwait(false);
            return;
        }

        IllusionTempleLeagueStage? stage = arguments.Stage switch
        {
            Elimination => IllusionTempleLeagueStage.Elimination,
            SemiFinal => IllusionTempleLeagueStage.SemiFinal,
            Final => IllusionTempleLeagueStage.Final,
            Reward => IllusionTempleLeagueStage.Reward,
            _ => null,
        };
        if (stage is not { } forced)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.ChatCommandUsage), CommandExtensions.CreateUsage(typeof(IllusionTempleLeagueChatCommandArgs), Command)).ConfigureAwait(false);
            return;
        }

        league.ForcedStage = forced;
        if (forced != IllusionTempleLeagueStage.Reward)
        {
            start.ForceStart();
        }

        await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.IllusionTempleLeagueStageForced), forced).ConfigureAwait(false);
    }
}
