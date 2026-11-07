// <copyright file="ArcaWarTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.ArcaWar;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views.NPC;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;

/// <summary>
/// The Arca War of Season 21 (S21Emu <c>Event/ArcaBattle.xml</c>, webzen guide): guilds register, fight over three
/// obelisks on Acheron and Deventer, and keep the buff of the obelisks they hold at the end.
/// </summary>
[TestFixture]
public class ArcaWarTest
{
    private static readonly DateTime Start = new(2026, 11, 9, 10, 45, 0, DateTimeKind.Utc);

    private Season21EventWorld _world = null!;

    private ArcaWarPlugIn _plugIn = null!;

    private DateTime _now;

    private uint _nextGuildId = 100;

    private ArcaWarContext Context => this._plugIn.GetContext(this._world.GameContext);

    /// <summary>
    /// Initializes the world with the Acheron maps and the war.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._world = await Season21EventWorld.CreateAsync(new AddSeason21AcheronGuardianUpdatePlugIn(), new AddSeason21ArcaWarUpdatePlugIn()).ConfigureAwait(false);
        this._world.GameContext.ServerTimeZone = TimeZoneInfo.Utc;
    }

    /// <summary>
    /// Creates a new war, a minute before its start.
    /// </summary>
    [SetUp]
    public void SetupWar()
    {
        var configuration = this._world.GetStoredConfiguration<ArcaWarConfiguration>(typeof(ArcaWarPlugIn).GUID)!;
        configuration.StateFile = string.Empty;
        this._now = Start.AddMinutes(-1);
        this._plugIn = new ArcaWarPlugIn { Configuration = configuration, UtcNow = () => this._now };
        this._world.AddFeaturePlugIn(this._plugIn);
        this._world.Register<IPlayerAttackRestrictionPlugIn>(this._plugIn);
        this._world.Register<IPlayerKillPenaltyExemptionPlugIn>(this._plugIn);
        this._world.Register<IPlayerDamageFactorPlugIn>(this._plugIn);
        this._world.Register<IObjectAddedToMapPlugIn>(this._plugIn);
        this._world.Register<IObjectRemovedFromMapPlugIn>(this._plugIn);
    }

    /// <summary>
    /// The fork marker names the war.
    /// </summary>
    [Test]
    public void ForkNamesTheFeature() => Assert.That(ForkInfo.Has(ArcaWarPlugIn.FeatureId), Is.True);

    /// <summary>
    /// The masters register their guilds in the first minute, the members in the second; then the war waits a minute.
    /// </summary>
    [Test]
    public async ValueTask RegistrationFollowsTheStagesAsync()
    {
        var master = await this.CreateMemberAsync(GuildPosition.GuildMaster).ConfigureAwait(false);
        var member = await this.CreateMemberAsync(GuildPosition.NormalMember, master).ConfigureAwait(false);
        var stranger = await this.CreateMemberAsync(GuildPosition.NormalMember).ConfigureAwait(false);

        await this.AdvanceAsync(TimeSpan.Zero).ConfigureAwait(false);
        Assert.That(this.Context.Stage, Is.EqualTo(ArcaWarStage.Closed));
        Assert.That(await this._plugIn.RequestAsync(master, ArcaWarRequestKind.RegisterGuild).ConfigureAwait(false), Is.EqualTo(ArcaWarRequestResult.NotOpen));

        await this.AdvanceAsync(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
        Assert.That(this.Context.Stage, Is.EqualTo(ArcaWarStage.MasterRegistration));
        Assert.That(await this._plugIn.RequestAsync(member, ArcaWarRequestKind.RegisterGuild).ConfigureAwait(false), Is.EqualTo(ArcaWarRequestResult.NotGuildMaster));
        Assert.That(await this._plugIn.RequestAsync(member, ArcaWarRequestKind.RegisterMember).ConfigureAwait(false), Is.EqualTo(ArcaWarRequestResult.NotOpen));
        Assert.That(await this._plugIn.RequestAsync(master, ArcaWarRequestKind.RegisterGuild).ConfigureAwait(false), Is.EqualTo(ArcaWarRequestResult.Success));
        Assert.That(await this._plugIn.RequestAsync(master, ArcaWarRequestKind.RegisterGuild).ConfigureAwait(false), Is.EqualTo(ArcaWarRequestResult.AlreadyRegistered));

        await this.AdvanceAsync(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
        Assert.That(this.Context.Stage, Is.EqualTo(ArcaWarStage.MemberRegistration));
        Assert.That(await this._plugIn.RequestAsync(member, ArcaWarRequestKind.RegisterMember).ConfigureAwait(false), Is.EqualTo(ArcaWarRequestResult.Success));
        Assert.That(await this._plugIn.RequestAsync(member, ArcaWarRequestKind.RegisterMember).ConfigureAwait(false), Is.EqualTo(ArcaWarRequestResult.AlreadyRegistered));
        Assert.That(await this._plugIn.RequestAsync(stranger, ArcaWarRequestKind.RegisterMember).ConfigureAwait(false), Is.EqualTo(ArcaWarRequestResult.GuildNotRegistered));

        await this.AdvanceAsync(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
        Assert.That(this.Context.Stage, Is.EqualTo(ArcaWarStage.Waiting));
        Assert.That(this.Context.Guilds.Single().Members, Has.Count.EqualTo(2));
    }

    /// <summary>
    /// The war spawns the three obelisks with their health of the data and moves the registered members to the spawns of
    /// their group.
    /// </summary>
    [Test]
    public async ValueTask WarSpawnsTheObelisksAndMovesTheMembersAsync()
    {
        var (master, _) = await this.StartWarAsync().ConfigureAwait(false);

        Assert.That(this.Context.Stage, Is.EqualTo(ArcaWarStage.Playing));
        Assert.That(this.Context.Obelisks.Select(o => (o.Definition.MapNumber, o.Definition.X, o.Definition.Y)), Is.EqualTo(new[] { ((short)92, (byte)89, (byte)41), ((short)92, (byte)188, (byte)101), ((short)96, (byte)162, (byte)54) }));
        Assert.That(this.Context.Obelisks.Select(o => o.Npc?.Health), Has.All.EqualTo(500_000));
        Assert.That(this.Context.Obelisks.Select(o => o.Element), Has.None.EqualTo(ArcaElement.None));
        Assert.That(this.Context.Monsters, Has.Count.EqualTo(30), "ten monsters in each of the three groups");
        await Season21EventWorld.CompleteMapChangeAsync(master).ConfigureAwait(false);
        Assert.That(master.CurrentMap?.Definition.Number, Is.EqualTo((short)92));
    }

    /// <summary>
    /// The shield of an obelisk drops for a guild after its members stood on all three auras for the capture seconds;
    /// only that guild can then hurt it.
    /// </summary>
    [Test]
    public async ValueTask ShieldDropsForTheGuildOnAllThreeAurasAsync()
    {
        var (master, members) = await this.StartWarAsync(3).ConfigureAwait(false);
        var (enemy, _) = await this.RegisterGuildAsync(0, false).ConfigureAwait(false);
        var obelisk = this.Context.Obelisks[0];
        var auras = obelisk.Definition.Auras;
        for (var i = 0; i < 3; i++)
        {
            await this.PlaceAsync(members[i], 92, auras[i].X, auras[i].Y).ConfigureAwait(false);
        }

        await this.PlaceAsync(master, 92, (byte)(obelisk.Definition.X + 1), obelisk.Definition.Y).ConfigureAwait(false);
        await this.PlaceAsync(enemy, 92, (byte)(obelisk.Definition.X - 1), obelisk.Definition.Y).ConfigureAwait(false);

        await this.AdvanceAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        Assert.That(obelisk.IsOpenFor(master.GuildStatus!.GuildId), Is.False, "the shield needs the capture time");
        Assert.That(await obelisk.Npc!.AttackByAsync(master, null, false).ConfigureAwait(false), Is.Null);

        await this.AdvanceAsync(TimeSpan.FromSeconds(this._plugIn.Configuration!.CaptureSeconds)).ConfigureAwait(false);
        Assert.That(obelisk.IsOpenFor(master.GuildStatus.GuildId), Is.True);
        Assert.That(await obelisk.Npc.AttackByAsync(master, null, false).ConfigureAwait(false), Is.Not.Null);
        Assert.That(await obelisk.Npc.AttackByAsync(enemy, null, false).ConfigureAwait(false), Is.Null, "the shield is down only for the guild on the auras");

        await this.PlaceAsync(members[2], 92, 60, 190).ConfigureAwait(false);
        await this.AdvanceAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        Assert.That(obelisk.IsOpenFor(master.GuildStatus.GuildId), Is.False, "the shield is back when an aura is left");
    }

    /// <summary>
    /// The guild whose member destroys an obelisk holds it; the member gets the points, and the obelisk comes back after
    /// its regeneration time with the occupied health.
    /// </summary>
    [Test]
    public async ValueTask TakenObeliskComesBackWithTheOccupiedHealthAsync()
    {
        var (master, _) = await this.StartWarAsync().ConfigureAwait(false);
        var obelisk = this.Context.Obelisks[1];
        obelisk.OpenForGuildId = master.GuildStatus!.GuildId;

        await this.Context.OnKilledAsync(obelisk.Npc!, master).ConfigureAwait(false);

        Assert.That(obelisk.OwnerGuildId, Is.EqualTo(master.GuildStatus.GuildId));
        Assert.That(this.Context.GetPoints(master), Is.EqualTo(50));
        Assert.That(obelisk.Npc, Is.Null);
        await this.AdvanceAsync(TimeSpan.FromSeconds(19)).ConfigureAwait(false);
        Assert.That(obelisk.Npc, Is.Null);
        await this.AdvanceAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        Assert.That(obelisk.Npc?.Health, Is.EqualTo(300_000));
        Assert.That(obelisk.IsOpenFor(master.GuildStatus.GuildId), Is.False, "a guild can't hurt its own obelisk");
    }

    /// <summary>
    /// On the maps of the war, members of one guild can't hurt each other, members of other guilds fight without the player
    /// killer penalty, and a kill gives the points.
    /// </summary>
    [Test]
    public async ValueTask GuildsFightEachOtherButNotThemselvesAsync()
    {
        var (master, members) = await this.StartWarAsync(1).ConfigureAwait(false);
        var (enemy, _) = await this.RegisterGuildAsync(0, false).ConfigureAwait(false);
        var outsider = await this.CreateMemberAsync(GuildPosition.GuildMaster).ConfigureAwait(false);
        foreach (var (player, x) in new[] { (master, (byte)60), (members[0], (byte)61), (enemy, (byte)62), (outsider, (byte)63) })
        {
            await this.PlaceAsync(player, 92, x, 120).ConfigureAwait(false);
        }

        Assert.That(this.Context.PreventsAttack(master, members[0]), Is.True, "same guild");
        Assert.That(this.Context.PreventsAttack(master, enemy), Is.False);
        Assert.That(this.Context.PreventsAttack(master, outsider), Is.True, "the outsider takes no part");
        Assert.That(this.Context.ExemptsFromPenalty(master, enemy), Is.True);
        Assert.That(this.Context.ExemptsFromPenalty(master, members[0]), Is.False);

        await this.Context.OnKilledAsync(enemy, master).ConfigureAwait(false);
        Assert.That(this.Context.GetPoints(master), Is.EqualTo(10));
        var monster = this.Context.Monsters.First();
        await this.Context.OnKilledAsync(monster, master).ConfigureAwait(false);
        Assert.That(this.Context.GetPoints(master), Is.EqualTo(15));
        Assert.That(this.Context.Monsters, Has.Count.EqualTo(29));
    }

    /// <summary>
    /// At the end, the guild which holds an obelisk gets the buff of its element for its online members, every participant
    /// the experience of its points; after the closing minutes everyone is moved off the maps.
    /// </summary>
    [Test]
    public async ValueTask EndGivesTheBuffAndTheExperienceAsync()
    {
        var (master, members) = await this.StartWarAsync(1).ConfigureAwait(false);
        var obelisk = this.Context.Obelisks[0];
        obelisk.OpenForGuildId = master.GuildStatus!.GuildId;
        await this.Context.OnKilledAsync(obelisk.Npc!, master).ConfigureAwait(false);
        var experience = master.SelectedCharacter!.Experience;
        var buff = this._plugIn.Configuration!.Buffs.Single(b => b.Element == obelisk.Element);

        await this.AdvanceAsync(TimeSpan.FromMinutes(30)).ConfigureAwait(false);

        Assert.That(this.Context.Stage, Is.EqualTo(ArcaWarStage.Closing));
        Assert.That(master.MagicEffectList.ContainsEffect(buff.EffectNumber), Is.True);
        Assert.That(members[0].MagicEffectList.ContainsEffect(buff.EffectNumber), Is.True, "every member of the guild");
        Assert.That(master.SelectedCharacter.Experience, Is.EqualTo(experience + 500_000), "50 points: 11 to 50");
        Assert.That(this.Context.Obelisks.Select(o => o.Npc), Has.All.Null);
        Assert.That(this.Context.Monsters, Is.Empty);

        await this.AdvanceAsync(TimeSpan.FromMinutes(10)).ConfigureAwait(false);
        Assert.That(this.Context.Stage, Is.EqualTo(ArcaWarStage.Closed));
        Assert.That(this.Context.Guilds, Is.Empty);
    }

    /// <summary>
    /// A master pays the Signs of Lord of the fee (webzen: 10) to register the guild; with fewer the guild stays out.
    /// </summary>
    [Test]
    public async ValueTask MasterPaysTheSignsOfLordAsync()
    {
        await this.AdvanceAsync(TimeSpan.Zero).ConfigureAwait(false);
        await this.AdvanceAsync(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
        var master = await this.CreateMemberAsync(GuildPosition.GuildMaster, signs: 9).ConfigureAwait(false);

        var fee = this._plugIn.Configuration!.RegistrationFee;
        Assert.That((fee?.Group, fee?.Number, fee?.Level, fee?.Count), Is.EqualTo(((byte?)14, (short?)21, (byte?)3, (byte?)10)));
        Assert.That(await this._plugIn.RequestAsync(master, ArcaWarRequestKind.RegisterGuild).ConfigureAwait(false), Is.EqualTo(ArcaWarRequestResult.NotEnoughSignsOfLord));
        Assert.That(CountSigns(master), Is.EqualTo(9), "nothing is taken from a refused master");

        await this.GiveSignsAsync(master, 1).ConfigureAwait(false);
        Assert.That(await this._plugIn.RequestAsync(master, ArcaWarRequestKind.RegisterGuild).ConfigureAwait(false), Is.EqualTo(ArcaWarRequestResult.Success));
        Assert.That(CountSigns(master), Is.Zero);
    }

    /// <summary>
    /// The damage between the members of the war is reduced by the settings of the data (S21Emu <c>DamageReduction</c>):
    /// against another guild by <c>OppositeTeam</c>, within the own guild (friendly fire) by <c>SelfTeam</c>.
    /// </summary>
    [Test]
    public async ValueTask TeamDamageIsReducedAsync()
    {
        var (master, members) = await this.StartWarAsync(1).ConfigureAwait(false);
        var (enemy, _) = await this.RegisterGuildAsync(0, false).ConfigureAwait(false);
        var configuration = this._plugIn.Configuration!;
        configuration.OppositeTeamDamageReduction = 30;
        configuration.SelfTeamDamageReduction = 20;
        configuration.FriendlyFire = true;
        foreach (var (player, x) in new[] { (master, (byte)60), (members[0], (byte)61), (enemy, (byte)62) })
        {
            await this.PlaceAsync(player, 92, x, 120).ConfigureAwait(false);
        }

        Assert.That(this.DamageFactor(master, enemy), Is.EqualTo(0.7).Within(0.0001));
        Assert.That(this.DamageFactor(master, members[0]), Is.EqualTo(0.8).Within(0.0001));

        configuration.OppositeTeamDamageReduction = 100;
        var before = enemy.Attributes![Stats.CurrentHealth] + enemy.Attributes[Stats.CurrentShield];
        for (var i = 0; i < 20; i++)
        {
            await enemy.AttackByAsync(master, null, false).ConfigureAwait(false);
        }

        Assert.That(enemy.Attributes[Stats.CurrentHealth] + enemy.Attributes[Stats.CurrentShield], Is.EqualTo(before), "a full reduction takes every hit");
    }

    /// <summary>
    /// A member of a guild which holds an obelisk has the reward of Acheron (effect 187, S21Emu <c>Effect.txt</c>: 20 %
    /// experience, 10 % item drop) while it is in Acheron, and loses it when it leaves.
    /// </summary>
    [Test]
    public async ValueTask HoldersHaveTheBonusInAcheronAsync()
    {
        var (master, _) = await this.StartWarAsync().ConfigureAwait(false);
        var obelisk = this.Context.Obelisks[0];
        obelisk.OpenForGuildId = master.GuildStatus!.GuildId;
        await this.Context.OnKilledAsync(obelisk.Npc!, master).ConfigureAwait(false);
        await this.AdvanceAsync(TimeSpan.FromMinutes(30)).ConfigureAwait(false);
        var outsider = await this.CreateMemberAsync(GuildPosition.GuildMaster).ConfigureAwait(false);

        await this.WarpToSpawnAsync(master, 91).ConfigureAwait(false);
        await this.WarpToSpawnAsync(outsider, 91).ConfigureAwait(false);

        Assert.That(master.MagicEffectList.ContainsEffect(187), Is.True);
        Assert.That(master.Attributes![Stats.BonusExperienceRate], Is.EqualTo(0.2f).Within(0.0001));
        Assert.That(master.Attributes[Stats.ItemDropRateBonus], Is.EqualTo(0.1f).Within(0.0001));
        Assert.That(outsider.MagicEffectList.ContainsEffect(187), Is.False, "only the holders");

        await this.WarpToSpawnAsync(master, 0).ConfigureAwait(false);
        Assert.That(master.MagicEffectList.ContainsEffect(187), Is.False, "only in Acheron");
        Assert.That(master.Attributes[Stats.BonusExperienceRate], Is.Zero);
    }

    /// <summary>
    /// Sir Lesnar exchanges 1 to 100 Trophies of Battle for an item of the bag of their tens (S21Emu bags 153-162): the
    /// rate starts at 50 % for each ten and grows by 5 % for each further trophy of it
    /// (<c>TrophiesOfBattleMixRate</c>, <c>TrophiesOfBattleMixRateAdd</c>).
    /// </summary>
    [Test]
    public async ValueTask SirLesnarExchangesTheTrophiesAsync()
    {
        Assert.That(this._world.Configuration.Monsters.Where(m => m.Number == 584), Has.Exactly(1).Items);
        var lesnar = this._world.Configuration.Monsters.Single(m => m.Number == 584);
        Assert.That(lesnar.NpcWindow, Is.EqualTo(NpcWindow.NpcDialog));
        var settings = lesnar.ItemCraftings.Single(c => c.Number == TrophiesOfBattleCrafting.Number).SimpleCraftingSettings!;
        var handler = new TrophiesOfBattleCrafting(settings);
        var player = await this.CreateMemberAsync(GuildPosition.NormalMember).ConfigureAwait(false);

        await this.AddTrophiesAsync(player, 15).ConfigureAwait(false);
        Assert.That(handler.TryGetRequiredItems(player, out _, out var rate), Is.Null);
        Assert.That(rate, Is.EqualTo(70), "the fifth trophy of the second ten");

        var success = settings.SuccessPercent;
        settings.SuccessPercent = 100;
        try
        {
            var (result, _) = await handler.DoMixAsync(player, 0).ConfigureAwait(false);
            Assert.That(result, Is.EqualTo(CraftingResult.Success));
        }
        finally
        {
            settings.SuccessPercent = success;
        }

        var tier = settings.ResultItems.Where(r => r.Reference == 2).Select(r => r.ItemDefinition).ToArray();
        Assert.That(player.TemporaryStorage!.Items.Single().Definition, Is.AnyOf(tier));

        foreach (var item in player.TemporaryStorage.Items.ToList())
        {
            await player.TemporaryStorage.RemoveItemAsync(item).ConfigureAwait(false);
        }

        await this.AddTrophiesAsync(player, 101).ConfigureAwait(false);
        Assert.That(handler.TryGetRequiredItems(player, out _, out _), Is.EqualTo(CraftingResult.TooManyItems));
    }

    private static int CountSigns(Player player)
        => player.Inventory!.Items.Count(i => i.Definition is { Group: 14, Number: 21 } && i.Level == 3);

    private async ValueTask<(Player Master, List<Player> Members)> StartWarAsync(int members = 0)
    {
        await this.AdvanceAsync(TimeSpan.Zero).ConfigureAwait(false);
        await this.AdvanceAsync(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
        var guild = await this.RegisterGuildAsync(members, true).ConfigureAwait(false);
        await this.AdvanceAsync(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
        foreach (var member in guild.Members)
        {
            Assert.That(await this._plugIn.RequestAsync(member, ArcaWarRequestKind.RegisterMember).ConfigureAwait(false), Is.EqualTo(ArcaWarRequestResult.Success));
        }

        await this.AdvanceAsync(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
        await this.AdvanceAsync(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
        return guild;
    }

    private async ValueTask<(Player Master, List<Player> Members)> RegisterGuildAsync(int members, bool inRegistration)
    {
        var master = await this.CreateMemberAsync(GuildPosition.GuildMaster).ConfigureAwait(false);
        var list = new List<Player>();
        for (var i = 0; i < members; i++)
        {
            list.Add(await this.CreateMemberAsync(GuildPosition.NormalMember, master).ConfigureAwait(false));
        }

        if (inRegistration)
        {
            Assert.That(await this._plugIn.RequestAsync(master, ArcaWarRequestKind.RegisterGuild).ConfigureAwait(false), Is.EqualTo(ArcaWarRequestResult.Success));
        }
        else
        {
            this.Context.AddGuild(master.GuildStatus!.GuildId, "Enemy", master.SelectedCharacter!.Id);
        }

        return (master, list);
    }

    private async ValueTask<Player> CreateMemberAsync(GuildPosition position, Player? masterOfGuild = null, int signs = 10)
    {
        var player = await this._world.CreatePlayerAsync(CharacterClassNumber.DarkKnight, 300, 0).ConfigureAwait(false);
        player.GuildStatus = new GuildMemberStatus(masterOfGuild?.GuildStatus?.GuildId ?? this._nextGuildId++, position);
        await this._world.GameContext.AddPlayerAsync(player).ConfigureAwait(false);
        if (position == GuildPosition.GuildMaster)
        {
            await this.GiveSignsAsync(player, signs).ConfigureAwait(false);
        }

        return player;
    }

    private async ValueTask GiveSignsAsync(Player player, int count)
    {
        var definition = this._world.Configuration.Items.Single(i => i.Group == 14 && i.Number == 21);
        for (var i = 0; i < count; i++)
        {
            var item = player.PersistenceContext.CreateNew<Item>();
            item.Definition = definition;
            item.Level = 3;
            item.Durability = 1;
            Assert.That(await player.Inventory!.AddItemAsync(item).ConfigureAwait(false), Is.True);
        }
    }

    private async ValueTask AddTrophiesAsync(Player player, int count)
    {
        var definition = this._world.Configuration.Items.Single(i => i.Group == 13 && i.Number == 147);
        for (var left = count; left > 0; left -= definition.Durability)
        {
            var item = player.PersistenceContext.CreateNew<Item>();
            item.Definition = definition;
            item.Durability = Math.Min(left, definition.Durability);
            Assert.That(await player.TemporaryStorage!.AddItemAsync(item).ConfigureAwait(false), Is.True);
        }
    }

    private double DamageFactor(Player attacker, Player defender)
    {
        var args = new PlayerDamageFactorArgs();
        this._plugIn.ModifyDamageFactor(attacker, defender, args);
        return args.Factor;
    }

    private async ValueTask WarpToSpawnAsync(Player player, short mapNumber)
    {
        await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
        var map = this._world.Configuration.Maps.Single(m => m.Number == mapNumber && m.Discriminator == 0);
        await player.WarpToAsync(map.ExitGates.First(g => g.IsSpawnGate)).ConfigureAwait(false);
        await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
        Assert.That(player.CurrentMap?.Definition.Number, Is.EqualTo(mapNumber));
    }

    private async ValueTask PlaceAsync(Player player, short mapNumber, byte x, byte y)
    {
        await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
        if (player.CurrentMap?.Definition.Number != mapNumber)
        {
            var map = this._world.Configuration.Maps.Single(m => m.Number == mapNumber && m.Discriminator == 0);
            await player.WarpToAsync(new ExitGate { Map = map, X1 = x, X2 = x, Y1 = y, Y2 = y }).ConfigureAwait(false);
            await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
        }

        await player.MoveAsync(new Pathfinding.Point(x, y)).ConfigureAwait(false);
    }

    private ValueTask AdvanceAsync(TimeSpan by)
    {
        this._now += by;
        return this._plugIn.TickAsync(this._world.GameContext);
    }
}
