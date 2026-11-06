// Generates the Muun data of Season 21 (see ../Readme.md, "Muuns"):
//   node season21-muuns.mjs --data <S21 server Data folder>
// Writes Resources/Season21/Muuns.json from Item/Muun - Settings (the canonical folder; Data/Muun - Settings is an older
// copy): MuunInfo.xml, MuunOption.xml, MuunAddOption.xml, MuunExchange.txt, and Monica's spawn of
// Monster/MonsterSetBase/051 - Elbeland.xml. The output only depends on the inputs.
import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const project = join(here, '..', '..', '..');
const arg = name => { const i = process.argv.indexOf(`--${name}`); return i > 0 ? process.argv[i + 1] : undefined; };
const dataDir = arg('data') ?? process.env.S21_DATA;
if (!dataDir) {
  console.error('usage: node season21-muuns.mjs --data <Data>');
  process.exit(1);
}

const read = path => readFileSync(join(dataDir, path), 'utf8').replace(/<!--[\s\S]*?-->/g, '');
const attrs = text => Object.fromEntries([...text.matchAll(/(\w+)="([^"]*)"/g)].map(m => [m[1], m[2]]));
const infos = path => [...read(path).matchAll(/<Info ([^>]*)\/>/g)].map(m => attrs(m[1]));
const folder = 'Item/Muun - Settings';
const muunGroupOffset = 16 * 512;
const values = a => [0, 1, 2, 3, 4, 5].map(i => +a[`OptionValue${i}`]);

// --- Muuns: the item number is the index minus 16 x 512; the evolved Muun is another item ---
const muuns = infos(`${folder}/MuunInfo.xml`).map(a => ({
  n: +a.Index - muunGroupOffset,
  t: +a.Type,
  s: +a.SubType,
  r: +a.Rank,
  o: +a.OptionGroup,
  a: +a.AddOptionType,
  // SetPeriod 1 is a fixed calendar window (2013 in the data), long over: no additional option days.
  d: a.SetPeriod === '1' ? 0 : +(a.Duration ?? 0),
  e: +a.EvolutionItemIndex < 0 ? -1 : +a.EvolutionItemIndex - muunGroupOffset,
})).sort((x, y) => x.n - y.n);

// --- Options: value 0 is the evolved one, 1-5 the Muun levels; condition 1 before, condition 2 after the evolution ---
const options = infos(`${folder}/MuunOption.xml`).map(a => ({
  g: +a.OptionGroup,
  t: +a.OptionType,
  min: +a.MinLevel,
  max: +a.MaxLevel,
  v: values(a),
  c1: [+a.Option1_ConditionType, +a.Option1_ConditionVal1, +(a.Option1_ConditionVal2 ?? -1)],
  c2: [+a.Option2_ConditionType, +a.Option2_ConditionVal1, +(a.Option2_ConditionVal2 ?? -1)],
  name: a.OptionName.split('#')[0],
})).sort((x, y) => x.g - y.g || x.min - y.min);

const addOptions = infos(`${folder}/MuunAddOption.xml`).map(a => ({ t: +a.OptionType, min: +a.MinLevel, max: +a.MaxLevel, k: +a.AddOptionType, v: values(a) }));

// --- Exchange: section 0 the energy of an evolution stone, 1 the energy per rank and level, 2 the offers, 3 their stones ---
const sections = new Map();
let current = null;
for (const line of read(`${folder}/MuunExchange.txt`).split(/\r?\n/).map(l => l.replace(/\/\/.*$/, '').trim()).filter(Boolean)) {
  if (line === 'end') { current = null; continue; }
  if (current === null && /^\d+$/.test(line)) { current = +line; sections.set(current, []); continue; }
  sections.get(current)?.push(line.split(/\s+/));
}

const stoneEnergy = +sections.get(0)[0][0];
const rankEnergy = sections.get(1).filter(r => r[0] !== '*').map(r => ({ r: +r[0], p: +r[1] }));
const levelEnergy = sections.get(1).map(r => ({ l: +r[2], p: +r[3] }));
const bags = new Map(sections.get(3).map(r => [+r[0], { g: +r[1], n: +r[2], tg: +r[3], tn: +r[4] }]));
const exchange = sections.get(2).map((r, index) => {
  const bag = bags.get(+r[3]);
  return { i: index, cg: +r[0], cn: +r[1], cc: +r[2], g: bag.g, n: bag.n, tg: bag.tg, tn: bag.tn };
});

const spawn = attrs(read('Monster/MonsterSetBase/051 - Elbeland.xml').match(/<Config ([^>]*Class="682"[^>]*)\/>/)[1]);
const monica = [{ npc: 682, map: 51, x: +spawn.PositionX, y: +spawn.PositionY, d: +spawn.Direction }];

const json = { muuns, options, addOptions, rankEnergy, levelEnergy, exchange, monica };
const lines = Object.entries(json).map(([k, rows]) => `  "${k}": [\n${rows.map(r => `    ${JSON.stringify(r)}`).join(',\n')}\n  ]`);
writeFileSync(join(project, 'Resources', 'Season21', 'Muuns.json'), `{\n  "stoneEnergy": ${stoneEnergy},\n${lines.join(',\n')}\n}\n`);
console.log(`muuns ${muuns.length}, options ${options.length}, add options ${addOptions.length}, exchange ${exchange.length}`);
