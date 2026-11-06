// <copyright file="WorldSystemsPacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.HelperPlus;
using MUnique.OpenMU.GameLogic.HuntingRecord;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.SeasonPass;
using MUnique.OpenMU.GameLogic.SoulConverter;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView.World;
using MUnique.OpenMU.Network.Packets.Babylon;
using MiningResultPacket = MUnique.OpenMU.Network.Packets.Babylon.MiningResult;

/// <summary>
/// Tests the packets of the world systems of the Babylon protocol: mining, soul converter, ranking, hunting record,
/// season pass and helper plus. A Season 6 client gets none of them.
/// </summary>
[TestFixture]
public class WorldSystemsPacketsTest
{
    /// <summary>
    /// Only the Babylon client gets the mining view; a Season 6 client has none and is told by a message instead.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheMiningView()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, MiningBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<IMiningViewPlugIn>(), Is.InstanceOf<MiningBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<IMiningViewPlugIn>(), Is.Null);
    }

    /// <summary>
    /// Only the Babylon client gets the hunting record and ranking view.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheHuntingRecordView()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, HuntingRecordBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<IHuntingRecordViewPlugIn>(), Is.InstanceOf<HuntingRecordBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<IHuntingRecordViewPlugIn>(), Is.Null);
    }

    /// <summary>
    /// The hunting record list carries one row a day with its time, kills, experience and damage.
    /// </summary>
    [Test]
    public async ValueTask HuntingRecordListHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var day = new HuntingDay { Map = 122, Day = new DateTime(2026, 11, 6, 0, 0, 0, DateTimeKind.Utc), Seconds = 3600, Kills = 420, Experience = 5_000_000_000, Damage = 9_000_000_000 };

        await new HuntingRecordBabylonPlugIn(player).ShowHuntingRecordAsync(122, [day]).ConfigureAwait(false);

        var packet = new HuntingRecordList(output.ToArray());
        Assert.That((packet.Header.Code, packet.Header.SubCode), Is.EqualTo(((byte)0xEE, (byte)0xD1)));
        Assert.That((packet.MapNumber, packet.DayCount), Is.EqualTo(((ushort)122, (byte)1)));
        var shown = packet[0];
        Assert.That((shown.Day, shown.Seconds, shown.Kills), Is.EqualTo((1793923200u, 3600u, 420u)));
        Assert.That((shown.Experience, shown.Damage), Is.EqualTo((5_000_000_000ul, 9_000_000_000ul)));
    }

    /// <summary>
    /// The ranking page carries the kind, the paging, the own rank and the characters with their class code.
    /// </summary>
    [Test]
    public async ValueTask RankingPageHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);

        await new HuntingRecordBabylonPlugIn(player).ShowRankingPageAsync(RankingKind.WeeklyKills, 1, 3, 12, [new GameLogic.HuntingRecord.RankingEntry(11, "Hunter", 4, 7000)]).ConfigureAwait(false);

        var packet = new RankingPage(output.ToArray());
        Assert.That((packet.Header.Code, packet.Header.SubCode), Is.EqualTo(((byte)0xEE, (byte)0xD0)));
        Assert.That((packet.Kind, packet.Page, packet.PageCount, packet.OwnRank, packet.EntryCount), Is.EqualTo(((byte)1, (byte)1, (byte)3, (ushort)12, (byte)1)));
        var entry = packet[0];
        Assert.That((entry.Rank, entry.Name, entry.Value), Is.EqualTo(((ushort)11, "Hunter", 7000u)));
        Assert.That(entry.ClassCode, Is.EqualTo(ClassCodes.ToCode(4)), "The class code of the Dark Knight.");
    }

    /// <summary>
    /// The mining result carries the spot, the result, the stage and the jewels.
    /// </summary>
    [Test]
    public async ValueTask MiningResultHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var spot = new NonPlayerCharacter(null!, new MonsterDefinition { Number = 607 }, null!) { Id = 0x1234 };

        await new MiningBabylonPlugIn(player).ShowMiningResultAsync(spot, GameLogic.Mining.MiningResult.Collected, 5, 125, 1000).ConfigureAwait(false);

        var data = output.ToArray();
        Assert.That(data, Has.Length.EqualTo(MiningResultPacket.Length));
        var packet = new MiningResultPacket(data);
        Assert.That((packet.Header.Code, packet.Header.SubCode), Is.EqualTo(((byte)0xEE, (byte)0xA3)));
        Assert.That((packet.NpcId, packet.Result, packet.Stage), Is.EqualTo(((ushort)0x1234, (byte)3, (byte)5)));
        Assert.That((packet.ItemCount, packet.BonusCount), Is.EqualTo(((ushort)125, (ushort)1000)));
    }

    /// <summary>
    /// Only the Babylon client gets the monster soul converter view.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheSoulConverterView()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, SoulConverterBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<ISoulConverterViewPlugIn>(), Is.InstanceOf<SoulConverterBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<ISoulConverterViewPlugIn>(), Is.Null);
    }

    /// <summary>
    /// The converter state carries the tier, the activations left, the expiry and the missions.
    /// </summary>
    [Test]
    public async ValueTask SoulConverterStateHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var mission = new SoulMissionProgress { MapNumber = 33, MonsterNumber = 308, Souls = 120, RequiredSouls = 500 };

        await new SoulConverterBabylonPlugIn(player).ShowSoulConverterStateAsync(1, 2, new DateTime(2026, 11, 12, 0, 0, 0, DateTimeKind.Utc), [mission]).ConfigureAwait(false);

        var packet = new SoulConverterState(output.ToArray());
        Assert.That((packet.Header.Code, packet.Header.SubCode), Is.EqualTo(((byte)0xEE, (byte)0xA5)));
        Assert.That((packet.Tier, packet.PurchasesLeft, packet.ExpiresAt, packet.MissionCount), Is.EqualTo(((byte)1, (byte)2, 1794441600u, (byte)1)));
        var shown = packet[0];
        Assert.That((shown.MapNumber, shown.MonsterNumber, shown.Souls, shown.RequiredSouls), Is.EqualTo(((ushort)33, (ushort)308, (ushort)120, (ushort)500)));
    }

    /// <summary>
    /// The converter result carries the result and the reward kind.
    /// </summary>
    [Test]
    public async ValueTask SoulConverterResultHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);

        await new SoulConverterBabylonPlugIn(player).ShowSoulConverterResultAsync(SoulConverterClaimResult.KindNotOffered, SoulRewardKind.Accessory).ConfigureAwait(false);

        var packet = new Network.Packets.Babylon.SoulConverterResult(output.ToArray());
        Assert.That((packet.Header.SubCode, packet.Result, packet.RewardKind), Is.EqualTo(((byte)0xA7, (byte)3, (byte)3)));
    }

    /// <summary>
    /// Only the Babylon client gets the helper plus view.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheHelperPlusView()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, HelperPlusBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<IHelperPlusViewPlugIn>(), Is.InstanceOf<HelperPlusBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<IHelperPlusViewPlugIn>(), Is.Null);
    }

    /// <summary>
    /// The helper plus rules carry the flags, the level, the ranges and the cost.
    /// </summary>
    [Test]
    public async ValueTask HelperPlusRulesHaveTheirFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var configuration = new HelperPlusConfiguration { PlusMinimumTotalLevel = 400, HuntRange = 6, PickupRange = 5, PlusHuntRange = 10, PlusPickupRange = 9, PlusCostPercent = 150 };

        await new HelperPlusBabylonPlugIn(player).ShowHelperPlusRulesAsync(HelperPlusFlags.MapHasZones | HelperPlusFlags.PlusOn, configuration).ConfigureAwait(false);

        var packet = new HelperPlusRules(output.ToArray());
        Assert.That((packet.Header.SubCode, packet.Flags, packet.PlusMinimumLevel), Is.EqualTo(((byte)0xE0, (byte)5, (ushort)400)));
        Assert.That((packet.HuntRange, packet.PickupRange, packet.PlusHuntRange, packet.PlusPickupRange, packet.PlusCostPercent), Is.EqualTo(((byte)6, (byte)5, (byte)10, (byte)9, (ushort)150)));
    }

    /// <summary>
    /// The helper plus move result carries the zone and the result.
    /// </summary>
    [Test]
    public async ValueTask HelperPlusMoveResultHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);

        await new HelperPlusBabylonPlugIn(player).ShowHelperPlusMoveResultAsync(42, GameLogic.HelperPlus.HelperPlusMoveResult.NotEnoughMoney).ConfigureAwait(false);

        var packet = new Network.Packets.Babylon.HelperPlusMoveResult(output.ToArray());
        Assert.That((packet.Header.SubCode, packet.ZoneIndex, packet.Result), Is.EqualTo(((byte)0xE2, (ushort)42, (byte)3)));
    }

    /// <summary>
    /// Only the Babylon client gets the season pass view.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheSeasonPassView()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, SeasonPassBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<ISeasonPassViewPlugIn>(), Is.InstanceOf<SeasonPassBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<ISeasonPassViewPlugIn>(), Is.Null);
    }

    /// <summary>
    /// The season pass state carries the season, the level, the passes, the claims and the missions; a special mission has
    /// the bit 7 in its index.
    /// </summary>
    [Test]
    public async ValueTask SeasonPassStateHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var state = new SeasonPassView(11, new DateTime(2026, 11, 12, 0, 0, 0, DateTimeKind.Utc), 3, 650, 0b011, 0b101, 0b1, 0, [new SeasonPassMissionState(1, 2, 300), new SeasonPassMissionState(128, 0, 7200)]);

        await new SeasonPassBabylonPlugIn(player).ShowSeasonPassStateAsync(state).ConfigureAwait(false);

        var packet = new SeasonPassState(output.ToArray());
        Assert.That((packet.Header.SubCode, packet.Season, packet.EndsAt, packet.Level, packet.Experience), Is.EqualTo(((byte)0xD2, (ushort)11, 1794441600u, (byte)3, 650u)));
        Assert.That((packet.Passes, packet.ClaimedFree, packet.ClaimedGold, packet.ClaimedPremium, packet.MissionCount), Is.EqualTo(((byte)3, 5u, 1u, 0u, (byte)2)));
        Assert.That((packet[0].Index, packet[0].Completions, packet[0].Progress), Is.EqualTo(((byte)1, (byte)2, 300u)));
        Assert.That((packet[1].Index, packet[1].Progress), Is.EqualTo(((byte)0x80, 7200u)));
    }

    /// <summary>
    /// The claim result carries the pass, the level and the result.
    /// </summary>
    [Test]
    public async ValueTask SeasonPassClaimResultHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);

        await new SeasonPassBabylonPlugIn(player).ShowSeasonPassClaimResultAsync(SeasonPassKind.Gold, 4, GameLogic.SeasonPass.SeasonPassClaimResult.AlreadyClaimed).ConfigureAwait(false);

        var packet = new Network.Packets.Babylon.SeasonPassClaimResult(output.ToArray());
        Assert.That((packet.Header.SubCode, packet.Pass, packet.Level, packet.Result), Is.EqualTo(((byte)0xD6, (byte)1, (byte)4, (byte)2)));
    }
}
