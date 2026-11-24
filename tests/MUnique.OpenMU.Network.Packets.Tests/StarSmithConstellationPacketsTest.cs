// <copyright file="StarSmithConstellationPacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Network.Packets.Tests;

using NUnit.Framework;
using MUnique.OpenMU.Network.Packets.Babylon;

/// <summary>
/// Star Smithing PR 7a's packet sizes (GameLogic/StarSmithing/Readme.md "PR 7a contract", "Wire").
/// </summary>
[TestFixture]
public class StarSmithConstellationPacketsTest
{
    /// <summary>
    /// EE 9A carries a point, EE 9B an action, EE 9D the attempt, EE 9E a node or stash answer, EE 9F the result, and
    /// EE 36 grows by the layers.
    /// </summary>
    [Test]
    public void TheSizesAreTheContracts()
    {
        Assert.Multiple(() =>
        {
            Assert.That(StarSmithForgeNodeRef.Length, Is.EqualTo(8));
            Assert.That(StarSmithForgeSealRef.Length, Is.EqualTo(5));
            Assert.That(StarSmithForgeAttemptRef.Length, Is.EqualTo(179));
            Assert.That(StarSmithForgeNodeResultRef.Length, Is.EqualTo(107));
            Assert.That(StarSmithForgeResultRef.Length, Is.EqualTo(41));
            Assert.That(ItemCustomizationDetailRef.Length, Is.EqualTo(600));
        });
    }
}
