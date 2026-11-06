// <copyright file="Season21SkillsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EnhanceTree;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Skills;

/// <summary>
/// Adds the skills of Season 21 (<see cref="Season21SkillCatalog"/>): every skill the configuration doesn't have, the
/// classes of Season 21 on the skills they may use, the master tree nodes of the added skills and the books which teach
/// them. See Readme.md in this folder, "Skills".
/// </summary>
/// <remarks>
/// Existing skills keep every value; they only get the classes and evolutions after Season 6 (numbers from
/// <see cref="ClassCodes.ForkNumberOffset"/>) which the data names and they don't have yet.
/// </remarks>
internal sealed class Season21SkillsInitializer : SkillsInitializerBase
{
    /// <summary>
    /// The slots of one root of the client's master tree: 4 columns, 9 ranks.
    /// </summary>
    private const int SlotsPerRoot = 36;

    /// <summary>
    /// The slots of one rank of the client's master tree.
    /// </summary>
    private const int SlotsPerRank = 4;

    /// <summary>
    /// The highest base class of Season 6 (rage fighter).
    /// </summary>
    private const int LastSeason6Base = (int)BaseClassCode.RageFighter;

    private static readonly HashSet<int> PassiveUses = [3, 7, 9, 10, 11];

    private readonly Season21SkillData _data = Season21SkillCatalog.Data;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21SkillsInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21SkillsInitializer(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
    }

    /// <summary>
    /// Creates the configuration of the skill enhancement tree from the data.
    /// </summary>
    /// <returns>The configuration.</returns>
    public static EnhanceTreeConfiguration CreateEnhanceTreeConfiguration()
    {
        var data = Season21SkillCatalog.Data;
        return new EnhanceTreeConfiguration
        {
            Nodes = data.EnhanceNodes.Select(n => new EnhanceTreeNode
            {
                BaseClass = n.Cls,
                Group = n.Group,
                Index = n.Index,
                Skill = n.Skill,
                Color = n.Color,
                MinimumLevel = n.Min,
                MaximumLevel = n.Max,
                FormulaId = n.Formula,
                RelatedSkill = n.Related,
                Requirements = n.Requires.Select(r => new EnhanceTreeRequirement { Skill = r[0], Points = r[1] }).ToList(),
            }).ToList(),
            Passives = data.EnhancePassives.Select(p => new EnhanceTreePassive
            {
                BaseClass = p.Cls,
                Group = p.Group,
                Index = p.Index,
                Skill = p.Skill,
                Stages = p.Stages.Select(s => new EnhanceTreePassiveStage { PointCondition = s[0], PointsPerLevel = (byte)s[1], MaximumLevel = (byte)s[2], FormulaId = s[3] }).ToList(),
            }).ToList(),
            MagicFields = data.MagicFields.Select(f => new EnhanceTreeMagicField { Type = f.Type, Skill = f.Skill, Seconds = f.Seconds, CooldownSeconds = f.Cooldown }).ToList(),
            Formulas = new Dictionary<short, string>(data.Formulas),
        };
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        var added = new List<(Season21SkillRow Row, Skill Skill)>();
        foreach (var row in this._data.Skills)
        {
            var classes = this.GetQualifiedClasses(row).ToList();
            if (classes.Count == 0)
            {
                continue;
            }

            if (this.GameConfiguration.Skills.FirstOrDefault(s => s.Number == row.Number) is { } existing)
            {
                foreach (var characterClass in classes.Where(c => c.Number >= ClassCodes.ForkNumberOffset && !existing.QualifiedCharacters.Contains(c)))
                {
                    existing.QualifiedCharacters.Add(characterClass);
                }

                continue;
            }

            var skill = this.CreateSkill(row);
            classes.ForEach(skill.QualifiedCharacters.Add);
            added.Add((row, skill));
        }

        foreach (var (row, skill) in added)
        {
            this.AddMasterDefinition(row, skill);
        }

        this.LinkBooks();
    }

