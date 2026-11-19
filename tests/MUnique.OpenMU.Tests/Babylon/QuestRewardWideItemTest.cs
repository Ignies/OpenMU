// <copyright file="QuestRewardWideItemTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.DataModel.Configuration.Quests;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.GameServer.RemoteView.Quest;
using MUnique.OpenMU.Network.Packets.ServerToClient;

/// <summary>
/// The quest state and progress (F6 0C) of the fifth class quests 10-12 reward an ability card (20/235-237). On the
/// Season 21 wire (W32, 32 bytes an item) such an item doesn't fit the reward's item field of the Season 6 struct (12
/// bytes, or 15 in the extended one): the struct keeps the reward's type, number and item id, which the client reads.
/// </summary>
[TestFixture]
public class QuestRewardWideItemTest
{
    /// <summary>
    /// The card reward serializes on the W32 wire without a failure: type item, the item id of 20/235, the count.
    /// </summary>
    [Test]
    public void CardRewardFitsTheSeasonSixStructOnTheWideWire()
    {
        var reward = new Persistence.BasicModel.QuestReward
        {
            RewardType = QuestRewardType.Item,
            Value = 1,
            ItemReward = new Persistence.BasicModel.Item { Definition = new Persistence.BasicModel.ItemDefinition { Group = 20, Number = 235, Width = 1, Height = 1 }, Durability = 1 },
        };
        var data = new byte[QuestRewardRef.Length];
        Exception? failure = null;
        try
        {
            QuestStructExtensions.AssignReward(reward, new QuestRewardRef(data), new ItemSerializerW32());
        }
        catch (ArgumentException ex)
        {
            failure = ex;
        }

        Assert.That(failure, Is.Null);
        var written = new QuestRewardRef(data);
        Assert.That((written.RewardId, written.RewardCount), Is.EqualTo(((ushort)((20 << 9) | 235), 1u)));
    }
}
