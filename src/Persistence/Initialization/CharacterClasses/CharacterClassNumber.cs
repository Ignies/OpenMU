// <copyright file="CharacterClassNumber.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

/// <summary>
/// The default character class numbers.
/// </summary>
public enum CharacterClassNumber : byte
{
    /// <summary>
    /// The dark wizard.
    /// </summary>
    DarkWizard = 0,

    /// <summary>
    /// The soul master.
    /// </summary>
    SoulMaster = 2,

    /// <summary>
    /// The grand master.
    /// </summary>
    GrandMaster = 3,

    /// <summary>
    /// The dark knight.
    /// </summary>
    DarkKnight = 4,

    /// <summary>
    /// The blade knight.
    /// </summary>
    BladeKnight = 6,

    /// <summary>
    /// The blade master.
    /// </summary>
    BladeMaster = 7,

    /// <summary>
    /// The fairy elf.
    /// </summary>
    FairyElf = 8,

    /// <summary>
    /// The muse elf.
    /// </summary>
    MuseElf = 10,

    /// <summary>
    /// The high elf.
    /// </summary>
    HighElf = 11,

    /// <summary>
    /// The magic gladiator.
    /// </summary>
    MagicGladiator = 12,

    /// <summary>
    /// The duel master.
    /// </summary>
    DuelMaster = 13,

    /// <summary>
    /// The dark lord.
    /// </summary>
    DarkLord = 16,

    /// <summary>
    /// The lord emperor.
    /// </summary>
    LordEmperor = 17,

    /// <summary>
    /// The summoner.
    /// </summary>
    Summoner = 20,

    /// <summary>
    /// The bloody summoner.
    /// </summary>
    BloodySummoner = 22,

    /// <summary>
    /// The dimension master.
    /// </summary>
    DimensionMaster = 23,

    /// <summary>
    /// The rage fighter.
    /// </summary>
    RageFighter = 24,

    /// <summary>
    /// The fist master.
    /// </summary>
    FistMaster = 25,

    /// <summary>
    /// The soul wizard, number 131 = 128 + class code (fork worlds).
    /// </summary>
    SoulWizard = 131,

    /// <summary>
    /// The darkness wizard, number 132 = 128 + class code (fork worlds).
    /// </summary>
    DarknessWizard = 132,

    /// <summary>
    /// The dragon knight, number 139 = 128 + class code (fork worlds).
    /// </summary>
    DragonKnight = 139,

    /// <summary>
    /// The ignition knight, number 140 = 128 + class code (fork worlds).
    /// </summary>
    IgnitionKnight = 140,

    /// <summary>
    /// The noble elf, number 147 = 128 + class code (fork worlds).
    /// </summary>
    NobleElf = 147,

    /// <summary>
    /// The royal elf, number 148 = 128 + class code (fork worlds).
    /// </summary>
    RoyalElf = 148,

    /// <summary>
    /// The magic knight, number 155 = 128 + class code (fork worlds).
    /// </summary>
    MagicKnight = 155,

    /// <summary>
    /// The duple knight, number 156 = 128 + class code (fork worlds).
    /// </summary>
    DupleKnight = 156,

    /// <summary>
    /// The empire lord, number 163 = 128 + class code (fork worlds).
    /// </summary>
    EmpireLord = 163,

    /// <summary>
    /// The force empire, number 164 = 128 + class code (fork worlds).
    /// </summary>
    ForceEmpire = 164,

    /// <summary>
    /// The dimension summoner, number 171 = 128 + class code (fork worlds).
    /// </summary>
    DimensionSummoner = 171,

    /// <summary>
    /// The endless summoner, number 172 = 128 + class code (fork worlds).
    /// </summary>
    EndlessSummoner = 172,

    /// <summary>
    /// The fist blazer, number 179 = 128 + class code (fork worlds).
    /// </summary>
    FistBlazer = 179,

    /// <summary>
    /// The bloody fighter, number 180 = 128 + class code (fork worlds).
    /// </summary>
    BloodyFighter = 180,

    /// <summary>
    /// The grow lancer, number 184 = 128 + class code (fork worlds).
    /// </summary>
    GrowLancer = 184,

    /// <summary>
    /// The mirage lancer, number 186 = 128 + class code (fork worlds).
    /// </summary>
    MirageLancer = 186,

    /// <summary>
    /// The shining lancer, number 187 = 128 + class code (fork worlds).
    /// </summary>
    ShiningLancer = 187,

    /// <summary>
    /// The arcane lancer, number 188 = 128 + class code (fork worlds).
    /// </summary>
    ArcaneLancer = 188,

    /// <summary>
    /// The rune wizard, number 192 = 128 + class code (fork worlds).
    /// </summary>
    RuneWizard = 192,

    /// <summary>
    /// The rune spell master, number 193 = 128 + class code (fork worlds).
    /// </summary>
    RuneSpellMaster = 193,

    /// <summary>
    /// The rune grand master, number 194 = 128 + class code (fork worlds).
    /// </summary>
    RuneGrandMaster = 194,

    /// <summary>
    /// The majestic rune mage, number 195 = 128 + class code (fork worlds).
    /// </summary>
    MajesticRuneMage = 195,