    private static DamageType GetDamageType(Season21SkillRow row)
    {
        if (row.Kind is not (0 or 7) || PassiveUses.Contains(row.Use))
        {
            return DamageType.None;
        }

        return row.DivisionStat switch
        {
            4 => DamageType.Wizardry,
            > 0 => DamageType.Physical,
            _ => IsWizardBase(row) ? DamageType.Wizardry : DamageType.Physical,
        };
    }

    private static bool IsWizardBase(Season21SkillRow row)
    {
        var firstBase = Array.FindIndex(row.Classes, v => v > 0);
        if (firstBase <= LastSeason6Base)
        {
            return firstBase is (int)BaseClassCode.DarkWizard or (int)BaseClassCode.Summoner;
        }

        return Season21ClassData.Classes.FirstOrDefault(c => (int)c.BaseClass == firstBase)?.Archetype == Season21ClassArchetype.Wizard;
    }

    private IEnumerable<CharacterClass> GetQualifiedClasses(Season21SkillRow row)
    {
        for (var baseClass = 0; baseClass < row.Classes.Length; baseClass++)
        {
            var firstEvolution = row.Classes[baseClass] - 1;
            if (firstEvolution < 0)
            {
                continue;
            }

            for (var evolution = Math.Min(firstEvolution, (int)ClassEvolution.Fifth); evolution <= (int)ClassEvolution.Fifth; evolution++)
            {
                if (ClassCodes.FromCode(ClassCodes.GetCode((BaseClassCode)baseClass, (ClassEvolution)evolution)) is { } number
                    && this.GameConfiguration.CharacterClasses.FirstOrDefault(c => c.Number == number) is { } characterClass)
                {
                    yield return characterClass;
                }
            }
        }
    }

    private Skill CreateSkill(Season21SkillRow row)
    {
        var effect = row.Effect > 0 ? this.GetOrCreateEffect(row.Effect, row.Number) : null;
        var effectWorks = effect?.Duration is not null;
        var isArea = row.Radius > 0;
        var (skillType, target, restriction) = (row.Kind, row.Use) switch
        {
            (_, var use) when PassiveUses.Contains(use) => (SkillType.PassiveBoost, SkillTarget.Explicit, SkillTargetRestriction.Undefined),
            (1, _) when effectWorks && isArea => (SkillType.Buff, SkillTarget.ImplicitParty, SkillTargetRestriction.Party),
            (1, _) when effectWorks => (SkillType.Buff, SkillTarget.ImplicitPlayer, SkillTargetRestriction.Self),
            (2, _) when effectWorks => (SkillType.DirectHit, SkillTarget.Explicit, SkillTargetRestriction.Undefined),
            (0 or 7, 1) => (SkillType.CastleSiegeSkill, SkillTarget.Explicit, SkillTargetRestriction.Undefined),
            (0 or 7, not 5) when isArea => (SkillType.AreaSkillAutomaticHits, SkillTarget.Explicit, SkillTargetRestriction.Undefined),
            (0 or 7, not 5) => (SkillType.DirectHit, SkillTarget.Explicit, SkillTargetRestriction.Undefined),
            _ => (SkillType.Other, SkillTarget.Explicit, SkillTargetRestriction.Undefined),
        };

        var number = (SkillNumber)row.Number;
        var range = row.Range > 0 || !isArea ? row.Range : row.Radius;
        this.CreateSkill(
            number,
            Season21SkillNames.ResourceManager.GetLocalizedString($"Skill{row.Number}"),
            CharacterClasses.None,
            GetDamageType(row),
            row.Damage,
            range,
            row.Ability,
            row.Mana,
            row.Level,
            row.Energy,
            row.Leadership,
            skillType: skillType,
            skillTarget: target,
            implicitTargetRange: skillType == SkillType.Buff ? row.Radius : (short)0,
            targetRestriction: restriction);
        if (skillType == SkillType.AreaSkillAutomaticHits && row.Range > 0)
        {
            this.AddAreaSkillSettings(number, false, default, default, default, effectRange: row.Radius);
        }

        var skill = this.GameConfiguration.Skills.First(s => s.Number == row.Number);

        // An effect without duration can't be applied yet: an attack would try to, so only a skill which doesn't apply it keeps the link.
        if (effectWorks || skillType == SkillType.Other)
        {
            skill.MagicEffectDef = effect;
        }

        return skill;
    }

