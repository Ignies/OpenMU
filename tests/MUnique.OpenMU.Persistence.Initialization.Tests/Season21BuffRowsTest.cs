// <copyright file="Season21BuffRowsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Every Season 21 skill with a magic effect is a buff once the Season 21 updates ran: an <c>Other</c> with an effect logs
/// a warning at each cast (its effect is applied by no skill type). The rows not fixed yet are listed (the Hidden handoff's
/// <c>handoff/s21/buff_rows.md</c> is the brief); the list only shrinks: a fix takes its numbers out, a new row fails.
/// </summary>
[TestFixture]
internal class Season21BuffRowsTest
{
    /// <summary>The Season 21 skills which are still an <c>Other</c> with an effect (no duration yet), to be fixed.</summary>
    private static readonly short[] Pending =
    [
        244, 272, 273, 278, 289, 298, 363, 406, 430, 433, 459, 460, 461, 463,
        652, 693, 700, 701, 703, 706, 708, 709, 710, 740, 770, 771, 772, 773,
        801, 802, 803, 804, 805, 806, 810, 828, 829, 851, 852, 853, 854, 857,
        861, 862, 863, 865, 866, 867, 868, 869, 870, 885, 886, 887, 895, 905,
        906, 912, 913, 914, 915, 928, 1204, 1224, 1225, 1228, 1229, 1230, 1500, 1502,
        1503, 2009, 2010, 2011, 2014, 2015, 2016, 2021, 2024, 2026, 2027, 2032, 2090, 2097,
        2098, 2099, 2100, 2108,
    ];

    /// <summary>
    /// No Season 21 skill outside the pending list is an <c>Other</c> with an effect.
    /// </summary>
    [Test]
    public async Task NoNewSeason21SkillIsAnOtherWithAnEffectAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        var before = gameConfiguration.Skills.Select(s => s.Number).ToHashSet();
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new FixSeason21RuneWizardBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new FixSeason21SlayerAndRageFighterBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var others = gameConfiguration.Skills
            .Where(s => !before.Contains(s.Number) && s.SkillType == SkillType.Other && s.MagicEffectDef is not null && !Pending.Contains(s.Number))
            .OrderBy(s => s.Number)
            .Select(s => $"{s.Number} {s.Name} (effect {s.MagicEffectDef!.Number}, duration {(s.MagicEffectDef.Duration is null ? "none" : "set")})")
            .ToList();
        Assert.That(string.Join("; ", others), Is.Empty, $"{others.Count} rows");
    }
}
