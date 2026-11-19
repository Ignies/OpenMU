// <copyright file="NewPowerAttributesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.NewPower;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The attributes of the fifth class "New Power" (Nuke, Bolt, Wide): one set per kind.
/// </summary>
[TestFixture]
public class NewPowerAttributesTest
{
    /// <summary>
    /// Each kind has its extra damage, attack increase and the four rates of the card options.
    /// </summary>
    /// <param name="kind">The kind.</param>
    [TestCase("Nuke")]
    [TestCase("Bolt")]
    [TestCase("Wide")]
    public void EachKindHasItsAttributes(string kind)
    {
        foreach (var part in new[] { "ExtraDamage", "AttackIncrease", "CriticalChance", "ExcellentChance", "DoubleChance", "TripleChance" })
        {
            Assert.That(typeof(Stats).GetProperty(kind + part)?.GetValue(null), Is.InstanceOf<AttributeDefinition>(), kind + part);
        }
    }
}
