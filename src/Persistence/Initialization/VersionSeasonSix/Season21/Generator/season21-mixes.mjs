// Generates Resources/Season21/MasteryMixes.json from the S21Emu file Item/Mastery - Settings/MasteryItemMix.xml.
// A material is [index min, index max, level min, level max, option min, option max, count min, count max], index = group * 512 + number.
// node season21-mixes.mjs --data "<S21Emu Data/Item>"
import { readFileSync, writeFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const args = process.argv.slice(2);
const data = args[args.indexOf('--data') + 1];
const xml = readFileSync(join(data, 'Mastery - Settings', 'MasteryItemMix.xml'), 'utf8').replace(/<!--[\s\S]*?-->/g, '');
const attrs = (tag) => Object.fromEntries([...tag.matchAll(/(\w+)="([^"]*)"/g)].map((m) => [m[1], m[2]]));
const num = (v) => Number.parseInt(v, 10);

const materials = new Map();
for (const m of xml.matchAll(/<Material\s(?:[^>"]|"[^"]*")*\/>/g)) {
  const a = attrs(m[0]);
  // index min, index max, level min, level max, option min, option max, count min, count max
  materials.set(num(a.ID), [a.IndexMin, a.IndexMax, a.LevelMin, a.LevelMax, a.OptionMin, a.OptionMax, a.CountMin, a.CountMax].map(num));
}

const mixes = [];
for (const m of xml.matchAll(/<Mix\s((?:[^>"]|"[^"]*")*)>([\s\S]*?)<\/Mix>/g)) {
  const a = attrs(m[1]);
  const list = attrs(m[2].match(/<MaterialList(?:[^>"]|"[^"]*")*\/>/)[0]);
  const result = m[2].match(/<ResultItem(?:[^>"]|"[^"]*")*\/>/);
  if (!result || num(a.RandomID) >= 0) {
    continue;
  }

  const r = attrs(result[0]);
  const ids = Object.keys(list).filter((k) => /^MID\d$/.test(k)).map((k) => num(list[k])).filter((id) => id >= 0);
  if (ids.some((id) => !materials.has(id))) {
    continue;
  }

  mixes.push({
    id: num(a.ID), name: a.Name.trim(), successRate: num(a.SuccessRate), money: num(a.Money), keep: num(a.OptionKeepType),
    materials: ids.map((id) => materials.get(id)),
    resultIndex: num(r.ItemIndex), resultLevel: num(r.ItemLevel),
  });
}

const out = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'Resources', 'Season21', 'MasteryMixes.json');
writeFileSync(out, '[\n' + mixes.map((x) => JSON.stringify(x)).join(',\n') + '\n]\n');
console.log(`${mixes.length} mixes, ${materials.size} materials -> ${out}`);