    /// <summary>
    /// The infinity rune wizard, number 196 = 128 + class code (fork worlds).
    /// </summary>
    InfinityRuneWizard = 196,

    /// <summary>
    /// The slayer, number 200 = 128 + class code (fork worlds).
    /// </summary>
    Slayer = 200,

    /// <summary>
    /// The royal slayer, number 201 = 128 + class code (fork worlds).
    /// </summary>
    RoyalSlayer = 201,

    /// <summary>
    /// The master slayer, number 202 = 128 + class code (fork worlds).
    /// </summary>
    MasterSlayer = 202,

    /// <summary>
    /// The slaughterer, number 203 = 128 + class code (fork worlds).
    /// </summary>
    Slaughterer = 203,

    /// <summary>
    /// The rogue slayer, number 204 = 128 + class code (fork worlds).
    /// </summary>
    RogueSlayer = 204,

    /// <summary>
    /// The gun crusher, number 208 = 128 + class code (fork worlds).
    /// </summary>
    GunCrusher = 208,

    /// <summary>
    /// The gun breaker, number 209 = 128 + class code (fork worlds).
    /// </summary>
    GunBreaker = 209,

    /// <summary>
    /// The master gun breaker, number 210 = 128 + class code (fork worlds).
    /// </summary>
    MasterGunBreaker = 210,

    /// <summary>
    /// The heist gun crusher, number 211 = 128 + class code (fork worlds).
    /// </summary>
    HeistGunCrusher = 211,

    /// <summary>
    /// The magnus gun crusher, number 212 = 128 + class code (fork worlds).
    /// </summary>
    MagnusGunCrusher = 212,

    /// <summary>
    /// The light wizard, number 216 = 128 + class code (fork worlds).
    /// </summary>
    LightWizard = 216,

    /// <summary>
    /// The light master, number 217 = 128 + class code (fork worlds).
    /// </summary>
    LightMaster = 217,

    /// <summary>
    /// The shine wizard, number 218 = 128 + class code (fork worlds).
    /// </summary>
    ShineWizard = 218,

    /// <summary>
    /// The luminous wizard, number 219 = 128 + class code (fork worlds).
    /// </summary>
    LuminousWizard = 219,

    /// <summary>
    /// The glory wizard, number 220 = 128 + class code (fork worlds).
    /// </summary>
    GloryWizard = 220,

    /// <summary>
    /// The lemuria mage, number 224 = 128 + class code (fork worlds).
    /// </summary>
    LemuriaMage = 224,

    /// <summary>
    /// The war mage, number 225 = 128 + class code (fork worlds).
    /// </summary>
    WarMage = 225,

    /// <summary>
    /// The archmage, number 226 = 128 + class code (fork worlds).
    /// </summary>
    Archmage = 226,

    /// <summary>
    /// The mystic mage, number 227 = 128 + class code (fork worlds).
    /// </summary>
    MysticMage = 227,

    /// <summary>
    /// The battle mage, number 228 = 128 + class code (fork worlds).
    /// </summary>
    BattleMage = 228,

    /// <summary>
    /// The illusion knight, number 232 = 128 + class code (fork worlds).
    /// </summary>
    IllusionKnight = 232,

    /// <summary>
    /// The mirage knight, number 233 = 128 + class code (fork worlds).
    /// </summary>
    MirageKnight = 233,

    /// <summary>
    /// The illusion master, number 234 = 128 + class code (fork worlds).
    /// </summary>
    IllusionMaster = 234,

    /// <summary>
    /// The mystic knight, number 235 = 128 + class code (fork worlds).
    /// </summary>
    MysticKnight = 235,

    /// <summary>
    /// The phantom pain knight, number 236 = 128 + class code (fork worlds).
    /// </summary>
    PhantomPainKnight = 236,

    /// <summary>
    /// The alchemist, number 240 = 128 + class code (fork worlds).
    /// </summary>
    Alchemist = 240,

    /// <summary>
    /// The alchemist mage, number 241 = 128 + class code (fork worlds).
    /// </summary>
    AlchemistMage = 241,

    /// <summary>
    /// The alchemist master, number 242 = 128 + class code (fork worlds).
    /// </summary>
    AlchemistMaster = 242,

    /// <summary>
    /// The alchemist force, number 243 = 128 + class code (fork worlds).
    /// </summary>
    AlchemistForce = 243,

    /// <summary>
    /// The alchemist creator, number 244 = 128 + class code (fork worlds).
    /// </summary>
    AlchemistCreator = 244,

    /// <summary>
    /// The crusader, number 248 = 128 + class code (fork worlds).
    /// </summary>
    Crusader = 248,

    /// <summary>
    /// The impact crusader, number 249 = 128 + class code (fork worlds).
    /// </summary>
    ImpactCrusader = 249,

    /// <summary>
    /// The master paladin, number 250 = 128 + class code (fork worlds).
    /// </summary>
    MasterPaladin = 250,

    /// <summary>
    /// The sacred paladin, number 251 = 128 + class code (fork worlds).
    /// </summary>
    SacredPaladin = 251,

    /// <summary>
    /// The templar commander, number 252 = 128 + class code (fork worlds).
    /// </summary>
    TemplarCommander = 252,
}
