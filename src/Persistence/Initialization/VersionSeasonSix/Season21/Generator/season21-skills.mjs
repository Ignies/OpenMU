// Generates the skill data of Season 21 (see ../Readme.md, "Skills"):
//   node season21-skills.mjs --data <S21 server Data folder>
// Writes Resources/Season21/Skills.json and Properties/Season21SkillNames.resx (+ .Designer.cs). The rows are the
// source's, transcribed; every mapping onto the configuration is done by Season21SkillsInitializer.
import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const project = join(here, '..', '..', '..');
const arg = name => { const i = process.argv.indexOf(`--${name}`); return i > 0 ? process.argv[i + 1] : undefined; };
const dataDir = arg('data') ?? process.env.S21_DATA;
if (!dataDir) {
  console.error('usage: node season21-skills.mjs --data <Data>');
  process.exit(1);
}

const read = path => readFileSync(join(dataDir, path), 'utf8').replace(/<!--[\s\S]*?-->/g, '');
const attrs = text => Object.fromEntries([...text.matchAll(/(\w+)="([^"]*)"/g)].map(m => [m[1], m[2]]));
const int = (row, key) => (row[key] === undefined || row[key] === '' || row[key] === '*' ? 0 : parseInt(row[key], 10));
const textRows = text => text.split(/\r?\n/).map(l => l.replace(/\/\/.*$/, '').trim()).filter(l => l && l !== 'end');
const classBlocks = (text, tag) => [...text.matchAll(/<Class ID="(\d+)"[^>]*>([\s\S]*?)<\/Class>/g)]
  .flatMap(c => [...c[2].matchAll(new RegExp(`<${tag} ([^>]*)/>`, 'g'))].map(m => ({ cls: +c[1], row: attrs(m[1]) })));

const CLASSES = ['DW', 'DK', 'FE', 'MG', 'DL', 'SU', 'RF', 'GL', 'RW', 'SL', 'GC', 'KM', 'LM', 'IK', 'AL', 'CS'];

// --- Skill/Skill.xml ---
const skills = [...read('Skill/Skill.xml').matchAll(/<Skill ([^>]*)\/>/g)].map(m => attrs(m[1])).map(r => ({
  number: int(r, 'Index'), name: r.Name.trim(), damage: int(r, 'Damage'), mana: int(r, 'Mana'), ability: int(r, 'BP'),
  range: int(r, 'Range'), radius: int(r, 'Radio'), delay: int(r, 'Delay'), effect: int(r, 'Effect'),
  level: int(r, 'RequireLevel'), energy: int(r, 'RequireEnergy'), leadership: int(r, 'RequireLeadership'),
  killCount: int(r, 'RequireKillCount'), divisionStat: int(r, 'DivisionStat0'), kind: int(r, 'SkillDamageType'),
  use: int(r, 'SkillUseType'), classes: CLASSES.map(c => int(r, c)),
}));
const skillNumbers = new Set();
for (const s of skills) {
  if (skillNumbers.has(s.number)) throw new Error(`duplicate skill ${s.number}`);
  skillNumbers.add(s.number);
}

// --- Skill/Formula.txt, section "SkillTree" (3): the value of a tree node by its level ---
const formulaText = readFileSync(join(dataDir, 'Skill/Formula.txt'), 'utf8');
const formulas = {};
let section = -1;
for (const line of formulaText.split(/\r?\n/).map(l => l.replace(/\/\/.*$/, '').trim()).filter(Boolean)) {
  if (/^\d+$/.test(line)) { section = +line; continue; }
  if (line === 'end') { section = -1; continue; }
  const m = line.match(/^(\d+)\s+"([^"]*)"/);
  if (m && section === 3) formulas[+m[1]] = m[2].replace(/%d/g, 'level').replace(/\s+/g, '');
}

