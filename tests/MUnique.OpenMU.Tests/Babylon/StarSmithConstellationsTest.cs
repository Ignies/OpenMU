// <copyright file="StarSmithConstellationsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// PR 7a's default catalogue (Readme.md "PR 7a contract", "Default constellations").
/// </summary>
[TestFixture]
public class StarSmithConstellationsTest
{
    /// <summary>
    /// The 22 looks constellations are default recipes 101 to 122, beside the boost recipes 1 to 11.
    /// </summary>
    [Test]
    public void TheDefaultsHoldTheLooksConstellations()
    {
        var numbers = StarSmithingConfiguration.DefaultRecipes().Select(r => (int)r.Number).ToList();
        Assert.That(numbers, Is.EqualTo(Enumerable.Range(1, 11).Concat(Enumerable.Range(101, 22))));
    }
}
