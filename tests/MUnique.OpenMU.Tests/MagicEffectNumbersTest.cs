// <copyright file="MagicEffectNumbersTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;

/// <summary>
/// Tests that the magic effect list takes the effect numbers of Season 21 (up to 458, e.g. the debuffs 273-279).
/// </summary>
[TestFixture]
public class MagicEffectNumbersTest
{
    /// <summary>
    /// An effect above 255 is added, found and removed like any other.
    /// </summary>
    /// <param name="number">The effect number.</param>
    [TestCase((short)273)]
    [TestCase((short)458)]
    public async ValueTask EffectAboveByteRangeIsTrackedAsync(short number)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var effect = new MagicEffect(TimeSpan.FromMinutes(1), new MagicEffectDefinition { Number = number });

        Assert.That(async () => await player.MagicEffectList.AddEffectAsync(effect).ConfigureAwait(false), Throws.Nothing);

        Assert.That(player.MagicEffectList.ContainsEffect(number), Is.True);
        await effect.DisposeAsync().ConfigureAwait(false);
        Assert.That(player.MagicEffectList.ContainsEffect(number), Is.False);
    }
}