// --- Skill/MasterSkillTree.xml ---
const master = classBlocks(read('Skill/MasterSkillTree.xml'), 'Skill').map(({ cls, row }) => ({
  cls, index: int(row, 'Index'), skill: int(row, 'Skill'), group: int(row, 'Group'), min: int(row, 'MinLevel'),
  max: int(row, 'MaxLevel'), formula: int(row, 'ExtValue'), replace: int(row, 'ReplaceSkill'),
  requires: [int(row, 'RequireSkill0'), int(row, 'RequireSkill1')].filter(n => n > 0), valueType: int(row, 'ValueType'),
}));

// --- Skill/EnhanceSkillTree/*: the newer copy in the folder wins (S21.1-2, two requirement fixes) ---
const enhance = classBlocks(read('Skill/EnhanceSkillTree/EnhanceSkillTree.xml'), 'Skill')
  .filter(({ row }) => int(row, 'Skill') > 0)
  .map(({ cls, row }) => ({
    cls, group: int(row, 'Group'), index: int(row, 'Index'), skill: int(row, 'Skill'), color: int(row, 'Color'),
    min: int(row, 'MinLevel'), max: int(row, 'MaxLevel'), formula: int(row, 'ExtValue'), related: int(row, 'RelatedSkill'),
    requires: [1, 2, 3].map(i => [int(row, `RequireSkill${i}`), int(row, `RequireSkillPoint${i}`)]).filter(([s]) => s > 0),
  }));
const passives = classBlocks(read('Skill/EnhanceSkillTree/EnhanceSkillTreePassive.xml'), 'Skill').map(({ cls, row }) => ({
  cls, group: int(row, 'Group'), index: int(row, 'Index'), skill: int(row, 'Skill'),
  stages: [0, 1, 2].map(i => [int(row, `PointCondition${i}`), int(row, `NeedPoint${i}`), int(row, `MaxPoint${i}`), int(row, `FormulaID${i}`)]),
}));
const fieldText = read('Skill/EnhanceSkillTree/EnhanceSkillTreeMagicField.xml');
const fields = [...fieldText.matchAll(/<Field ([^>]*)\/>/g)].map(m => attrs(m[1])).map(r => ({
  type: int(r, 'Type'), skill: int(r, 'Class'), seconds: int(r, 'DisappearSec'), cooldown: int(r, 'CooldownSec'), name: r.Name,
}));

// --- Effect.txt: the names and groups of the effects ---
const effects = textRows(readFileSync(join(dataDir, 'Effect.txt'), 'utf8'))
  .map(l => l.match(/^(\d+)\s+(\d+)\s+\S+\s+"([^"]*)"/)).filter(Boolean)
  .map(m => ({ number: +m[1], group: +m[2], name: m[3].trim() }));

// --- Character/CharacterSkillInit.xml ---
const startSkills = classBlocks(read('Character/CharacterSkillInit.xml'), 'Skill').map(({ cls, row }) => ({ cls, skill: int(row, 'Index') }));

// --- books: Skill/SkillRequire.txt names the books which need a master skill; the rest match by name ---
const requireRows = textRows(read('Skill/SkillRequire.txt')).slice(1).map(l => l.split(/\s+/).map(Number));
const items = [];
for (const s of read('Item/Item.xml').matchAll(/<Section Index="(\d+)"[^>]*>([\s\S]*?)<\/Section>/g)) {
  if (s[1] !== '12' && s[1] !== '15') continue;
  for (const m of s[2].matchAll(/<Item ([^>]*)\/>/g)) {
    const r = attrs(m[1]);
    items.push({ group: +s[1], number: int(r, 'Index'), name: r.Name, slot: int(r, 'Slot'), classes: CLASSES.map(c => int(r, c)) });
  }
}
const norm = n => n.toLowerCase().replace(/\[[^\]]*\]/g, '').replace(/\b(orb|scroll|parchment|crystal|book|of|the|transmutation|synthesis)\b/g, ' ')
  .split(/[^a-z0-9]+/).filter(Boolean).sort().join(' ');
