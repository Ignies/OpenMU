// <copyright file="RuudExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Ruud;

using MUnique.OpenMU.GameLogic.Views.Economy;

/// <summary>
/// Adds and takes the Ruud of a character (<see cref="Character.Ruud"/>).
/// </summary>
public static class RuudExtensions
{
    /// <summary>
    /// Gets the Ruud of the player's character.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The Ruud.</returns>
    public static int GetRuud(this Player player) => player.SelectedCharacter?.Ruud ?? 0;

    /// <summary>
    /// Adds Ruud to the player's character, up to the configured maximum, and shows it.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="amount">The amount.</param>
    /// <returns>The amount which was added.</returns>
    public static async ValueTask<int> AddRuudAsync(this Player player, int amount)
    {
        if (amount <= 0 || player.SelectedCharacter is not { } character)
        {
            return 0;
        }

        var maximum = RuudFeaturePlugIn.Get(player.GameContext)?.Configuration?.MaximumRuud ?? int.MaxValue;
        var added = (int)Math.Clamp((long)character.Ruud + amount, 0, Math.Max(maximum, character.Ruud)) - character.Ruud;
        if (added <= 0)
        {
            return 0;
        }

        character.Ruud += added;
        await player.ShowRuudAsync(added).ConfigureAwait(false);
        return added;
    }

    /// <summary>
    /// Takes Ruud from the player's character, if it has enough, and shows it.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="amount">The amount.</param>
    /// <returns><c>true</c>, if the character had enough Ruud.</returns>
    public static async ValueTask<bool> TryTakeRuudAsync(this Player player, int amount)
    {
        if (amount < 0 || player.SelectedCharacter is not { } character || character.Ruud < amount)
        {
            return false;
        }

        character.Ruud -= amount;
        await player.ShowRuudAsync(-amount).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Sets the Ruud of the player's character, e.g. by a game master, and shows it.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="amount">The new amount.</param>
    /// <returns>The task.</returns>
    public static async ValueTask SetRuudAsync(this Player player, int amount)
    {
        if (player.SelectedCharacter is not { } character)
        {
            return;
        }

        var change = Math.Max(amount, 0) - character.Ruud;
        character.Ruud = Math.Max(amount, 0);
        await player.ShowRuudAsync(change).ConfigureAwait(false);
    }

    /// <summary>
    /// Shows the Ruud of the player's character, or tells about the change with a message when the client can't show it.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="change">The change; 0 when it's just shown.</param>
    /// <returns>The task.</returns>
    public static async ValueTask ShowRuudAsync(this Player player, int change)
    {
        if (player.ViewPlugIns.GetPlugIn<IRuudViewPlugIn>() is { } view)
        {
            await view.ShowRuudAsync(player.GetRuud(), change).ConfigureAwait(false);
        }
        else if (change > 0)
        {
            await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.RuudGained), change, player.GetRuud()).ConfigureAwait(false);
        }
        else
        {
            // A client without a Ruud view gets no message for spent Ruud: it can't spend any.
        }
    }
}
