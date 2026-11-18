// <copyright file="ItemCustomizationReportGateTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.ItemCustomization;

/// <summary>
/// A player reports an item's name or lore from its tooltip (EE 37): once per item and version, at most one report every
/// ten seconds.
/// </summary>
[TestFixture]
public class ItemCustomizationReportGateTest
{
    private static readonly Guid Item = Guid.NewGuid();

    /// <summary>
    /// The first report goes through; the same again is "already", whenever it comes.
    /// </summary>
    [Test]
    public void OncePerItemAndVersion()
    {
        var gate = new ItemCustomizationReportGate();
        Assert.That(gate.Try("Reporter", Item, 1, 0), Is.EqualTo(ItemCustomizationReportResult.Sent));
        Assert.That(gate.Try("Reporter", Item, 1, 1_000), Is.EqualTo(ItemCustomizationReportResult.Already));
        Assert.That(gate.Try("Reporter", Item, 1, 60_000), Is.EqualTo(ItemCustomizationReportResult.Already));
    }

    /// <summary>
    /// A new version (the name changed) can be reported again, and another player reports on their own.
    /// </summary>
    [Test]
    public void ANewVersionOrAnotherReporter()
    {
        var gate = new ItemCustomizationReportGate();
        gate.Try("Reporter", Item, 1, 0);
        Assert.That(gate.Try("Other", Item, 1, 0), Is.EqualTo(ItemCustomizationReportResult.Sent));
        Assert.That(gate.Try("Reporter", Item, 2, 10_000), Is.EqualTo(ItemCustomizationReportResult.Sent));
    }

    /// <summary>
    /// Another item within ten seconds of the last report is "too fast".
    /// </summary>
    [Test]
    public void AtMostOneEveryTenSeconds()
    {
        var gate = new ItemCustomizationReportGate();
        gate.Try("Reporter", Item, 1, 0);
        Assert.That(gate.Try("Reporter", Guid.NewGuid(), 1, 9_999), Is.EqualTo(ItemCustomizationReportResult.TooFast));
        Assert.That(gate.Try("Reporter", Guid.NewGuid(), 1, 10_000), Is.EqualTo(ItemCustomizationReportResult.Sent));
    }

    /// <summary>
    /// The result codes on the wire (EE 38).
    /// </summary>
    [Test]
    public void ResultCodes()
    {
        Assert.That(
            new[] { ItemCustomizationReportResult.Sent, ItemCustomizationReportResult.Already, ItemCustomizationReportResult.NotFound, ItemCustomizationReportResult.TooFast }.Select(r => (byte)r),
            Is.EqualTo(new byte[] { 0, 1, 2, 3 }));
    }
}
