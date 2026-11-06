// <copyright file="LevelRewardsPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Rewards;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The level rewards of Season 21: a character which reaches a total level of the configuration gets its item
/// into the gremory case (into the inventory when the case isn't available).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.LevelRewardsPlugIn_Name), Description = nameof(PlugInResources.LevelRewardsPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("E3A6C1D8-5B27-4F90-8C4E-1A7D3F9B2C65")]
public class LevelRewardsPlugIn : IFeaturePlugIn, IDisabledByDefault, ISupportCustomConfiguration<LevelRewardsConfiguration>, ISupportDefaultCustomConfiguration,
    ICharacterLevelUpPlugIn, ICharacterMasterLevelUpPlugIn, IObjectAddedToMapPlugIn
{
    /// <inheritdoc />
    public LevelRewardsConfiguration? Configuration { get; set; }

    /// <inheritdoc />
    public object CreateDefaultConfig() => new LevelRewardsConfiguration();

    /// <inheritdoc />
    public void CharacterLeveledUp(Player player)
    {
        _ = this.RewardAsync(player).AsTask();
    }

    /// <inheritdoc />
    public async ValueTask CharacterMasterLeveledUpAsync(Player player)
    {
        await this.RewardAsync(player).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is Player player)
        {
            await this.RewardAsync(player).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gives the rewards whose level the player's character meets in the world's progression mode and which it didn't
    /// get yet (<see cref="Character.ClaimedLevelRewards"/>, so a reset doesn't give them again).
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The number of given rewards.</returns>
    public async ValueTask<int> RewardAsync(Player player)
    {
        if (this.Configuration is not { } configuration || player.Attributes is not { } attributes || player.SelectedCharacter is not { } character)
        {
            return 0;
        }

        var baseClass = character.CharacterClass is { } characterClass ? ClassCodes.GetBaseClass(characterClass.Number) : null;
        var given = 0;
        try
        {
            foreach (var reward in configuration.Rewards.Where(r => r.Id is >= 1 and <= LevelReward.MaximumId
                         && (character.ClaimedLevelRewards & GetBit(r)) == 0
                         && (r.Classes.Count == 0 || (baseClass is { } code && r.Classes.Contains(code)))
                         && player.MeetsSeason21Level(r.TotalLevel)))
            {
                if (await RewardDelivery.DeliverAsync(player, reward).ConfigureAwait(false))
                {
                    character.ClaimedLevelRewards |= GetBit(reward);
                    given++;
                }
            }
        }
        catch (Exception ex)
        {
            player.Logger.LogError(ex, "Error when giving the level rewards to {player}.", player);
        }

        return given;
    }

    private static long GetBit(LevelReward reward) => 1L << (reward.Id - 1);
}
