// Generates the Ability Enhancement Core data of Season 21 (see ../Readme.md, "Ability Enhancement Core"):
//   node season21-ability.mjs --data <S21 server Data folder> --names <client Lang/Eng/ParticleOptionText.txt>
// Writes Resources/Season21/AbilityCore.json from Skill/AbilityEnhancementCore (AbilityOption, AbilityAddOption,
// AbilityParticleBook, AbilityParticleSkill, AbilityLevelUP, AbilityReduction) and the section "Skilltree 5th" of
// Skill/Formula.txt. The option names come from the client table ParticleOptionText (the server data has none).
// The values of every option are evaluated for the card levels 0 to 5. The output only depends on the inputs.
import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const project = join(here, '..', '..', '..');
const arg = name => { const i = process.argv.indexOf(`--${name}`); return i > 0 ? process.argv[i + 1] : undefined; };
const dataDir = arg('data') ?? process.env.S21_DATA;
const namesFile = arg('names');
if (!dataDir || !namesFile) {
  console.error('usage: node season21-ability.mjs --data <Data> --names <ParticleOptionText.txt>');
  process.exit(1);
}

const read = path => readFileSync(join(dataDir, path), 'utf8').replace(/<!--[\s\S]*?-->/g, '');
const attrs = text => Object.fromEntries([...text.matchAll(/(\w+)="([^"]*)"/g)].map(m => [m[1], m[2]]));
const tags = (text, tag) => [...text.matchAll(new RegExp(`<${tag} ([^>]*?)/?>`, 'g'))].map(m => attrs(m[1]));
const folder = 'Skill/AbilityEnhancementCore';
const classColumns = ['DW', 'DK', 'FE', 'MG', 'DL', 'SU', 'RF', 'GL', 'RW', 'SL', 'GC', 'KM', 'LM', 'IK', 'AL', 'CS'];
const levels = [0, 1, 2, 3, 4, 5];
const kinds = { skill: 1, option: 2, elite: 3, extra: 4 };
const colors = { Blue: 1, Green: 2, Purple: 3 };

// --- Formula.txt, section 15 "Skilltree 5th": every %d is the card level ---
const formulas = {};
let section = -1;
for (const line of readFileSync(join(dataDir, 'Skill/Formula.txt'), 'utf8').split(/\r?\n/).map(l => l.replace(/\/\/.*$/, '').trim()).filter(Boolean)) {
  if (/^\d+$/.test(line)) { section = +line; continue; }
  if (line === 'end') { section = -1; continue; }
  const m = line.match(/^(\d+)\s+"(.*)"$/);
  if (m && section === 15) formulas[+m[1]] = m[2];
}
const evaluate = (id, level) => {
  const expression = formulas[id].replace(/%d/g, `(${level})`).replace(/\^/g, '**');
  return Math.round(Function(`"use strict"; return (${expression});`)() * 1000) / 1000;
};
const values = id => levels.map(level => evaluate(id, level));

// --- Option names (client): group 0 the skill card options, group 1 the card options; type 1 is a rate in percent ---
const names = { 0: {}, 1: {} };
let group = -1;
for (const line of readFileSync(namesFile, 'utf8').split(/\r?\n/).map(l => l.replace(/\/\/.*$/, '').trim()).filter(Boolean)) {
  if (/^\d+$/.test(line) && group < 0) { group = +line; continue; }
  if (line.startsWith('end')) { group = -1; continue; }
  const m = line.match(/^(\d+)\s+"([^"]*)"\s+(-?\d+)/);
  if (m && group >= 0) names[group][+m[1]] = { name: m[2].trim(), rate: m[3] === '1' };
}

// --- Classes of an option of a card (AbilityParticleBook): a bit per class in the Season 21 order ---
const book = tags(read(`${folder}/AbilityParticleBook.xml`), 'CardInfo');
const classMask = row => classColumns.reduce((mask, column, bit) => (row[column] === '1' ? mask | (1 << bit) : mask), 0);
const classesOf = (number, option) => {
  const row = book.find(r => +r.IType === number && (+r.OptionIndex1 === option || +r.OptionIndex2 === option));
  return row ? classMask(row) : 0xFFFF;
};

