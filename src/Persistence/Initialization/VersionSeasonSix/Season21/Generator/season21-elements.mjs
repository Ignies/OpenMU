// Generates the element data of Season 21 (see ../Readme.md, "Elements, pentagrams and earrings"):
//   node season21-elements.mjs --data <S21 server Data folder>
// Writes Resources/Season21/Elements.json. The rows are the source's, transcribed; every mapping onto the
// configuration is done by Season21ElementsInitializer.
import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const project = join(here, '..', '..', '..');
const arg = name => { const i = process.argv.indexOf(`--${name}`); return i > 0 ? process.argv[i + 1] : undefined; };
const dataDir = arg('data') ?? process.env.S21_DATA;
if (!dataDir) {
  console.error('usage: node season21-elements.mjs --data <Data>');
  process.exit(1);
}

const read = path => readFileSync(join(dataDir, path), 'utf8').replace(/<!--[\s\S]*?-->/g, '');
const attrs = text => Object.fromEntries([...text.matchAll(/(\w+)="([^"]*)"/g)].map(m => [m[1], m[2]]));
const int = (row, key) => (row[key] === undefined || row[key] === '' ? 0 : parseInt(row[key], 10));
const tags = (text, tag) => [...text.matchAll(new RegExp(`<${tag} ([^>]*?)/?>`, 'g'))].map(m => attrs(m[1]));
const ItemGroupSize = 512;
const group12 = index => { if (Math.floor(index / ItemGroupSize) !== 12) throw new Error(`not group 12: ${index}`); return index % ItemGroupSize; };

// --- Item.xml: the pentagrams (slot 236) of group 12 ---
const itemXml = read('Item/Item.xml');
const section12 = itemXml.slice(itemXml.indexOf('<Section Index="12"'), itemXml.indexOf('<Section Index="13"'));
const pentagramOptions = new Map(tags(read('Item/Pentagram - Settings/PentagramOption.xml'), 'Pentagram')
  .map(r => [int(r, 'Index'), [1, 2, 3, 4, 5, 6, 7].map(n => int(r, `OptionNum${n}`)).filter(n => n > 0)]));
const pentagrams = tags(section12, 'Item').filter(r => int(r, 'Slot') === 236).map(r => ({
  number: int(r, 'Index'), damageMin: int(r, 'DamageMin'), damageMax: int(r, 'DamageMax'), defense: int(r, 'Defense'),
  attackRate: int(r, 'ElementalAttack'), defenseRate: int(r, 'ElementalDefense'), options: pentagramOptions.get(int(r, 'Index')) ?? [],
}));

// --- PentagramOption.xml OptionSetting: the pentagram options and the errtels they need ---
const pentagramOptionSettings = tags(read('Item/Pentagram - Settings/PentagramOption.xml'), 'Errtel').map(r => ({
  number: int(r, 'OptionNum'), value: int(r, 'Value'), name: r.Name.replace(/%%/g, '%'),
  needs: [1, 2, 3, 4, 5].map(n => [int(r, `Type${n}`), int(r, `Rank${n}`), int(r, `Level${n}`)]).filter(([type]) => type >= 0),
}));

// --- PentagramJewelOption.xml: the rank options of the errtels ---
const errtels = new Map();
for (const r of tags(read('Item/Pentagram - Settings/PentagramJewelOption.xml'), 'JewelInfo')) {
  const number = group12(int(r, 'ItemIndex'));
  if (!errtels.has(number)) errtels.set(number, { number, options: [] });
  errtels.get(number).options.push({
    number: int(r, 'Index'), rank: int(r, 'RankNumber'), rate: int(r, 'RankOptionRate'), name: r.Name.replace(/%%/g, '%'),
    values: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map(n => int(r, `Value${n}`)),
  });
}

// --- ErrtelMastery.xml: the sets of the mastery errtels; option group g is the bonus of g + 2 errtels of the set ---
const errtelSets = tags(read('Item/Pentagram - Settings/ErrtelMastery.xml'), 'OptionInfo').map(r => ({
  number: int(r, 'Index'), name: r.Name,
  bonuses: [0, 1, 2].flatMap(g => ['A', 'B'].map(k => ({ pieces: g + 2, option: int(r, `Index${k}${g}`), value: int(r, `Value${k}${g}`) })))
    .filter(b => b.option >= 0),
}));

// --- ElementalUpgrade.xml + ElementalItemRefinemenMixRate.xml + PentagramJewelMixRate.txt: the refinery ---
const upgrade = read('Item/ElementalItemRefinemen - Settings/ElementalUpgrade.xml');
const upgradeHead = attrs(upgrade.slice(upgrade.indexOf('<ElementalUpgrade'), upgrade.indexOf('>', upgrade.indexOf('<ElementalUpgrade'))));
const pentagramUpgradeRates = Array.from({ length: 16 }, (_, level) => int(upgradeHead, `Level${level}`));
const errtelUpgradeRates = tags(upgrade, 'ErrtelInfo').filter(r => int(r, 'MasteryGrade') === 1).map(r => [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map(n => int(r, `Level${n}`)))[0] ?? [];
const mixes = tag => tags(upgrade.slice(upgrade.indexOf(`<${tag}>`), upgrade.indexOf(`</${tag}>`)), 'Mix').map(r => ({
  main: int(r, 'MainIndex'), result: int(r, 'RIndex'),
  materials: [0, 1, 2, 3].map(n => [int(r, `MType${n}`), int(r, `MIndex${n}`), int(r, `MDur${n}`)]).filter(([type]) => type > 0),
}));
const rankUpRates = tags(upgrade, 'ErrtelInfo').filter(r => int(r, 'MasteryGrade') === 0)
  .map(r => ({ rank: int(r, 'Rank'), rates: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map(n => int(r, `Level${n}`)) }));
const mixRate = read('Item/ElementalItemRefinemen - Settings/ElementalItemRefinemenMixRate.xml');
const rankUpMoney = tags(mixRate.slice(mixRate.indexOf('<RankUpgrade'), mixRate.indexOf('</RankUpgrade>')), 'Info')
  .map(r => ({ rank: int(r, 'Rank'), money: int(r, 'Money') }));
const levelUp = tags(mixRate.slice(mixRate.indexOf('<LevelUp>'), mixRate.indexOf('</LevelUp>')), 'Info').map(r => ({
  level: int(r, 'Level'), money: int(r, 'Money'),
  ranks: [0, 1, 2, 3, 4].map(n => [int(r, `Rate${n}`), int(r, `ElementalRune${n}`)]).filter(([rate]) => rate > 0),
}));
const textRows = text => text.split(/\r?\n/).map(l => l.replace(/\/\/.*$/, '').trim()).filter(l => l && l !== 'end');
const removalRates = textRows(read('Item/Pentagram - Settings/PentagramJewelMixRate.txt')).slice(1)
  .map(l => l.split(/\s+/).map(Number)).map(([level, ...rates]) => ({ level, rates }));

// --- Monster.xml: the element columns ---
const monsters = tags(read('Monster/Monster.xml'), 'Monster').map(r => ({
  number: int(r, 'Index'), element: int(r, 'ElementalAttribute'), defense: int(r, 'ElementalDefense'),
  damageMin: int(r, 'ElementalDamageMin'), damageMax: int(r, 'ElementalDamageMax'),
  attackRate: int(r, 'ElementalAttackRate'), defenseRate: int(r, 'ElementalDefenseRate'),
})).filter(m => m.element || m.defense || m.damageMin || m.damageMax || m.attackRate || m.defenseRate);

// --- Skill.xml: the element imprint bonus (percent) ---
const skillImprints = tags(read('Skill/Skill.xml'), 'Skill').map(r => ({ skill: int(r, 'Index'), percent: int(r, 'ElementImprintBonus') }))
  .filter(s => s.percent !== 100 && s.percent > 0);

// --- EarringItemOption.txt: sections 0-4 ---
const earringSections = (() => {
  const lines = read('Item/EarringItemOption.txt').split(/\r?\n/).map(l => l.replace(/\/\/.*$/, '').trim()).filter(l => l);
  const result = {};
  let current = null;
  for (const line of lines) {
    if (line === 'end') { current = null; continue; }
    if (current === null && /^\d+$/.test(line)) { current = +line; result[current] = []; continue; }
    result[current].push(line.split(/\s+/).map(Number));
  }

  return result;
})();
const earrings = {
  optionCountRates: earringSections[0][0].slice(1),
  items: earringSections[1].map(([index, group, ...rest]) => ({
    number: group12(index), tier: group % 8,
    options: [0, 1, 2, 3, 4].map(n => rest.slice(n * 3, (n * 3) + 3)).map(([option, valueIndex, rate]) => ({ option, valueIndex, rate })),
  })),
  links: earringSections[2].map(([tier, ...rest]) => ({
    tier, options: [0, 1, 2].map(n => rest.slice(n * 2, (n * 2) + 2)).filter(([option]) => option >= 0).map(([option, valueIndex]) => ({ option, valueIndex })),
  })),
  values: earringSections[3].map(([tier, index, value]) => ({ tier, index, value })),
  enhancement: earringSections[4].map(([level, rate, stones, money]) => ({ level, rate, stones, money })),
};

const json = {
  pentagrams, pentagramOptions: pentagramOptionSettings, errtels: [...errtels.values()], errtelSets,
  refinery: { pentagramUpgrades: mixes('PentagramUpgradeMix'), errtelUpgrades: mixes('ErrtelUpgradeMix'), pentagramUpgradeRates, errtelUpgradeRates, rankUpRates, rankUpMoney, levelUp, removalRates },
  monsters, skillImprints, earrings,
};
const out = join(project, 'Resources', 'Season21', 'Elements.json');
// One row per line, like Items.json, so a regeneration diffs by row.
const format = (value, indent) => {
  if (Array.isArray(value) && value.some(v => typeof v === 'object')) {
    return `[\n${value.map(v => `${indent} ${JSON.stringify(v)}`).join(',\n')}\n${indent}]`;
  }

  if (value && typeof value === 'object' && !Array.isArray(value)) {
    return `{\n${Object.entries(value).map(([k, v]) => `${indent} ${JSON.stringify(k)}: ${format(v, `${indent} `)}`).join(',\n')}\n${indent}}`;
  }

  return JSON.stringify(value);
};
writeFileSync(out, `${format(json, '')}\n`);
console.log(`${pentagrams.length} pentagrams, ${errtelSets.length} errtel sets, ${errtels.size} errtels, ${monsters.length} monsters, ${skillImprints.length} skill imprints, ${earrings.items.length} earrings -> ${out}`);