    private MagicEffectDefinition GetOrCreateEffect(short number, short skillNumber)
    {
        if (this.GameConfiguration.MagicEffects.FirstOrDefault(e => e.Number == number) is { } existing)
        {
            return existing;
        }

        var row = this._data.Effects.FirstOrDefault(e => e.Number == number);
        var effect = this.Context.CreateNew<MagicEffectDefinition>();
        effect.SetGuid(number);
        effect.Number = number;
        effect.Name = Season21SkillNames.ResourceManager.GetLocalizedString(row is null ? $"Skill{skillNumber}" : $"Effect{number}");
        effect.SubType = row is { Group: > 0 and <= byte.MaxValue } ? (byte)row.Group : (byte)0;
        effect.InformObservers = true;
        effect.StopByDeath = true;
        this.GameConfiguration.MagicEffects.Add(effect);
        return effect;
    }

    private void AddMasterDefinition(Season21SkillRow row, Skill skill)
    {
        if (skill.MasterDefinition is not null
            || this._data.MasterNodes.Where(n => n.Skill == row.Number).OrderBy(n => n.Cls).FirstOrDefault() is not { } node)
        {
            return;
        }

        var definition = this.Context.CreateNew<MasterSkillDefinition>();
        skill.MasterDefinition = definition;
        definition.Root = this.GetRoot(node.Group);
        definition.Rank = (byte)((((node.Index - 1) % SlotsPerRoot) / SlotsPerRank) + 1);
        definition.MinimumLevel = Math.Max((byte)1, node.Min);
        definition.MaximumLevel = node.Max;
        definition.ValueFormula = this._data.Formulas.GetValueOrDefault(node.Formula, "0");
        definition.DisplayValueFormula = definition.ValueFormula;
        definition.Aggregation = AggregateType.AddRaw;
        definition.ReplacedSkill = node.Replace > 0 ? this.GameConfiguration.Skills.FirstOrDefault(s => s.Number == node.Replace) : null;
        foreach (var required in node.Requires)
        {
            if (this.GameConfiguration.Skills.FirstOrDefault(s => s.Number == required) is { } requiredSkill)
            {
                definition.RequiredMasterSkills.Add(requiredSkill);
            }
        }
    }

    private MasterSkillRoot GetRoot(byte group)
    {
        var season6Node = this._data.MasterNodes
            .Where(n => n.Cls <= LastSeason6Base && n.Group == group)
            .Select(n => this.GameConfiguration.Skills.FirstOrDefault(s => s.Number == n.Skill)?.MasterDefinition?.Root)
            .FirstOrDefault(r => r is not null);
        if (season6Node is not null)
        {
            return season6Node;
        }

        var root = this.Context.CreateNew<MasterSkillRoot>();
        root.Name = group switch
        {
            1 => LocalizedString.FromResource(() => MasterSkillRootNames.LeftCommonSkills),
            2 => LocalizedString.FromResource(() => MasterSkillRootNames.MiddleRoot),
            _ => LocalizedString.FromResource(() => MasterSkillRootNames.RightRoot),
        };
        this.GameConfiguration.MasterSkillRoots.Add(root);
        return root;
    }

    private void LinkBooks()
    {
        foreach (var book in this._data.Books)
        {
            if (this.GameConfiguration.Items.FirstOrDefault(i => i.Group == book.Group && i.Number == book.Number) is { Skill: null } item
                && this.GameConfiguration.Skills.FirstOrDefault(s => s.Number == book.Skill) is { } skill)
            {
                item.Skill = skill;
            }
        }
    }
}
