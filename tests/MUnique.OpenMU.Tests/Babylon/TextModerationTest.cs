// <copyright file="TextModerationTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.TextModeration;

/// <summary>
/// Player text on items (Star Smithing, docs/star_smithing.md "Player text"): the name and lore rules and the word list,
/// checked on NFC text with look-alike letters folded first.
/// </summary>
[TestFixture]
public class TextModerationTest
{
    private static readonly TextModerationConfiguration Words = new()
    {
        Words =
        {
            new ModeratedWord { Word = "badword", Substring = true },
            new ModeratedWord { Word = "ass", Substring = false, Language = "en" },
        },
    };

    /// <summary>
    /// A plain name passes as written, its ends trimmed; letters of any script, digits, ' and - are names.
    /// </summary>
    [TestCase("Frost Edge", "Frost Edge")]
    [TestCase("  Frost Edge ", "Frost Edge")]
    [TestCase("Épée Noire", "Épée Noire")]
    [TestCase("O'Brien-2", "O'Brien-2")]
    [TestCase("火の剣", "火の剣")]
    public void ANameThatPasses(string text, string expected)
    {
        var verdict = TextModeration.Check(TextKind.Name, text, Words);
        Assert.That((verdict.Refusal, verdict.Text), Is.EqualTo((TextRefusal.None, expected)));
    }

    /// <summary>
    /// The name is checked and stored as NFC: a decomposed accent is the same name.
    /// </summary>
    [Test]
    public void ANameIsNormalised()
    {
        var verdict = TextModeration.Check(TextKind.Name, "Épée", Words);
        Assert.That(verdict.Text, Is.EqualTo("Épée"));
    }

    /// <summary>
    /// What a name may not be.
    /// </summary>
    [TestCase("", TextRefusal.Empty)]
    [TestCase("ab", TextRefusal.TooShort)]
    [TestCase("abcdefghijklmnopqrstuvwxy", TextRefusal.TooLong)]
    [TestCase("Frost  Edge", TextRefusal.DoubleSpace)]
    [TestCase("Frost_Edge", TextRefusal.BadCharacter)]
    [TestCase("Frost!", TextRefusal.BadCharacter)]
    [TestCase("Frost​Edge", TextRefusal.BadCharacter)]
    public void ANameThatIsRefused(string text, TextRefusal refusal)
    {
        Assert.That(TextModeration.Check(TextKind.Name, text, Words).Refusal, Is.EqualTo(refusal));
    }

    /// <summary>
    /// Lore: up to 140 characters on up to 3 lines, punctuation allowed.
    /// </summary>
    [Test]
    public void LoreThatPasses()
    {
        var verdict = TextModeration.Check(TextKind.Lore, "Forged in ice, 1,000 years ago.\r\nNever thawed!", Words);
        Assert.That((verdict.Refusal, verdict.Text), Is.EqualTo((TextRefusal.None, "Forged in ice, 1,000 years ago.\nNever thawed!")));
    }

    /// <summary>
    /// What lore may not be: too long, too many lines, control characters, links, colour codes.
    /// </summary>
    [TestCase(141, null, TextRefusal.TooLong)]
    [TestCase(0, "one\ntwo\nthree\nfour", TextRefusal.TooManyLines)]
    [TestCase(0, "tab\there", TextRefusal.BadCharacter)]
    [TestCase(0, "see http://x.io", TextRefusal.Link)]
    [TestCase(0, "visit www.example.com now", TextRefusal.Link)]
    [TestCase(0, "the best server: mu-best.net", TextRefusal.Link)]
    [TestCase(0, "red #ff0000 blade", TextRefusal.ColourCode)]
    [TestCase(0, "^1red blade", TextRefusal.ColourCode)]
    [TestCase(0, "{color:red}blade", TextRefusal.ColourCode)]
    public void LoreThatIsRefused(int length, string? text, TextRefusal refusal)
    {
        Assert.That(TextModeration.Check(TextKind.Lore, text ?? new string('a', length), Words).Refusal, Is.EqualTo(refusal));
    }

    /// <summary>
    /// A substring entry is found inside words, whatever the case, with digits and symbols read as the letters they
    /// stand for and look-alike letters of other scripts folded.
    /// </summary>
    [TestCase("A B4DW0RD here")]
    [TestCase("my b@dword")]
    [TestCase("bаdword")]
    [TestCase("supERbadwordy")]
    public void ASubstringEntryIsFound(string text)
    {
        var verdict = TextModeration.Check(TextKind.Lore, text, Words);
        Assert.That((verdict.Refusal, verdict.Word), Is.EqualTo((TextRefusal.Word, "badword")));
    }

    /// <summary>
    /// An exact entry is a whole word only: "Glass" is no hit for "ass".
    /// </summary>
    [Test]
    public void AnExactEntryIsAWholeWord()
    {
        Assert.That(TextModeration.Check(TextKind.Name, "Glass Blade", Words).Refusal, Is.EqualTo(TextRefusal.None));
        Assert.That(TextModeration.Check(TextKind.Name, "Ass Blade", Words).Refusal, Is.EqualTo(TextRefusal.Word));
    }

    /// <summary>
    /// Without a configuration only the rules apply.
    /// </summary>
    [Test]
    public void WithoutAWordList()
    {
        Assert.That(TextModeration.Check(TextKind.Name, "Badword Blade", null).Refusal, Is.EqualTo(TextRefusal.None));
    }
}