const option = (number, index, formula, extra = {}) => {
  const name = names[1][index];
  return { o: index, name: name?.name ?? `Option ${index}`, rate: name?.rate ?? false, classes: classesOf(number, index), v: values(formula), ...extra };
};

// --- Option, elite and extra damage cards (AbilityOption.xml) ---
const cards = [];
for (const m of read(`${folder}/AbilityOption.xml`).matchAll(/<CardInfo ([^>]*)>([\s\S]*?)<\/CardInfo>/g)) {
  const card = attrs(m[1]);
  const name = card.Name;
  const kind = name.startsWith('[Extra') ? kinds.extra : name.includes('Elite') ? kinds.elite : kinds.option;
  const color = colors[(name.match(/^\[(\w+)\]/) ?? [])[1]] ?? 0;
  const type = +((name.match(/Type (\d)/) ?? [])[1] ?? 0);
  const options = tags(m[2], 'Option').map(o => option(+card.IType, +o.OptionIndex, +o.FormulaSubID, { i: +o.Index }));
  cards.push({ g: +card.ISection, n: +card.IType, name, kind, color, type, options });
}

// --- Skill cards (AbilityParticleSkill.xml): the fifth skill, its 4th skill, the static option and the add options ---
const addOptions = tags(read(`${folder}/AbilityAddOption.xml`), 'Option');
for (const m of read(`${folder}/AbilityParticleSkill.xml`).matchAll(/<Class ID="(\d+)"[^>]*>([\s\S]*?)<\/Class>/g)) {
  for (const skill of tags(m[2], 'Skill')) {
    const number = +skill.IType;
    const staticName = names[0][+skill.OptionID];
    cards.push({
      g: +skill.ISection,
      n: number,
      kind: kinds.skill,
      color: 0,
      type: +skill.CardType,
      ui: +skill.UIIndex,
      skill: +skill.SkillIndex,
      req: +skill.ReqSkillIndex,
      classes: 1 << +m[1],
      st: { id: +skill.OptionID, name: staticName?.name ?? `Static ${skill.OptionID}`, v: values(+skill.FormulaID) },
      options: addOptions.map(a => ({ ...option(number, +a.OptionIndex, +a.FormulaSubID), chance: +a.Unk1, name: names[0][+a.OptionIndex]?.name ?? `Option ${a.OptionIndex}` })),
    });
  }
}
cards.sort((x, y) => x.g - y.g || x.n - y.n);

// --- Level up and reduction per card kind (1 skill, 2 option, 3 elite) ---
const levelUps = [];
for (const m of read(`${folder}/AbilityLevelUP.xml`).matchAll(/<CardType ID="(\d+)"[^>]*>([\s\S]*?)<\/CardType>/g)) {
  for (const l of tags(m[2], 'LvlInfo')) {
    levelUps.push({ kind: +m[1], level: +l.Level, cards: +l.ReqCardCnt, money: +l.ReqMoney, sg: +l.RISection, sn: +l.RIType, stones: +l.ReqStoneCnt });
  }
}

const reductions = tags(read(`${folder}/AbilityReduction.xml`), 'CardType')
  .map(r => ({ kind: +r.ID, money: +r.RequiereMoney, sg: +r.RISection, sn: +r.RIType, stones: +r.CreateStoneCnt }));

const json = { cards, levelUps, reductions };
const lines = Object.entries(json).map(([k, rows]) => `  "${k}": [\n${rows.map(r => `    ${JSON.stringify(r)}`).join(',\n')}\n  ]`);
writeFileSync(join(project, 'Resources', 'Season21', 'AbilityCore.json'), `{\n${lines.join(',\n')}\n}\n`);
console.log(`cards ${cards.length} (options ${cards.reduce((s, c) => s + c.options.length, 0)}), level ups ${levelUps.length}, reductions ${reductions.length}`);
