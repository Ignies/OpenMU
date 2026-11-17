// <copyright file="Season21BloodHowlingTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the Rage Fighter's Blood Howling (565, effect 166; its Strengthener 567, effect 167), which the skills update
/// creates as an <c>Other</c> with an effect. S21Emu Skill.xml: Type 0, Range 0, Radio 0 (a buff on the caster);
/// <c>GameServerInfo - Skill.dat</c> BloodHowlingTimeConstA 60.
/// </summary>
[TestFixture]
internal class Season21BloodHowlingTest
{
    /// <summary>
    /// Blood Howling and its Strengthener are buffs on the caster, and their effects last 60 seconds.
    /// </summary>
    [Test]
    public async Task BloodHowlingIsABuffOnTheCasterAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        await new FixSeason21SlayerAndRageFighterBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        foreach (var number in new short[] { 565, 567 })
        {
            var skill = gameConfiguration.Skills.Single(s => s.Number == number);
            Assert.That(skill.SkillType, Is.EqualTo(SkillType.Buff), $"skill {number}");
            Assert.That(skill.Target, Is.EqualTo(SkillTarget.ImplicitPlayer));
            Assert.That(skill.TargetRestriction, Is.EqualTo(SkillTargetRestriction.Self));
            Assert.That(skill.MagicEffectDef?.Duration?.ConstantValue.Value, Is.EqualTo(60), $"effect of {number}");
        }
    }
}
