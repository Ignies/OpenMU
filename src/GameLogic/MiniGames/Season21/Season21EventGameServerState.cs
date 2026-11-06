// <copyright file="Season21EventGameServerState.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using MUnique.OpenMU.GameLogic.PlugIns.PeriodicTasks;

/// <summary>
/// The state of a scheduled Season 21 event on a game server.
/// </summary>
public class Season21EventGameServerState : PeriodicTaskGameServerState
{
    private readonly string _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21EventGameServerState"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="description">The description of the event.</param>
    public Season21EventGameServerState(IGameContext context, string description)
        : base(context)
    {
        this._description = description;
    }

    /// <inheritdoc />
    public override string Description => this._description;
}
