// <copyright file="ClassCreationRuleTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Classes;

using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using BasicAccount = MUnique.OpenMU.Persistence.BasicModel.Account;
using BasicCharacter = MUnique.OpenMU.Persistence.BasicModel.Character;
using BasicCharacterClass = MUnique.OpenMU.Persistence.BasicModel.CharacterClass;
using BasicGameMap = MUnique.OpenMU.Persistence.BasicModel.GameMapDefinition;
using BasicStatAttribute = MUnique.OpenMU.Persistence.BasicModel.StatAttribute;

/// <summary>
/// Tests <see cref="ClassCreationRule"/>, the rule which the server enforces on a creation and the Babylon client gets
/// for every class.
/// </summary>
[TestFixture]
public class ClassCreationRuleTest
{
    /// <summary>
    /// A class without an unlock flag and with a creation level needs another character at the level, or the unlock.
    /// </summary>
    [Test]
    public void ClassWithoutFlagNeedsTheLevelOrTheUnlock()
    {
        var slayer = CreateClass(128 + 72, levelRequirement: 200);
        var account = new BasicAccount();

        Assert.That(ClassCreationRule.GetState(account, slayer), Is.EqualTo(ClassCreationState.LevelRequired));

        account.Characters.Add(CreateCharacter(199));
        Assert.That(ClassCreationRule.GetState(account, slayer), Is.EqualTo(ClassCreationState.LevelRequired));

        account.Characters.Add(CreateCharacter(200));
        Assert.That(ClassCreationRule.GetState(account, slayer), Is.EqualTo(ClassCreationState.Creatable));

        var unlocked = new BasicAccount();
        unlocked.UnlockedCharacterClasses.Add(slayer);
        Assert.That(ClassCreationRule.GetState(unlocked, slayer), Is.EqualTo(ClassCreationState.Creatable));
    }

    /// <summary>
    /// A class with a Season 6 unlock flag is creatable once unlocked; before, the client checks it, as in the original.
    /// </summary>
    [Test]
    public void ClassWithFlagNeedsTheUnlock()
    {
        var gladiator = CreateClass(12, levelRequirement: 220, flag: 4);
        var account = new BasicAccount();
        account.Characters.Add(CreateCharacter(400));

        Assert.That(ClassCreationRule.GetState(account, gladiator), Is.EqualTo(ClassCreationState.UnlockRequired));
        Assert.That(ClassCreationRule.IsRefused(ClassCreationState.UnlockRequired), Is.False);

        account.UnlockedCharacterClasses.Add(gladiator);
        Assert.That(ClassCreationRule.GetState(account, gladiator), Is.EqualTo(ClassCreationState.Creatable));
    }

    /// <summary>
    /// A class which can't get created, is inactive or has an inactive home map is unavailable.
    /// </summary>
    [Test]
    public void InactiveClassIsUnavailable()
    {
        var account = new BasicAccount();

        Assert.That(ClassCreationRule.GetState(account, CreateClass(0)), Is.EqualTo(ClassCreationState.Creatable));
        var notCreatable = CreateClass(0);
        notCreatable.CanGetCreated = false;
        Assert.That(ClassCreationRule.GetState(account, notCreatable), Is.EqualTo(ClassCreationState.Unavailable));
        var inactive = CreateClass(0);
        inactive.IsActive = false;
        Assert.That(ClassCreationRule.GetState(account, inactive), Is.EqualTo(ClassCreationState.Unavailable));
        var closedHome = CreateClass(0);
        closedHome.HomeMap!.IsActive = false;
        Assert.That(ClassCreationRule.GetState(account, closedHome), Is.EqualTo(ClassCreationState.Unavailable));
        Assert.That(ClassCreationRule.IsRefused(ClassCreationState.Unavailable), Is.True);
        Assert.That(ClassCreationRule.IsRefused(ClassCreationState.LevelRequired), Is.True);
    }

    private static BasicCharacterClass CreateClass(byte number, int levelRequirement = 0, byte flag = 0)
    {
        return new BasicCharacterClass
        {
            Number = number,
            CanGetCreated = true,
            IsActive = true,
            HomeMap = new BasicGameMap { IsActive = true },
            LevelRequirementByCreation = (short)levelRequirement,
            CreationAllowedFlag = flag,
        };
    }

    private static BasicCharacter CreateCharacter(int level)
    {
        var character = new BasicCharacter();
        character.Attributes.Add(new BasicStatAttribute(Stats.Level, level));
        return character;
    }
}
