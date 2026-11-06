// Generates the world systems data of Season 21 (see ../Readme.md, "World systems"):
//   node season21-world.mjs --data <S21 server Data folder>
// Writes Resources/Season21/WorldSystems.json: the hunting record maps (HuntingRecord.txt), the monster soul converter
// (MonsterSoulConverter/, EventItemBag/ 288-355), the hunting zones of the helper plus (Character/MuHelperPlus.xml) and the
// season pass (Event/BattlePass/BattlePass.xml). The output only depends on the inputs.
import { readFileSync, readdirSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const project = join(here, '..', '..', '..');
const arg = name => { const i = process.argv.indexOf(`--${name}`); return i > 0 ? process.argv[i + 1] : undefined; };
const dataDir = arg('data') ?? process.env.S21_DATA;
if (!dataDir) {
  console.error('usage: node season21-world.mjs --data <Data>');
  process.exit(1);
}

const read = path => readFileSync(join(dataDir, path), 'utf8').replace(/<!--[\s\S]*?-->/g, '');
const attrs = text => Object.fromEntries([...text.matchAll(/(\w+)="([^"]*)"/g)].map(m => [m[1], m[2]]));
const sections = path => {
  // A text file of numbered sections: "<n>" then rows up to "end".
  const result = new Map();
  let current = null;
  for (const line of read(path).split(/\r?\n/).map(l => l.replace(/\/\/.*$/, '').trim()).filter(Boolean)) {
    if (line === 'end') { current = null; continue; }
    const cells = line.match(/"[^"]*"|\S+/g);
    if (current === null && cells.length === 1 && /^\d+$/.test(cells[0])) { current = +cells[0]; result.set(current, []); continue; }
    if (current !== null) result.get(current).push(cells.map(c => c.replace(/^"|"$/g, '')));
  }
  return result;
};
const item = index => ({ g: Math.floor(index / 512), i: index % 512 });

// --- Hunting record: section 0 the maps, section 1 the monsters which don't count ---
const hunting = sections('HuntingRecord.txt');
const huntingRecord = {
  maps: [...new Set(hunting.get(0).map(r => +r[0]))],
  excluded: hunting.get(1).map(r => +r[0]),
};

// --- Monster soul converter: purchases a week, the converter items, the missions per tier, the reward bags ---
const conversion = sections(join('MonsterSoulConverter', 'MonsterSoulConversion.txt'));
const missions = sections(join('MonsterSoulConverter', 'MonsterSoulConversionMissionInfo.txt'));
const tierLevels = { 1: 400, 2: 800, 3: 1150, 4: 1250, 5: 1450 }; // the comments of both files ("Level 400 or higher" ...)
const bagFiles = readdirSync(join(dataDir, 'EventItemBag'));
const bagFile = (number, name) => bagFiles.find(f => parseInt(f, 10) === number && (!name || f.includes(name)));
const rewardBags = {
  ruud: [288, 344, 347, 350, 353],
  jewels: [289, 345, 348, 351, 354],
  options: [290, 346, 349, 352, 355],
  accessory: [null, null, null, null, 291],
};
const readBag = file => {
  const text = read(join('EventItemBag', file));
  const configs = new Map([...text.matchAll(/<Config ([^>]*)\/>/g)].map(m => attrs(m[1])).map(a => [a.Index, +a.DropRate]));
  const ruud = new Map([...text.matchAll(/<RuudInfo ([^>]*)\/>/g)].map(m => attrs(m[1])).map(a => [a.Section, [+a.RuudMin, +a.RuudMax]]));
  const items = [...text.matchAll(/<ItemEx ([^>]*)\/>/g)].map(m => attrs(m[1]));
  const draws = [];
  for (const info of [...text.matchAll(/<DropInfo ([^>]*)\/>/g)].map(m => attrs(m[1]))) {
    const chance = Math.min(configs.get(info.Index) ?? 0, +info.SectionRate);
    if (chance <= 0 || info.Section === '*') continue;
    const [ruudMin, ruudMax] = ruud.get(info.Section) ?? [0, 0];
    const choices = items.filter(i => i.Section === info.Section).map(i => ({ ...item(+i.Index), l: +i.Level, d: +i.Durability }));
    if (choices.length === 0 && ruudMax <= 0) continue;
    draws.push({ c: chance, items: choices, ruudMin, ruudMax });
  }
  return draws;
};
const soulConverter = {
  purchases: +conversion.get(0)[0][0],
  tiers: conversion.get(1).map(r => {
    const tier = +r[0];
    const rewards = Object.entries(rewardBags).filter(([, bags]) => bags[tier - 1] !== null)
      .map(([kind, bags]) => ({ kind, draws: readBag(bagFile(bags[tier - 1], kind === 'accessory' ? 'Accessory 5' : undefined)) }));
    return {
      tier, g: +r[1], i: +r[2], missions: +r[3], level: tierLevels[tier],
      monsters: missions.get(tier).map(m => ({ map: +m[1], monster: +m[2], souls: +m[3], rate: +m[4] })),
      rewards,
    };
  }),
};

// --- Helper plus: the hunting grounds (with their move list entry) and the zones of each ground ---
const helper = read(join('Character', 'MuHelperPlus.xml'));
const helperPlus = {
  grounds: [...helper.matchAll(/<Grounds ([^>]*)\/>/g)].map(m => attrs(m[1])).map(a => ({ index: +a.Index, move: +a.MoveReqIndex, name: a.Name })),
  zones: [...helper.matchAll(/<Info Index="\d+" GroundsIndex[^>]*\/>/g)].map(m => attrs(m[0])).map(a => ({
    index: +a.Index, ground: +a.GroundsIndex, map: +a.MapIndex, zone: +a.HuntingZone, element: +a.HuntingElement,
    x1: +a.StartX, y1: +a.StartY, x2: +a.EndX, y2: +a.EndY, x: +a.HuntingCordX, y: +a.HuntingCordY,
  })),
};

// --- Season pass: switches, schedule, levels, daily missions, rewards per pass and level, the buff values ---
const pass = read(join('Event', 'BattlePass', 'BattlePass.xml'));
const head = attrs(pass.match(/<BattlePass ([^>]*)>/)[1]);
const block = name => (pass.match(new RegExp(`<${name}>([\\s\\S]*?)</${name}>`)) ?? ['', ''])[1];
const infos = text => [...text.matchAll(/<Info ([^>]*)\/>/g)].map(m => attrs(m[1]));
const seasonPass = {
  gold: head.EnableGoldPass === '1',
  premium: head.EnablePremiumPass === '1',
  ticketExperience: +head.PassTicketExperiencie,
  schedule: infos(block('Schedule')).map(a => ({ id: +a.ID, start: a.StartDate, end: a.EndDate })),
  levels: infos(block('Level')).map(a => +a.MaxExperience),
  missions: infos(block('DailyMission')).map(a => ({ index: +a.Index, experience: +a.MissionExperience, required: +a.MissionRequiere, repeat: +a.MissionRepeat })),
  special: infos(block('SpecialMission')).map(a => ({ index: +a.Index, experience: +a.MissionExp, required: +a.MissionRequiere, repeat: +a.MissionRepeat })),
  rewards: infos(block('Reward')).map(a => ({ id: +a.ID, type: +a.Type, effect: +a.EffectIndex, g: +a.ISection, i: +a.IType, n: +a.ICount, pass: +a.PassType, name: a.Name })),
  lists: [...pass.matchAll(/<PassInfo ID="(\d+)">([\s\S]*?)<\/PassInfo>/g)].map(m => ({
    pass: +m[1],
    levels: infos(m[2]).map(a => [0, 1, 2, 3, 4, 5].map(k => +a[`Reward${k}`]).filter(r => r >= 0)),
  })),
  buffs: [...pass.matchAll(/<EffectInfo ([^>]*)>([\s\S]*?)<\/EffectInfo>/g)].map(m => ({
    effect: +attrs(m[1]).EffectIndex,
    values: [...m[2].matchAll(/<LevelInfo ([^>]*)\/>/g)].map(l => +attrs(l[1]).Value),
  })),
};

const output = join(project, 'Resources', 'Season21', 'WorldSystems.json');
writeFileSync(output, JSON.stringify({ huntingRecord, soulConverter, helperPlus, seasonPass }));
console.log(`hunting maps ${huntingRecord.maps.length}, excluded ${huntingRecord.excluded.length}; converter tiers ${soulConverter.tiers.length}; `
  + `helper grounds ${helperPlus.grounds.length}, zones ${helperPlus.zones.length}; pass levels ${seasonPass.levels.length}, missions ${seasonPass.missions.length}, rewards ${seasonPass.rewards.length}`);
