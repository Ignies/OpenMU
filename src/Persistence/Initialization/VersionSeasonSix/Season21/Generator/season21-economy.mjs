// Generates the economy data of Season 21 (see ../Readme.md, "Economy"):
//   node season21-economy.mjs --data <S21 server Data folder>
// Writes Resources/Season21/Economy.json: the NPC shops with their tabs (Shop/, ShopManager.txt), the Ruud shop
// (ShopRuud/RuudShop.txt), the Ruud of monsters (EventItemBagManager.txt, EventItemBag/) and of the Ruud boxes
// (Item/Item.xml names). The output only depends on the inputs.
import { readFileSync, readdirSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const project = join(here, '..', '..', '..');
const arg = name => { const i = process.argv.indexOf(`--${name}`); return i > 0 ? process.argv[i + 1] : undefined; };
const dataDir = arg('data') ?? process.env.S21_DATA;
if (!dataDir) {
  console.error('usage: node season21-economy.mjs --data <Data>');
  process.exit(1);
}

const read = path => readFileSync(join(dataDir, path), 'utf8').replace(/<!--[\s\S]*?-->/g, '');
const attrs = text => Object.fromEntries([...text.matchAll(/(\w+)="([^"]*)"/g)].map(m => [m[1], m[2]]));
const textRows = path => read(path).split(/\r?\n/).map(l => l.replace(/\/\/.*$/, '').trim()).filter(Boolean).map(l => l.split(/\s+/));

// --- NPC shops: ShopManager.txt maps the shop file index to the NPC number; TabInfo 1-5 are our tabs 0-4 ---
const npcOfShop = new Map(textRows('ShopManager.txt').filter(r => r.length >= 2 && /^\d+$/.test(r[0])).map(r => [+r[0], +r[1]]));
const shops = [];
for (const file of readdirSync(join(dataDir, 'Shop')).filter(f => f.endsWith('.xml')).sort()) {
  const npc = npcOfShop.get(parseInt(file, 10));
  if (npc === undefined) continue;
  for (const tab of read(join('Shop', file)).matchAll(/<TabInfo Index="(\d+)">([\s\S]*?)<\/TabInfo>/g)) {
    for (const item of tab[2].matchAll(/<Item ([^>]*)\/>/g)) {
      const a = attrs(item[1]);
      shops.push({ n: npc, t: +tab[1] - 1, g: +a.Section, i: +a.Type, l: +a.Level, d: +a.Durability, s: a.Skill === '1' ? 1 : 0, k: a.Luck === '1' ? 1 : 0, o: +a.Option });
    }
  }
}

// --- Ruud shop: section 1 is for every class, sections 2-17 for the base classes 0-15 ---
const ruudShop = [];
let section = null;
for (const row of read('ShopRuud/RuudShop.txt').split(/\r?\n/).map(l => l.replace(/\/\/.*$/, '').trim()).filter(Boolean)) {
  if (row === 'end') { section = null; continue; }
  const p = row.split(/\s+/).map(Number);
  if (p.length === 1) { section = section === null ? p[0] : section; continue; }
  if (section === 1) ruudShop.push({ c: null, g: p[0], i: p[1], l: 0, n: p[2], lc: 0, oc: 0, ol: 0, p: p[3] });
  else if (section >= 2 && section <= 17) ruudShop.push({ c: section - 2, g: p[0], i: p[1], l: p[2], n: p[3], lc: p[4], oc: p[5], ol: p[6], p: p[7] });
}

// --- Ruud of monsters: the item bags of a monster class with a Ruud range ---
const bagFiles = readdirSync(join(dataDir, 'EventItemBag'));
const ruudMonsters = [];
for (const row of textRows('EventItemBagManager.txt').filter(r => r.length >= 5 && /^\d+$/.test(r[0]) && r[3] !== '*')) {
  const file = bagFiles.find(f => parseInt(f, 10) === +row[0]);
  if (!file) continue;
  const ranges = [...read(join('EventItemBag', file)).matchAll(/<RuudInfo ([^>]*)\/>/g)].map(m => attrs(m[1])).filter(r => +r.RuudMax > 0);
  if (ranges.length === 0) continue;
  ruudMonsters.push({ m: +row[3], min: Math.min(...ranges.map(r => +r.RuudMin)), max: Math.max(...ranges.map(r => +r.RuudMax)) });
}

// --- Ruud boxes: the amount is in the name, e.g. "Ruud Box [1,000]" ---
const ruudBoxes = [];
for (const s of read('Item/Item.xml').matchAll(/<Section Index="(\d+)"[^>]*>([\s\S]*?)<\/Section>/g)) {
  for (const item of s[2].matchAll(/<Item ([^>]*)\/>/g)) {
    const a = attrs(item[1]);
    const amount = /^Ruud Box \[([\d,]+)\]$/.exec(a.Name ?? '');
    if (amount) ruudBoxes.push({ g: +s[1], i: +a.Index, a: +amount[1].replace(/,/g, '') });
  }
}

// --- Level rewards: Character/ItemCompensationLevel.xml; Index is group * 512 + number; class columns by base class ---
const CLASS_CODES = { DW: 0, DK: 1, FE: 2, MG: 3, DL: 4, SU: 5, RF: 6, GL: 7, RW: 8, SL: 9, GC: 10, KM: 11, LM: 12, IK: 13, AL: 14, CS: 15 };
const levelRewards = [...read('Character/ItemCompensationLevel.xml').matchAll(/<Reward ([^>]*)\/>/g)].map(m => attrs(m[1])).map(a => {
  const classes = Object.entries(CLASS_CODES).filter(([name]) => a[name] !== '0').map(([, code]) => code);
  return { id: +a.rewardID + 1, lv: +a.RewardLevel, g: Math.floor(+a.Index / 512), i: +a.Index % 512, l: +a.Level, n: +a.Durability, c: classes.length === 16 ? [] : classes };
});

// --- Event contribution: Event/EventMapPointIntegration.xml; the attributes per event become the points of a rule ---
const integration = read('Event/EventMapPointIntegration.xml');
const contributionBoxes = [...integration.matchAll(/<BoxInfo ([^>]*)\/>/g)].map(m => attrs(m[1]))
  .map(a => ({ g: Math.floor(+a.ItemIndex / 512), i: +a.ItemIndex % 512, n: +a.Count, min: +a.MinPoint, max: Math.max(+a.MaxPoint, 0) }));
const event = name => attrs((new RegExp(`<${name} ([^>]*)/>`).exec(integration) ?? [null, ''])[1]);
const bc = event('BloodCastle'), cc = event('ChaosCastle'), ds = event('DevilSquare'), it = event('IllusionTempleRenewal'), ig = event('ImperialGuardian'), dg = event('DoppelGanger');
// Special monsters: Blood Castle gate 131 and statue 132, Imperial Guardian bosses 504-511, Doppelganger ice walker 531.
// Devil Square: points per rank (r); Illusion Temple: taking the relic from its statue (StatueKillPoints) and bringing it home
// (StatueDefeatPoints) as relic claim (rc) and relic score (rs).
const contributionRules = [
  { t: 'BloodCastle', m: +bc.MonsterKillPoints, sun: 0, w: 0, p: 0, r: [], rc: 0, rs: 0, s: [[131, +bc.DoorPoints], [132, +bc.SaintStatuePoints]] },
  { t: 'ChaosCastle', m: +cc.MonsterKillPoints, sun: 0, w: +cc.WinnerPoint, p: +cc.PlayerKillPoint, r: [], rc: 0, rs: 0, s: [] },
  { t: 'DevilSquare', m: +ds.MonsterKillPoints, sun: 0, w: 0, p: 0, r: [+ds.Rank1, +ds.Rank2, +ds.Rank3], rc: 0, rs: 0, s: [] },
  { t: 'IllusionTemple', m: 0, sun: 0, w: +it.WinnerPoints, p: +it.KillPoints, r: [], rc: +it.StatueKillPoints, rs: +it.StatueDefeatPoints, s: [] },
  { t: 'ImperialGuardian', m: +ig.MonsterKillPointsWeekdays, sun: +ig.MonsterKillPointsSunday, w: 0, p: 0, r: [], rc: 0, rs: 0, s: [504, 505, 506, 507, 508, 509, 510, 511].map(n => [n, +ig.BossKillPointWeekdays]) },
  { t: 'Doppelganger', m: +dg.MonsterKillPoint, sun: 0, w: +dg.WinnerPoints, p: +dg.PlayerKillPoint, r: [], rc: 0, rs: 0, s: [[531, +dg.IceWalkerKill]] },
];

const json = { shops, ruudShop, ruudMonsters, ruudBoxes, levelRewards, contributionBoxes, contributionRules };
const lines = Object.entries(json).map(([k, rows]) => `  "${k}": [\n${rows.map(r => `    ${JSON.stringify(r)}`).join(',\n')}\n  ]`);
writeFileSync(join(project, 'Resources', 'Season21', 'Economy.json'), `{\n${lines.join(',\n')}\n}\n`);
console.log(`shops ${shops.length}, Ruud shop ${ruudShop.length}, Ruud monsters ${ruudMonsters.length}, Ruud boxes ${ruudBoxes.length}`);
