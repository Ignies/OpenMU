// <copyright file="MagicEffectsListViewTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Views.World;
using Moq;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests which effects of <see cref="MagicEffectsList"/> reach the player's view: the numbers 200 to 255 are OpenMU's own
/// invisible effects; the numbers past a byte are effects of later seasons (S21Emu <c>Effect.txt</c>), which the
/// client shows.
/// </summary>
[TestFixture]
public class MagicEffectsListViewTest
{
    /// <summary>
    /// An effect numbered past a byte, such as the Blessing of Eunice (447), is shown and its end too.
    /// </summary>
    [Test]
    public async ValueTask AnEffectPastAByteReachesTheViewAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var effect = CreateEffect(447);

        await player.MagicEffectList.AddEffectAsync(effect).ConfigureAwait(false);
        Mock.Get(player.ViewPlugIns.GetPlugIn<IActivateMagicEffectPlugIn>()!).Verify(v => v.ActivateMagicEffectAsync(effect, player), Times.Once);

        await effect.DisposeAsync().ConfigureAwait(false);
        Mock.Get(player.ViewPlugIns.GetPlugIn<IDeactivateMagicEffectPlugIn>()!).Verify(v => v.DeactivateMagicEffectAsync(effect, player), Times.Once);
    }

    /// <summary>
    /// OpenMU's own invisible effects (200 to 255) stay hidden.
    /// </summary>
    [Test]
    public async ValueTask AnInvisibleEffectDoesNotReachTheViewAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var effect = CreateEffect(210);

        await player.MagicEffectList.AddEffectAsync(effect).ConfigureAwait(false);
        await effect.DisposeAsync().ConfigureAwait(false);

        Mock.Get(player.ViewPlugIns.GetPlugIn<IActivateMagicEffectPlugIn>()!).Verify(v => v.ActivateMagicEffectAsync(It.IsAny<MagicEffect>(), It.IsAny<IAttackable>()), Times.Never);
        Mock.Get(player.ViewPlugIns.GetPlugIn<IDeactivateMagicEffectPlugIn>()!).Verify(v => v.DeactivateMagicEffectAsync(It.IsAny<MagicEffect>(), It.IsAny<IAttackable>()), Times.Never);
    }

    private static MagicEffect CreateEffect(short number)
        => new(TimeSpan.FromMinutes(30), new BasicModel.MagicEffectDefinition { Number = number });
}
