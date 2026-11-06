// <copyright file="WorldStateFileTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.FieldBosses;

using System.IO;
using MUnique.OpenMU.GameLogic;

/// <summary>
/// Tests the <see cref="WorldStateFile"/>, which keeps small runtime states over a restart.
/// </summary>
[TestFixture]
public class WorldStateFileTest
{
    /// <summary>
    /// A saved state is read back by a new instance; a missing or broken file reads as nothing.
    /// </summary>
    [Test]
    public void SavedStateIsReadBack()
    {
        var path = Path.Combine(Path.GetTempPath(), $"openmu-state-{Guid.NewGuid():N}.json");
        try
        {
            Assert.That(new WorldStateFile(path).Load<Dictionary<string, int>>(), Is.Null);

            new WorldStateFile(path).Save(new Dictionary<string, int> { ["Nix"] = 3 });
            Assert.That(new WorldStateFile(path).Load<Dictionary<string, int>>()!["Nix"], Is.EqualTo(3));

            File.WriteAllText(path, "{ not json");
            Assert.That(new WorldStateFile(path).Load<Dictionary<string, int>>(), Is.Null);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
