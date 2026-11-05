// <copyright file="ForkHelloAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Babylon;

using MUnique.OpenMU.GameLogic.Resets;
using MUnique.OpenMU.GameLogic.Views.Babylon;

/// <summary>
/// The action when the Babylon client says hello after its login: it negotiates the features of the fork.
/// </summary>
public class ForkHelloAction
{
    /// <summary>
    /// The number of stars which a world without a reset limit announces.
    /// </summary>
    internal const ushort UnlimitedStars = ushort.MaxValue;

    /// <summary>
    /// Negotiates the features with the client and answers with the welcome.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="clientWire">The protocol version of the client.</param>
    /// <param name="wantedFeatures">The features the client wants.</param>
    public async ValueTask HelloAsync(Player player, ushort clientWire, uint wantedFeatures)
    {
        var activeFeatures = clientWire >= ForkFeatures.WireVersion ? ForkFeatures.Negotiate(wantedFeatures) : 0;
        player.ForkFeatures = activeFeatures;
        await player.InvokeViewPlugInAsync<IForkWelcomePlugIn>(p => p.ShowForkWelcomeAsync(activeFeatures, player.GameContext.Configuration.MaximumLevel, GetMaximumStars(player))).ConfigureAwait(false);
    }

    private static ushort GetMaximumStars(Player player)
    {
        if (player.GameContext.FeaturePlugIns.GetPlugIn<ResetFeaturePlugIn>()?.Configuration is not { } configuration)
        {
            return 0;
        }

        return configuration.ResetLimit is > 0 ? (ushort)Math.Min(configuration.ResetLimit.Value, UnlimitedStars - 1) : UnlimitedStars;
    }
}
