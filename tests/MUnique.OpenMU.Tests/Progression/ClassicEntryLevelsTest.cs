// <copyright file="ClassicEntryLevelsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Progression;

using MUnique.OpenMU.GameLogic.Progression;
using EnterGate = MUnique.OpenMU.Persistence.BasicModel.EnterGate;
using WarpInfo = MUnique.OpenMU.Persistence.BasicModel.WarpInfo;

/// <summary>
/// Tests that the warp list and the gates of the classic maps use <see cref="ProgressionConfiguration.ClassicMapEntryLevels"/>.
/// </summary>
[TestFixture]
public class ClassicEntryLevelsTest
{
    private const ushort RaklionWarp = 34;
    private const short SwampGate = 274;

    /// <summary>
    /// The original mode takes the Season 21 levels of the table; the normalized modes keep Season 6 by default, and
    /// take the Season 21 levels (or their normalized ones) when chosen.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <param name="classic">The classic entry levels.</param>
    /// <param name="warpLevel">The expected level of the Raklion warp.</param>
    /// <param name="gateLevel">The expected level of the Swamp of Calmness gate.</param>
    [TestCase(ProgressionMode.Original, ClassicMapEntryLevels.Season6, 380, 380)]
    [TestCase(ProgressionMode.Normalized, ClassicMapEntryLevels.Season6, 280, 250)]
    [TestCase(ProgressionMode.Stars, ClassicMapEntryLevels.Season21, 380, 380)]
    [TestCase(ProgressionMode.Normalized, ClassicMapEntryLevels.Season21Normalized, 182, 182)]
    public async Task WarpAndGateUseTheClassicLevelsAsync(ProgressionMode mode, ClassicMapEntryLevels classic, int warpLevel, int gateLevel)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var configuration = new ProgressionConfiguration
        {
            Mode = mode,
            ClassicMapEntryLevels = classic,
            Season21ClassicWarpLevels = new() { [RaklionWarp] = 380 },
            Season21ClassicGateLevels = new() { [SwampGate] = 380 },
        };
        player.GameContext.FeaturePlugIns.AddPlugIn(new ProgressionFeaturePlugIn { Configuration = configuration }, true);
        var warp = new WarpInfo { Index = RaklionWarp, LevelRequirement = 280 };
        var gate = new EnterGate { Number = SwampGate, LevelRequirement = 250 };

        Assert.That(warp.GetLevelRequirement(player.GameContext), Is.EqualTo(warpLevel));
        Assert.That(gate.GetLevelRequirement(player.GameContext), Is.EqualTo(gateLevel));
    }

    /// <summary>
    /// A warp or gate which the table doesn't hold, or the progression off, keeps the stored level.
    /// </summary>
    [Test]
    public async Task WithoutTableOrProgressionTheStoredLevelStaysAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var warp = new WarpInfo { Index = RaklionWarp, LevelRequirement = 280 };
        Assert.That(warp.GetLevelRequirement(player.GameContext), Is.EqualTo(280));

        player.GameContext.FeaturePlugIns.AddPlugIn(new ProgressionFeaturePlugIn { Configuration = new() { Mode = ProgressionMode.Original } }, true);
        Assert.That(warp.GetLevelRequirement(player.GameContext), Is.EqualTo(280));
    }
}
