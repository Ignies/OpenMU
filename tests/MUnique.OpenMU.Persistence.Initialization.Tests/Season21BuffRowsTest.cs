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
        244, 289, 298, 459, 460, 461, 463, 700, 701, 740,
        770, 771, 772, 773, 828, 829, 851, 852, 853, 854, 857, 861, 862, 863,
        885, 886, 887, 905, 906,
        928, 2009, 2010, 2011, 2021,
        2024, 2032, 2090, 2108,
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
        await new FixSeason21GrowLancerBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new FixSeason21DarkKnightBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new FixSeason21FairyElfBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new FixSeason21DarkWizardBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new FixSeason21MagicGladiatorBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new FixSeason21DarkLordBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new FixSeason21BuffValuesFromSeason22UpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var others = gameConfiguration.Skills
            .Where(s => !before.Contains(s.Number) && s.SkillType == SkillType.Other && s.MagicEffectDef is not null && !Pending.Contains(s.Number))
            .OrderBy(s => s.Number)
            .Select(s => $"{s.Number} {s.Name} (effect {s.MagicEffectDef!.Number}, duration {(s.MagicEffectDef.Duration is null ? "none" : "set")})")
            .ToList();
        Assert.That(string.Join("; ", others), Is.Empty, $"{others.Count} rows");
    }
}