const keys = n => [norm(n), norm(n).replace(/ /g, ''), n.toLowerCase().replace(/\b(orb|scroll|parchment|crystal|book|of|the)\b/g, '').replace(/[^a-z0-9]/g, '')];
const names = s => [s.name, s.name.replace(/\([^)]*\)/g, ''), ...[...s.name.matchAll(/\(([^)]*)\)/g)].map(m => m[1])].flatMap(keys);
const matches = (s, item) => keys(item.name).some(k => names(s).includes(k));
const learnable = s => s.use === 0 || s.use === 1 || s.use === 14;
const pick = (candidates, shares) => (candidates.some(shares) ? candidates.filter(shares) : candidates).sort((a, b) => a.number - b.number)[0];
const books = [];
for (const item of items.filter(i => i.slot < 0)) {
  const required = requireRows.find(r => r[1] === item.group && r[2] === item.number);
  const shares = s => s.classes.some((v, i) => v > 0 && item.classes[i] > 0);
  const skill = required ? skills.find(s => s.number === required[0])
    : pick(skills.filter(s => learnable(s) && matches(s, item)), shares);
  if (skill) {
    books.push({ group: item.group, number: item.number, skill: skill.number, ...(required ? { requiredSkill: required[3], requiredMasterSkill: required[4], requiredPoints: required[5] } : {}) });
  }
}

const isDefault = v => v === 0 || v === '' || (Array.isArray(v) && (v.length === 0 || v.every(x => x === 0)));
const compact = r => Object.fromEntries(Object.entries(r).filter(([k, v]) => k === 'skill' || !isDefault(v)));
const lines = rows => rows.map(r => `    ${JSON.stringify(compact(r))}`).join(',\n');
writeFileSync(join(project, 'Resources', 'Season21', 'Skills.json'), `{
  "formulas": ${JSON.stringify(formulas)},
  "skills": [
${lines(skills.map(({ name, ...r }) => r))}
  ],
  "masterNodes": [
${lines(master)}
  ],
  "enhanceNodes": [
${lines(enhance)}
  ],
  "enhancePassives": [
${lines(passives)}
  ],
  "magicFields": [
${lines(fields.map(({ name, ...r }) => r))}
  ],
  "effects": [
${lines(effects.map(({ name, ...r }) => r))}
  ],
  "startSkills": [
${lines(startSkills)}
  ],
  "books": [
${lines(books)}
  ]
}
`);

const xml = s => s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
const resources = [...skills.map(s => [`Skill${s.number}`, s.name]), ...effects.map(e => [`Effect${e.number}`, e.name])];
writeFileSync(join(project, 'Properties', 'Season21SkillNames.resx'), `<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
${resources.map(([key, name]) => `  <data name="${key}" xml:space="preserve">\n    <value>${xml(name)}</value>\n  </data>`).join('\n')}
</root>
`);
writeFileSync(join(project, 'Properties', 'Season21SkillNames.Designer.cs'), `//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by VersionSeasonSix/Season21/Generator/season21-skills.mjs.
//     The names are looked up by "Skill<number>" and "Effect<number>", so there are no properties.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MUnique.OpenMU.Persistence.Initialization.Properties {
    using System;

    /// <summary>
    ///   The names of the skills and effects of the Season 21 skill data.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("season21-skills.mjs", "1.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Season21SkillNames {

        private static System.Resources.ResourceManager resourceMan;

        internal Season21SkillNames() {
        }

        /// <summary>
        ///   Gets the cached ResourceManager instance used by this class.
        /// </summary>
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    resourceMan = new System.Resources.ResourceManager("MUnique.OpenMU.Persistence.Initialization.Properties.Season21SkillNames", typeof(Season21SkillNames).Assembly);
                }
                return resourceMan;
            }
        }
    }
}
`);

const unmatched = items.filter(i => i.slot < 0 && i.number < 400 && !books.some(b => b.group === i.group && b.number === i.number));
console.log(`skills: ${skills.length}; master nodes: ${master.length}; enhance nodes: ${enhance.length}; passives: ${passives.length}; formulas: ${Object.keys(formulas).length}; effects: ${effects.length}; books: ${books.length}`);
if (arg('log')) writeFileSync(arg('log'), unmatched.map(i => `${i.group}/${i.number} ${i.name}`).join('\n'));
