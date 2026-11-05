// Generates the item catalog of Season 21 (see ../Readme.md, "Item catalog"):
//   node season21-items.mjs --data <S21 server Data/Item folder> --history <mu-online-history source.md>
// Writes Resources/Season21/Items.json and Properties/Season21CatalogItemNames.resx (+ .Designer.cs). It prints how many
// versions fall back to Season 21 Part 1; --log <file> writes their list. The output only depends on the inputs.
import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const project = join(here, '..', '..', '..');
const arg = name => { const i = process.argv.indexOf(`--${name}`); return i > 0 ? process.argv[i + 1] : undefined; };
const dataDir = arg('data') ?? process.env.S21_ITEM_DATA;
const historyFile = arg('history') ?? process.env.MU_HISTORY;
if (!dataDir || !historyFile) {
  console.error('usage: node season21-items.mjs --data <Data/Item> --history <source.md>');
  process.exit(1);
}

const read = path => readFileSync(join(dataDir, path), 'utf8').replace(/<!--[\s\S]*?-->/g, '');
const attrs = text => Object.fromEntries([...text.matchAll(/(\w+)="([^"]*)"/g)].map(m => [m[1], m[2]]));
const int = (row, key) => (row[key] === undefined || row[key] === '' ? 0 : parseInt(row[key], 10));
const key = (group, number) => group * 512 + number;
const textRows = path => read(path).split(/\r?\n/).map(l => l.replace(/\/\/.*$/, '').trim()).filter(l => l && l !== 'end').map(l => l.split(/\s+/));

const CLASSES = ['DW', 'DK', 'FE', 'MG', 'DL', 'SU', 'RF', 'GL', 'RW', 'SL', 'GC', 'KM', 'LM', 'IK', 'AL', 'CS'];

// --- Item.xml ---
const items = [];
for (const section of read('Item.xml').matchAll(/<Section Index="(\d+)"[^>]*>([\s\S]*?)<\/Section>/g)) {
  const group = +section[1];
  for (const m of section[2].matchAll(/<Item ([^>]*)\/>/g)) {
    items.push({ group, row: attrs(m[1]) });
  }
}
const seen = new Set();
for (const { group, row } of items) {
  const k = key(group, +row.Index);
  if (seen.has(k)) throw new Error(`duplicate item ${group}/${row.Index}`);
  seen.add(k);
}

// --- per item option files ---
const itemOptions = new Map();
for (const section of read('ItemOption - Settings/ItemOption.xml').matchAll(/<Section Type="(\d+)"[^>]*>([\s\S]*?)<\/Section>/g)) {
  for (const item of section[2].matchAll(/<Item Index="(\d+)"[^>]*>([\s\S]*?)<\/Item>/g)) {
    const options = [...item[2].matchAll(/<Option ([^>]*)\/>/g)].map(o => attrs(o[1])).map(o => [+o.Type, +o.Index, +o.Value]);
    itemOptions.set(key(+section[1], +item[1]), options);
  }
}
const sockets = new Map();
for (const m of read('Socket - Settings/SocketItemType.xml').matchAll(/<Item ([^>]*)\/>/g)) {
  const a = attrs(m[1]);
  sockets.set(key(+a.Section, +a.Type), +a.MaxSocket);
}
const guardian = new Set(textRows('380 Items - Setting/380ItemType.txt').map(r => key(+r[0], +r[1])));
const mastery = new Map(textRows('Mastery - Settings/MasteryItem.txt').filter(r => r.length >= 3).map(r => [+r[0], [+r[1], +r[2]]]));
const lucky = new Set();
for (const m of read('Lucky Item - Settings/LuckySystem.xml').matchAll(/<Item ([^>]*)\/>/g)) {
  const a = attrs(m[1]);
  lucky.add(key(+a.Category, +a.Index));
}
const ancient = new Map();
for (const m of read('SetItem - Settings/SetItemType.xml').matchAll(/<ItemOptionList ([^>]*)\/>/g)) {
  const a = attrs(m[1]);
  const sets = [a.OptionIndex0, a.OptionIndex1, a.OptionIndex2].map(Number).filter(s => s > 0);
  if (sets.length > 0) ancient.set(+a.Index, sets);
}
const values = new Map();
const levels = new Map();
for (const r of textRows('ItemValue.txt')) {
  if (!values.has(+r[0]) && (r[1] === '0' || r[1] === '*')) values.set(+r[0], +r[3]);
  if (r[1] !== '*') levels.set(+r[0], Math.max(levels.get(+r[0]) ?? 0, +r[1]));
}

// --- versions, from the MU Online history (Khdoop/mu-online-history, source.md) ---
const VERSIONS = {
  '1': 'Season1', '2': 'Season2', '3': 'Season3Episode1', '3+': 'Season3Episode2', '4': 'Season4Episode1', '4.5': 'Season4Episode2',
  '5 Part 1': 'Season5Episode1', '5 Part 2': 'Season5Episode2', '5 Part 3': 'Season5Episode3', '5 Part 4': 'Season5Episode4',
  '6 Part 1': 'Season6Episode1', '6 Part 2': 'Season6Episode2', '6 Part 3': 'Season6Episode3',
};
const EPISODES = { 7: [0, 1, 2], 8: [1, 2], 9: [1], 10: [2], 11: [1, 2], 12: [1, 2], 13: [1], 21: [1] };
const S0 = [[2900, 'Version029'], [3400, 'Version034'], [4300, 'Version043'], [4500, 'Version045'], [4800, 'Version048'], [6000, 'Version060'],
  [6400, 'Version064'], [6600, 'Version066'], [6800, 'Version068'], [7500, 'Version075'], [8400, 'Version084'], [8903, 'Version089c'],
  [9402, 'Version094b'], [9504, 'Version095d'], [9511, 'Version095k'], [9625, 'Version096y'], [9704, 'Version097d'], [9716, 'Version097p'],
  [9718, 'Version097r'], [9818, 'Version098r'], [9900, 'Version099'], [9933, 'Version099GPlus'], [10019, 'Version100s']];

/** The game version of a heading of the history: the episode of the same season, the nearest one where ours has none. */
function versionOf(heading) {
  if (heading.startsWith('0.') || heading.startsWith('1.0')) {
    const m = heading.match(/^(\d)\.(\d+)([a-zA-Z]?)/);
    const code = +m[1] * 10000 + +m[2].padEnd(2, '0') * 100 + (m[3] ? m[3].toLowerCase().charCodeAt(0) - 96 : 0);
    return S0.filter(([c]) => c <= code).at(-1)[1];
  }
  const ex = heading.match(/^eX70(\d)/);
  if (ex) return ['Season7', 'Season7Episode1', 'Season7Episode2'][+ex[1]];
  const s = heading.match(/^Season (\d+|X)(?: Part (\d))?/);
  if (!s) return undefined;
  const season = s[1] === 'X' ? 10 : +s[1];
  if (season <= 6) return VERSIONS[heading.replace(/^Season /, '').replace(/ \(.*$/, '').replace(/(Part \d).*/, '$1').trim()];
  const part = s[2] ? +s[2] : 1;
  const episodes = EPISODES[season] ?? [1, 2];
  const episode = episodes.reduce((best, e) => (Math.abs(e - part) < Math.abs(best - part) ? e : best), episodes[0]);
  return episode === 0 ? 'Season7' : `Season${season}Episode${episode}`;
}

const normalize = s => ` ${s.toLowerCase().replace(/\bthe\b/g, ' ').replace(/[^a-z0-9]+/g, ' ').trim()} `.replace(/ +/g, ' ');
const history = [];
{
  const text = readFileSync(historyFile, 'utf8').replace(/<sup>[\s\S]*?<\/sup>/g, '').replace(/<\/?[a-z][^>]*>/g, '');
  let current;
  for (const line of text.split(/\r?\n/)) {
    const top = line.match(/^# \*\*(.+?)\s*(\(|:\*\*)/);
    const sub = line.match(/^\*\*((?:0|1)\.\d+[a-zA-Z+]*)\b/) ?? line.match(/^\*\*(Season 9 Part 2)/);
    const heading = top?.[1] ?? sub?.[1];
    if (heading) {
      const version = versionOf(heading.trim());
      if (version) history.push(current = { version, text: '' });
      continue;
    }
    if (current) current.text += ` ${normalize(line)} `;
  }
}
const PIECES = /\s+(helm|helmet|mask|crown|hood|armor|pants|gloves|gauntlets|boots|shoes)$/i;
const TIERS = ['Bloodangel', 'Darkangel', 'Holyangel', 'Soul', 'Blue Eye', 'Silver Heart', 'Manticore', 'Brilliant', 'Apocalypse'];
function introducedIn(name, masteryTier) {
  if (/^\[(bound|speed|pc room)\]/i.test(name)) return undefined;
  const plain = normalize(name.replace(/\[[^\]]*\]/g, ' ')).trim();
  const candidates = [plain];
  if (PIECES.test(plain)) candidates.push(`${plain.replace(PIECES, '')} set`);
  if (masteryTier !== undefined) {
    const tier = normalize(TIERS[masteryTier]).trim();
    candidates.push(`mastery set ${tier}`, `mastery weapon ${tier}`);
  }
  const patterns = candidates.filter(c => c.length >= 4).map(c => ` ${c} `);
  return history.find(h => patterns.some(p => h.text.includes(p)))?.version;
}

// --- what the Season 6 item format can't carry yet ---
function waiting(group, row, name) {
  const kindA = int(row, 'KindA');
  const slot = int(row, 'Slot');
  if (group > 15) return 'group';
  if (slot === 236) return 'pentagram';
  if (slot === 237 || slot === 238) return 'earring';
  if (kindA === 76 || kindA === 106) return 'wing grade';
  if (kindA === 44 || (kindA >= 85 && kindA <= 88)) return 'errtel';
  if (kindA === 63 || kindA === 64 || kindA === 67) return 'muun';
  if (kindA === 75) return 'quiver';
  if (kindA >= 119 && kindA <= 121 || int(row, 'PCRoom') === 1 || /^\[(speed|pc room)\]/i.test(name)) return 'expiry';
  if (/\[Lvl\. ([6-9]|10)\]/.test(name)) return 'sphere level';
  return null;
}

// --- output ---
const pascal = s => s.replace(/[^A-Za-z0-9]+/g, ' ').trim().split(' ').map(w => w[0].toUpperCase() + w.slice(1)).join('') || 'Item';
const fallbacks = [];
const rows = items.map(({ group, row }) => {
  const number = +row.Index;
  const name = row.Name.trim();
  const found = introducedIn(name, mastery.get(key(group, number))?.[1]);
  if (!found) fallbacks.push(`${group}/${number} ${name}`);
  const bans = (row.Transaction === '0' ? 1 : 0) | (row.Dump === '0' ? 2 : 0) | (row.StoreWarehouse === '0' ? 4 : 0)
    | (row.PersonalStore === '0' ? 8 : 0) | (row.SellToNPC === '0' ? 16 : 0) | (row.Repair === '0' ? 32 : 0);
  const k = key(group, number);
  return {
    group, number, name, key: `${pascal(name)}_${group}_${number}`,
    version: found ?? 'Season21Episode1', versionFromHistory: !!found,
    slot: int(row, 'Slot'), skill: int(row, 'Skill'), width: int(row, 'Width'), height: int(row, 'Height'), twoHand: row.TwoHand === '1',
    dropLevel: int(row, 'Level'), durability: int(row, 'Durability'), magicDurability: int(row, 'MagicDurability'),
    damageMin: int(row, 'DamageMin'), damageMax: int(row, 'DamageMax'), speed: int(row, 'AttackSpeed'), walkSpeed: int(row, 'WalkSpeed'),
    magicPower: int(row, 'MagicDamageRate'), defense: int(row, 'Defense'), defenseRate: int(row, 'DefenseSuccessRate'), magicDefense: int(row, 'MagicDefense'),
    requirements: ['RequireLevel', 'RequireStrength', 'RequireDexterity', 'RequireEnergy', 'RequireVitality', 'RequireLeadership'].map(c => int(row, c)),
    classes: CLASSES.map(c => int(row, c)),
    kindA: int(row, 'KindA'), kindB: int(row, 'KindB'), itemGroup: int(row, 'ItemGroup'), setStat: int(row, 'SetOptionStatType'),
    takesOptions: row.Option === '1', drops: row.Drop === '1', bans, stack: int(row, 'ItemOverlap'), expensive: row.Expensive === '1',
    legendary: int(row, 'LegendaryOption'), resistances: [0, 1, 2, 3, 4, 5, 6].map(i => int(row, `Resistance${i}`)),
    value: values.get(k) ?? int(row, 'Value'), buyMoney: int(row, 'BuyMoney'), maximumLevel: levels.get(k) ?? 0,
    options: itemOptions.get(k) ?? [], sockets: sockets.get(k) ?? 0, guardian: guardian.has(k), mastery: mastery.get(k) ?? null,
    lucky: lucky.has(k), ancientSets: ancient.get(k) ?? [], waitsFor: waiting(group, row, name),
  };
});

const keys = new Set();
for (const r of rows) { if (keys.has(r.key)) throw new Error(`duplicate key ${r.key}`); keys.add(r.key); }

const isDefault = v => v === 0 || v === false || v === null || (Array.isArray(v) && v.every(x => x === 0));
const compact = r => Object.fromEntries(Object.entries(r).filter(([k, v]) => (k === 'mastery' ? v !== null : !isDefault(v))));
writeFileSync(join(project, 'Resources', 'Season21', 'Items.json'), `[\n${rows.map(r => JSON.stringify(compact(r))).join(',\n')}\n]\n`);
if (arg('log')) writeFileSync(arg('log'), `${fallbacks.join('\n')}\n`);

const xml = s => s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
writeFileSync(join(project, 'Properties', 'Season21CatalogItemNames.resx'), `<?xml version="1.0" encoding="utf-8"?>
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
${rows.map(r => `  <data name="${r.key}" xml:space="preserve">\n    <value>${xml(r.name)}</value>\n  </data>`).join('\n')}
</root>
`);
writeFileSync(join(project, 'Properties', 'Season21CatalogItemNames.Designer.cs'), `//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by VersionSeasonSix/Season21/Generator/season21-items.mjs.
//     The names are looked up by the keys of the item catalog, so there are no properties.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MUnique.OpenMU.Persistence.Initialization.Properties {
    using System;

    /// <summary>
    ///   The names of the items of the Season 21 item catalog, by the key of each item.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("season21-items.mjs", "1.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Season21CatalogItemNames {

        private static System.Resources.ResourceManager resourceMan;

        internal Season21CatalogItemNames() {
        }

        /// <summary>
        ///   Gets the cached ResourceManager instance used by this class.
        /// </summary>
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    resourceMan = new System.Resources.ResourceManager("MUnique.OpenMU.Persistence.Initialization.Properties.Season21CatalogItemNames", typeof(Season21CatalogItemNames).Assembly);
                }
                return resourceMan;
            }
        }
    }
}
`);

const count = (pred) => rows.filter(pred).length;
console.log(`items: ${rows.length}; versions from the history: ${count(r => r.versionFromHistory)}; Season 21 fallback: ${fallbacks.length}; waiting for the wide format: ${count(r => r.waitsFor)}`);
