// Generates the guide quests of Season 21 (see ../Readme.md, "Guide quests"):
//   node season21-guide-quests.mjs --data <S21 server Data folder>
// Writes Resources/Season21/GuideQuests.json from QuestGuide/QuestGuide.txt: per step its number, name, chapter, minimum
// level, classes, objective, target, count, spot, gate and rewards. The output only depends on the input.
//
// A step is three lines: `<number> "<name>"`, the rule row (0) and the objective row (1).
// Rule row: 1 chapter, 6 minimum level, 12-27 one flag per base class (DW, DK, FE, MG, DL, SU, RF, GL, RW, SL, GC, LW, LM, IK, AL, CR).
// Objective row: 1 kind (1 collect, 2 hunt, 3 level, 4 class quest, 5 move), 2 target (item group, monster, level,
// quest), 3 item number, 4 count, 7 gate, 8 map, 9 x, 10 y, 11 experience, 12 money, 13-24 four items (group, number, count).
import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const arg = name => { const i = process.argv.indexOf(`--${name}`); return i > 0 ? process.argv[i + 1] : undefined; };
const dataDir = arg('data') ?? process.env.S21_DATA;
if (!dataDir) {
  console.error('usage: node season21-guide-quests.mjs --data <Data>');
  process.exit(1);
}

const lines = readFileSync(join(dataDir, 'QuestGuide', 'QuestGuide.txt'), 'utf8')
  .split(/\r?\n/)
  .map(l => l.replace(/\/\/.*$/, '').trim())
  .filter(Boolean);

const CLASS_COUNT = 16;
const steps = [];
for (let i = 0; i < lines.length && lines[i] !== 'end'; i++) {
  const head = lines[i].match(/^(\d+)\s+"([^"]*)"$/);
  if (!head) throw new Error(`line ${i}: expected a step head, got "${lines[i]}"`);
  const rule = lines[++i].split(/\s+/).map(Number);
  const goal = lines[++i].split(/\s+/).map(Number);
  if (rule[0] !== 0 || goal[0] !== 1) throw new Error(`step ${head[1]}: unexpected rows`);
  const classes = rule.slice(12, 12 + CLASS_COUNT).flatMap((flag, c) => (flag === 1 ? [c] : []));
  const kind = goal[1];
  const items = [];
  for (let k = 13; k + 2 < 25; k += 3) {
    if (goal[k] >= 0 && goal[k + 1] >= 0) items.push({ group: goal[k], number: goal[k + 1], level: 0, count: Math.max(1, goal[k + 2]) });
  }

  steps.push({
    number: +head[1],
    name: head[2],
    chapter: rule[1],
    minimumLevel: rule[6],
    classes: classes.length === CLASS_COUNT ? [] : classes,
    objective: kind,
    target: Math.max(0, goal[2]),
    targetNumber: Math.max(0, goal[3]),
    count: kind === 1 || kind === 2 ? goal[4] : kind === 3 ? goal[2] : 1,
    map: goal[8],
    x: goal[9],
    y: goal[10],
    gate: goal[7],
    experience: goal[11],
    money: goal[12],
    items,
  });
}

const out = join(here, '..', '..', '..', 'Resources', 'Season21', 'GuideQuests.json');
writeFileSync(out, `[\n${steps.map(s => ` ${JSON.stringify(s)}`).join(',\n')}\n]\n`);
console.log(`${steps.length} steps -> ${out}`);
