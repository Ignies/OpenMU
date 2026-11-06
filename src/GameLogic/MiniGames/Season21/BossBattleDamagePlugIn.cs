// <copyright file="BossBattleDamagePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Counts the damage of the players on the boss of a running Boss Battle Together.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.BossBattleDamagePlugIn_Name), Description = nameof(PlugInResources.BossBattleDamagePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("035F9C64-D278-48B3-A0B1-08980ABBD873")]
public class BossBattleDamagePlugIn : IAttackableGotHitPlugIn
{
    /// <inheritdoc />
    public void AttackableGotHit(IAttackable attackable, IAttacker attacker, HitInfo hitInfo)
    {
        if (attackable is Monster monster
            && (attacker as Player ?? (attacker as IPlayerSurrogate)?.Owner) is { CurrentMiniGame: BossBattleContext battle } player
            && battle.Boss == monster)
        {
            battle.RegisterHit(player, hitInfo.HealthDamage);
        }
    }
}
