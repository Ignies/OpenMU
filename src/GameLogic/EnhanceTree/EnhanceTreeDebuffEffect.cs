// <copyright file="EnhanceTreeDebuffEffect.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

using System.Threading;
using Timer = System.Timers.Timer;

/// <summary>
/// A debuff of the skill enhancement tree on its target: poison deals <see cref="Damage"/> each <see cref="Interval"/>
/// while it lasts, bleeding adds <see cref="Percent"/> of a skill hit plus <see cref="Damage"/> to the next
/// <see cref="EnhanceTreeDebuff.Count"/> skill hits; chill only carries its power-ups.
/// </summary>
public sealed class EnhanceTreeDebuffEffect : MagicEffect
{
    private readonly Timer? _poisonTimer;

    private int _remainingCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnhanceTreeDebuffEffect"/> class.
    /// </summary>
    /// <param name="definition">The definition of the magic effect.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="debuff">The debuff.</param>
    /// <param name="attacker">The attacker who placed it.</param>
    /// <param name="owner">The target.</param>
    /// <param name="damage">The damage of a poison tick or the flat damage of a bleeding.</param>
    /// <param name="percent">The percentage of a skill hit which bleeds.</param>
    /// <param name="powerUps">The power-ups.</param>
    public EnhanceTreeDebuffEffect(MagicEffectDefinition definition, TimeSpan duration, EnhanceTreeDebuff debuff, IAttacker attacker, IAttackable owner, float damage, float percent, params ElementWithTarget[] powerUps)
        : base(duration, definition, powerUps)
    {
        this.Kind = debuff.Kind;
        this.Attacker = attacker;
        this.Owner = owner;
        this.Damage = damage;
        this.Percent = percent;
        this._remainingCount = debuff.Count;
        this.Interval = TimeSpan.FromSeconds(debuff.Count > 0 ? (double)debuff.Seconds / debuff.Count : debuff.Seconds);
        if (this.Kind == EnhanceTreeDebuffKind.Poison && damage > 0 && this.Interval > TimeSpan.Zero)
        {
            this._poisonTimer = new Timer(this.Interval);
            this._poisonTimer.Elapsed += this.OnPoisonTimerElapsed;
            this._poisonTimer.Start();
        }
    }

    /// <summary>
    /// Gets the kind.
    /// </summary>
    public EnhanceTreeDebuffKind Kind { get; }

    /// <summary>
    /// Gets the attacker who placed the debuff.
    /// </summary>
    public IAttacker Attacker { get; }

    /// <summary>
    /// Gets the target.
    /// </summary>
    public IAttackable Owner { get; }

    /// <summary>
    /// Gets the damage of a poison tick or the flat damage of a bleeding.
    /// </summary>
    public float Damage { get; }

    /// <summary>
    /// Gets the percentage of a skill hit which bleeds.
    /// </summary>
    public float Percent { get; }

    /// <summary>
    /// Gets the time between two poison ticks: the seconds of the debuff by its count.
    /// </summary>
    public TimeSpan Interval { get; }

    /// <summary>
    /// Adds the bleeding of a skill hit which the target took.
    /// </summary>
    /// <param name="hitDamage">The damage of the hit.</param>
    /// <returns>The task.</returns>
    public async ValueTask BleedAsync(uint hitDamage)
    {
        if (this.Kind != EnhanceTreeDebuffKind.Bleed || this.IsDisposed || this.IsDisposing || !this.Owner.IsAlive
            || Interlocked.Decrement(ref this._remainingCount) < 0)
        {
            return;
        }

        var damage = (hitDamage * this.Percent / 100f) + this.Damage;
        if (damage >= 1)
        {
            await this.Owner.ApplyBleedingDamageAsync(this.Attacker, (uint)damage).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (this._poisonTimer is { } timer)
        {
            timer.Stop();
            timer.Dispose();
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "Catching all Exceptions.")]
    private async void OnPoisonTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            if (this.IsDisposed || this.IsDisposing || !this.Owner.IsAlive)
            {
                return;
            }

            await this.Owner.ApplyPoisonDamageAsync(this.Attacker, (uint)this.Damage).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            (this.Owner as ILoggerOwner)?.Logger.LogError(ex, "Error when applying the poison of the enhance tree");
        }
    }
}
